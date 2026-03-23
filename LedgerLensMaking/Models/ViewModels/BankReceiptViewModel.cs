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
    public class BankReceiptViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand SfCommand { get; }
        public ICommand SaveReceiptCommand { get; }
        private bool _isSfEnabled = false;
        private bool _isSubledgerFormDealtWith = false;
        public ICommand DeleteCommand { get; }


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
        private List<Chart> _bankAccounts;
        public List<Chart> BankAccounts
        {
            get => _bankAccounts;
            set
            {
                _bankAccounts = value;
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
        private string _amountInput;
        public string AmountInput
        {
            get => _amountInput;
            set
            {
                _amountInput = value;
                OnPropertyChanged();
                UpdateAmountMessage();
            }
        }
        private string _amountMessage;
        public string AmountMessage
        {
            get => _amountMessage;
            private set
            {
                _amountMessage = value;
                OnPropertyChanged();
            }
        }
        private Chart _selectedBankAccount;
        public Chart SelectedBank
        {
            get => _selectedBankAccount;
            set
            {
                _selectedBankAccount = value;
                OnPropertyChanged();
                LoadAccountsForDropDown(); // Make sure this is called when a bank account is selected
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

        private List<BankEntryGridModel> _bankEntryGridModels;
        public List<BankEntryGridModel> BankEntryGridData
        {
            get=> _bankEntryGridModels;
            set
            {
                _bankEntryGridModels = value;
                OnPropertyChanged();
            }
        }
        private Chart _selectedGLAccount;
        public Chart SelectedGLAccount
        {
            get => _selectedGLAccount;
            set
            {
                _selectedGLAccount = value;
                OnPropertyChanged();
                // Enable the button if SubledgerFlag is 'SH' or 'SL'
                IsSfEnabled = _selectedGLAccount != null &&
                              (_selectedGLAccount.SubledgerFlag == "SH" || _selectedGLAccount.SubledgerFlag == "SL");
            }
        }
        private QrySubledgerFDBalance _selectedFD;
        public QrySubledgerFDBalance SelectedFD
        {
            get => _selectedFD;
            set
            {
                _selectedFD = value;
                OnPropertyChanged();
            }
        }

        private SelectedShareDetails _selectedShare;
        public SelectedShareDetails SelectedShare
        {
            get => _selectedShare;
            set
            {
                _selectedShare = value;
                OnPropertyChanged();
            }
        }
        public bool IsSfEnabled
        {
            get => _isSfEnabled;
            set
            {
                _isSfEnabled = value;
                OnPropertyChanged();
            }
        }
        public void HandleFDSelection(QrySubledgerFDBalance selectedFD)
        {
            // Logic to handle the selected FD, e.g., store it, update UI, etc.
            // For example:
            SelectedFD = selectedFD; // Assuming you have a property to hold the selected FD
        }
        public void HandleShareSelection(SelectedShareDetails selectedShare)
        {
            
            SelectedShare = selectedShare; // Assuming you have a property to hold the selected FD
        }

        public BankReceiptViewModel()
        {
            IndividualName = GlobalVariables.IndividualName + "'s Bank Receipts";
            LoadBankAccounts();
            SaveReceiptCommand = new RelayCommand(OnSaveReceipt);
            SfCommand = new RelayCommand(OnSfCommandExecute);
            DeleteCommand = new RelayCommand<BankEntryGridModel>(DeleteSelectedEntry);
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown));

        }
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                IncrementDate();
                e.Handled = true;
            }
            else if (e.Key == Key.F3)
            {
                IncrementRefInput();
                e.Handled = true;
            }
        }

        private void IncrementDate()
        {
            if (DateTime.TryParse(DateInput, out var date))
            {
                DateInput = date.AddDays(1).ToString("dd-MMM-yyyy");
            }
        }
        private void IncrementRefInput()
        {
            if (!string.IsNullOrEmpty(RefInput) && RefInput.Length > 2)
            {
                string prefix = RefInput.Substring(0, 2);
                if (int.TryParse(RefInput.Substring(2), out int number))
                {
                    RefInput = prefix + (number + 1).ToString();
                }
            }
        }
        private void UpdateAmountMessage()
        {
            string amount = AmountInput?.Split('.')[0]; // Get the part before the decimal

            if (!string.IsNullOrEmpty(amount) && amount.Length >= 4)
            {
                if (amount.Length == 4)
                {
                    AmountMessage = "1K";
                }
                else if (amount.Length == 5)
                {
                    AmountMessage = "10K";
                }
                else if (amount.Length == 6)
                {
                    AmountMessage = "1L";
                }
                else if (amount.Length == 7)
                {
                    AmountMessage = "10L";
                }
                else if (amount.Length == 8)
                {
                    AmountMessage = "1C";
                }
                else if (amount.Length == 9)
                {
                    AmountMessage = "10C";
                }
                else
                {
                    AmountMessage = "";
                }
            }
            else
            {
                AmountMessage = "";
            }
        }
        private void DeleteSelectedEntry(BankEntryGridModel bankEntry)
        {
            if (bankEntry != null)
            {

                if (DatabaseHelper.CheckDeletion(bankEntry.Unix) == false)
                {
                    MessageBox.Show("That transaction cannot be deleted", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Exclamation  );
                }
                else
                {
                    MessageBox.Show("Deleting....", "Continue", MessageBoxButton.OK, MessageBoxImage.Information );
                    DatabaseHelper.DeleteTransactions(bankEntry.Unix);
                    RePopulateBankGrid();
                }
            }
            else
            {
                MessageBox.Show("No entry selected.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }
        private void OnSfCommandExecute()
        {
            // Create and show the dialog window
           // var parentViewModel = /* Get your BankReceiptViewModel instance */;
            //var subledgerViewModel = new BankReceiptSubledgerViewModel(parentViewModel);

            if (SelectedGLAccount != null && SelectedGLAccount.SubledgerFlag=="SL")
            {
                var subledgerWindow = new BankReceiptSubledgerWindow
                {
                    DataContext = new BankReceiptSubledgerViewModel(SelectedGLAccount.AccountId, SelectedGLAccount.Account, HandleFDSelection)
                };
                _isSubledgerFormDealtWith = false;
                subledgerWindow.ShowDialog(); // Show as a modal dialog
                if (SelectedGLAccount != null)
                {
                    _isSubledgerFormDealtWith = true;
                    //MessageBox.Show("Thanks for Dealing with the Subledger Dialog box" + SelectedFD.Subaccount);
                }
            }
            else if (SelectedGLAccount != null && SelectedGLAccount.SubledgerFlag=="SH")
            {
                var subsharesWindow = new BankReceiptsSharesWindow()
                {
                    DataContext = new BankReceiptSharesViewModel(SelectedGLAccount.AccountId, SelectedGLAccount.Account,HandleShareSelection)
                };
                _isSubledgerFormDealtWith = false;
                subsharesWindow.ShowDialog(); // Show as a modal dialog
            }
        }


        private void OnSaveReceipt()
        {
            if (!CheckFormErrors())
            {
                MakeEntry();
                RePopulateBankGrid();
            }

        }
        private bool CheckFormErrors()
        {
           
            bool hasErrors = false;

            if (SelectedBank == null)
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

            if (SelectedGLAccount == null)
            {
                hasErrors = true;
                MessageBox.Show("Please select a GL Account.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (SelectedGLAccount != null && SelectedGLAccount.SubledgerFlag == "SL" && _isSubledgerFormDealtWith == false)
            {
                hasErrors = true;
                MessageBox.Show("Please deal with the Subledger Form for Deposits.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (!decimal.TryParse(AmountInput, out decimal amount) || amount == 0)
            {
                hasErrors = true;
                MessageBox.Show("Please enter a valid Amount.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            bankEntryHelper.BankCode = SelectedBank.AccountId;
            bankEntryHelper.BankDescription = SelectedBank.Account;
            bankEntryHelper.TranactionChartCode = SelectedGLAccount.AccountId;
            bankEntryHelper.YearId = GlobalVariables.SelectedYearId;

            bankEntryHelper.TransactionCodeDescription = SelectedGLAccount.Account;
            bankEntryHelper.TypeOfTransaction1Recepit2Payment = 1;
            bankEntryHelper.Reference = RefInput;
            bankEntryHelper.Narration = NarrationInput;
            bankEntryHelper.Amount = decimal.Parse(AmountInput);
            bankEntryHelper.TransactionType = "BE"; // OR CE
            bankEntryHelper.TDate = DateTime.Parse(DateInput);

            if(SelectedGLAccount.SubledgerFlag=="SL")
            {
                bankEntryHelper.SubledgerTranactionsNo0Fd1Shares2 = 1;
                bankEntryHelper.SubledgerCode = SelectedFD.SubledgerId; // This is not to be hard coded
                bankEntryHelper.SubledgerCodeDescription = SelectedFD.Subaccount; // This is not to be hard coded
                bankEntryHelper.InterestOrGainAmount =  decimal.Parse(AmountInput)-SelectedFD.TotalFD  ;
                bankEntryHelper.InterestAccountCode = GlobalVariables.InterestAccountCode;
                bankEntryHelper.InterestAccountDescription = GlobalVariables.InterestAccountDesc;
            }
            else if(SelectedGLAccount.SubledgerFlag=="SH")
            {
                bankEntryHelper.SubledgerTranactionsNo0Fd1Shares2 = 2; // SHARES 
                bankEntryHelper.SharesAccountCode = SelectedShare.SelectedShare.ShareId;
                bankEntryHelper.SubledgerCodeDescription = SelectedShare.SelectedShare.Company;
                bankEntryHelper.QtyOfShares = SelectedShare.Quantity ;
                bankEntryHelper.ShareSellingPrice = SelectedShare.Rate;
               

                bankEntryHelper.LTCapgainAccountCode = GlobalVariables.LTCGAccountCode;
                bankEntryHelper.STCapgainAccountCode = GlobalVariables.STCGAccountCode;
                bankEntryHelper.LTCaplossAccountCode = GlobalVariables.LTCLAccountCode;
                bankEntryHelper.STCaplossAccountCode = GlobalVariables.STCLAccountCode;
            }


            bankEntryHelper.BankEntryHelperStart();

        }
        private void RePopulateBankGrid()
        {
           // RefInput = string.Empty;
            SelectedGLAccount = null;
            AmountInput = string.Empty;
            NarrationInput = "Being";
            SelectedFD = null;

            BankEntryGridData = DatabaseHelper.GetGridDetails(SelectedBank.AccountId, GlobalVariables.SelectedYearId);
            decimal RunningTotal = 0.00M;
            foreach (var row in BankEntryGridData)
            {
                RunningTotal += row.Amount;
                row.RunningBalance = RunningTotal;

            }
            CurrentBalance = RunningTotal;
            OnPropertyChanged(nameof(BankEntryGridData)); // Ensure this line is called
        }

        private void LoadBankAccounts()
        {
            // Fetch the list of bank accounts from the database
            BankAccounts = DatabaseHelper.GetBankAccounts();
        }
        private void LoadAccountsForDropDown()
        {
            if (SelectedBank != null)
            {
                GLAccounts = DatabaseHelper.GetAccounts(SelectedBank.AccountId);
                BankEntryGridData = DatabaseHelper.GetGridDetails(SelectedBank.AccountId, GlobalVariables.SelectedYearId);
                decimal RunningTotal = 0.00M;
                foreach(var row in BankEntryGridData)
                {
                    RunningTotal += row.Amount;
                    row.RunningBalance = RunningTotal;

                }
                CurrentBalance = RunningTotal;
                OnPropertyChanged(nameof(BankEntryGridData)); // Ensure this line is called
            }
            else
            {
                GLAccounts = new List<Chart>();
                BankEntryGridData = new List<BankEntryGridModel>();
                OnPropertyChanged(nameof(BankEntryGridData)); // Ensure this line is called
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
