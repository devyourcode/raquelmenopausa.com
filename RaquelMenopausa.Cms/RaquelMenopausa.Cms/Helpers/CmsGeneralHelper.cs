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
                        PropertyNameCaseInsensitive = true
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

        public static class TagTranslator
        {
            private static readonly Dictionary<string, string> _translations = new(StringComparer.OrdinalIgnoreCase)
        {
            { "myths", "Mitos" },
            { "taboos", "Tabus" },
            { "informative", "Informativo" },

            { "smellAndTasteChanges", "Alterações de olfato e paladar" },
            { "bodyWeightChanges", "Alterações no peso corporal" },
            { "sleepChanges", "Alterações no sono" },
            { "visualChanges", "Alterações visuais" },
            { "sexualityChanges", "Alterações na sexualidade" },
            { "cognitiveChanges", "Alterações cognitivas" },
            { "emotionalChanges", "Alterações emocionais" },
            { "respiratorySystemChanges", "Alterações no sistema respiratório" },
            { "gastrointestinalSystemChanges", "Alterações no sistema gastrointestinal" },
            { "cardiovascularSystemChanges", "Alterações no sistema cardiovascular" },
            { "oralChanges", "Alterações orais" },
            { "skinAndHairChanges", "Alterações na pele e cabelo" },
            { "muscularAndSkeletalChanges", "Alterações musculares e esqueléticas" },
            { "urinarySystemChanges", "Alterações no sistema urinário" },
            { "reproductiveSystemChanges", "Alterações no sistema reprodutivo" },
            { "hotFlashesAndNightSweats", "Ondas de calor e suores noturnos" },

            { "hormoneTherapy", "Terapia hormonal" },
            { "psychologicalTherapy", "Terapia psicológica" },
            { "sleep", "Sono" },
            { "nutrition", "Nutrição" },
            { "physicalExercise", "Exercício físico" }
        };

            public static string Translate(string label)
            {
                if (string.IsNullOrWhiteSpace(label))
                    return label;

                return _translations.TryGetValue(label, out var traducao)
                    ? traducao
                    : label;
            }
        }

    }
}
