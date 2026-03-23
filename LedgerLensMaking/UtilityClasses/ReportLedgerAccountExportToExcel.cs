using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
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
    public static class ReporLedgersAccountExportToExcel
    {
        public static void ExportAccountToExcel(List<ReportLegerModel> reportLegers, string IndividualLine, string PeriodLine, string PrintDate)
        {
            try
            {
                // Prompt the user to choose a location to save the file
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    Title = "Save Ledger Account",
                    FileName = GlobalVariables.IndividualName+" LedgerAccount.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    using (var workbook = new XLWorkbook())
                    {
                        // Create a worksheet
                        var worksheet = workbook.Worksheets.Add("Ledger Account");

                        worksheet.Cell(1, 1).Value = IndividualLine;
                        worksheet.Cell(2, 1).Value = PeriodLine;
                        worksheet.Cell(3, 1).Value = PrintDate;


                        // Center the title and style it
                        var headerRangeTitle = worksheet.Range("A1:F1");
                        headerRangeTitle.Merge();
                        headerRangeTitle.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                        headerRangeTitle.Style.Font.FontColor = XLColor.Black;
                        headerRangeTitle.Style.Font.Bold = true;
                        headerRangeTitle.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRangeTitle.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        var headerRangePeriod = worksheet.Range("A2:F2");
                        headerRangePeriod.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRangePeriod.Merge();
                        headerRangePeriod.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                        headerRangePeriod.Style.Font.FontColor = XLColor.Black;
                        headerRangePeriod.Style.Font.Bold = true;


                        var headerRangePrint = worksheet.Range("A3:F3");
                        headerRangePrint.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRangePrint.Merge();
                        headerRangePrint.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                        headerRangePrint.Style.Font.FontColor = XLColor.Black;
                        headerRangePrint.Style.Font.Bold = true;

                        // Style the headers
                        // Check if the collection is not empty before accessing the first element
                        if (reportLegers != null && reportLegers.Any())
                        {
                            worksheet.Cell(4, 1).Value = reportLegers.First().Account;
                        }
                        else
                        {
                            worksheet.Cell(4, 1).Value = "No data available"; // Fallback if the list is empty
                        }

                        var headerRange1 = worksheet.Range("A4:F4");
                        headerRange1.Merge();
                        headerRange1.Style.Font.Bold = true;
                        headerRange1.Style.Fill.BackgroundColor = XLColor.LightGray;
                        headerRange1.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;



                        // Set the headers
                        worksheet.Cell(5, 1).Value = "Date";
                        worksheet.Cell(5, 2).Value = "Ref";
                        worksheet.Cell(5, 3).Value = "Particulars";
                        worksheet.Cell(5, 4).Value = "Debit";
                        worksheet.Cell(5, 5).Value = "Credit";
                        worksheet.Cell(5, 6).Value = "Running Balance";


                        // Insert data
                        int currentRow = 6;
                        foreach (var item in reportLegers)
                        {
                            worksheet.Cell(currentRow, 1).Value = item.TDate.ToString("dd-MMM-yyyy");
                            worksheet.Cell(currentRow, 2).Value = item.Ref;
                            worksheet.Cell(currentRow, 3).Value = item.Particulars;
                            worksheet.Cell(currentRow, 4).Value = item.DebitColumn;
                            worksheet.Cell(currentRow, 5).Value = item.CreditColumn;
                            worksheet.Cell(currentRow, 6).Value = item.RunningBalance;

                            // Add narration as a comment
                            if (!string.IsNullOrEmpty(item.Narration))
                            {
                                var cellWithComment = worksheet.Cell(currentRow, 3);
                                var comment = cellWithComment.CreateComment(); // Create a comment
                                comment.AddText(item.Narration); // Add the narration as comment text
                                comment.Style.Alignment.SetAutomaticSize();
                            }

                            // Apply number formatting for debit and credit columns
                            worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0.00";
                            worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";
                            worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0.00";

                            currentRow++;
                        }

                        // Calculate totals
                        worksheet.Cell(currentRow, 1).Value = "Total";
                        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                        worksheet.Cell(currentRow, 4).FormulaA1 = $"=SUM(D6:D{currentRow - 1})";
                        worksheet.Cell(currentRow, 5).FormulaA1 = $"=SUM(E6:E{currentRow - 1})";
                        worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";

                        // Style the totals row
                        var totalsRange = worksheet.Range(currentRow, 1, currentRow, 5);
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
        public static void ExportAllAccountsToExcel(List<ReportLegerModel> reportLegers, string IndividualLine, string PeriodLine, string PrintDate)
        {
            try
            {
                // Prompt the user to choose a location to save the file
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                    Title = "Save Ledger Account",
                    FileName = GlobalVariables.IndividualName+ " GL.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    using (var workbook = new XLWorkbook())
                    {
                        // Create a worksheet
                        var worksheet = workbook.Worksheets.Add("Ledger Account");

                        worksheet.Cell(1, 1).Value = IndividualLine;
                        worksheet.Cell(2, 1).Value = PeriodLine;
                        worksheet.Cell(3, 1).Value = PrintDate;

                        // Center the title and style it
                        var headerRangeTitle = worksheet.Range("A1:F1");
                        headerRangeTitle.Merge();
                        headerRangeTitle.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                        headerRangeTitle.Style.Font.FontColor = XLColor.Black;
                        headerRangeTitle.Style.Font.Bold = true;
                        headerRangeTitle.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRangeTitle.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        var headerRangePeriod = worksheet.Range("A2:F2");
                        headerRangePeriod.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRangePeriod.Merge();
                        headerRangePeriod.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                        headerRangePeriod.Style.Font.FontColor = XLColor.Black;
                        headerRangePeriod.Style.Font.Bold = true;

                        var headerRangePrint = worksheet.Range("A3:F3");
                        headerRangePrint.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRangePrint.Merge();
                        headerRangePrint.Style.Fill.BackgroundColor = XLColor.FloralWhite;
                        headerRangePrint.Style.Font.FontColor = XLColor.Black;
                        headerRangePrint.Style.Font.Bold = true;

                        // Insert headers for columns
                        worksheet.Cell(5, 1).Value = "Date";
                        worksheet.Cell(5, 2).Value = "Ref";
                        worksheet.Cell(5, 3).Value = "Particulars";
                        worksheet.Cell(5, 4).Value = "Debit";
                        worksheet.Cell(5, 5).Value = "Credit";
                        worksheet.Cell(5, 6).Value = "Running Balance";

                        var headerRange = worksheet.Range("A5:F5");
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // Variables for tracking the running totals
                        decimal totalDebit = 0;
                        decimal totalCredit = 0;
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
                                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                                worksheet.Cell(currentRow, 4).FormulaA1 = $"=SUM(D{startRow}:D{currentRow - 1})";
                                worksheet.Cell(currentRow, 5).FormulaA1 = $"=SUM(E{startRow}:E{currentRow - 1})";
                                worksheet.Range(currentRow, 1, currentRow, 6).Style.Fill.BackgroundColor = XLColor.DarkPastelBlue;
                                worksheet.Range(currentRow, 1, currentRow, 6).Style.Font.FontColor = XLColor.Black;

                                worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0.00";
                                worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";


                                currentRow++; // Move to the next row for the new account header

                                // Write the new account header in AirForceBlue
                                PutAccountRow(worksheet, item.Account, currentRow);
                                currentRow++; // Move to the next row to write transaction data
                                startRow = currentRow; // Reset the start row for the new account
                            }

                            // Update the current account and add the row data
                            currentAccount = item.Account;
                            PutCurrentRowData(worksheet, currentRow, item);
                            totalDebit += item.DebitColumn;
                            totalCredit += item.CreditColumn;

                            currentRow++;
                        }

                        // Insert the final totals for the last account
                        worksheet.Cell(currentRow, 1).Value = "Total";
                        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                        worksheet.Cell(currentRow, 4).FormulaA1 = $"=SUM(D{startRow}:D{currentRow - 1})";
                        worksheet.Cell(currentRow, 5).FormulaA1 = $"=SUM(E{startRow}:E{currentRow - 1})";
                        worksheet.Range(currentRow, 1, currentRow, 6).Style.Fill.BackgroundColor = XLColor.DarkPastelBlue;
                        worksheet.Range(currentRow, 1, currentRow, 6).Style.Font.FontColor = XLColor.Black;
                        worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0.00";
                        worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";


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

        private static void PutCurrentRowData(IXLWorksheet worksheet, int currentRow, ReportLegerModel item)
        {
            worksheet.Cell(currentRow, 1).Value = item.TDate.ToString("dd-MMM-yyyy");
            worksheet.Cell(currentRow, 2).Value = item.Ref;
            worksheet.Cell(currentRow, 3).Value = item.Particulars;
            worksheet.Cell(currentRow, 4).Value = item.DebitColumn;
            worksheet.Cell(currentRow, 5).Value = item.CreditColumn;
            worksheet.Cell(currentRow, 6).Value = item.RunningBalance;

            // Add narration as a comment
            if (!string.IsNullOrEmpty(item.Narration))
            {
                var cellWithComment = worksheet.Cell(currentRow, 3);
                var comment = cellWithComment.CreateComment(); // Create a comment
                comment.AddText(item.Narration); // Add the narration as comment text
                comment.Style.Alignment.SetAutomaticSize();
            }

            // Apply number formatting for debit and credit columns
            worksheet.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(currentRow, 5).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(currentRow, 6).Style.NumberFormat.Format = "#,##0.00";
        }

        private static void PutAccountRow(IXLWorksheet worksheet, string account, int currentRow)
        {
            worksheet.Cell(currentRow, 1).Value = account;
            var headerAccount = worksheet.Range($"A{currentRow}:F{currentRow}");
            headerAccount.Merge();
            headerAccount.Style.Font.Bold = true;
            headerAccount.Style.Fill.BackgroundColor = XLColor.DarkMidnightBlue;
            headerAccount.Style.Font.FontColor = XLColor.AliceBlue;
            headerAccount.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        }


    }
}
