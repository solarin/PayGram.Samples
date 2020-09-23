using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using PayGram.UserAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StandaloneServer.HttpServer
{
	public class HttpServerManager
	{
		public event EventHandler OnConfigured;

		/// <summary>
		/// Triggers when there is an incoming notification from PayGram
		/// </summary>
		public event EventHandler<PayGramEventArgs> OnPayGramEvent;
		/// <summary>
		/// The local port where the http server will listen
		/// </summary>
		public int BindPort { get; private set; }
		/// <summary>
		/// The local address where the http server will listen
		/// </summary>
		public string BindIp { get; private set; }
		/// <summary>
		/// This is a security token that the http server will check to make sure that the requests come from PayGram
		/// Don't share it with anybody, other than the PayGram bot at https://t.me/opgmbot
		/// </summary>
		public string PayGramToken { get; set; }
		/// <summary>
		/// This is the callback url to set at https://t.me/opgmbot Settings - Developer - Set callback url
		/// Don't share this link with anybody else
		/// </summary>
		public string CallbackUrl => $"http{(SupportsHttps ? "s" : "")}://{BindIp}:{(SupportsHttps ? BindPort + 1 : BindPort)}/?token={PayGramToken}";

		public bool SupportsHttps { get; private set; }

		public bool IsStarted { get; private set; }
		public string CertificateFile { get; internal set; }
		public string CertificatePassword { get; internal set; }

		CancellationTokenSource tokenSource;
		bool _requestedShutdown;
		IWebHost host;

		public HttpServerManager(string bindIp, int bindPort)
		{
			BindIp = bindIp;
			BindPort = bindPort;
			WebHostEnvironment.father = this;
		}

		object sync = new object();

		public void Start()
		{
			lock (sync)
			{
				if (IsStarted) return;
				IsStarted = true;
			}
			tokenSource = new CancellationTokenSource();
			_requestedShutdown = false;
			Task.Run(() => Job(), tokenSource.Token);
		}


		async void Job()
		{
			tokenSource = new CancellationTokenSource();

			try
			{
				host = new WebHostBuilder()
								.UseSockets()
								.UseKestrel(SetOptions)
								.UseStartup<WebHostEnvironment>()
								.Build();

				Console.WriteLine("Host configured");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Can't start webhost {ex}");
				return;
			}


			try
			{
				host.Run();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error on WebHost Thread {ex}");
				if (_requestedShutdown == false)
				{
					await Task.Delay(20000, tokenSource.Token);
					Job();
				}
			}
		}

		internal bool RaiseOnPayGramEvent(UserCallbackInfo info)
		{
			var e = new PayGramEventArgs(info);
			OnPayGramEvent?.Invoke(this, e);
			return e.Success;
		}

		void SetOptions(KestrelServerOptions op)
		{
			if (IPAddress.TryParse(BindIp, out IPAddress ipAddress) == false)
				ipAddress = IPAddress.Loopback;

			op.Listen(ipAddress, BindPort, listenOptions => listenOptions.UseConnectionLogging());

			// if we have a certificate, we also listen for https connections
			FileInfo fiCertFile = null;
			if (File.Exists(CertificateFile))
				fiCertFile = new FileInfo(CertificateFile);

			if (fiCertFile != null)
			{
				op.Listen(ipAddress, BindPort + 1, listenOptions =>
				{
					listenOptions.UseConnectionLogging();
					listenOptions.UseHttps(fiCertFile.FullName, CertificatePassword);
				});
				SupportsHttps = true;
			}
			OnConfigured?.Invoke(this, EventArgs.Empty);
		}

		public async Task Stop()
		{
			lock (sync)
			{
				if (IsStarted == false || _requestedShutdown) return;
				_requestedShutdown = true;
			}
			tokenSource.Cancel();
			if (host != null)
				await host.StopAsync();
			IsStarted = false;
		}
	}
}
