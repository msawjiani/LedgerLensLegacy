using LedgerLensMaking.Models.Data;
using LedgerLensMaking.Models.UIModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using LedgerLensMaking.UtilityClasses;
using LedgerLensMaking.Windows;

namespace LedgerLensMaking.Models.ViewModels
{
    public class SalesShareJournalViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand SfCommand { get; }
        public ICommand SavePaymentCommand { get; }



        private string _individualName;
        public string IndividualName
        {
            get => _individualName;
            set
            {
                _individualName = value;
                OnPropertyChanged();
            }
        }
        private decimal _currentBalance;
        public decimal CurrentBalance
        {
            get => _currentBalance;
            set
            {
                _currentBalance = value;
                OnPropertyChanged();
            }
        }
        private List<Chart> _creditAccounts;
        public List<Chart> CreditAccounts
        {
            get => _creditAccounts;
            set
            {
                _creditAccounts = value;
                OnPropertyChanged();
            }
        }
        private Chart _selectedCreditAccount;
        public Chart SelectedCreditAccount
        {
            get => _selectedCreditAccount;
            set
            {
                _selectedCreditAccount = value;
                LoadSharesGrid();
                OnPropertyChanged();
                
            }
        }


        private List<Chart> _debitAccounts;
        public List<Chart> DebitAccounts
        {
            get => _debitAccounts;
            set
            {
                _debitAccounts = value;
                OnPropertyChanged();
            }
        }
        private Chart _selectedDebitAccount;
        public Chart SelectedDebitAccount
        {
            get => _selectedDebitAccount;
            set
            {
                _selectedDebitAccount = value;
                OnPropertyChanged();
                
            }
        }
        private List<QryShareBalanceNett> _qryShareBalances;
        public List<QryShareBalanceNett> QryShareBalancesNetts
        {
            get => _qryShareBalances;
            set
            {
                _qryShareBalances = value;
                OnPropertyChanged();
            }
        }
        private QryShareBalanceNett _selectedShare;
        public QryShareBalanceNett SelectedShare
        {
            get => _selectedShare;
            set
            {
                _selectedShare = value;
                OnPropertyChanged();
            }
        }
        private string _dateInput;
        public string DateInput
        {
            get => _dateInput;
            set
            {
                _dateInput = value;
                SalesDateInput = _dateInput;
                OnPropertyChanged();
            }
        }
        private string _refInput;
        public string RefInput
        {
            get => _refInput;
            set
            {
                _refInput = value;
                OnPropertyChanged();
            }
        }
        private string _narrationInput;
        public string NarrationInput
        {
            get => _narrationInput;
            set
            {
                _narrationInput = value;
                OnPropertyChanged();
            }
        }
        private string _salesdateInput;
        public string SalesDateInput
        {
            get => _salesdateInput;
            set
            {
                _salesdateInput = value;
                OnPropertyChanged();
            }
        }
        private string _salesqtyInput;
        public string SalesQtyInput
        {
            get => _salesqtyInput;
            set
            {
                _salesqtyInput = value;
                OnPropertyChanged();
            }
        }
        private string _salesrateInput;
        public string SalesRateInput
        {
            get => _salesrateInput;
            set
            {
                _salesrateInput = value;
                OnPropertyChanged();
            }
        }

        private BankEntryGridModel _selectedBankEntryGridModel;
        public BankEntryGridModel SelectedBankEntryGridModel
        {
            get => _selectedBankEntryGridModel;
            set
            {
                _selectedBankEntryGridModel = value;
                OnPropertyChanged();
            }
        }
        private List<Chart> _glAccounts;
        public List<Chart> GLAccounts
        {
            get => _glAccounts;
            set
            {
                _glAccounts = value;
                OnPropertyChanged();
            }
        }

        public SalesShareJournalViewModel()
        {
            IndividualName = GlobalVariables.IndividualName + "'s Sales Share Journal";
            LoadCreditAccounts();
            SavePaymentCommand = new RelayCommand(OnSaveReceipt);

        }



