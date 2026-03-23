using LedgerLensMaking.Models.Data;
using LedgerLensMaking.Models.UIModels;
using LedgerLensMaking.UtilityClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using LedgerLensMaking;
using System.Diagnostics;
using System.Windows.Navigation;


public class JournalEntryViewModel : INotifyPropertyChanged
{
    public List<QryChartWithBalance> AllAccounts { get; set; } // Holds all accounts
    public ObservableCollection<QryChartWithBalance> AvailableAccounts { get; set; } // Filtered list of accounts
    public ObservableCollection<JournalModel> JournalEntries { get; set; } // Holds all journal entries
    public event PropertyChangedEventHandler PropertyChanged;


    public ICommand DebitCommand { get; }
    public ICommand CreditCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand EditCommand { get; }
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
    private bool _isGridEnabled = true; // Default to true, assuming grid should be enabled by default
    public bool IsGridEnabled
    {
        get => _isGridEnabled;
        set
        {
            _isGridEnabled = value;
            OnPropertyChanged(); // Notify the UI that this property has changed
        }
    }
    private JournalModel _selectedJournalEntry;
    public JournalModel SelectedJournalEntry
    {
        get => _selectedJournalEntry;
        set
        {
            _selectedJournalEntry = value;
            OnPropertyChanged(); // Notify the UI that this property has changed
        }
    }

    private QryChartWithBalance _selectedAccount;
    public QryChartWithBalance SelectedAccount
    {
        get => _selectedAccount;
        set
        {
            _selectedAccount = value;
            OnPropertyChanged();
        }
    }

    private decimal _totalDebits;
    public decimal TotalDebits
    {
        get => _totalDebits;
        set
        {
            _totalDebits = value;
            OnPropertyChanged();
            CheckIfBalanced();
        }
    }

    private decimal _totalCredits;
    public decimal TotalCredits
    {
        get => _totalCredits;
        set
        {
            _totalCredits = value;
            OnPropertyChanged();
            CheckIfBalanced();
        }
    }
    private decimal _differenceBalance;
    public decimal DifferenceBalance
    {
        get=> _differenceBalance;
        set
        {
            _differenceBalance = value;
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
        }
    }

    private bool _isSaveEnabled;
    public bool IsSaveEnabled
    {
        get => _isSaveEnabled;
        set
        {
            _isSaveEnabled = value;
            OnPropertyChanged();
        }
    }

    public JournalEntryViewModel()
    {
        IndividualName = GlobalVariables.IndividualName + "'s Journal Entry";
        IsSaveEnabled = false;

        AllAccounts = DatabaseHelper.GetGlAccountsForJournal();
        AvailableAccounts = new ObservableCollection<QryChartWithBalance>(AllAccounts);
        JournalEntries = new ObservableCollection<JournalModel>();
        JournalEntries.CollectionChanged += JournalEntries_CollectionChanged;

        DebitCommand = new RelayCommand(DebitClicked);
        CreditCommand = new RelayCommand(CreditClicked);
        SaveCommand = new RelayCommand(SaveClicked, () => IsSaveEnabled); // Use CanExecute to enable/disable button
        DeleteCommand = new RelayCommand<JournalModel>(DeleteSelectedEntry);
    }

