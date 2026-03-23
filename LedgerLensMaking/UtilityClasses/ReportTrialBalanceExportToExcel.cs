using ClosedXML.Excel;
using LedgerLensMaking.Models.UIModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LedgerLensMaking.UtilityClasses
{
    public static class ReportTrialBalanceExportToExcel
    {
        public static void ExportToExcel(List<ReportTrialBalanceModel> trialBalances, string IndividualLine, string PeriodLine, string PrintDate , string Type)
        {
            try
            {
                // Prompt the user to choose a location to save the file
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    Title = "Save Trial Balance Report",
                    FileName = GlobalVariables.IndividualName+ " TrialBalance.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    using (var workbook = new XLWorkbook())
                    {
                        // Create a worksheet
                        var worksheet = workbook.Worksheets.Add("Trial Balance");

                        worksheet.Cell(1, 1).Value = IndividualLine;
                        worksheet.Cell(2, 1).Value = PeriodLine;
                        worksheet.Cell(3, 1).Value = PrintDate;

                        // Center the title and style it
                        var headerRangeTitle = worksheet.Range("A1:C1");
                        headerRangeTitle.Merge();
                        headerRangeTitle.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                        headerRangeTitle.Style.Font.FontColor = XLColor.Black;
                        headerRangeTitle.Style.Font.Bold = true;
                        headerRangeTitle.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; // Center horizontally
                        headerRangeTitle.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;   // Center vertically

                        // Center the period and style it
                        var headerRangePeriod = worksheet.Range("A2:C2");
                        headerRangePeriod.Merge();
                        headerRangePeriod.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                        headerRangePeriod.Style.Font.FontColor = XLColor.Black;
                        headerRangePeriod.Style.Font.Bold = true;
                        headerRangePeriod.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRangePeriod.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        // Center the print date and style it
                        var headerRangePrint = worksheet.Range("A3:C3");
                        headerRangePrint.Merge();
                        headerRangePrint.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                        headerRangePrint.Style.Font.FontColor = XLColor.Black;
                        headerRangePrint.Style.Font.Bold = true;
                        headerRangePrint.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRangePrint.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;


                        // Set the headers
                        worksheet.Cell(4, 1).Value = "Account";
                        worksheet.Cell(4, 2).Value = "Debit";
                        worksheet.Cell(4, 3).Value = "Credit";

                        // Style the headers
                        var headerRange = worksheet.Range("A4:C4");
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Insert data
                        int currentRow = 5;
                        foreach (var item in trialBalances)
                        {
                            if (Type != "All" && (item.DebitColumn == 0 && item.CreditColumn == 0))
                            {
                                continue;
                            }
                            worksheet.Cell(currentRow, 1).Value = item.Account;
                            worksheet.Cell(currentRow, 2).Value = item.DebitColumn;
                            worksheet.Cell(currentRow, 3).Value = item.CreditColumn;

                            // Apply number formatting for debit and credit columns
                            worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                            worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "#,##0.00";

                            currentRow++;
                        }

                        // Calculate totals
                        worksheet.Cell(currentRow, 1).Value = "Total";
                        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                        worksheet.Cell(currentRow, 2).FormulaA1 = $"=SUM(B2:B{currentRow - 1})";
                        worksheet.Cell(currentRow, 3).FormulaA1 = $"=SUM(C2:C{currentRow - 1})";
                        worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "#,##0.00";

                        // Style the totals row
                        var totalsRange = worksheet.Range(currentRow, 1, currentRow, 3);
                        totalsRange.Style.Font.Bold = true;
                        totalsRange.Style.Fill.BackgroundColor = XLColor.DarkPastelBlue;
                        totalsRange.Style.Font.FontColor = XLColor.Black;

                        // Adjust column widths to fit content
                        worksheet.Columns().AdjustToContents();

                        // Save the file
                        workbook.SaveAs(filePath);

                        MessageBox.Show("Export successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while exporting: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
