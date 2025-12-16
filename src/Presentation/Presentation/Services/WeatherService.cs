using AdaptiveCards.Templating;
using Audis.Analyzer.Common.Extensions.V1;
using Audis.Analyzer.Common.V1;
using Audis.Analyzer.Contract.V1;
using Audis.Primitives;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Presentation.Services
{
    public class WeatherService : IAdaptiveCardController
    {
        private readonly KnowledgeIdentifier knowledgeIdentifier = new("#wetter");
        private readonly string necessaryKnowledgeValue = "ja";
        private readonly IWebHostEnvironment environment;

        public WeatherService(IWebHostEnvironment environment)
        {
            this.environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        public async Task<ActionResult<AdaptiveCardResultDto>> GetAdaptiveCardResult(
            TenantId tenantId,
            AnalyzerRequestDto analyzerRequestDto,
            CancellationToken cancellationToken)
        {

            if (analyzerRequestDto.TryGetSingleKnowledgeValue(this.knowledgeIdentifier, out var knowledgeValue) &&
                knowledgeValue.Value == this.necessaryKnowledgeValue && (analyzerRequestDto.Data == null || !analyzerRequestDto.Data.Value<bool>($"hasHandledWeather:{knowledgeValue.Value}")))
            {
                // read data from demo adaptive card
                var rootPath = this.environment.ContentRootPath;
                var cardTemplateJson = File.ReadAllText(Path.Combine(rootPath, "Resources/weather-card.json"));
                var cardData = File.ReadAllText(Path.Combine(rootPath, "Resources/weather-data.json"));

                // create adaptive card
                var template = new AdaptiveCardTemplate(cardTemplateJson);

                var adaptiveCard = template.Expand(cardData);

                var resultData = new JObject
                {
                    [$"hasHandledWeather:{knowledgeValue.Value}"] = true
                };

                // merge data
                if (analyzerRequestDto.Data is not null)
                {
                    resultData.Merge(
                        analyzerRequestDto.Data,
                        new JsonMergeSettings
                        {
                            // union array values together to avoid duplicates
                            MergeArrayHandling = MergeArrayHandling.Union
                        });
                }

                var result = new AdaptiveCardResultDto
                {
                    InterrogationId = analyzerRequestDto.InterrogationId,
                    InterrogationProcessStepId = analyzerRequestDto.InterrogationProcessStepId,
                    RemoveWithNextProcessStep = false,
                    Title = "Wetter - Asten",
                    Timestamp = DateTime.UtcNow,
                    AdaptiveCardJson = adaptiveCard,
                    SuggestedKnowledge = new List<SuggestedKnowledgeDto>
                        {
                            new()
                            {
                                KnowledgeIdentifier = new KnowledgeIdentifier("#wetter.temperatur"),
                                LastUpdated = DateTime.UtcNow,
                                Origin = KnowledgeOrigin.Client,
                                Probability = 1,
                                Values = new HashSet<KnowledgeValueDto>()
                                {
                                    new() {
                                        KnowledgeValue = new KnowledgeValue("10 Grad Celsius"),
                                    }
                                }
                            }
                        },
                    Data = resultData
                };

                return result;
            }

            return null;
        }
    }
}
