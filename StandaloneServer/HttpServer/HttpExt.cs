using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpUtils
{
	public static class StreamUtils
	{
		public static async Task<string> ReadRequestAsync(this Stream stream)
		{
			if (stream == null) return null;
			byte[] buff = new byte[1024];
			int read = 0;
			StringBuilder sb = new StringBuilder();
			while ((read = await stream.ReadAsync(buff, 0, buff.Length)) > 0)
			{
				sb.Append(ASCIIEncoding.UTF8.GetString(buff, 0, read));

			}
			return sb.ToString();
		}
	}

	public static class HttpContextUtils
	{
		public static async Task<string> GetRequestStringAsync(this HttpRequest request)
		{
			if (request == null) return null;
			return await request.Body?.ReadRequestAsync();
		}

		/// <summary>
		/// Sends a file only if its type is .jpeg, .jpg, .png
		/// </summary>
		/// <param name="context"></param>
		/// <param name="file"></param>
		/// <param name="fileError"></param>
		/// <returns></returns>
		public static async Task SendFile(this HttpContext context, FileInfo file, string fileError)
		{
			string contentType;
			if (file.Name.EndsWith(".jpeg") || file.Name.EndsWith(".jpg"))
				contentType = "image/jpeg";
			else if (file.Name.EndsWith(".png"))
				contentType = "image/png";
			else
			{
				await context.ResponseError("Access not allowed, Goodbye &#129324;", "Error", 403, fileError);
				return;
			}

			var content = System.IO.File.ReadAllBytes(file.FullName);
			context.Response.ContentType = contentType;
			await context.Response.Body.WriteAsync(content, 0, content.Length);

		}
		public static string GetRequestedFileName(this HttpContext context)
		{
			var path = context.Request.Path;
			if (path.HasValue == false) return null;
			return path;
		}

		/// <summary>
		/// Sends the specified file formatting its content with the arguments passed
		/// </summary>
		/// <param name="context"></param>
		/// <param name="file"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		public static async Task SendFile(this HttpContext context, FileInfo file, params object[] args)
		{
			var content = System.IO.File.ReadAllText(file.FullName);
			content = string.Format(content, args);
			await context.Response.WriteAsync(content);
		}

		public static async Task ResponseError(this HttpContext context, string msg, string error, int status = 500, string file = null)
		{
			context.Response.StatusCode = status;
			string content = "Error processing the request. {0}, {1}, {2}";
			if (File.Exists(file))
			{
				try
				{
					content = File.ReadAllText(file);
				}
				catch { }
			}
			content = string.Format(content, msg, error, status);
			await context.Response.WriteAsync(content);
		}
	}
}