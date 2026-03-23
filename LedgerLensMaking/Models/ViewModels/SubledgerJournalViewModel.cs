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
    public class SubledgerJournalViewModel : INotifyPropertyChanged
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
                LoadSubledgerGrid();
                OnPropertyChanged();

            }
        }
        private List<SubledgerChart> _subledgerCharts;
        public List<SubledgerChart> SubledgerCharts
        {
            get => _subledgerCharts;
            set
            {
                _subledgerCharts = value;
                OnPropertyChanged();
            }
        }
        private SubledgerChart _selectedSubledger;
        public SubledgerChart SelectedSubledgerChart
        {
            get => _selectedSubledger;
            set
            {
                _selectedSubledger = value;
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
        private string _amountInput;
        public string AmountInput
        {
            get => _amountInput;
            set
            {
                _amountInput = value;
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

        public SubledgerJournalViewModel()
        {
            IndividualName = GlobalVariables.IndividualName + "'s Subledger Journal";
            LoadCreditAccounts();
            SavePaymentCommand = new RelayCommand(OnSaveReceipt);

        }



        private void OnSaveReceipt()
        {

            if (!CheckFormErrors())
            {
                if (SelectedSubledgerChart != null)
                {

                    MakeEntry();
                    ClearInputs();

                }
                else
                {
                    MessageBox.Show("You Must select a Subledger");
                }
            }

        }
        private void ClearInputs()
        {

           // DateInput = null;
            //RefInput = null;
            NarrationInput = "Being";
            AmountInput = null;
            SelectedSubledgerChart = null;
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



            if (string.IsNullOrWhiteSpace(NarrationInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a Narration.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }


            return hasErrors;
        }
        private void MakeEntry()
        {
            BankEntryHelper bankEntryHelper = new BankEntryHelper();
            bankEntryHelper.BankCode = SelectedCreditAccount.AccountId   ;
            bankEntryHelper.BankDescription = SelectedCreditAccount.Account;
            bankEntryHelper.TranactionChartCode = SelectedDebitAccount.AccountId ;
            bankEntryHelper.TransactionCodeDescription = SelectedDebitAccount.Account;
            bankEntryHelper.YearId = GlobalVariables.SelectedYearId;
            bankEntryHelper.TypeOfTransaction1Recepit2Payment = 2; // CAREFUL CHANGE THIS ACCORDINGLY
            bankEntryHelper.Reference = RefInput;
            bankEntryHelper.Narration = NarrationInput;
            bankEntryHelper.Amount = decimal.Parse(AmountInput);
            bankEntryHelper.TransactionType = "BE"; // OR CE
            bankEntryHelper.TDate = DateTime.Parse(DateInput);
            bankEntryHelper.SubledgerTranactionsNo0Fd1Shares2 = 1;
            bankEntryHelper.SubledgerCode = SelectedSubledgerChart.SubledgerId; // This is not to be hard coded
            bankEntryHelper.SubledgerCodeDescription = SelectedSubledgerChart.Subaccount; // This is not to be hard coded


            bankEntryHelper.BankEntryHelperStart();

        }


        private void LoadCreditAccounts()
        {
            // Fetch the list of bank accounts from the database
            CreditAccounts = DatabaseHelper.GetCreditAccountsForShareJournal("Subledger");
            DebitAccounts = DatabaseHelper.GetCreditAccountsForShareJournal("Other");
        }
        private void LoadSubledgerGrid()
        {
            if (SelectedDebitAccount != null)
            {
                SubledgerCharts = DatabaseHelper.GetSubledgerForGLAccount(SelectedDebitAccount.AccountId);
            }
            else
            {
                SubledgerCharts = new List<SubledgerChart>(); // Clear the DataGrid if no account is selected
            }
        }


        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
