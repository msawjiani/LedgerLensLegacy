using ClosedXML.Excel;
using LedgerLensMaking.Models.Data;
using LedgerLensMaking.Models.UIModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LedgerLensMaking.UtilityClasses
{
    public static class ReporFDsExportToExcel
    {
        public static void ExportFDsExcel(List<QrySubledgerFDBalance> reportLegers, string IndividualLine, string PeriodLine, string PrintDate)
        {
            try
            {
                // Prompt the user to choose a location to save the file
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    Title = "Save Ledger Account",
                    FileName = GlobalVariables.IndividualName + "FD.xlsx"
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
                        var headerRangeTitle = worksheet.Range("A1:B1");
                        headerRangeTitle.Merge();
                        headerRangeTitle.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                        headerRangeTitle.Style.Font.FontColor = XLColor.Black;
                        headerRangeTitle.Style.Font.Bold = true;
                        headerRangeTitle.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRangeTitle.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        var headerRangePeriod = worksheet.Range("A2:B2");
                        headerRangePeriod.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRangePeriod.Merge();
                        headerRangePeriod.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                        headerRangePeriod.Style.Font.FontColor = XLColor.Black;
                        headerRangePeriod.Style.Font.Bold = true;

                        var headerRangePrint = worksheet.Range("A3:B3");
                        headerRangePrint.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRangePrint.Merge();
                        headerRangePrint.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                        headerRangePrint.Style.Font.FontColor = XLColor.Black   ;
                        headerRangePrint.Style.Font.Bold = true;

                        // Insert headers for columns
                        worksheet.Cell(5, 1).Value = "Subledger";
                        worksheet.Cell(5, 2).Value = "Amount";

                        var headerRange = worksheet.Range("A5:B5");
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.SlateGray;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Set the column width to 110px
                        worksheet.Column(1).Width = 14; // Approximation for 110px in Excel
                        worksheet.Column(2).Width = 14;

                        // Variables for tracking the running totals
                        decimal totalDebit = 0;
                        string currentAccount = null;
                        int startRow = 6;
                        int currentRow = startRow;

                        // Start by adding the first account in AirForceBlue row
                        if (reportLegers.Any())
                        {
                            currentAccount = reportLegers.First().Account;
                            PutAccountRow(worksheet, currentAccount, currentRow);
                            currentRow++; // Move to the next row to write transaction data
                            startRow = currentRow; // Update the start row for the first account
                        }

                        foreach (var item in reportLegers)
                        {
                            // Check if the account has changed
                            if (currentAccount != null && currentAccount != item.Account)
                            {
                                // Insert the totals for the previous account
                                worksheet.Cell(currentRow, 1).Value = "Total";
                                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.DarkPastelBlue;
                                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                                worksheet.Cell(currentRow, 2).FormulaA1 = $"=SUM(B{startRow}:B{currentRow - 1})";
                                worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.DarkPastelBlue;
                                worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.DarkPastelBlue;
                                worksheet.Cell(currentRow, 1).Style.Font.FontColor = XLColor.Black;


                                currentRow++; // Move to the next row for the new account header

                                // Write the new account header in AirForceBlue
                                PutAccountRow(worksheet, item.Account, currentRow);
                                currentRow++; // Move to the next row to write transaction data
                                startRow = currentRow; // Reset the start row for the new account
                            }

                            // Update the current account and add the row data
                            currentAccount = item.Account;
                            PutCurrentRowData(worksheet, currentRow, item);
                            totalDebit += item.TotalFD;

                            currentRow++;
                        }

                        // Insert the final totals for the last account
                        worksheet.Cell(currentRow, 1).Value = "Total";
                        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                        worksheet.Cell(currentRow, 2).FormulaA1 = $"=SUM(B{startRow}:B{currentRow - 1})";
                        worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.DarkPastelBlue;
                        worksheet.Cell(currentRow, 2).Style.Fill.BackgroundColor = XLColor.DarkPastelBlue;
                        worksheet.Cell(currentRow, 1).Style.Font.FontColor = XLColor.Black;
                        worksheet.Cell(currentRow, 2).Style.Font.FontColor = XLColor.Black;


                        // Adjust column widths to fit content (if you need it dynamic after the fixed width)
                        worksheet.Columns().AdjustToContents();
                        worksheet.Column(1).Width = 32;

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

        private static void PutCurrentRowData(IXLWorksheet worksheet, int currentRow, QrySubledgerFDBalance item)
        {
            worksheet.Cell(currentRow, 1).Value = item.Subaccount;
            worksheet.Cell(currentRow, 2).Value = item.TotalFD;

            // Apply number formatting for the Amount column
            worksheet.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
        }

        private static void PutAccountRow(IXLWorksheet worksheet, string account, int currentRow)
        {
            worksheet.Cell(currentRow, 1).Value = account;
            var headerAccount = worksheet.Range($"A{currentRow}:B{currentRow}");
            headerAccount.Merge();
            headerAccount.Style.Font.Bold = true;
            headerAccount.Style.Fill.BackgroundColor = XLColor.DarkMidnightBlue;
            headerAccount.Style.Font.FontColor = XLColor.AliceBlue;
            headerAccount.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        }
    }
}
