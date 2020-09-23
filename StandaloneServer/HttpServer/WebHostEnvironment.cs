using HttpUtils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using PayGram.UserAPI;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StandaloneServer.HttpServer
{
	internal class WebHostEnvironment
	{
		internal static HttpServerManager father;
		public const string DEFAULT_ERROR_MESSAGE = "Access not allowed, Goodbye &#129324;";

		public WebHostEnvironment()
		{
		}

		public void Configure(IApplicationBuilder app)
		{
			var serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
			try
			{
				// this middleware checks whether the server was put in standby
				//app.Use(async (context, next) => await staticFiles.Process(context, next));
				app.Run((context) => ProcessRequest(context));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error configuring webhost {ex}");
			}
		}

		private async Task ProcessRequest(HttpContext context)
		{
			try
			{
				// check wether the request contains the token that we specified and that only PayGram knows
				var url = context.Request.GetDisplayUrl();

				bool success = string.IsNullOrWhiteSpace(url) == false;

				success = success && url.IndexOf(father.PayGramToken, StringComparison.InvariantCulture) != -1;

				if (success)
				{
					var req = await context.Request.GetRequestStringAsync();
					success = string.IsNullOrWhiteSpace(req) == false;
					if (success)
					{
						var payGramReq = JsonConvert.DeserializeObject<UserCallbackInfo>(req);
						success = father.RaiseOnPayGramEvent(payGramReq);
					}
				}

				if (success)
					await context.Response.WriteAsync("ok");
				else
					await context.ResponseError("ko", "500", 500);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error processing request {context.Request.GetDisplayUrl()} {ex}");
				await context.ResponseError("Internal error", "500", 500);
			}
		}
	}
}