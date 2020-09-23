using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace BotWebServer
{
	public class Startup
	{
		public const string TELEGRAMID_KEY = "MyTelegramId";
		public const string BOT_TOKEN_KEY = "BotToken";
		public const string TELEGRAMCALLBACK_KEY = "TelegramCallback";
		public const string TELEGRAMSECRET_KEY = "TelegramSecret";
		public const string PAYGRAMSECRET_KEY = "PayGramSecret";

		TelegramBotClient telegramBot = null;

		public static string BotToken { get; private set; }
		public static string TelegramCallback { get; private set; }
		public static string TelegramSecret { get; private set; }
		public static string PayGramSecret { get; private set; }
		public static int MyTelegramId { get; private set; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			BotToken = configuration.GetValue<string>(BOT_TOKEN_KEY);
			telegramBot = new TelegramBotClient(BotToken);

			TelegramCallback = Configuration.GetValue<string>(TELEGRAMCALLBACK_KEY);
			TelegramSecret = Configuration.GetValue<string>(TELEGRAMSECRET_KEY);
			PayGramSecret = Configuration.GetValue<string>(PAYGRAMSECRET_KEY);
			MyTelegramId = Configuration.GetValue<int>(TELEGRAMID_KEY);
		}

		public IConfiguration Configuration { get; }


		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddSingleton<IConfiguration>(Configuration);
			services.Add(new ServiceDescriptor(typeof(TelegramBotClient), telegramBot));

			UriBuilder urib = new UriBuilder(TelegramCallback + TelegramSecret);
			if (urib.Uri.IsLoopback == false) //telegram does not support loopback callback, as well as paygram, because it is impossible
				telegramBot.SetWebhookAsync(urib.ToString()).Wait();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
