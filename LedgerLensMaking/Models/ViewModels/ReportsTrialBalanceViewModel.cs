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
using System.Windows;
using System.Windows.Input;

public class ReportsTrialBalanceViewModel : INotifyPropertyChanged
{
    public ObservableCollection<QryChartWithBalance> AllAccounts { get; set; } // Full Chart

    private ObservableCollection<ReportTrialBalanceModel> _trialBalances;
    public ObservableCollection<ReportTrialBalanceModel> TrialBalances
    {
        get => _trialBalances;
        set
        {
            _trialBalances = value;
            OnPropertyChanged();
        }
    }
    public ICommand ExportExcel { get; }
    public ICommand ExportRestricted { get; }
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

    public ReportsTrialBalanceViewModel()
    {
        ExportExcel = new RelayCommand(ExportClicked, () => TrialBalances.Any()); // Enable if there are a
        ExportRestricted = new RelayCommand(ExportRestrictedClicked, () => TrialBalances.Any()); // Enable if there are a
        IndividualName = GlobalVariables.IndividualName + "'s Trial Balance";
        PeriodLine ="For the Year  Ended " + GlobalVariables.SelectedStartDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture) + " to  " + GlobalVariables.SelectedEndDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
        PrintDate = "Printed on " + DateTime.Today.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
        TrialBalances = new ObservableCollection<ReportTrialBalanceModel>();

        // Assuming DatabaseHelper.GetGlAccountsForJournal fetches your accounts with balances
        AllAccounts = new ObservableCollection<QryChartWithBalance>(DatabaseHelper.GetGlAccountsForJournal("Trial Balance"));

        // Populate the Trial Balance
        PopulateTrialBalances();

        // Calculate totals after population
        CalculateTotals();

        
    }

    private void PopulateTrialBalances()
    {
        TrialBalances.Clear();

        foreach (var account in AllAccounts)
        {
            // Fill in the Debit and Credit columns based on the account balance
            var trialBalanceModel = new ReportTrialBalanceModel
            {
                AccountId = account.AccountId,
                Account = account.Account,
                DebitColumn = account.Balance >= 0 ? account.Balance : 0,  // Positive balances go to Debit
                CreditColumn = account.Balance < 0 ? -account.Balance : 0  // Negative balances go to Credit
            };

            TrialBalances.Add(trialBalanceModel);
        }
    }

    private void ExportClicked()
    {
        // Logic for saving trial balance report if needed
        ReportTrialBalanceExportToExcel.ExportToExcel(TrialBalances.ToList(),IndividualName,PeriodLine,PrintDate,"All");
    }
    private void ExportRestrictedClicked()
    {
        // Logic for saving trial balance report if needed
        ReportTrialBalanceExportToExcel.ExportToExcel(TrialBalances.ToList(), IndividualName, PeriodLine, PrintDate, "Restricted");
    }

    public void CalculateTotals()
    {
        TotalDebits = TrialBalances.Sum(j => j.DebitColumn);
        TotalCredits = TrialBalances.Sum(j => j.CreditColumn);
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
