using ClosedXML.Excel;
using LedgerLennMaking.Models.Data;
using LedgerLensMaking;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Linq;  // Add this to enable LINQ methods like First()
using System;

public static class ReporSharesAtCostExportToExcel
{
    public static void ExportSharesAtCostToExcel(List<QryShareBalance> reportLedgers, string IndividualLine, string PeriodLine, string PrintDate)
    {
        try
        {
            // Prompt the user to choose a location to save the file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                Title = "Save Ledger Account",
                FileName = GlobalVariables.IndividualName + "Shares.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                using (var workbook = new XLWorkbook())
                {
                    // Create a worksheet
                    var worksheet = workbook.Worksheets.Add("Shares at Cost");

                    worksheet.Cell(1, 1).Value = IndividualLine;
                    worksheet.Cell(2, 1).Value = PeriodLine;
                    worksheet.Cell(3, 1).Value = PrintDate;

                    // Center the title and style it
                    var headerRangeTitle = worksheet.Range("A1:G1");
                    headerRangeTitle.Merge();
                    headerRangeTitle.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                    headerRangeTitle.Style.Font.FontColor = XLColor.Black;
                    headerRangeTitle.Style.Font.Bold = true;
                    headerRangeTitle.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRangeTitle.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    var headerRangePeriod = worksheet.Range("A2:G2");
                    headerRangePeriod.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRangePeriod.Merge();
                    headerRangePeriod.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                    headerRangePeriod.Style.Font.FontColor = XLColor.Black;
                    headerRangePeriod.Style.Font.Bold = true;

                    var headerRangePrint = worksheet.Range("A3:G3");
                    headerRangePrint.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRangePrint.Merge();
                    headerRangePrint.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                    headerRangePrint.Style.Font.FontColor = XLColor.Black;
                    headerRangePrint.Style.Font.Bold = true;

                    // Insert headers for columns
                    worksheet.Cell(5, 1).Value = "Date";
                    worksheet.Cell(5, 2).Value = "Purchase Date";
                    worksheet.Cell(5, 3).Value = "Qty";
                    worksheet.Cell(5, 4).Value = "Qty Sold";
                    worksheet.Cell(5, 5).Value = "Balance";
                    worksheet.Cell(5, 6).Value = "Purchase Rate";
                    worksheet.Cell(5, 7).Value = "Value at Cost";

                    var headerRange = worksheet.Range("A5:G5");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Variables for tracking the running totals
                    decimal totalDebit = 0;
                    decimal totalCredit = 0;
                    decimal glAccountTotal = 0;
                    string currentAccount = null;
                    string currentGLAccount = null;  // Track GL Account for extra row
                    int startRow = 6;
                    int currentRow = startRow;

                    // Start by adding the first account in AirForceBlue row
                    if (reportLedgers.Any())
                    {
                        currentAccount = reportLedgers.First().Company;
                        currentGLAccount = reportLedgers.First().Account;  // Initialize GL account
                        PutAccountRow(worksheet, currentAccount, currentRow);
                        currentRow++; // Move to the next row to write transaction data
                        startRow = currentRow; // Update the start row for the first account
                    }

                    foreach (var item in reportLedgers)
                    {
                        // Check if the company account has changed
                        if (currentAccount != null && currentAccount != item.Company)
                        {
                            // Insert the totals for the previous account
                            worksheet.Cell(currentRow, 1).Value = "Total";
                            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                            worksheet.Cell(currentRow, 5).FormulaA1 = $"=SUM(E{startRow}:E{currentRow - 1})";
                            worksheet.Cell(currentRow, 7).FormulaA1 = $"=SUM(G{startRow}:G{currentRow - 1})";
                            worksheet.Range(currentRow, 1, currentRow, 7).Style.Fill.BackgroundColor = XLColor.DarkPastelBlue;
                            worksheet.Range(currentRow, 1, currentRow, 7).Style.Font.FontColor = XLColor.Black;
                            worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";
                            worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";

                            currentRow++; // Move to the next row for the new account header

                            // Insert the GL account break if necessary
                            if (currentGLAccount != item.Account)
                            {
                                PutExtraAccountRowForBreak(worksheet, currentGLAccount, currentRow,glAccountTotal);
                                currentGLAccount = item.Account;  // Update the current GL account here
                                currentRow++;
                                glAccountTotal = 0;
                            }

                            // Write the new account header in AirForceBlue
                            PutAccountRow(worksheet, item.Company, currentRow);
                            currentRow++; // Move to the next row to write transaction data
                            startRow = currentRow; // Reset the start row for the new account
                        }

                        // Update the current account and add the row data
                        currentAccount = item.Company;
                        PutCurrentRowData(worksheet, currentRow, item);
                        totalDebit += item.Balance;
                        totalCredit += item.AtCost;
                        glAccountTotal += item.AtCost;
                        

                        currentRow++;
                    }

                    // Insert the final totals for the last account
                    worksheet.Cell(currentRow, 1).Value = "Total";
                    worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 5).FormulaA1 = $"=SUM(E{startRow}:E{currentRow - 1})";
                    worksheet.Cell(currentRow, 7).FormulaA1 = $"=SUM(G{startRow}:G{currentRow - 1})";
                    worksheet.Range(currentRow, 1, currentRow, 7).Style.Fill.BackgroundColor = XLColor.DarkPastelBlue;
                    worksheet.Range(currentRow, 1, currentRow, 7).Style.Font.FontColor = XLColor.Black;

                    worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";

                    currentRow = currentRow + 1;
                    PutExtraAccountRowForBreak(worksheet, currentGLAccount, currentRow,glAccountTotal);  // Add the extra row at the end if necessary

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

    private static void PutCurrentRowData(IXLWorksheet worksheet, int currentRow, QryShareBalance item)
    {
        worksheet.Cell(currentRow, 1).Value = item.TDate.ToString("dd-MMM-yyyy");
        worksheet.Cell(currentRow, 2).Value = item.SharePurchaseTdate.ToString("dd-MMM-yyyy");
        worksheet.Cell(currentRow, 3).Value = item.QtyPurchased;
        worksheet.Cell(currentRow, 4).Value = item.QS;
        worksheet.Cell(currentRow, 5).Value = item.Balance;
        worksheet.Cell(currentRow, 6).Value = item.PurchaseRate;
        worksheet.Cell(currentRow, 7).Value = item.AtCost;

        // Apply number formatting for numeric columns
        worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";
    }

    private static void PutAccountRow(IXLWorksheet worksheet, string account, int currentRow)
    {
        worksheet.Cell(currentRow, 1).Value = account;
        var headerAccount = worksheet.Range($"A{currentRow}:G{currentRow}");
        headerAccount.Merge();
        headerAccount.Style.Font.Bold = true;
        headerAccount.Style.Fill.BackgroundColor = XLColor.DarkMidnightBlue;
        headerAccount.Style.Font.FontColor = XLColor.AliceBlue;
        headerAccount.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
    }

    private static void PutExtraAccountRowForBreak(IXLWorksheet worksheet, string account, int currentRow,decimal glAccountTotal)
    {
        worksheet.Cell(currentRow, 1).Value = account;
        var headerAccount = worksheet.Range($"A{currentRow}:F{currentRow}");
        headerAccount.Merge();
        headerAccount.Style.Font.Bold = true;
        headerAccount.Style.Font.FontColor = XLColor.MintCream;
        headerAccount.Style.Fill.BackgroundColor = XLColor.DarkSlateGray;
        headerAccount.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        worksheet.Cell(currentRow, 7).Value = glAccountTotal;
        worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.DarkSlateGray;
        worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Cell(currentRow, 7).Style.Font.FontColor = XLColor.MintCream;
        worksheet.Cell(currentRow,7).Style.Font.Bold=true;
    }
}
