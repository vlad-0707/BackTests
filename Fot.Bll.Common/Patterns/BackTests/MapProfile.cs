using Fot.Dal.Models.Patterns.BackTests;
using Fot.Dto.Patterns.BackTests;

namespace Fot.Bll.Common.Patterns.BackTests;

public class MapProfile : Profile
{
	public MapProfile()
	{
		CreateMap<BackTestDal, PatternBackTestDto>();
		CreateMap<BackTestPartDal, PatternBackTestPartDto>();

		CreateMap<BackTestResultDal, BackTestResultDto>();
		CreateMap<BackTestsPartPnlSettingDal, BackTestsPartPnlSettingDto>()
			.ReverseMap();

		CreateMap<CreatePatternBackTestRequest, BackTestDal>();
		CreateMap<CreatePatternBackTestPartRequest, BackTestPartDal>();

		CreateMap<UpdatePatternBackTestRequest, BackTestDal>()
			.ForMember(x => x.Id, mo => mo.Ignore())
			.ForMember(x => x.Parts, mo => mo.Ignore())
			;

		CreateMap<UpdatePatternBackTestPartRequest, BackTestPartDal>()
			.ForMember(x => x.PnlSettings, mo => mo.Ignore());

		CreateMap<BackTestPartDal, BackTestPartStateDto>();

		CreateMap<BackTestDal, BackTestStateDto>();
	}
}
