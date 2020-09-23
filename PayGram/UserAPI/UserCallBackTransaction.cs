namespace PayGram.UserAPI
{
	public class UserCallBackTransaction
	{
		/// <summary>
		/// The effective amount of money, expressed in the currency of the account where the
		/// transaction took place, that were credited or debited
		/// </summary>
		public decimal TransactionAmount { get; set; }
	}
}
