using System;

namespace PayGram.UserAPI
{
	public class UserCallbackInvoiceInfo : UserCallbackBalanceInfo
	{
		public Guid InvoiceCode { get; set; }
	}
}
