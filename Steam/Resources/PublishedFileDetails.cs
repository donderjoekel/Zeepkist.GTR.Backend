using Newtonsoft.Json;

namespace TNRD.Zeepkist.GTR.Backend.Steam.Resources;

public class PublishedFileDetails
{
    [JsonProperty("result")] public int Result { get; set; }
    [JsonProperty("publishedfileid")] public string PublishedFileId { get; set; }
    [JsonProperty("creator")] public string Creator { get; set; }
    [JsonProperty("creator_appid")] public uint CreatorAppId { get; set; }
    [JsonProperty("consumer_appid")] public uint ConsumerAppId { get; set; }
    [JsonProperty("consumer_shortcutid")] public uint ConsumerShortcutId { get; set; }
    [JsonProperty("filename")] public string Filename { get; set; }
    [JsonProperty("file_size")] public string FileSize { get; set; }
    [JsonProperty("preview_file_size")] public string PreviewFileSize { get; set; }
    [JsonProperty("preview_url")] public string PreviewUrl { get; set; }
    [JsonProperty("url")] public string Url { get; set; }
    [JsonProperty("hcontent_file")] public string HContentFile { get; set; }
    [JsonProperty("hcontent_preview")] public string HContentPreview { get; set; }
    [JsonProperty("title")] public string Title { get; set; }
    [JsonProperty("file_description")] public string FileDescription { get; set; }
    [JsonProperty("time_created")] public long TimeCreated { get; set; }
    [JsonProperty("time_updated")] public long TimeUpdated { get; set; }
    [JsonProperty("visibility")] public int Visibility { get; set; }
    [JsonProperty("flags")] public int Flags { get; set; }
    [JsonProperty("workshop_file")] public bool WorkshopFile { get; set; }
    [JsonProperty("workshop_accepted")] public bool WorkshopAccepted { get; set; }
    [JsonProperty("show_subscribe_all")] public bool ShowSubscribeAll { get; set; }
    [JsonProperty("num_comments_public")] public int NumCommentsPublic { get; set; }
    [JsonProperty("banned")] public bool Banned { get; set; }
    [JsonProperty("ban_reason")] public string BanReason { get; set; }
    [JsonProperty("banner")] public string Banner { get; set; }
    [JsonProperty("can_be_deleted")] public bool CanBeDeleted { get; set; }
    [JsonProperty("app_name")] public string AppName { get; set; }
    [JsonProperty("file_type")] public int FileType { get; set; }
    [JsonProperty("can_subscribe")] public bool CanSubscribe { get; set; }
    [JsonProperty("subscriptions")] public int Subscriptions { get; set; }
    [JsonProperty("favorited")] public int Favorited { get; set; }
    [JsonProperty("followers")] public int Followers { get; set; }

    [JsonProperty("lifetime_subscriptions")]
    public int LifetimeSubscriptions { get; set; }

    [JsonProperty("lifetime_favorited")] public int LifetimeFavorited { get; set; }
    [JsonProperty("lifetime_followers")] public int LifetimeFollowers { get; set; }
    [JsonProperty("lifetime_playtime")] public string LifetimePlaytime { get; set; }

    [JsonProperty("lifetime_playtime_sessions")]
    public string LifetyimePlaytimeSessions { get; set; }

    [JsonProperty("views")] public int Views { get; set; }
    [JsonProperty("num_children")] public int NumChildren { get; set; }
    [JsonProperty("num_reports")] public int NumReports { get; set; }
    [JsonProperty("language")] public int Language { get; set; }

    [JsonProperty("maybe_inappropriate_sex")]
    public bool MaybeInappropriateSex { get; set; }

    [JsonProperty("maybe_inappropriate_violence")]
    public bool MaybeInapproprtiateViolence { get; set; }

    [JsonProperty("revision_change_number")]
    public string RevisionChangeNumber { get; set; }

    [JsonProperty("revision")] public int Revision { get; set; }

    [JsonProperty("ban_text_check_result")]
    public int BanTextCheckResult { get; set; }
}