    private void DeleteSelectedEntry(JournalModel journalEntry)
    {
        if (journalEntry != null)
        {
            // Find the corresponding account for the deleted journal entry
            var accountToReAdd = AllAccounts.FirstOrDefault(a => a.AccountId == journalEntry.AccountId);

            // Remove the journal entry
            JournalEntries.Remove(journalEntry);

            // Add the account back to AvailableAccounts if it's not already present
            if (accountToReAdd != null && !AvailableAccounts.Contains(accountToReAdd))
            {
                AvailableAccounts.Add(accountToReAdd);
            }

            // Notify UI that AvailableAccounts has changed
            OnPropertyChanged(nameof(AvailableAccounts));

            // Recalculate totals after deletion
            CalculateTotals();

            MessageBox.Show($"Deleted: {journalEntry.Account}");
        }
    }
    private bool CheckFormErrors()
    {

        bool hasErrors = false;

        if (SelectedAccount == null)
        {
            hasErrors = true;
            MessageBox.Show("Please Select an Account.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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


    private void DebitClicked()
    {
        if ( !CheckFormErrors() &&  SelectedAccount != null && !JournalEntries.Any(e => e.AccountId == SelectedAccount.AccountId))
        {
            JournalEntries.Add(new JournalModel
            {
                AccountId = SelectedAccount.AccountId,
                Account = SelectedAccount.Account,
                DebitColumn = Convert.ToDecimal(AmountInput),
                CreditColumn = 0,
                DateStr = DateInput,    
                Ref=RefInput,
                Narration=NarrationInput,

                BalanceAfter = SelectedAccount.Balance + Convert.ToDecimal(AmountInput),
            });
            UpdateAvailableAccounts(); // Update available accounts after adding
        }
    }

    private void CreditClicked()
    {
        if (!CheckFormErrors()&&SelectedAccount != null && !JournalEntries.Any(e => e.AccountId == SelectedAccount.AccountId))
        {
            JournalEntries.Add(new JournalModel
            {
                AccountId = SelectedAccount.AccountId,
                Account = SelectedAccount.Account,
                DebitColumn = 0,
                CreditColumn = Convert.ToDecimal(AmountInput),
                BalanceBefore = SelectedAccount.Balance,
                BalanceAfter = SelectedAccount.Balance - Convert.ToDecimal(AmountInput),
                DateStr = DateInput,
                Ref = RefInput,
                Narration = NarrationInput,
            });
            UpdateAvailableAccounts(); // Update available accounts after adding
        }
    }

    private void UpdateAvailableAccounts()
    {
        // Update available accounts by removing those already selected in the journal entries
        var selectedAccountIds = JournalEntries.Select(e => e.AccountId).ToList();
        AvailableAccounts = new ObservableCollection<QryChartWithBalance>(AllAccounts.Where(a => !selectedAccountIds.Contains(a.AccountId)));
        OnPropertyChanged(nameof(AvailableAccounts)); // Notify the UI that AvailableAccounts has changed
    }
    private void ClearFormAndReset()
    {
        RefInput = null;
        NarrationInput = null;
        IsSaveEnabled = false;
        DateInput = null;
        AmountInput = null;

        // Repopulate available accounts
        UpdateAvailableAccounts(); // Ensures accounts are restored after saving

        // Optionally, clear the selected account as well
        SelectedAccount = null;
    }
    private void SaveClicked()
    {

        int unixTime = DatabaseHelper.GetUnix(); // Assuming DatabaseHelper has this method
        string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={GlobalVariables.DatabaseFileName};";

        // Loop through each journal entry and save to the database
        foreach (var journalEntry in JournalEntries)
        {
            try
            {
                GeneralLedger generalLedger = new GeneralLedger();
                decimal amount = journalEntry.DebitColumn != 0 ? journalEntry.DebitColumn : -journalEntry.CreditColumn;
                generalLedger.AccountId = journalEntry.AccountId;
                generalLedger.YearId = GlobalVariables.SelectedYearId;
                generalLedger.Tdate = Convert.ToDateTime(journalEntry.DateStr);
                generalLedger.Unix = unixTime;
                generalLedger.Ref = journalEntry.Ref;
                generalLedger.Particulars = "Consolidated Journal Entry";
                generalLedger.Amount = amount;
                generalLedger.TransactionType = "JE";
                generalLedger.Narration = journalEntry.Narration;

                TransactionRepository transactionRepository = new TransactionRepository(connectionString);
                transactionRepository.InsertGeneralLedgerTransaction(generalLedger);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing journal entry for {journalEntry.Account}: {ex.Message}");
                // Optionally log the error for debugging
            }
        }
        ClearFormAndReset();


    }

    private void JournalEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        CalculateTotals();
    }

    public void CalculateTotals()
    {
        TotalDebits = JournalEntries.Sum(j => j.DebitColumn);
        TotalCredits = JournalEntries.Sum(j => j.CreditColumn);
    }

    private void CheckIfBalanced()
    {
        DifferenceBalance = TotalDebits-TotalCredits;
        IsSaveEnabled = TotalDebits == TotalCredits && TotalDebits > 0;
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
