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
    public class ShareJournalViewModel : INotifyPropertyChanged
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
                LoadSharesGrid() ;
            }
        }
        private List<SharesChart> _sharesCharts;
        public List<SharesChart> SharesCharts
        {
            get => _sharesCharts;
            set
            {
                _sharesCharts = value;
                OnPropertyChanged();
            }
        }
        private SharesChart _selectedShare;
        public SharesChart SelectedShare
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
        private string _purchasedateInput;
        public string PurchaseDateInput
        {
            get => _purchasedateInput;
            set
            {
                _purchasedateInput = value;
                OnPropertyChanged();
            }
        }
        private string _purchaseqtyInput;
        public string PurchaseQtyInput
        {
            get => _purchaseqtyInput;
            set
            {
                _purchaseqtyInput = value;
                OnPropertyChanged();
            }
        }
        private string _purchaserateInput;
        public string PurchaseRateInput
        {
            get => _purchaserateInput;
            set
            {
                _purchaserateInput = value;
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

        public ShareJournalViewModel()
        {
            IndividualName = GlobalVariables.IndividualName + "'s Purchase Share Journal ";
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

            //DateInput = null;
            //RefInput = null;
            NarrationInput = "Being";
            PurchaseQtyInput = null;
            PurchaseDateInput = null;
            PurchaseRateInput = null;
            SelectedShare = null;   
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

            if (string.IsNullOrWhiteSpace(RefInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a Reference.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }



            if (string.IsNullOrWhiteSpace(PurchaseDateInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a valid Purchase Date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (!DateTime.TryParse(PurchaseDateInput, out DateTime parsedDate))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a valid date format for Purchase Date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (string.IsNullOrWhiteSpace(NarrationInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a Narration.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if(string.IsNullOrEmpty(PurchaseQtyInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a Purchase Qty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (string.IsNullOrEmpty(PurchaseRateInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a Purchase Rate.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }


            return hasErrors;
        }
        private void MakeEntry()
        {
            BankEntryHelper bankEntryHelper = new BankEntryHelper();
            bankEntryHelper.BankCode = SelectedCreditAccount.AccountId;
            bankEntryHelper.BankDescription = SelectedCreditAccount.Account;

            bankEntryHelper.TranactionChartCode = SelectedDebitAccount.AccountId;
            bankEntryHelper.YearId = GlobalVariables.SelectedYearId;
            bankEntryHelper.TransactionCodeDescription = SelectedDebitAccount.Account;
            bankEntryHelper.TypeOfTransaction1Recepit2Payment = 2; // CAREFUL CHANGE THIS ACCORDINGLY
            bankEntryHelper.Reference = RefInput;
            bankEntryHelper.Narration = NarrationInput;
            bankEntryHelper.Amount = decimal.Parse(PurchaseRateInput) * decimal.Parse(PurchaseQtyInput);
            bankEntryHelper.TransactionType = "JE"; // OR CE
            bankEntryHelper.TDate = DateTime.Parse(DateInput);


            bankEntryHelper.SubledgerTranactionsNo0Fd1Shares2 = 2;
            bankEntryHelper.SubledgerCode = SelectedShare.ShareId;
            bankEntryHelper.SubledgerCodeDescription = SelectedShare.Company;
            bankEntryHelper.QtyOfShares = decimal.Parse(PurchaseQtyInput);
            bankEntryHelper.TDateForSharePurchase = DateTime.Parse(PurchaseDateInput);
            bankEntryHelper.ShareBuyingPrice = decimal.Parse(PurchaseRateInput);



            bankEntryHelper.BankEntryHelperStart();

        }


        private void LoadCreditAccounts()
        {
            // Fetch the list of bank accounts from the database
            CreditAccounts = DatabaseHelper.GetCreditAccountsForShareJournal("SH");
            DebitAccounts = DatabaseHelper.GetDebitAccountsForShareJournal();
        }
        private void LoadSharesGrid()
        {
            if (SelectedCreditAccount != null)
            {
                SharesCharts = DatabaseHelper.GetCompaniesForGLAccount(SelectedDebitAccount.AccountId);
            }
            else
            {
                SharesCharts = new List<SharesChart>(); // Clear the DataGrid if no account is selected
            }
        }


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
