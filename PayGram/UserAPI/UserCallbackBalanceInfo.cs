namespace PayGram.UserAPI
{
	public class UserCallbackBalanceInfo : UserCallBackTransaction
	{
		/// <summary>
		/// The PG$ balance after the transaction
		/// </summary>
		public decimal Balance { get; set; }

		public decimal BalanceBeforeTransaction { get { return Balance - TransactionAmount; } }
	}
}
