using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using LedgerLensMaking.Models.Data;
using LedgerLensMaking.Models.UIModels;
using LedgerLensMaking.UtilityClasses;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace LedgerLensMaking.Models.ViewModels
{
    public class TurboSharePurchaseViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand ImportCommand { get; }
        public ICommand DeleteRowCommand {  get; }
        public ICommand FinalImportCommand { get; }
        //public ObservableCollection<ExcelShareImport> ExcelRows { get; set; } // Holds all rows from Excel
        public List<Chart> AllAccounts { get; set; }
        public List<SharesChart> AllShares { get; set; }

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
        private bool _isGridEnabled = false;
        public bool IsGridEnabled
        {
            get => _isGridEnabled;
            set
            {
                _isGridEnabled = value;
                OnPropertyChanged();
            }
        }
        private bool _isFinalImportEnabled = false;
        public bool IsFinalImportEnabled
        {
            get => _isFinalImportEnabled;
            set
            {
                _isFinalImportEnabled = value;
                OnPropertyChanged();
            }
        }
        private string _dateInput;
        public string DateInput
        {
            get => _dateInput;
            set
            {
                _dateInput = value;
                OnPropertyChanged();
            }
        }

        private string _refInput;
        public string RefInput
        {
            get => _refInput;
            set
            {
                _refInput = value;
                OnPropertyChanged();
            }
        }

        private string _narrationInput;
        public string NarrationInput
        {
            get => _narrationInput;
            set
            {
                _narrationInput = value;
                OnPropertyChanged();
            }
        }

        private ExcelShareImport _excelEntry;
        public ExcelShareImport SelectedExcelEntry
        {
            get => _excelEntry;
            set
            {
                _excelEntry = value;
                OnPropertyChanged(); // May not be required not planning to 
            }
        }
        private ObservableCollection<ExcelShareImport> _excelRows;
        public ObservableCollection<ExcelShareImport> ExcelRows
        {
            get => _excelRows;
            set
            {
                _excelRows = value;
                OnPropertyChanged(); // Notify the UI about the change
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public TurboSharePurchaseViewModel()
        {
            IndividualName = GlobalVariables.IndividualName + "'s Share Purchase Import";
            PeriodLine = "For the Year  Ended " + GlobalVariables.SelectedStartDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture) + " to  " + GlobalVariables.SelectedEndDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
            PrintDate = "Imported on " + DateTime.Today.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
            ImportCommand = new RelayCommand(ImportClicked);
            FinalImportCommand = new RelayCommand(FinalImportClicked);
            DeleteRowCommand = new RelayCommand<ExcelShareImport>(DeleteRow);
            IsFinalImportEnabled=true;
            ExcelRows = new ObservableCollection<ExcelShareImport>();
            LoadAccounts();// This will load the Charts and Shares in a List
        }
        private void FinalImportClicked()
        {
            CheckForErrors();
            if (IsFinalImportEnabled)
            {
                MessageBox.Show("Importing!");
                PerformInserts();
                MessageBox.Show("Import Completed!");
            }
            else
            {
                MessageBox.Show("Please fix the errors and resume.");
            }
        }
        private void CheckForErrors()
        {
            IsFinalImportEnabled = true;

            if (string.IsNullOrWhiteSpace(DateInput))
            {
                IsFinalImportEnabled =false;
                MessageBox.Show("Please enter a valid Date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (!DateTime.TryParse(DateInput, out DateTime parsedDate))
            {
                IsFinalImportEnabled = false;
                MessageBox.Show("Please enter a valid date format for Date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (parsedDate < GlobalVariables.SelectedStartDate || parsedDate > GlobalVariables.SelectedEndDate)
            {
                IsFinalImportEnabled = false;
                MessageBox.Show($"The date must be between {GlobalVariables.SelectedStartDate:dd-MMM-yyyy} and {GlobalVariables.SelectedEndDate:dd-MMM-yyyy}.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (string.IsNullOrWhiteSpace(RefInput))
            {
                IsFinalImportEnabled = false;
                MessageBox.Show("Please enter a Reference.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }


            if (string.IsNullOrWhiteSpace(NarrationInput))
            {
                IsFinalImportEnabled = false;
                MessageBox.Show("Please enter a Narration.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if(IsFinalImportEnabled==false)
                return;



            foreach (var row in ExcelRows)
            {
                if (row.ErrorMessage != null)
                {
                    IsFinalImportEnabled = false;
                }
            }
        }
        private void DeleteRow(ExcelShareImport excelShareImport)
        {
            ExcelRows.Remove(excelShareImport); // Remove the selected row
            OnPropertyChanged(nameof(ExcelRows)); // Notify the UI of the update
            MessageBox.Show($"Delete Clicked {excelShareImport.Company} ");
        }
        private void LoadAccounts()
        {
            AllAccounts = new List<Chart>();
            AllAccounts = DatabaseHelper.GetCharts();
            AllShares = new List<SharesChart>();
            AllShares = DatabaseHelper.GetSharesChart();
            IsGridEnabled = true;
           

        }
        private void ImportClicked()
        {
            IsGridEnabled = true;
            ExcelRows.Clear();
            OnPropertyChanged(nameof(ExcelRows));

            var dialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx",
                Title = "Select an Excel File"
            };

            if (dialog.ShowDialog() == true)
            {
                var path = dialog.FileName;
                IsFinalImportEnabled = true; // Reset to true before checking for errors
                OnPropertyChanged(nameof(IsFinalImportEnabled));

                using (var workbook = new XLWorkbook(path))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed();

                    foreach (var row in rows.Skip(1)) // Skipping header row
                    {
                        var importRow = new ExcelShareImport
                        {
                            GLAccountForCredit = row.Cell(1).Value.ToString(),
                            GLSharesAccountForDebit = row.Cell(2).Value.ToString(),
                            Company = row.Cell(3).Value.ToString(),
                            TradeDate = DateTime.Parse(row.Cell(4).Value.ToString()),
                            Qty = decimal.Parse(row.Cell(5).Value.ToString()),
                            Rate = decimal.Parse(row.Cell(6).Value.ToString()),
                            Amt = decimal.Parse(row.Cell(5).Value.ToString()) * decimal.Parse(row.Cell(6).Value.ToString()),
                            ErrorMessage=null,
                        };

                        ExcelRows.Add(importRow);
                    }
                }

                // Iterate through the collection to populate the codes and detect errors
                foreach (var importRow in ExcelRows)
                {
                    var matchingGLAccount = AllAccounts.FirstOrDefault(a => a.Account == importRow.GLAccountForCredit);
                    if (matchingGLAccount != null)
                    {
                        importRow.GLAccountCodeForCredit = matchingGLAccount.AccountId;
                    }
                    else
                    {
                        importRow.ErrorMessage = "GL Account not found.";
                        IsFinalImportEnabled = false;
                        OnPropertyChanged(nameof(IsFinalImportEnabled)); // Notify UI immediately
                    }

                    var matchingGLAccountDr = AllAccounts.FirstOrDefault(a => a.Account == importRow.GLSharesAccountForDebit);
                    if (matchingGLAccountDr != null)
                    {
                        importRow.GLSharesAccountCodeForDebit = matchingGLAccountDr.AccountId;
                    }
                    else
                    {
                        importRow.ErrorMessage = "GL Account For Debit Not Found.";
                        IsFinalImportEnabled = false;
                        OnPropertyChanged(nameof(IsFinalImportEnabled));
                    }

                    var matchingShares = AllShares.FirstOrDefault(s =>
                        s.Company == importRow.Company && s.AccountId == importRow.GLSharesAccountCodeForDebit);

                    if (matchingShares != null)
                    {
                        importRow.ShareId = matchingShares.ShareId;
                    }
                    else
                    {
                        importRow.ErrorMessage = "Shares account not found or does not belong to the provided GL account.";
                        IsFinalImportEnabled = false;
                        OnPropertyChanged(nameof(IsFinalImportEnabled));
                    }
                }

                // Notify the UI that the import rows and final import status have been updated
                OnPropertyChanged(nameof(IsFinalImportEnabled));
                OnPropertyChanged(nameof(ExcelRows));
            }
        }
        private void PerformInserts()
        {
            foreach (var row in ExcelRows)
            {
                BankEntryHelper bankEntryHelper = new BankEntryHelper();
                bankEntryHelper.BankCode = row.GLAccountCodeForCredit;
                bankEntryHelper.BankDescription = row.GLAccountForCredit;
                bankEntryHelper.TranactionChartCode = row.GLSharesAccountCodeForDebit;
                bankEntryHelper.YearId = GlobalVariables.SelectedYearId;
                bankEntryHelper.TransactionCodeDescription = row.GLSharesAccountForDebit;
                bankEntryHelper.TypeOfTransaction1Recepit2Payment = 2; // CAREFUL CHANGE THIS ACCORDINGLY
                bankEntryHelper.Reference = RefInput;
                bankEntryHelper.Narration = NarrationInput;
                bankEntryHelper.Amount = row.Amt;
                bankEntryHelper.TransactionType = "BE"; // OR CE
                bankEntryHelper.TDate = Convert.ToDateTime(row.TradeDate); // Original was Convert.ToDate(DateInput) 
                bankEntryHelper.SubledgerTranactionsNo0Fd1Shares2 = 2;
                bankEntryHelper.SubledgerCode = row.ShareId;
                bankEntryHelper.SubledgerCodeDescription = row.Company;
                bankEntryHelper.QtyOfShares = row.Qty;
                bankEntryHelper.TDateForSharePurchase = row.TradeDate;
                bankEntryHelper.ShareBuyingPrice = row.Rate;
                bankEntryHelper.BankEntryHelperStart();
            }
        }





    }
}
