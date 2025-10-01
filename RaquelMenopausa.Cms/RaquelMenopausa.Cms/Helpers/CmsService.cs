using RaquelMenopausa.Cms.Models.Dto;
using System.Security.Claims;
using System.Text.Json;

namespace RaquelMenopausa.Cms.Helpers
{
    public class CmsService
    {
        private readonly HttpClient _client;

        public CmsService(IHttpClientFactory factory)
        {
            _client = factory.CreateClient("CMS");
        }

        public async Task<List<ArticleStatusOptionDto>> GetArticleStatusOptionsAsync(string token)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/cms-dashboard/contents/get-article-status-options");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<List<string>>();

            return result.Select(x => new ArticleStatusOptionDto { Value = x }).ToList();
        }

        public async Task<TagResponseDto> GetTagsAsync(string token)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync("/api/cms-dashboard/contents/get-tags-options");
            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadFromJsonAsync<List<Dictionary<string, JsonElement>>>();

            var result = new TagResponseDto
            {
                ArticleCategories = raw.FirstOrDefault(x => x.ContainsKey("articleCategories"))?["articleCategories"]
                    .Deserialize<List<ArticleCategoryDto>>(),

                SymptomCategories = raw.FirstOrDefault(x => x.ContainsKey("symptomCategories"))?["symptomCategories"]
                    .Deserialize<List<SymptomCategoryDto>>(),

                Solutions = raw.FirstOrDefault(x => x.ContainsKey("solutions"))?["solutions"]
                    .Deserialize<List<SolutionDto>>()
            };

            return result;
        }


    }

}
