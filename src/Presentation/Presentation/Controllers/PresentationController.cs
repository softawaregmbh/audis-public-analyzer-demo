using Audis.Analyzer.Common.V1;
using Audis.Primitives;
using Microsoft.AspNetCore.Mvc;
using Presentation.Services;

namespace Audis.Analyzer.HazardousMaterialsSlim.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PresentationController : ControllerBase
    {
        private readonly WeatherService weatherService;

        public PresentationController(WeatherService weatherService)
        {
            this.weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        }

        [HttpPost("analyzer/{tenantId}/weather")]
        public async Task<ActionResult<AdaptiveCardResultDto>> GetAdaptiveCardResult(
            [FromRoute] TenantId tenantId,
            [FromBody] AnalyzerRequestDto analyzerRequestDto,
            CancellationToken cancellationToken)
        {
            return await this.weatherService.GetAdaptiveCardResult(tenantId, analyzerRequestDto, cancellationToken);
        }
    }
}
