using DocumentFormat.OpenXml.Spreadsheet;
using RaquelMenopausa.Cms.Controllers;
using RaquelMenopausa.Cms.Models;
using RaquelMenopausa.Cms.Models.Dto;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace RaquelMenopausa.Cms.Helpers
{
    public class CmsService
    {
        private readonly HttpClient _client;
        private readonly ILogger<CmsService> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly Context _db;


        public CmsService(IHttpClientFactory factory, ILogger<CmsService> logger, Context db, IWebHostEnvironment env)
        {
            _client = factory.CreateClient("CMS");
            _db = db;
            _env = env;
            _logger = logger;
        }

        

        public CmsService(ILogger<CmsService> logger, Context db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
            _logger = logger;
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

            status ??= "ALL";
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

        public async Task<ConteudoDto> DeleteAsync(string token, string articleId)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.DeleteAsync($"/api/cms-dashboard/contents/delete-article/{articleId}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ConteudoDto>();
            return result;
        }

        public async Task<ConteudoPagedResponse> GetArtigosAsync(int skip, int take, string search = null, string status = null, string tag = null, List<int> articleCategories = null, List<int> symptomCategories = null, List<int> solutions = null, string token = null)
        {
            var query = new List<string>();

            if (!string.IsNullOrEmpty(search))
                query.Add($"search={Uri.EscapeDataString(search)}");

            status ??= "ALL";
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


            content.Add(new StringContent(title ?? ""), "title");
            content.Add(new StringContent(intro ?? ""), "intro");
            content.Add(new StringContent(text ?? ""), "text");
            content.Add(new StringContent(references ?? ""), "references");
            content.Add(new StringContent(color ?? ""), "color");
            content.Add(new StringContent(status ?? ""), "status");
            content.Add(new StringContent(subject ?? ""), "subject");

            if (articleCategories?.Any() == true)
            {
                foreach (var id in articleCategories)
                    content.Add(new StringContent(id.ToString()), "articleCategories");
            }
            else
            {
                content.Add(new StringContent(string.Empty), "articleCategories");
            }

            if (symptomCategories?.Any() == true)
            {
                foreach (var id in symptomCategories)
                    content.Add(new StringContent(id.ToString()), "symptomCategories");
            }
            else
            {
                content.Add(new StringContent(string.Empty), "symptomCategories");
            }

            if (solutions?.Any() == true)
            {
                foreach (var id in solutions)
                    content.Add(new StringContent(id.ToString()), "solutions");
            }
            else
            {
                content.Add(new StringContent(string.Empty), "solutions");
            }


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

            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseString);


            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateArticleAsync(string articleId, string hash, string title, string intro, string text, string references, string color, string status, string subject,
        List<int> articleCategories,
        List<int> symptomCategories,
        List<int> solutions,
        IFormFile imageUpload,
        IFormFile audioVideoUpload,
        bool changedImage,
        bool changedAudioVideo,
        string token)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(articleId ?? ""), "article_id");
            content.Add(new StringContent(hash ?? ""), "hash");
            content.Add(new StringContent(title ?? ""), "title");
            content.Add(new StringContent(intro ?? ""), "intro");
            content.Add(new StringContent(text ?? ""), "text");
            content.Add(new StringContent(references ?? ""), "references");
            content.Add(new StringContent(color ?? ""), "color");
            content.Add(new StringContent(status ?? ""), "status");
            content.Add(new StringContent(subject ?? ""), "subject");

            content.Add(new StringContent(changedImage.ToString().ToLower()), "changedImage");
            content.Add(new StringContent(changedAudioVideo.ToString().ToLower()), "changedAudioVideo");

            if (articleCategories?.Any() == true)
                content.Add(new StringContent(string.Join(", ", articleCategories)), "articleCategories");

            if (symptomCategories?.Any() == true)
                content.Add(new StringContent(string.Join(", ", symptomCategories)), "symptomCategories");

            if (solutions?.Any() == true)
                content.Add(new StringContent(string.Join(", ", solutions)), "solutions");

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

            var request = new HttpRequestMessage(HttpMethod.Put, "/api/cms-dashboard/contents/update-article")
            {
                Content = content
            };

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task<ConteudoDto> PublishAsync(string token, string id)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsync($"/api/cms-dashboard/contents/publish-article/{id}", new StringContent(""));

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ConteudoDto>();
            return result;
        }

        public async Task<byte[]> GetArticlesCsvAsync(string token, string search = null, string status = null, string tag = null)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var query = new List<string>();

            if (!string.IsNullOrEmpty(search))
                query.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrEmpty(status))
                query.Add($"status={Uri.EscapeDataString(status)}");
            else
                query.Add("status=ALL");


            if (!string.IsNullOrEmpty(tag))
                query.Add($"articleCategories={Uri.EscapeDataString(tag)}");

            var queryString = string.Join("&", query);
            var endpoint = $"/api/cms-dashboard/contents/get-articles-csv?{queryString}";

            var response = await _client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<UsuariaPagedResponse> GetUsuariasAsync(int skip, int take, string search = null, string status = null, string token = null, DateTime? initialDate = null, DateTime? finalDate = null)
        {
            var query = new List<string>();

            if (initialDate.HasValue)
            {
                var ini = initialDate.Value.Date; 
                query.Add($"initialDate={ini:yyyy-MM-ddTHH:mm:ssZ}");
            }

            if (finalDate.HasValue)
            {
                DateTime fim;

                if (finalDate.Value.Date < DateTime.MaxValue.Date)
                {
                    fim = finalDate.Value.Date.AddDays(1).AddSeconds(-1);
                }
                else
                {
                    fim = DateTime.MaxValue;
                }

                query.Add($"finalDate={fim:yyyy-MM-ddTHH:mm:ssZ}");
            }


            query.Add($"search={(search ?? "").Trim()}");

            status ??= "ALL";
            query.Add($"status={status}");



            var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/cms-dashboards/users/get-users-paged/{skip}/{take}{qs}");

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var paged = new UsuariaPagedResponse();

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                var items = doc.RootElement[0].GetRawText();
                var total = doc.RootElement[1].GetInt32();

                paged.Items = JsonSerializer.Deserialize<List<UsuariaDto>>(items,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                paged.TotalCount = total;
            }

            return paged;
        }

        public async Task<IndicatorsUsersDto> GetIndicatorsUsers(string token, string search = null, string status = null, DateTime? initialDate = null, DateTime? finalDate = null)
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var query = new List<string>();

            if (!string.IsNullOrEmpty(search))
                query.Add($"search={Uri.EscapeDataString(search)}");

            status ??= "ALL";
            query.Add($"status={status}");

            if (initialDate.HasValue)
            {
                var ini = initialDate.Value.Date;
                query.Add($"initialDate={ini:yyyy-MM-ddTHH:mm:ssZ}");
            }

            if (finalDate.HasValue)
            {
                var fim = finalDate.Value.Date;
                query.Add($"finalDate={fim:yyyy-MM-ddTHH:mm:ssZ}");
            }



            var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";

            var response = await _client.GetAsync($"/api/cms-dashboards/users/get-users-indicators{qs}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IndicatorsUsersDto>();
            return result ?? new IndicatorsUsersDto();
        }

        public async Task<byte[]> GetUsersCsvAsync(string token, string search = null, string status = null, DateTime? initialDate = null, DateTime? finalDate = null)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var query = new List<string>();

            if (!string.IsNullOrEmpty(search))
                query.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrEmpty(status))
                query.Add($"status={Uri.EscapeDataString(status)}");
            else
                query.Add("status=ALL");


            if (initialDate.HasValue)
                query.Add($"initialDate={initialDate.Value:yyyy-MM-dd}");

            if (finalDate.HasValue)
                query.Add($"finalDate={finalDate.Value:yyyy-MM-dd}");

            var queryString = string.Join("&", query);
            var endpoint = $"/api/cms-dashboards/users/get-users-csv?{queryString}";

            var response = await _client.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<UsuariaDto> SuspendAccountAsync(string token, string id)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsync($"/api/cms-dashboards/users/suspend-account/{id}", new StringContent(""));

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<UsuariaDto>();
            return result;
        }

        public async Task<UsuariaDto> ActivateAccountAsync(string token, string id)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.PostAsync($"/api/cms-dashboards/users/activate-account/{id}", new StringContent(""));

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<UsuariaDto>();
            return result;
        }

        public async Task EnvioEmailAsync(string id, string subject, string content, string token)
        {
            var data = new Dictionary<string, string>
            {
                { "userId", id },
                { "subject", subject ?? "" },
                { "content", content ?? "" }
            };

            var conteudo = new FormUrlEncodedContent(data);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/cms-dashboards/users/send-email")
            {
                Content = conteudo
            };

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task ResetSenhaAsync(string id, string newPassword, string token)
        {
            var data = new Dictionary<string, string>
            {
                { "userId", id },
                { "newPassword", newPassword ?? "" },
            };

            var conteudo = new FormUrlEncodedContent(data);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/cms-dashboards/users/reset-password")
            {
                Content = conteudo
            };

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task AddObservationAsync(string id, string observations, string token)
        {
            var data = new Dictionary<string, string>
            {
                { "userId", id },
                { "observations", observations ?? "" },
            };

            var conteudo = new FormUrlEncodedContent(data);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/cms-dashboards/users/add-observations")
            {
                Content = conteudo
            };

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task<UsuariaDto> GetUsuariaAsync(string token, string id)
        {
            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync($"/api/cms-dashboards/users/get-user/{id}");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<UsuariaDto>();
            return result;
        }

        public async Task UpdateUsuariaAsync(string userId, string hash, string name, string aliasName, string email, bool emailVerified, bool isSuspended, bool isAdmin, IFormFile profileImageUpload, bool changedProfileImage, string token)
        {
            using var content = new MultipartFormDataContent();

            content.Add(new StringContent(userId ?? ""), "userId");
            content.Add(new StringContent(hash ?? ""), "hash");
            content.Add(new StringContent(name ?? ""), "name");
            content.Add(new StringContent(aliasName ?? ""), "aliasName");
            content.Add(new StringContent(email ?? ""), "email");
            content.Add(new StringContent(emailVerified.ToString().ToLower()), "emailVerified");
            content.Add(new StringContent(isSuspended.ToString().ToLower()), "isSuspended");
            content.Add(new StringContent(isAdmin.ToString().ToLower()), "isAdmin");
            content.Add(new StringContent(changedProfileImage.ToString().ToLower()), "changedProfileImage");

            if (profileImageUpload != null)
            {
                var imageContent = new StreamContent(profileImageUpload.OpenReadStream());
                imageContent.Headers.ContentType = new MediaTypeHeaderValue(profileImageUpload.ContentType);
                content.Add(imageContent, "profileImageUpload", profileImageUpload.FileName);
            }

            var request = new HttpRequestMessage(HttpMethod.Put, "/api/cms-dashboards/users/update-user")
            {
                Content = content
            };

            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payloadPreview = await request.Content.ReadAsStringAsync();
            Console.WriteLine(payloadPreview);

            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erro: {response.StatusCode} - {errorBody}");
            }

            response.EnsureSuccessStatusCode();
        }

        public async Task<string> GetValidTokenAsync()
        {
            var registro = _db.Configs
                .FirstOrDefault(c => c.Chave == "token" && c.Situacao);

            var token = await RefreshTokenAsync();

            if (registro != null)
            {
                registro.Valor = token;
                _db.SaveChanges();
            }

            return token;
        }

        private async Task<string> RefreshTokenAsync()
        {
            var loginRequest = new
            {
                email = "user3@email.com",
                password = "Senha*!123"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/signin", loginRequest);
            response.EnsureSuccessStatusCode();

            using var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var root = jsonDoc.RootElement;

            var accessToken = root.GetProperty("access_token").GetString();

            return accessToken;
        }
    }
}


