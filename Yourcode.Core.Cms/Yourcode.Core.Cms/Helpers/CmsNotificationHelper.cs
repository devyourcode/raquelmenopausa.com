using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaquelMenopausa.Cms.Helpers
{
    public static class CmsNotificationHelper
    {
        private const string AuthUrl = "https://api.yourcode.com.br/api/auth/login";
        private const string EmailUrl = "https://api.yourcode.com.br/api/v1/notifications/email/add";

        public static async Task<bool> SendEmail(int clienteId, int projetoId, string nome, string remetente, string destinatario, string assunto, string mensagem)
        {
            string token = await ObterTokenJwtAsync("contato@yourcode.com.br", "LfyzaUem@RlbOvgN5");

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Falha ao obter o token");
                return false;
            }

            bool enviado = await CallApiEmailAsync(
                clienteId: clienteId,
                projetoId: projetoId,
                nome: nome,
                remetente: remetente,
                destinatario: destinatario,
                assunto: assunto,
                mensagem: mensagem,
                tokenJwt: token
            );

            return enviado;
        }

        /// <summary>
        /// Autentica na API e retorna o token JWT
        /// </summary>
        public static async Task<string> ObterTokenJwtAsync(string usuario, string senha)
        {
            using var client = new HttpClient();

            var payload = new
            {
                username = usuario,
                password = senha
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(AuthUrl, content);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            return result != null && result.ContainsKey("token") ? result["token"].ToString() : null;
        }

        /// <summary>
        /// Envia um e-mail pela API utilizando autenticação JWT
        /// </summary>
        public static async Task<bool> CallApiEmailAsync(int clienteId, int projetoId, string nome, string remetente, string destinatario, string assunto, string mensagem, string tokenJwt)
        {
            try
            {
                using var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenJwt);

                var payload = new
                {
                    clienteId,
                    projetoId,
                    nome,
                    remetente,
                    destinatario,
                    assunto,
                    mensagem
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(EmailUrl, content);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

    }
}
