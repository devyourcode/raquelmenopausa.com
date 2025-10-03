using System.Text.Json.Serialization;

namespace RaquelMenopausa.Cms.Models.Dto
{
    public class ArticleStatusOptionDto
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

}
