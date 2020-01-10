using Newtonsoft.Json;

namespace CopebotNET.Utilities.JsonTemplates
{
	public class ImageUploadResponse {
		[JsonProperty("data")]
		public ImageDataObject ImageData { get; private set; }

		[JsonProperty("success")]
		public bool WasSuccessful { get; private set; }

		[JsonProperty("status")]
		public int StatusCode { get; private set; }
	}

	public struct ImageDataObject {
		[JsonProperty("id")]
		public string ImageId { get; private set; }

		[JsonProperty("url_viewer")]
		public string ViewerUrl { get; private set; }

		[JsonProperty("url")]
		public string Url { get; private set; }

		[JsonProperty("display_url")]
		public string DisplayUrl { get; private set; }

		[JsonProperty("title")]
		public string Title { get; private set; }

		[JsonProperty("time")]
		public string UploadTime { get; private set; }

		[JsonProperty("image")]
		public ImageObject Image { get; private set; }

		[JsonProperty("thumb")]
		public ImageObject ThumbnailImage { get; private set; }

		[JsonProperty("medium")]
		public ImageObject MediumImage { get; private set; }

		[JsonProperty("delete_url")]
		public string DeleteUrl { get; private set; }
	}

	public struct ImageObject {
		[JsonProperty("filename")]
		public string Filename { get; private set; }

		[JsonProperty("name")]
		public string Name { get; private set; }

		[JsonProperty("mime")]
		public string MimeType { get; private set; }

		[JsonProperty("extension")]
		public string Extension { get; private set; }

		[JsonProperty("url")]
		public string Url { get; private set; }

		[JsonProperty("size")]
		public int Size { get; private set; }
	}
}