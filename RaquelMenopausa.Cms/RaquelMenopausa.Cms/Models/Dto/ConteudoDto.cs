using System.Text.Json.Serialization;

namespace RaquelMenopausa.Cms.Models.Dto
{
    public class ConteudoDto
    {
        [JsonPropertyName("article_id")]
        public string ArticleId { get; set; }

        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("intro")]
        public string Intro { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("references")]
        public string References { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("url_image")]
        public string UrlImage { get; set; }

        [JsonPropertyName("url_video")]
        public string UrlVideo { get; set; }

        [JsonPropertyName("url_audio")]
        public string UrlAudio { get; set; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; }

        [JsonPropertyName("shares_count")]
        public int SharesCount { get; set; }

        [JsonPropertyName("likes_count")]
        public int LikesCount { get; set; }

        [JsonPropertyName("hearts_count")]
        public int HeartsCount { get; set; }

        [JsonPropertyName("sads_count")]
        public int SadsCount { get; set; }

        [JsonPropertyName("comments_count")]
        public int CommentsCount { get; set; }

        [JsonPropertyName("date_created")]
        public DateTime DateCreated { get; set; }

        [JsonPropertyName("date_updated")]
        public DateTime DateUpdated { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("published_at")]
        public DateTime? PublishedAt { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("share_link")]
        public string ShareLink { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("articleArticleCategories")]
        public List<ArticleCategoryLinkDto> ArticleArticleCategories { get; set; }

        [JsonPropertyName("articleSymptomCategories")]
        public List<ArticleSymptomCategoryLinkDto> ArticleSymptomCategories { get; set; }

        [JsonPropertyName("articleSolutions")]
        public List<ArticleSolutionLinkDto> ArticleSolutions { get; set; }
    }

    // ----------- CATEGORIAS -------------
    public class ArticleCategoryLinkDto
    {
        [JsonPropertyName("article_id")]
        public string ArticleId { get; set; }

        [JsonPropertyName("article_category_id")]
        public int ArticleCategoryId { get; set; }

        [JsonPropertyName("articleCategory")]
        public ArticleCategoryDetailDto ArticleCategory { get; set; }
    }

    public class ArticleCategoryDetailDto
    {
        [JsonPropertyName("article_category_id")]
        public int ArticleCategoryId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    // ----------- SINTOMAS -------------
    public class ArticleSymptomCategoryLinkDto
    {
        [JsonPropertyName("article_id")]
        public string ArticleId { get; set; }

        [JsonPropertyName("symptom_category_id")]
        public int SymptomCategoryId { get; set; }

        [JsonPropertyName("symptomCategory")]
        public SymptomCategoryDetailDto SymptomCategory { get; set; }
    }

    public class SymptomCategoryDetailDto
    {
        [JsonPropertyName("symptom_category_id")]
        public int SymptomCategoryId { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("order")]
        public int Order { get; set; }
    }

    // ----------- SOLUÇÕES -------------
    public class ArticleSolutionLinkDto
    {
        [JsonPropertyName("article_id")]
        public string ArticleId { get; set; }

        [JsonPropertyName("solution_id")]
        public int SolutionId { get; set; }

        [JsonPropertyName("solution")]
        public SolutionDetailDto Solution { get; set; }
    }

    public class SolutionDetailDto
    {
        [JsonPropertyName("solution_id")]
        public int SolutionId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
    }

}
