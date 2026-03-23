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

public class ReportGeneralLedgerViewModel : INotifyPropertyChanged
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

    public ReportGeneralLedgerViewModel()
    {

        IndividualName = GlobalVariables.IndividualName + "'s General Ledger";
        PeriodLine = "For the Year Ended " + GlobalVariables.SelectedStartDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture) + " to  " + GlobalVariables.SelectedEndDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
        PrintDate = "Printed on " + DateTime.Today.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
        ExportExcel = new RelayCommand(OnExportToExcel);
        PopulateGLData();
        // Load GL accounts when the ViewModel is initialized
        

        // Calculate totals after population

    }
    private void OnExportToExcel()
    {
        //ReportLedgerAccountExportToExcel.ExportAccountToExcel(LedgerAccounts.ToList(), IndividualName, PeriodLine, PrintDate);
        ReporLedgersAccountExportToExcel.ExportAllAccountsToExcel(LedgerAccounts.ToList(), IndividualName, PeriodLine, PrintDate);
    }


    private void PopulateGLData()
    {

        // Fetch the ledger data for the selected GL account
        LedgerAccounts = new ObservableCollection<ReportLegerModel>(
            DatabaseHelper.GetLedgerAccount(-1, GlobalVariables.SelectedYearId, "Full"));
        PopulateColumnDetails();


    }

    private void PopulateColumnDetails()
    {
        decimal RunningTotal = 0.00M;
        string CurrentAccount=LedgerAccounts.FirstOrDefault()?.Account;

        foreach (var row in LedgerAccounts)
        {
            if (CurrentAccount!=row.Account)
            {
                RunningTotal = 0;
                CurrentAccount = row.Account;   
            }
            RunningTotal += row.Amount;
            row.RunningBalance = RunningTotal;
            row.DebitColumn = row.Amount >= 0 ? row.Amount : 0;  // Positive balances go to Debit
            row.CreditColumn = row.Amount < 0 ? -row.Amount : 0; // Negative balances go to Credit


        }


    }




    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
