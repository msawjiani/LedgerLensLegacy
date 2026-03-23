namespace LedgerLensMaking.Models.UIModels
{
    public class TransferContext
    {
        public int BankAccountId { get; set; }
        public string BankAccountName { get; set; }

        public string DateInput { get; set; }
        public string RefInput { get; set; }
        public string NarrationInput { get; set; }
        public string AmountInput { get; set; }

        public int? GLAccountId { get; set; }
        public string GLAccountName { get; set; }
    }
}
