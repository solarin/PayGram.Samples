using PayGram.UserAPI;
using System;

namespace StandaloneServer.HttpServer
{
	public class PayGramEventArgs : EventArgs
	{
		/// <summary>
		/// Set this property to indicate the PayGram server whether the request was successful, or not.
		/// If set to false, the PayGram server will retry to send this notification again to the callback url
		/// that was set at the moment that the notification was created
		/// </summary>
		public bool Success { get; set; }
		/// <summary>
		/// The notification content passed by the PayGram server
		/// </summary>
		public UserCallbackInfo Info { get; set; }

		public PayGramEventArgs(UserCallbackInfo info)
		{
			Info = info;
			Success = true;
		}
	}
}
