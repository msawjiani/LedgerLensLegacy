using ClosedXML.Excel;
using LedgerLensMaking.Models.UIModels;
using LedgerLensMaking;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public static class ReportShareBalanceTotalsExportToExcel
{
    public static void ExportToExcel(List<ShareBalanceTotal> rows, string individualName, string periodLine, string printDate)
    {
        using (var wb = new XLWorkbook())
        {
            var ws = wb.Worksheets.Add("Share Balance Totals");

            ws.Cell("A1").Value = individualName;
            ws.Cell("A2").Value = periodLine;
            ws.Cell("A3").Value = printDate;
            ws.Range("A1:D1").Merge().Style.Font.SetBold().Font.SetFontSize(14);
            ws.Range("A2:D2").Merge().Style.Font.SetItalic();
            ws.Range("A3:D3").Merge();

            ws.Cell("A5").Value = "Account";
            ws.Cell("B5").Value = "Company";
            ws.Cell("C5").Value = "Qty";
            ws.Cell("D5").Value = "BalanceAtCost";
            ws.Range("A5:D5").Style.Font.SetBold();

            int r = 6;
            foreach (var x in rows)
            {
                ws.Cell(r, 1).Value = x.Account;
                ws.Cell(r, 2).Value = x.Company;
                ws.Cell(r, 3).Value = x.Qty;
                ws.Cell(r, 4).Value = x.BalanceAtCost;
                r++;
            }

            // Totals row
            ws.Cell(r, 1).Value = "Totals:";
            ws.Cell(r, 3).FormulaA1 = $"SUM(C6:C{r - 1})";
            ws.Cell(r, 4).FormulaA1 = $"SUM(D6:D{r - 1})";
            ws.Range(r, 1, r, 4).Style.Font.SetBold();

            // Formats
            ws.Column(3).Style.NumberFormat.Format = "#,##0.00";
            ws.Column(4).Style.NumberFormat.Format = "#,##0.00";
            ws.Columns(1, 4).AdjustToContents();
            ws.SheetView.FreezeRows(5);

            var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                Title = "Save Share Balance Totals",
                FileName = $"{GlobalVariables.IndividualName}_ShareBalanceTotals.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                wb.SaveAs(sfd.FileName);
                try { Process.Start(new ProcessStartInfo(sfd.FileName) { UseShellExecute = true }); } catch { }
            }
        }
    }
}
