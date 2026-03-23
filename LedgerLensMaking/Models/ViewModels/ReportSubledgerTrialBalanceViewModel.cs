using LedgerLennMaking.Models.Data;
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

public class ReportSubledgerTrialBalanceViewModel : INotifyPropertyChanged
{
    //public ObservableCollection<QryShareBalance> QryShareBalances { get; set; } // Full Chart

    public ICommand ExportExcel { get; }
    public ICommand ExportRestricted { get; }
    private string _individualName;

    private ObservableCollection<QrySubledgerFDBalance> _qryFd;
    public ObservableCollection<QrySubledgerFDBalance> QryFDBalances
    {
        get => _qryFd;
        set
        {
            _qryFd = value;
            OnPropertyChanged();
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




    public event PropertyChangedEventHandler PropertyChanged;

    public ReportSubledgerTrialBalanceViewModel()
    {


        IndividualName = GlobalVariables.IndividualName + "'s Subledger Balances";
        PeriodLine = "For the Year  Ended " + GlobalVariables.SelectedStartDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture) + " to  " + GlobalVariables.SelectedEndDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
        PrintDate = "Printed on " + DateTime.Today.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
        QryFDBalances = new ObservableCollection<QrySubledgerFDBalance>();
        ExportExcel = new RelayCommand(ExportClicked); // Enable if there are a
        // Assuming DatabaseHelper.GetGlAccountsForJournal fetches your accounts with balances
        QryFDBalances = new ObservableCollection<QrySubledgerFDBalance>(DatabaseHelper.GetAllFDBalances());





    }


    private void ExportClicked()
    {
        // Logic for saving trial balance report if needed
        // MessageBox.Show("Export Clicked");
        ReporFDsExportToExcel.ExportFDsExcel(QryFDBalances.ToList()  , IndividualName, PeriodLine, PrintDate);
        //ReporSharesAtCostExportToExcel.ExportSharesAtCostToExcel(QryShareBalances.ToList(), IndividualName, PeriodLine, PrintDate);
    }


    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