        private void OnSaveReceipt()
        {

            if (!CheckFormErrors())
            {
                if (SelectedShare != null)
                {

                    MakeEntry();
                    ClearInputs();

                }
                else
                {
                    MessageBox.Show("You Must select a Share");
                }
            }

        }
        private void ClearInputs()
        {

            DateInput = null;
            RefInput = null;
            NarrationInput = "Being";
            SalesQtyInput = null;
            SalesDateInput = null;
            SalesRateInput = null;
        }
        private bool CheckFormErrors()
        {

            bool hasErrors = false;

            if (SelectedDebitAccount == null)
            {
                hasErrors = true;
                MessageBox.Show("Please select a Bank Account.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (string.IsNullOrWhiteSpace(DateInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a valid Date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (!DateTime.TryParse(DateInput, out DateTime parsedDate))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a valid date format for Date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (parsedDate < GlobalVariables.SelectedStartDate || parsedDate > GlobalVariables.SelectedEndDate)
            {
                hasErrors = true;
                MessageBox.Show($"The date must be between {GlobalVariables.SelectedStartDate:dd-MMM-yyyy} and {GlobalVariables.SelectedEndDate:dd-MMM-yyyy}.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (parsedDate < GlobalVariables.SelectedStartDate || parsedDate > GlobalVariables.SelectedEndDate)
            {
                hasErrors = true;
                MessageBox.Show($"The date must be between {GlobalVariables.SelectedStartDate:dd-MMM-yyyy} and {GlobalVariables.SelectedEndDate:dd-MMM-yyyy}.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (string.IsNullOrWhiteSpace(RefInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a Reference.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }



            if (string.IsNullOrWhiteSpace(SalesDateInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a valid Purchase Date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (!DateTime.TryParse(SalesDateInput, out DateTime parsedDate))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a valid date format for Purchase Date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (string.IsNullOrWhiteSpace(NarrationInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a Narration.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (string.IsNullOrEmpty(SalesQtyInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a Purchase Qty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (string.IsNullOrEmpty(SalesRateInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a Purchase Rate.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }


            return hasErrors;
        }
        private void MakeEntry()
        {
            BankEntryHelper bankEntryHelper = new BankEntryHelper();
            bankEntryHelper.BankCode = SelectedDebitAccount.AccountId;
            bankEntryHelper.BankDescription = SelectedDebitAccount.Account;

            bankEntryHelper.TranactionChartCode = SelectedCreditAccount.AccountId;
            bankEntryHelper.YearId = GlobalVariables.SelectedYearId;
            bankEntryHelper.TransactionCodeDescription = SelectedDebitAccount.Account;
            bankEntryHelper.TypeOfTransaction1Recepit2Payment = 1; // CAREFUL CHANGE THIS ACCORDINGLY
            bankEntryHelper.Reference = RefInput;
            bankEntryHelper.Narration = NarrationInput;
            bankEntryHelper.Amount = decimal.Parse(SalesRateInput) * decimal.Parse(SalesQtyInput); // GOD KNOWS HOW I MISSED THE RASCAL SALESQTY INPUT to decimal
            bankEntryHelper.TransactionType = "JE"; // OR CE
            bankEntryHelper.TDate = DateTime.Parse(DateInput);


            bankEntryHelper.SubledgerTranactionsNo0Fd1Shares2 = 2; // SHARES 
            bankEntryHelper.SharesAccountCode = SelectedShare.ShareId;
            bankEntryHelper.SubledgerCodeDescription = SelectedShare.Company;
            bankEntryHelper.QtyOfShares = decimal.Parse(SalesQtyInput);// Changing from int to Decimal 8-Jan-2025
            bankEntryHelper.ShareSellingPrice = decimal.Parse(SalesRateInput);


            bankEntryHelper.LTCapgainAccountCode = GlobalVariables.LTCGAccountCode;
            bankEntryHelper.STCapgainAccountCode = GlobalVariables.STCGAccountCode;
            bankEntryHelper.LTCaplossAccountCode = GlobalVariables.LTCLAccountCode;
            bankEntryHelper.STCaplossAccountCode = GlobalVariables.STCLAccountCode;


            bankEntryHelper.BankEntryHelperStart();

        }


        private void LoadCreditAccounts()
        {
            // Fetch the list of bank accounts from the database
            DebitAccounts = DatabaseHelper.GetCreditAccountsForShareJournal("SH");
            CreditAccounts = DatabaseHelper.GetDebitAccountsForShareJournal();
        }
        private void LoadSharesGrid()
        {
            if (SelectedCreditAccount != null)
            {
                QryShareBalancesNetts = DatabaseHelper.GetShareBalances(SelectedCreditAccount.AccountId);
            }
            else
            {
                QryShareBalancesNetts = new List<QryShareBalanceNett>(); // Clear the DataGrid if no account is selected
            }
        }


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
