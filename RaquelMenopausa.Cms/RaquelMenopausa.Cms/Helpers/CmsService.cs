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

        public async Task<IndicatorsDto> GetIndicators(string token, string search = null, string status = null, string tag = null)
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var query = new List<string>();

            if (!string.IsNullOrEmpty(search))
                query.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrEmpty(status))
                query.Add($"status={status}");

            if (!string.IsNullOrEmpty(tag))
            {
                if (tag.StartsWith("a_"))
                    query.Add($"articleCategories={tag.Substring(2)}");
                else if (tag.StartsWith("s_"))
                    query.Add($"symptomCategories={tag.Substring(2)}");
                else if (tag.StartsWith("so_"))
                    query.Add($"solutions={tag.Substring(3)}");
            }

            var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";

            var response = await _client.GetAsync($"/api/cms-dashboard/contents/get-article-indicators{qs}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IndicatorsDto>();
            return result ?? new IndicatorsDto();
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


        public async Task<ConteudoPagedResponse> GetArtigosAsync(int skip, int take, string search = null, string status = null, string tag = null, List<int> articleCategories = null, List<int> symptomCategories = null, List<int> solutions = null, string token = null)
        {
            var query = new List<string>();

            if (!string.IsNullOrEmpty(search))
                query.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrEmpty(status))
                query.Add($"status={status}");

            if (!string.IsNullOrEmpty(tag))
            {
                if (tag.StartsWith("a_"))
                    query.Add($"articleCategories={tag.Substring(2)}");
                else if (tag.StartsWith("s_"))
                    query.Add($"symptomCategories={tag.Substring(2)}");
                else if (tag.StartsWith("so_"))
                    query.Add($"solutions={tag.Substring(3)}");
            }

            var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/cms-dashboard/contents/get-articles-paged/{skip}/{take}{qs}");

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var paged = new ConteudoPagedResponse();

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                var items = doc.RootElement[0].GetRawText();
                var total = doc.RootElement[1].GetInt32();

                paged.Items = JsonSerializer.Deserialize<List<ConteudoDto>>(items,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                paged.TotalCount = total;
            }

            return paged;
        }

        public async Task CreateArtigoAsync(string title, string intro, string text, string references, string color, string status, string subject, List<int> articleCategories, List<int> symptomCategories, List<int> solutions, IFormFile imageUpload, IFormFile audioVideoUpload, string token)
        {
            using var content = new MultipartFormDataContent();

            references = "teste";

            content.Add(new StringContent(title ?? ""), "title");
            content.Add(new StringContent(intro ?? ""), "intro");
            content.Add(new StringContent(text ?? ""), "text");
            content.Add(new StringContent(references ?? ""), "references");
            content.Add(new StringContent(color ?? ""), "color");
            content.Add(new StringContent(status ?? ""), "status");
            content.Add(new StringContent(subject ?? ""), "subject");

            if (articleCategories != null)
                foreach (var id in articleCategories)
                    content.Add(new StringContent(id.ToString()), "articleCategories");

            if (symptomCategories != null)
                foreach (var id in symptomCategories)
                    content.Add(new StringContent(id.ToString()), "symptomCategories");

            if (solutions != null)
                foreach (var id in solutions)
                    content.Add(new StringContent(id.ToString()), "solutions");

            if (imageUpload != null)
            {
                var imageContent = new StreamContent(imageUpload.OpenReadStream());
                imageContent.Headers.ContentType = new MediaTypeHeaderValue(imageUpload.ContentType);
                content.Add(imageContent, "imageUpload", imageUpload.FileName);
            }

            if (audioVideoUpload != null)
            {
                var mediaContent = new StreamContent(audioVideoUpload.OpenReadStream());
                mediaContent.Headers.ContentType = new MediaTypeHeaderValue(audioVideoUpload.ContentType);
                content.Add(mediaContent, "audioVideoUpload", audioVideoUpload.FileName);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/cms-dashboard/contents/create-article")
            {
                Content = content
            };

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}


