using System.Text.Json.Serialization;

namespace RaquelMenopausa.Cms.Models.Dto
{
    public class TagResponseDto
    {
        public List<ArticleCategoryDto> ArticleCategories { get; set; }
        public List<SymptomCategoryDto> SymptomCategories { get; set; }
        public List<SolutionDto> Solutions { get; set; }
    }

    public class ArticleCategoryDto
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class SymptomCategoryDto
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class SolutionDto
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }


}
