using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net;

namespace LegendaryMapperV2.Model
{
    public class AssetInfo
    {
        [JsonProperty("app_name")]
        public string AppName { get; set; }

        [JsonProperty("asset_id")]
        public string AssetId { get; set; }

        [JsonProperty("build_version")]
        public string BuildVersion { get; set; }

        [JsonProperty("catalog_item_id")]
        public string CatalogItemId { get; set; }

        [JsonProperty("label_name")]
        public string LabelName { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, string> Metadata { get; set; }

        [JsonProperty("namespace")]
        public string Namespace { get; set; }
    }
    public class MetaAttribute
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class MetaImage
    {
        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("uploadedDate")]
        public DateTimeOffset UploadedDate { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonIgnore]
        public string UrlExt
        {
            get
            {
                string a = Url.AbsolutePath.Split('.').Last();
                if (a.Length > 5)
                    return "jpg";

                return a;
            }
        }

        public void SaveImageAs(string path)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(Url, $"{path}.{UrlExt}");
            }
        }
    }

    public class MetaReleaseDetails
    {
        [JsonProperty("appId")]
        public string AppId { get; set; }

        [JsonProperty("compatibleApps")]
        public List<string> CompatibleApps { get; set; }

        [JsonProperty("dateAdded")]
        public DateTimeOffset DateAdded { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("platform")]
        public List<string> Platform { get; set; }

        [JsonProperty("releaseNote")]
        public string ReleaseNote { get; set; }

        [JsonProperty("versionTitle")]
        public string VersionTitle { get; set; }
    }

    public class GameMetadataExt
    {
        [JsonProperty("categories")]
        public List<Dictionary<string, string>> Categories { get; set; }
        [JsonProperty("creationDate")]
        public DateTimeOffset CreationDate { get; set; }
        [JsonProperty("customAttributes")]
        public Dictionary<string, MetaAttribute> CustomAttributes { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("developer")]
        public string Developer { get; set; }

        [JsonProperty("developerId")]
        public string DeveloperId { get; set; }

        [JsonProperty("endOfSupport")]
        public bool EndOfSupport { get; set; }

        [JsonProperty("entitlementName")]
        public string EntitlementName { get; set; }

        [JsonProperty("entitlementType")]
        public string EntitlementType { get; set; }

        [JsonProperty("eulaIds")]
        public List<string> EulaIds { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("itemType")]
        public string ItemType { get; set; }

        [JsonProperty("keyImages")]
        public List<MetaImage> KeyImages { get; set; }

        [JsonProperty("lastModifiedDate")]
        public DateTimeOffset LastModifiedDate { get; set; }

        [JsonProperty("namespace")]
        public string Namespace { get; set; }

        [JsonProperty("releaseInfo")]
        public List<MetaReleaseDetails> ReleaseDetails { get; set; }

        [JsonProperty("selfRefundable")]
        public bool SelfRefundable { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("unsearchable")]
        public bool Unsearchable { get; set; }
    }
    public class GameMetadata
    {
        [JsonProperty("app_name")]
        public string AppName { get; set; }
        [JsonProperty("app_title")]
        public string AppTitle { get; set; }
        [JsonProperty("asset_infos")]
        public Dictionary<string, AssetInfo> AssetInfos { get; set; }
        [JsonProperty("base_urls")]
        public List<string> BaseUrls { get; set; }

        [JsonProperty("metadata")]
        public GameMetadataExt Metadata { get; set; }
    }
}