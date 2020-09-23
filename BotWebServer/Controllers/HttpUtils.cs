using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BotWebServer.Controllers
{
	public static class HttpUtils
	{
		public static async Task<string> ReadStream(this HttpContext context)
		{
			var req = context.Request;

			string bodyStr;

			// Arguments: Stream, Encoding, detect encoding, buffer size 
			// AND, the most important: keep stream opened
			using (StreamReader reader
					  = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
			{
				bodyStr = await reader.ReadToEndAsync();
			}

			return bodyStr;
		}
	}
}
