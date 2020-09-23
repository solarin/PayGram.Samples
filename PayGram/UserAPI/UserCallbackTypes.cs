namespace PayGram.UserAPI
{
	public enum UserCallbackTypes
	{
		/// <summary>
		/// The type of the callback after a transaction happened
		/// </summary>
		BalanceInfo,
		/// <summary>
		/// The type of the callback after a withdraw transaction happened
		/// </summary>
		WithdrawInfo,
		/// <summary>
		/// The type of the callback after an invoice was either paid or redeemed
		/// </summary>
		InvoiceInfo,
		/// <summary>
		/// The type of the callback to represent a basic response
		/// </summary>
		CallbackInfo
	}
}
