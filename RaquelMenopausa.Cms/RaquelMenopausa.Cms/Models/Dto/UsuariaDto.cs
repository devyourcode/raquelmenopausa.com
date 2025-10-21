using System.Text.Json.Serialization;

namespace RaquelMenopausa.Cms.Models.Dto
{
    public class UsuariaDto
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("authentication")]
        public bool Authentication { get; set; }

        [JsonPropertyName("emailVerificationCode")]
        public string EmailVerificationCode { get; set; }

        [JsonPropertyName("emailVerified")]
        public bool EmailVerified { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("google_id")]
        public string GoogleId { get; set; }

        [JsonPropertyName("facebook_id")]
        public string FacebookId { get; set; }

        [JsonPropertyName("apple_id")]
        public string AppleId { get; set; }

        [JsonPropertyName("photo_url")]
        public string PhotoUrl { get; set; }

        [JsonPropertyName("date_created")]
        public DateTime DateCreated { get; set; }

        [JsonPropertyName("date_updated")]
        public DateTime? DateUpdated { get; set; }

        [JsonPropertyName("date_deleted")]
        public DateTime? DateDeleted { get; set; }

        [JsonPropertyName("admin")]
        public bool Admin { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonIgnore]
        public string Status => Active ? "ACTIVE" : "SUSPENDED";

        [JsonPropertyName("banned")]
        public bool? Banned { get; set; }

        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; }

        [JsonPropertyName("sponsor_id")]
        public string SponsorId { get; set; }

        [JsonPropertyName("term_of_use_version")]
        public string TermOfUseVersion { get; set; }

        [JsonPropertyName("observations")]
        public string Observations { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("profile")]
        public ProfileDto Profile { get; set; }
    }

    public class ProfileDto
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; }

        [JsonPropertyName("city_id")]
        public string CityId { get; set; }

        [JsonPropertyName("show_astrology_sign")]
        public bool ShowAstrologySign { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("profile_image")]
        public string ProfileImage { get; set; }

        [JsonPropertyName("menopause_year")]
        public int? MenopauseYear { get; set; }

        [JsonPropertyName("marital_status")]
        public string MaritalStatus { get; set; }

        [JsonPropertyName("date_created")]
        public DateTime DateCreated { get; set; }

        [JsonPropertyName("date_deleted")]
        public DateTime? DateDeleted { get; set; }

        [JsonPropertyName("profile_description")]
        public string ProfileDescription { get; set; }

        [JsonPropertyName("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [JsonPropertyName("first_period")]
        public int? FirstPeriod { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("recommendation_needed")]
        public DateTime? RecommendationNeeded { get; set; }

        [JsonPropertyName("friend_counter")]
        public int FriendCounter { get; set; }
    }

    public class UsuariaPagedResponse
    {
        public List<UsuariaDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }

}
