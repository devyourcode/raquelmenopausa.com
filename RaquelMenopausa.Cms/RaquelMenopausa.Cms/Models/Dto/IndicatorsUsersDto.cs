using System.Text.Json.Serialization;

namespace RaquelMenopausa.Cms.Models.Dto
{
    public class IndicatorsUsersDto
    {
        [JsonPropertyName("usersCount")]
        public int UsersCount { get; set; }

        [JsonPropertyName("suspendedUsersCount")]
        public int SuspendedUsersCount { get; set; }

        [JsonPropertyName("activeUsersCount")]
        public int ActiveUsersCount { get; set; }
    }

}
