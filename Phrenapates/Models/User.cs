using System.Text.Json.Serialization;

namespace Phrenapates.Models
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    class UserAgreementResponse
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("data")]
        public List<string> Data { get; set; }
    }

    class UserLoginResponse : BaseResponse
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("birth")]
        public dynamic? Birth { get; set; }

        [JsonPropertyName("transcode")]
        public string Transcode { get; set; }

        [JsonPropertyName("current_timestamp_ms")]
        public long CurrentTimestampMs { get; set; }

        [JsonPropertyName("check7until")]
        public int Check7Until { get; set; }

        [JsonPropertyName("migrated")]
        public bool Migrated { get; set; }

        [JsonPropertyName("show_migrate_page")]
        public bool ShowMigratePage { get; set; }

        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; }

        [JsonPropertyName("kr_kmc_status")]
        public int KrKmcStatus { get; set; }
    }

    class UserCreateResponse : BaseResponse
    {
        [JsonPropertyName("uid")]
        public long Uid { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("isNew")]
        public int IsNew { get; set; }
    }

    class UserDestroyResponse : BaseResponse
    {
        [JsonPropertyName("current_timestamp_ms")]
        public long CurrentTimestampMs { get; set; }

        [JsonPropertyName("has_cool_days")]
        public bool HasCoolDays { get; set; }

        [JsonPropertyName("reborn_before_ms")]
        public long RebornBeforeMs { get; set; }

        [JsonPropertyName("user_destroy_wait_days")]
        public int UserDestroyWaitDays { get; set; }
    }
}
