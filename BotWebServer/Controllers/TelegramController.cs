using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotWebServer.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TelegramController : ControllerBase
	{
		private readonly ILogger<TelegramController> _logger;
		IConfiguration _configuration;
		TelegramBotClient _botClient;

		public TelegramController(ILogger<TelegramController> logger, IConfiguration configuration, TelegramBotClient botClient)
		{
			_logger = logger;
			_configuration = configuration;
			_botClient = botClient;
		}

		[Route("{secretToken}")]
		[HttpPost]
		public async Task<string> Get(string secretToken)
		{
			if (secretToken != Startup.TelegramSecret)
				return "ko";


			var req = await HttpContext.ReadStream();
			if (string.IsNullOrWhiteSpace(req)) return "ko";

			try
			{
				Update upd = JsonConvert.DeserializeObject<Update>(req);


				if (upd == null || upd.Type != UpdateType.Message) return "ko";

				var msg = upd.Message.Text;

				string[] prms = msg.Split(new char[] { ' ' });

				if (prms.Length < 2 || prms[0] != "pay" || decimal.TryParse(prms[1], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) == false)
				{
					await _botClient.SendTextMessageAsync(upd.Message.Chat.Id, "PayGram test payments. Send a command such as:\r\npay 10");
					return "ko";
				}

				// let's generate a link that will be used to open PayGram bot so that it will understand that 
				// the payment made by the user is for us
				string sAmount = amount.ToString(CultureInfo.InvariantCulture);
				string callbackData = "inv123"; // this will be given back by PayGram when the payment is complete
				string urlParams = $"a=p&t={Startup.MyTelegramId}&amt={sAmount}&cd={callbackData}";

				// Telegram won't accept any special character, so we encode the url in base64 before passing it to telegram
				string urlParams64 = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(urlParams));
				if (urlParams64.Length > 64)
					throw new Exception("Telegram does not support start parameters longer than 64 characters, reduce the size of your callback data");
				string myLink = "https://t.me/opgmbot?start=" + urlParams64;


				await _botClient.SendTextMessageAsync(upd.Message.Chat.Id, $"Click here to pay: {myLink}",
														replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton()
														{
															Text = $"Pay {amount:0.####} USD",
															Url = myLink
														}));
			}
			catch (Exception e)
			{
				return "ko";
			}
			return "ok";
		}



	}
}
