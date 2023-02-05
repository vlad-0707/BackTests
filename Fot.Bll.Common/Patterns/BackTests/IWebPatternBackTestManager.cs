using Fot.Dto.Filters;
using Fot.Dto.Patterns.BackTests;

namespace Fot.Bll.Common.Patterns.BackTests;

public interface IWebPatternBackTestManager
{
	Task<PatternBackTestDto> GetAsync(int id);
	Task<ListDto<PatternBackTestDto>> GetListAsync(CriteriaDto<PatternBackTestFilterDto> criteria);

	Task<PatternBackTestDto> CreateAsync(CreatePatternBackTestRequest request);
	Task<PatternBackTestDto> UpdateAsync(UpdatePatternBackTestRequest request);

	Task StartAsync(int id);
	Task StopAsync(int id);
	Task ResetAsync(int id);
}
