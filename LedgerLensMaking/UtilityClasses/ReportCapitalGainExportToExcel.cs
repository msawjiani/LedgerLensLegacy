using ClosedXML.Excel;
using LedgerLennMaking.Models.Data;
using LedgerLensMaking;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Linq;  // Add this to enable LINQ methods like First()
using System;
using LedgerLensMaking.Models.Data;

public static class ReporCapitalGainExportToExcel
{
    public static void ExportCapgainReportToExcel(List<QryCapgainProfit> reportLedgers, string IndividualLine, string PeriodLine, string PrintDate)
    {
        try
        {
            // Prompt the user to choose a location to save the file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                Title = "Save Ledger Account",
                FileName = GlobalVariables.IndividualName + "CapitalGain.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                using (var workbook = new XLWorkbook())
                {
                    // Create a worksheet
                    var worksheet = workbook.Worksheets.Add("Capital Gain Statement");

                    worksheet.Cell(1, 1).Value = IndividualLine;
                    worksheet.Cell(2, 1).Value = PeriodLine;
                    worksheet.Cell(3, 1).Value = PrintDate;

                    // Center the title and style it
                    var headerRangeTitle = worksheet.Range("A1:H1");
                    headerRangeTitle.Merge();
                    headerRangeTitle.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                    headerRangeTitle.Style.Font.FontColor = XLColor.Black;
                    headerRangeTitle.Style.Font.Bold = true;
                    headerRangeTitle.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRangeTitle.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    var headerRangePeriod = worksheet.Range("A2:H2");
                    headerRangePeriod.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRangePeriod.Merge();
                    headerRangePeriod.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                    headerRangePeriod.Style.Font.FontColor = XLColor.Black;
                    headerRangePeriod.Style.Font.Bold = true;

                    var headerRangePrint = worksheet.Range("A3:H3");
                    headerRangePrint.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRangePrint.Merge();
                    headerRangePrint.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                    headerRangePrint.Style.Font.FontColor = XLColor.Black;
                    headerRangePrint.Style.Font.Bold = true;

                    // Insert headers for columns
                    worksheet.Cell(5, 1).Value = "Purchase Date";
                    worksheet.Cell(5, 2).Value = "Purchase Rate";
                    worksheet.Cell(5, 3).Value = "Qty Purchased";
                    worksheet.Cell(5, 4).Value = "Sale Date";
                    worksheet.Cell(5, 5).Value = "Qty Sold";
                    worksheet.Cell(5, 6).Value = "Selling Price";
                    worksheet.Cell(5, 7).Value = "Profit / Loss";
                    worksheet.Cell(5, 8).Value = "Months";

                    var headerRange = worksheet.Range("A5:H5");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Variables for tracking the running totals
                    decimal glAccountTotal = 0;
                    decimal glGrandTotal = 0;
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
                            worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";
                            worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";
                            worksheet.Range(currentRow, 1, currentRow, 8).Style.Fill.BackgroundColor = XLColor.FloralWhite;
                            worksheet.Range(currentRow, 1, currentRow, 8).Style.Font.FontColor = XLColor.Black;



                            currentRow++; // Move to the next row for the new account header

                            // Insert the GL account break if necessary
                            if (currentGLAccount != item.Account)
                            {
                                PutExtraAccountRowForBreak(worksheet, currentGLAccount, currentRow, glAccountTotal);
                                currentGLAccount = item.Account;  // Update the current GL account here
                                currentRow++;
                                glGrandTotal += glAccountTotal;
                                glAccountTotal = 0.00M;
                            }

                            // Write the new account header in AirForceBlue
                            PutAccountRow(worksheet, item.Company, currentRow);
                            currentRow++; // Move to the next row to write transaction data
                            startRow = currentRow; // Reset the start row for the new account
                        }

                        // Update the current account and add the row data
                        currentAccount = item.Company;
                        PutCurrentRowData(worksheet, currentRow, item);
                        glAccountTotal += item.ProfitOrLoss;


                        currentRow++;
                    }

                    // Insert the final totals for the last account
                    worksheet.Cell(currentRow, 1).Value = "Total";
                    worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                    worksheet.Cell(currentRow, 5).FormulaA1 = $"=SUM(E{startRow}:E{currentRow - 1})";
                    worksheet.Cell(currentRow, 7).FormulaA1 = $"=SUM(G{startRow}:G{currentRow - 1})";

                    worksheet.Range(currentRow, 1, currentRow, 8).Style.Fill.BackgroundColor = XLColor.FloralWhite;
                    worksheet.Range(currentRow, 1, currentRow, 8).Style.Font.FontColor = XLColor.Black;

                    worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";

                    currentRow = currentRow + 1;
                    PutExtraAccountRowForBreak(worksheet, currentGLAccount, currentRow, glAccountTotal);  // Add the extra row at the end if necessary

                    // Adjust column widths to fit content
                    worksheet.Columns().AdjustToContents();
                    currentRow++;
                    glGrandTotal += glAccountTotal;
                    PutFinalRowWithLedgerTotal(worksheet, currentGLAccount, currentRow, glGrandTotal);

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

    private static void PutCurrentRowData(IXLWorksheet worksheet, int currentRow, QryCapgainProfit item)
    {
        worksheet.Cell(currentRow, 1).Value = item.PurchaseDate.ToString("dd-MMM-yyyy");
        worksheet.Cell(currentRow, 2).Value = item.PurchaseRate;
        worksheet.Cell(currentRow, 3).Value = item.QtyPurchased;
        worksheet.Cell(currentRow, 4).Value = item.SaleDate.ToString("dd-MMM-yyyy");
        worksheet.Cell(currentRow, 5).Value = item.QtySold;
        worksheet.Cell(currentRow, 6).Value = item.SellingPrice;
        worksheet.Cell(currentRow, 7).Value = item.ProfitOrLoss;
        worksheet.Cell(currentRow, 8).Value = item.Months;

        // Apply number formatting for numeric columns
        worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Cell(currentRow, 3).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Cell(currentRow, 8).Style.NumberFormat.Format = "#,##0";
    }

    private static void PutAccountRow(IXLWorksheet worksheet, string account, int currentRow)
    {
        worksheet.Cell(currentRow, 1).Value = account;
        var headerAccount = worksheet.Range($"A{currentRow}:H{currentRow}");
        headerAccount.Merge();
        headerAccount.Style.Font.Bold = true;
        headerAccount.Style.Fill.BackgroundColor = XLColor.BabyBlueEyes;
        headerAccount.Style.Font.FontColor = XLColor.Black;
        headerAccount.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
    }

    private static void PutExtraAccountRowForBreak(IXLWorksheet worksheet, string account, int currentRow, decimal glAccountTotal)
    {
        worksheet.Cell(currentRow, 1).Value = account;
        var headerAccount = worksheet.Range($"A{currentRow}:F{currentRow}");
        headerAccount.Merge();
        headerAccount.Style.Font.Bold = true;
        headerAccount.Style.Font.FontColor = XLColor.Celadon;
        headerAccount.Style.Fill.BackgroundColor = XLColor.CalPolyPomonaGreen;
        headerAccount.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        worksheet.Cell(currentRow, 7).Value = glAccountTotal;
        worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.CalPolyPomonaGreen;
        worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Cell(currentRow, 7).Style.Font.FontColor = XLColor.Celadon;
        
        worksheet.Cell(currentRow, 8).Style.Fill.BackgroundColor = XLColor.CalPolyPomonaGreen;
        worksheet.Cell(currentRow, 8).Style.Font.FontColor = XLColor.Celadon;
    }
    private static void PutFinalRowWithLedgerTotal(IXLWorksheet worksheet, string account, int currentRow, decimal glAccountTotal)
    {
        worksheet.Cell(currentRow, 1).Value = "Grand Ledger Balance";
        var headerAccount = worksheet.Range($"A{currentRow}:F{currentRow}");
        headerAccount.Merge();
        headerAccount.Style.Font.Bold = true;
        headerAccount.Style.Font.FontColor = XLColor.ColumbiaBlue;
        headerAccount.Style.Fill.BackgroundColor = XLColor.Cobalt;
        headerAccount.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        worksheet.Cell(currentRow, 7).Value = glAccountTotal;
        worksheet.Cell(currentRow, 7).Style.Fill.BackgroundColor = XLColor.Cobalt;
        worksheet.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0.00";
        worksheet.Cell(currentRow, 7).Style.Font.FontColor = XLColor.ColumbiaBlue;
        worksheet.Cell(currentRow, 8).Style.Fill.BackgroundColor = XLColor.Cobalt;
        worksheet.Cell(currentRow, 8).Style.Font.FontColor = XLColor.ColumbiaBlue;
    }
}
