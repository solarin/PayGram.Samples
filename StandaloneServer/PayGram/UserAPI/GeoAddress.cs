using Newtonsoft.Json;

namespace PayGram.UserAPI
{
	public class GeoAddress
	{
		public string BuildingNumber { get; set; }
		public string Street1 { get; set; }
		public string Street2 { get; set; }
		public string City { get; set; }
		public string Province { get; set; }
		public string ZIP_Postal_Code { get; set; }
		public string Country { get; set; }

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented,
													new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate });
		}
	}
}