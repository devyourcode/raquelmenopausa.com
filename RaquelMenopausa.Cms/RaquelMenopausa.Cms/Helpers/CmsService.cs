using RaquelMenopausa.Cms.Models.Dto;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Net.Http.Headers;
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

            var result = await response.Content.ReadFromJsonAsync<List<ArticleStatusOptionDto>>();

            return result ?? new List<ArticleStatusOptionDto>();
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

        public async Task<ConteudoDto> GetArticleAsync(string token, string articleId)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync($"/api/cms-dashboard/contents/get-article/{articleId}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ConteudoDto>();
            return result;
        }



        public async Task<List<ConteudoDto>> GetArtigosAsync(
    int skip,
    int take,
    string search = null,
    string status = null,
    List<int> articleCategories = null,
    List<int> symptomCategories = null,
    List<int> solutions = null,
    string token = null)
        {
            var query = new List<string>();

            if (!string.IsNullOrEmpty(search))
                query.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrEmpty(status))
                query.Add($"status={status}");

            if (articleCategories?.Any() == true)
                query.AddRange(articleCategories.Select(c => $"articleCategories={c}"));

            if (symptomCategories?.Any() == true)
                query.AddRange(symptomCategories.Select(c => $"symptomCategories={c}"));

            if (solutions?.Any() == true)
                query.AddRange(solutions.Select(s => $"solutions={s}"));

            var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"/api/cms-dashboard/contents/get-articles-paged/{skip}/{take}{qs}");

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(json);
            var result = JsonSerializer.Deserialize<List<List<ConteudoDto>>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.SelectMany(x => x).ToList() ?? new List<ConteudoDto>();
        }
    }



    }


