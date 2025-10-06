using System.Text.Json.Serialization;

namespace RaquelMenopausa.Cms.Models.Dto
{
    public class IndicatorsDto
    {
        [JsonPropertyName("articlesCount")]
        public int ArticlesCount { get; set; }

        [JsonPropertyName("publishedArticlesCount")]
        public int PublishedArticlesCount { get; set; }

        [JsonPropertyName("draftArticlesCount")]
        public int DraftArticlesCount { get; set; }
    }

}
