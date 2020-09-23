using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PayGram.UserAPI;
using Telegram.Bot;

namespace BotWebServer.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class PayGramController : ControllerBase
	{
		private readonly ILogger<PayGramController> _logger;
		IConfiguration _configuration;
		TelegramBotClient _botClient;

		public PayGramController(ILogger<PayGramController> logger, IConfiguration configuration, TelegramBotClient botClient)
		{
			_logger = logger;
			_configuration = configuration;
			_botClient = botClient;
		}

		[Route("{secretToken}")]
		[HttpPost]
		public async Task<string> Get(string secretToken)
		{
			if (secretToken != Startup.PayGramSecret)
				return "ko";

			var req = await HttpContext.ReadStream();
			if (string.IsNullOrWhiteSpace(req)) return "ko";

			var payGramReq = JsonConvert.DeserializeObject<UserCallbackInfo>(req);

			if (payGramReq == null) return "ko";

			string message;
			switch (payGramReq.CallbackType)
			{
				case UserCallbackTypes.InvoiceInfo:
					message = $"Your invoice {payGramReq.CallbackData} of amount {-payGramReq.InvoiceInfo.TransactionAmount:0.####} PG$ was paid on {payGramReq.DateUtc} and your balance is {payGramReq.InvoiceInfo.Balance:0.####}.";
					break;

				default:
					message = $"Not sure what to do here :D {payGramReq.CallbackType}";
					break;
			}


			await _botClient.SendTextMessageAsync(Startup.MyTelegramId, message);

			return "ok";
		}
	}
}
