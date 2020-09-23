using PayGram.UserAPI;
using StandaloneServer.HttpServer;
using System;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace StandaloneServer
{
	public class Manager
	{
		int myTelegramId;

		TelegramBotClient botClient;
		HttpServerManager httpMngr;
		const string BotToken = nameof(BotToken);
		const string LocalHttpServerIp = nameof(LocalHttpServerIp);
		const string LocalHttpServerPort = nameof(LocalHttpServerPort);
		const string MyTelegramId = nameof(MyTelegramId);
		const string PayGramToken = nameof(PayGramToken);
		const string CertFile = nameof(CertFile);
		const string CertPassword = nameof(CertPassword);


		public Manager()
		{
			// set up the bot
			botClient = new TelegramBotClient(ConfigurationManager.AppSettings[BotToken]);
			botClient.OnUpdate += BotClient_OnUpdate;

			// set up the httpserver where we will receive notifications from PayGram
			string ip = ConfigurationManager.AppSettings[LocalHttpServerIp];
			int port = int.Parse(ConfigurationManager.AppSettings[LocalHttpServerPort]);
			string payGramToken = ConfigurationManager.AppSettings[PayGramToken];
			string cert = ConfigurationManager.AppSettings[CertFile];
			string certPassword = ConfigurationManager.AppSettings[CertPassword];
			httpMngr = new HttpServerManager(ip, port);
			httpMngr.PayGramToken = payGramToken;
			httpMngr.CertificateFile = cert;
			httpMngr.CertificatePassword = certPassword;
			httpMngr.OnPayGramEvent += HttpMngr_OnPayGramEvent;
			httpMngr.OnConfigured += HttpMngr_OnConfigured;
			myTelegramId = int.Parse(ConfigurationManager.AppSettings[MyTelegramId]);
			if (myTelegramId == 0)
				throw new Exception("Specify your telegram Id, get it from @opgmbot -> Settings -> Developer");
		}

		private void HttpMngr_OnConfigured(object sender, EventArgs e)
		{
			Console.WriteLine("========= \r\n\r\n" +
							"Go to https://t.me/opgmbot\r\n" +
							"Settings -> Developer\r\n" +
							"Set callback -> " + httpMngr.CallbackUrl + "\r\n" +
							"and then...\r\n" +
							"Link Bot -> the name of your bot that you used when you registered it on BotFather\r\n\r\n" +
							"Attention: PayGram server won't be able to call your localhost/127.0.0.1/192.168.x.x IP, you must specify a public IP address" +
							"\r\n\r\n========="
							);
		}

		private void HttpMngr_OnPayGramEvent(object sender, PayGramEventArgs e)
		{
			string message;
			switch (e.Info.CallbackType)
			{
				case UserCallbackTypes.InvoiceInfo:
					message = $"Your invoice {e.Info.CallbackData} of amount {-e.Info.InvoiceInfo.TransactionAmount:0.####} PG$ was paid on {e.Info.DateUtc} and your balance is {e.Info.InvoiceInfo.Balance:0.####}.";
					break;

				default:
					message = $"Not sure what to do here :D {e.Info.CallbackType}";
					break;
			}

			e.Success = true;

			Console.WriteLine(message);
			botClient.SendTextMessageAsync(myTelegramId, message);
		}

		private void BotClient_OnUpdate(object sender, UpdateEventArgs e)
		{
			BotClientUpdateAsync(e);
		}

		private async Task BotClientUpdateAsync(UpdateEventArgs e)
		{
			if (e.Update == null || e.Update.Type != UpdateType.Message) return;

			var msg = e.Update.Message.Text;

			string[] prms = msg.Split(new char[] { ' ' });

			if (prms.Length < 2 || prms[0] != "pay" || decimal.TryParse(prms[1], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount) == false)
			{
				await botClient.SendTextMessageAsync(e.Update.Message.Chat.Id, "PayGram test payments. Send a command such as:\r\npay 10");
				return;
			}

			// let's generate a link that will be used to open PayGram bot so that it will understand that 
			// the payment made by the user is for us
			string sAmount = amount.ToString(CultureInfo.InvariantCulture);
			string callbackData = "inv123"; // this will be given back by PayGram when the payment is complete
			string urlParams = $"a=p&t={myTelegramId}&amt={sAmount}&cd={callbackData}";

			// Telegram won't accept any special character, so we encode the url in base64 before passing it to telegram
			string urlParams64 = Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(urlParams));
			if (urlParams64.Length > 64)
				throw new Exception("Telegram does not support start parameters longer than 64 characters, reduce the size of your callback data");
			string myLink = "https://t.me/opgmbot?start=" + urlParams64;


			await botClient.SendTextMessageAsync(e.Update.Message.Chat.Id, $"Click here to pay: {myLink}",
													replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton()
													{
														Text = $"Pay {amount:0.####} USD",
														Url = myLink
													}));
		}

		public async Task Go()
		{
			botClient.StartReceiving();
			httpMngr.Start();



			string cmd = null;

			while (cmd != "q")
			{
				Console.Write("> ");
				cmd = Console.ReadLine();

				switch (cmd)
				{
					case "q":
						botClient.StopReceiving();
						await httpMngr.Stop();
						return;
				}
			}
		}
	}
}
