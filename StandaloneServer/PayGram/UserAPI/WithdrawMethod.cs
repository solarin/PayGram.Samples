using Newtonsoft.Json;
using System;
using System.Linq;

namespace PayGram.UserAPI
{
	public class WithdrawMethod
	{
		/// <summary>
		///  Optional. The Full name of the bank account holder. 
		/// </summary>
		public string BeneficiaryAccountFullname { get; set; }
		/// <summary>
		/// The currency that the user wants to receive
		/// </summary>
		public string CurrencyCode { get; set; }
		/// <summary>
		/// Optional. The cryptocurrency address where the funds should be transferred to 
		/// </summary>
		public string CryptoAddress { get; set; }
		/// <summary>
		/// Optional. If the transfer is made to a bank account which has the IBAN number, this field is mandatory.
		/// </summary>
		public string BankIban { get; set; }
		/// <summary>
		/// Optional. If the transfer is made to a bank account which has the Bic or the SWIFT code, this field is mandatory.
		/// </summary>
		public string BicSwift { get; set; }
		/// <summary>
		/// Optional. If the transfer is made to a bank account which doesn't have the IBAN number, this field is mandatory.
		/// </summary>
		public string BankAccount { get; set; }
		/// <summary>
		/// Optional. If the transfer is made to a bank account which has the Routing Code (USA banks), this field is mandatory.
		/// </summary>
		public string BankRoutingNumber { get; set; }
		/// <summary>
		/// Optional. If the transfer is made to a bank account which has the Sort Code (UK banks), this field is mandatory.
		/// </summary>
		public string BankSortCode { get; set; }
		/// <summary>
		/// Optional, but adviced when the transfer is made to a bank account.
		/// </summary>
		public GeoAddress BankAddress { get; set; }
		/// <summary>
		/// Optional, but adviced when the transfer is made to a bank account.
		/// </summary>
		public GeoAddress BeneficiaryAddress { get; set; }

		public bool IsValidCrypto
		{
			get
			{
				return string.IsNullOrWhiteSpace(CryptoAddress) == false
					&& Enum.GetNames(typeof(CryptoCurrencies)).Where(x => x.Equals(CurrencyCode, StringComparison.InvariantCultureIgnoreCase)).Any();
			}
		}

		public bool IsValidBankAccount
		{
			get
			{
				return (string.IsNullOrWhiteSpace(BankAccount) == false || string.IsNullOrWhiteSpace(BankIban) == false)
						&& string.IsNullOrWhiteSpace(BeneficiaryAccountFullname) == false
						&& Enum.GetNames(typeof(FiatCurrencies)).Where(x => x.Equals(CurrencyCode, StringComparison.InvariantCultureIgnoreCase)).Any()
						&& ((string.IsNullOrWhiteSpace(BankIban) == false && string.IsNullOrWhiteSpace(BicSwift) == false)
							|| (string.IsNullOrWhiteSpace(BankAccount) == false && string.IsNullOrWhiteSpace(BankSortCode) == false)
							|| (string.IsNullOrWhiteSpace(BankAccount) == false && string.IsNullOrWhiteSpace(BankRoutingNumber) == false)
							);
			}
		}

		public string ToJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented,
													new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate });
		}
	}
}