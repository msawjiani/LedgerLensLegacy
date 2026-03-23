using ClosedXML.Excel;
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
using System.Windows.Input;

public class ReportShareBalanceViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    public ICommand ExportExcel { get; }

    private ObservableCollection<ShareBalanceTotal> _rows;
    public ObservableCollection<ShareBalanceTotal> Rows
    {
        get => _rows;
        set { _rows = value; OnPropertyChanged(); }
    }

    private bool _isGridEnabled = true;
    public bool IsGridEnabled
    {
        get => _isGridEnabled;
        set { _isGridEnabled = value; OnPropertyChanged(); }
    }

    public string IndividualName { get; set; }
    public string PeriodLine { get; set; }
    public string PrintDate { get; set; }

    // Optional footer totals
    public decimal TotalQty { get; private set; }
    public decimal TotalCost { get; private set; }

    public ReportShareBalanceViewModel()
    {
        IndividualName = GlobalVariables.IndividualName + "'s Share Balance (Totals)";
        PeriodLine = "For the year " +
                     GlobalVariables.SelectedStartDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture) +
                     " to " +
                     GlobalVariables.SelectedEndDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
        PrintDate = "Printed on " + DateTime.Today.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);

        ExportExcel = new RelayCommand(ExportClicked);
        LoadTotals();
    }

    private void LoadTotals()
    {
        // 1) Get the detailed rows you already have (reuse existing query)
        var detailed = DatabaseHelper.GetSharesAtCost() ?? new List<QryShareBalance>();

        // 2) Aggregate to totals per Account + Company
        //    If AtCost per-detail-row is already an AMOUNT, we sum it.
        //    If AtCost is a UNIT cost (unlikely by the name), fallback to Balance * PurchaseRate.
        var totals = detailed
            .GroupBy(x => new { x.Account, x.Company })
            .Select(g =>
            {
                var qty = g.Sum(r => r.Balance); // closing qty per company
                decimal costSum;

                var anyAtCost = g.Any(r => r.AtCost != 0m);
                if (anyAtCost)
                    costSum = g.Sum(r => r.AtCost);
                else
                    costSum = g.Sum(r => r.Balance * r.PurchaseRate);

                return new ShareBalanceTotal
                {
                    Account = g.Key.Account,
                    Company = g.Key.Company,
                    Qty = qty,
                    BalanceAtCost = costSum
                };
            })
            .OrderBy(t => t.Company)
            .ThenBy(t => t.Account)
            .ToList();

        Rows = new ObservableCollection<ShareBalanceTotal>(totals);

        // Footer totals (optional)
        TotalQty = Rows.Sum(r => r.Qty);
        TotalCost = Rows.Sum(r => r.BalanceAtCost);
        OnPropertyChanged(nameof(TotalQty));
        OnPropertyChanged(nameof(TotalCost));
    }

    private void ExportClicked()
    {
        // Reuse the new totals exporter
        ReportShareBalanceTotalsExportToExcel.ExportToExcel(
            Rows?.ToList() ?? new List<ShareBalanceTotal>(),
            IndividualName, PeriodLine, PrintDate);
    }
}
