using Fot.Bll.Common.Interfaces;
using Fot.Common.Models.Filtering;
using Fot.Common.Patterns;
using Fot.Dal;
using Fot.Dal.Models.Patterns.BackTests;
using Fot.Dto.Filters;
using Fot.Dto.Patterns.BackTests;

namespace Fot.Bll.Common.Patterns.BackTests;

public class WebPatternBackTestManager : IWebPatternBackTestManager
{
	public WebPatternBackTestManager(
		IBackTestTransmitter transmitter,
		IAccessManager accessManager,
		IFotContext context,
		IMapper mapper)
	{
		_accessManager = accessManager;
		_transmitter = transmitter;
		_context = context;
		_mapper = mapper;
	}

	private readonly IMapper _mapper;
	private readonly IFotContext _context;
	private readonly IAccessManager _accessManager;
	private readonly IBackTestTransmitter _transmitter;

	public async Task<PatternBackTestDto> GetAsync(int id)
	{
		var dal = await _context.Set<BackTestDal>()
			.AsSplitQuery()
			.AsNoTracking()
			.Include(x => x.Pattern)
			.Include(x => x.Parts)
				.ThenInclude(x => x.Results)
			.Include(x => x.Parts)
				.ThenInclude(x => x.PnlSettings)
			.FirstOrDefaultAsync(x => x.Id == id);

		if (dal is null)
		{
			throw new EntityNotFoundException(nameof(BackTestDal));
		}

		return _mapper.Map<PatternBackTestDto>(dal);
	}

	public async Task<ListDto<PatternBackTestDto>> GetListAsync(CriteriaDto<PatternBackTestFilterDto> criteria)
	{
		var paging = _mapper.Map<Paging>(criteria.Paging);
		var sortList = GetSorts(criteria.Sort);

		var filter = new FilterList<BackTestDal>()
			.Add(x => x.PatternId == criteria.Filter.PatternId);

		var qparam = new QueryParams<BackTestDal>(filter, paging, sortList);

		var dals = await _context.Set<BackTestDal>()
			.Include(x => x.Parts)
			.Include(x => x.Pattern)
			.GetListAsync(qparam)
			.CAF();

		var dtos = _mapper.Map<PatternBackTestDto[]>(dals);
		criteria.Paging.Total = paging.Total;

		return new ListDto<PatternBackTestDto>(dtos, criteria.Paging);
	}

	public async Task<PatternBackTestDto> CreateAsync(CreatePatternBackTestRequest request)
	{
		var dal = _mapper.Map<BackTestDal>(request);
		var appUser  = await _accessManager.TryGetCurrentUserAsync();

		dal.UserId = appUser.Id;
		dal.StatusId = BackTestStatusEnum.New;

		foreach (var part in dal.Parts)
		{
			part.StatusId = BackTestStatusEnum.New;
		}

		_context.Add(dal);
		await _context.SaveChangesAsync();

		return await GetAsync(dal.Id);
	}

	public async Task<PatternBackTestDto> UpdateAsync(UpdatePatternBackTestRequest request)
	{
		var dal = await _context
			.Set<BackTestDal>()
			.Include(x => x.Parts)
			.Include(x => x.Pattern)
			.FirstOrDefaultAsync(x => x.Id == request.Id)
			.CAF() ?? throw new EntityNotFoundException(nameof(BackTestDal)); ;

		if(dal.StatusId is not (BackTestStatusEnum.New or BackTestStatusEnum.Dropped))
		{
			throw new LiteException("Нельзя изменять запущенный бэк-тест");
		}

		await ActualizePartsAsync(dal, request);

		_mapper.Map(request, dal);
		await _context.SaveChangesAsync();

		return await GetAsync(dal.Id);
	}

	private async Task ActualizePartsAsync(BackTestDal backTest, UpdatePatternBackTestRequest request)
	{
		await DeletePartsAsync(backTest, request);
		await UpdatePartsAsync(backTest, request);
		await AddPartsAsync(backTest, request);
	}

	private Task AddPartsAsync(BackTestDal dal, UpdatePatternBackTestRequest request)
	{
		var addingParts = request.Parts
			.ExceptBy(dal.Parts.Select(x => new { x.SymbolId, x.TimeFrameId }),
				x => new { x.SymbolId, x.TimeFrameId });

		foreach (var partDto in addingParts)
		{
			var newDal = _mapper.Map<BackTestPartDal>(partDto);

			newDal.BackTestId = dal.Id;
			newDal.StatusId = BackTestStatusEnum.New;
			newDal.PnlSettings = _mapper.Map<BackTestsPartPnlSettingDal[]>(partDto.PnlSettings);

			_context.Add(newDal);
		}

		return _context.SaveChangesAsync();
	}

	private async Task UpdatePartsAsync(BackTestDal backTest, UpdatePatternBackTestRequest request)
	{
		var joined = backTest.Parts
			.Join(request.Parts,
				dal => new { dal.SymbolId, dal.TimeFrameId },
				dto => new { dto.SymbolId, dto.TimeFrameId },
				(dal, dto) => (Dal: dal, Dto: dto))
			.ToArray();

		foreach (var part in joined)
		{
			await UpdatePartAsync(part.Dal, part.Dto);
		}
	}

	private async Task UpdatePartAsync(BackTestPartDal partDal, UpdatePatternBackTestPartRequest partDto)
	{
		var pnls = await _context.Set<BackTestsPartPnlSettingDal>()
			.Where(x => x.BackTestPartId == partDal.Id)
			.ToArrayAsync();

		var removingPnls = pnls.ExceptBy(
				partDto.PnlSettings.Select(x => x.CheckAfterMilliseconds),
				x => x.CheckAfterMilliseconds
			)
			.ToArray();

		_context.RemoveRange(removingPnls);

		var addingPnls = partDto.PnlSettings.ExceptBy(
				partDal.PnlSettings.Select(x => x.CheckAfterMilliseconds),
				x => x.CheckAfterMilliseconds
			)
			.Select(part => new BackTestsPartPnlSettingDal
			{
				BackTestPartId = partDal.Id,
				CheckAfterMilliseconds = part.CheckAfterMilliseconds
			})
			.ToArray();

		_context.AddRange(addingPnls);

		await _context.SaveChangesAsync();
	}

	private async Task DeletePartsAsync(BackTestDal dal, UpdatePatternBackTestRequest request)
	{
		var parts = dal.Parts
			.ExceptBy(request.Parts.Select(x => new { x.SymbolId, x.TimeFrameId }),
			x => new { x.SymbolId, x.TimeFrameId })
			.ToArray();

		var pnlSettings = parts
			.SelectMany(x => x.PnlSettings)
			.ToArray();

		_context.RemoveRange(pnlSettings);
		await _context.SaveChangesAsync();

		_context.RemoveRange(parts);
		await _context.SaveChangesAsync();
	}

	private SortList<BackTestDal> GetSorts(SortDto sort)
	{
		var sorts = new SortList<BackTestDal>();
		return sorts;
	}

	public Task StartAsync(int id)
	{
		return _ = _transmitter.StartBackTestAsync(id);
	}

	public Task StopAsync(int id)
	{
		return _ = _transmitter.StopBackTestAsync(id);
	}

	public Task ResetAsync(int id)
	{
		return _ = _transmitter.ResetBackTestAsync(id);
	}
}
