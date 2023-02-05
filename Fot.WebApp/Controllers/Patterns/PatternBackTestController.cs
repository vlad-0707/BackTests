using Fot.Bll.Common.Patterns.BackTests;
using Fot.Common.Models;
using Fot.Dto.Patterns.BackTests;

namespace Fot.WebApp.Controllers.Patterns;

[ApiController]
[OpenApiTag("PatternBackTest")]
[Route("api/v1/pattern/back-test")]
public class PatternBackTestController : ControllerBase
{
	public PatternBackTestController(
		IWebPatternBackTestManager manager)
	{
		_manager = manager;
	}

	private readonly IWebPatternBackTestManager _manager;

	[HttpGet("{id}")]
	[Authorize(Roles = $"{Roles.BackTestManager}, {Roles.BackTestReader}")]
	[SwaggerResponse(System.Net.HttpStatusCode.OK, typeof(ApiResponse<PatternBackTestDto>))]
	public Task<PatternBackTestDto> GetAsync([FromRoute] int id)
	{
		return _manager.GetAsync(id);
	}


	[HttpPost("list")]
	[Authorize(Roles = $"{Roles.BackTestManager}, {Roles.BackTestReader}")]
	[SwaggerResponse(System.Net.HttpStatusCode.OK, typeof(ApiResponse<ListDto<PatternBackTestDto>>))]
	public Task<ListDto<PatternBackTestDto>> GetList(CriteriaDto<PatternBackTestFilterDto> criteria)
	{
		return _manager.GetListAsync(criteria);
	}

	[HttpPost("create")]
	[Authorize(Roles = Roles.BackTestManager)]
	[SwaggerResponse(System.Net.HttpStatusCode.OK, typeof(ApiResponse<PatternBackTestDto>))]
	public Task<PatternBackTestDto> Create(CreatePatternBackTestRequest request)
	{
		return _manager.CreateAsync(request);
	}

	[HttpPost("update")]
	[Authorize(Roles = Roles.BackTestManager)]
	[SwaggerResponse(System.Net.HttpStatusCode.OK, typeof(ApiResponse<PatternBackTestDto>))]
	public Task<PatternBackTestDto> Update(UpdatePatternBackTestRequest request)
	{
		return _manager.UpdateAsync(request);
	}

	[HttpPost("{id}/start")]
	[Authorize(Roles = Roles.BackTestManager)]
	[SwaggerResponse(System.Net.HttpStatusCode.OK,typeof(ApiResponse))]
	public Task Start([FromRoute] int id)
	{
		return _manager.StartAsync(id);
	}

	[HttpPost("{id}/stop")]
	[Authorize(Roles = Roles.BackTestManager)]
	[SwaggerResponse(System.Net.HttpStatusCode.OK, typeof(ApiResponse))]
	public Task Stop([FromRoute] int id)
	{
		return _manager.StopAsync(id);
	}

	[HttpPost("{id}/reset")]
	[Authorize(Roles = Roles.BackTestManager)]
	[SwaggerResponse(System.Net.HttpStatusCode.OK, typeof(ApiResponse))]
	public Task Reset([FromRoute] int id)
	{
		return _manager.ResetAsync(id);
	}
}
