using LedgerLensMaking;
using LedgerLensMaking.Models.Data;
using LedgerLensMaking.Models.UIModels;
using LedgerLensMaking.UtilityClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;

public class ReportLedgerViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ReportLegerModel> LedgerAccounts { get; set; }

    public ICommand ExportExcel { get; }
    //public ICommand ExportRestricted { get; }

    private string _individualName;
    private List<Chart> _glAccounts;
    private Chart _selectedGLAccount;

    public List<Chart> GLAccounts
    {
        get => _glAccounts;
        set
        {
            _glAccounts = value;
            OnPropertyChanged();
        }
    }

    public Chart SelectedGL
    {
        get => _selectedGLAccount;
        set
        {
            _selectedGLAccount = value;
            OnPropertyChanged();
            if (_selectedGLAccount != null) // Ensure that an account is selected
            {
                PopulateGLData(); // Populate the data grid based on the selected account
            }
        }
    }

    public string IndividualName
    {
        get => _individualName;
        set
        {
            _individualName = value;
            OnPropertyChanged();
        }
    }

    private string _periodLine;
    public string PeriodLine
    {
        get => _periodLine;
        set
        {
            _periodLine = value;
            OnPropertyChanged();
        }
    }

    private string _printDate;
    public string PrintDate
    {
        get => _printDate;
        set
        {
            _printDate = value;
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
        }
    }

    public ICommand SaveCommand { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    public ReportLedgerViewModel()
    {
    
        IndividualName = GlobalVariables.IndividualName + "'s Ledger Account Display";
        PeriodLine = "For the Year Ended " + GlobalVariables.SelectedStartDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture) + " to  " + GlobalVariables.SelectedEndDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
        PrintDate = "Printed on " + DateTime.Today.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
        ExportExcel = new RelayCommand(OnExportToExcel);


        // Load GL accounts when the ViewModel is initialized
        LoadAccountsForDropDown();

        // Calculate totals after population
        
    }
    private void OnExportToExcel()
    {
        //ReportLedgerAccountExportToExcel.ExportAccountToExcel(LedgerAccounts.ToList(), IndividualName, PeriodLine, PrintDate);
        ReporLedgersAccountExportToExcel.ExportAccountToExcel(LedgerAccounts.ToList(),IndividualName,PeriodLine,PrintDate   );
    }

    private void LoadAccountsForDropDown()
    {
        // Load GL Accounts without depending on SelectedGL initially
        GLAccounts = DatabaseHelper.GetCreditAccountsForShareJournal("GetEverything");
        OnPropertyChanged(nameof(GLAccounts)); // Notify UI that GLAccounts is updated
    }
    private void PopulateGLData()
    {
        if (SelectedGL != null)
        {
            // Fetch the ledger data for the selected GL account
            LedgerAccounts = new ObservableCollection<ReportLegerModel>(
                DatabaseHelper.GetLedgerAccount(SelectedGL.AccountId, GlobalVariables.SelectedYearId, "Single")
            
            );
            PopulateColumnDetails();

            // Notify the UI that LedgerAccounts has changed so it can update the grid
            OnPropertyChanged(nameof(LedgerAccounts));
        }
    }

    private void PopulateColumnDetails()
    {
        decimal RunningTotal = 0.00M;
        foreach (var row in LedgerAccounts)
        {
            RunningTotal += row.Amount;
            row.RunningBalance = RunningTotal;
            row.DebitColumn = row.Amount >= 0 ? row.Amount : 0;  // Positive balances go to Debit
            row.CreditColumn = row.Amount < 0 ? -row.Amount : 0; // Negative balances go to Credit


        }


    }


    private void ExportRestrictedClicked()
    {
        // Export logic here
    }


    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
