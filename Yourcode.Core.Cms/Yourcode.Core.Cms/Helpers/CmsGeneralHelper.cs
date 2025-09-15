using Serilog;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RaquelMenopausa.Cms.Helpers
{
    public static class CmsGeneralHelper
    {

        public static void SaveLogInfo(string message)
        {
            Log.Information(message);
        }

        public static void SaveLogError(string message)
        {
            Log.Error(message);
        }

        public static void SaveLogWarning(string message)
        {
            Log.Warning(message);
        }

        public static bool ValidateRecaptcha(string secret, string recaptchaUserResponse)
        {
            string url = $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={recaptchaUserResponse}";

            using (var httpClient = new HttpClient())
            {
                try
                {
                    string responseString = httpClient.GetStringAsync(url).Result;

                    var captchaResponse = JsonSerializer.Deserialize<CaptchaResponse>(responseString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // ignora maiúsculas/minúsculas
                    });

                    return captchaResponse != null && captchaResponse.success;
                }
                catch
                {
                    return false;
                }
            }
        }

        public class CaptchaResponse
        {
            public bool success { get; set; }
        }

        public static string RemoveCaracteresEspeciais(string text, bool removerAcentos = true)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            if (removerAcentos)
            {
                text = text.Normalize(NormalizationForm.FormD);
                var sb = new StringBuilder();
                foreach (var c in text)
                {
                    if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                        sb.Append(c);
                }
                text = sb.ToString();
            }

            text = Regex.Replace(text, @"[^a-zA-Z0-9\s]", "");

            return text;
        }

        public static string ToFriendlyUrl(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = text.ToLowerInvariant().Trim();

            text = Regex.Replace(text, @"\s+", " ");

            text = text.Replace(" ", "-");

            text = Regex.Replace(text, @"-+", "-");

            return text;
        }


    }
}
