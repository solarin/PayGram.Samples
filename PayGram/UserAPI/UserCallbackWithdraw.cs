using System;

namespace PayGram.UserAPI
{
	public class UserCallbackWithdraw : UserCallbackBalanceInfo
	{ 
		/// <summary>
		/// The fees paid for this transfer expressed in the currency of the source account from where the funds where withdrawn
		/// </summary>
		public decimal Fees { get; set; } 
		public Guid InvoiceCode { get; set; }
		/// <summary>
		/// The actual fees the user has incurred to withdraw. These fees were deducted from the money sent to him, 
		/// and therefore they are expressed in the currency of the transfer. These fees include the PayGram fees and and all the banking fees 
		/// on "our" side, but don't include additional fees applied by the receiving bank or intermediary bank.
		/// If the transfer happened in a currency different from the one the user had initially requested, this value is not signficant.
		/// </summary>
		public decimal TransferFees { get; set; } 
		public DateTime LastEventUtc { get; set; }
		public WithdrawAdminResponse AdminResponse { get; set; }
	}
}