using LedgerLensMaking.VewModels;
using LedgerLensMaking.Models.Data;
using LedgerLennMaking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LedgerLensMaking.UtilityClasses;
using LedgerLensMaking.UserControls;
using LedgerLens.Models.ViewModels;
using LedgerLensMaking.Models.ViewModels;

namespace LedgerLensMaking
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainWindowViewModel();
            DataContext = viewModel;
        }

        private void MenuItem_Selected(object sender, RoutedEventArgs e)
        {
            var selectedMenuItem = sender as TreeViewItem;
            if (selectedMenuItem != null)
            {
                switch (selectedMenuItem.Header.ToString())
                {
                    case "Select Individual":
                        var dialog = new Microsoft.Win32.OpenFileDialog
                        {
                            FileName = "Document",
                            DefaultExt = ".accdb",
                            Filter = "Access Database File (.accdb)|*.accdb"
                        };

                        bool? result = dialog.ShowDialog();

                        if (result == true)
                        {
                            string filename = dialog.FileName;
                            GlobalVariables.DatabaseFileName = filename;

                            var viewModel = new MainWindowViewModel();
                            DataContext = viewModel;

                            IndividualDetails individualDetails = new IndividualDetails();
                            Individual individual = individualDetails.GetIndividual();
                            GlobalVariables.PictureFileName = "Assets/" + individual.PhotoFile;
                            GlobalVariables.PAN = individual.PANNumber;
                            GlobalVariables.IndividualName = individual.IndividualName;
                            GlobalVariables.InterestAccountCode = individual.InterestAccountCode;
                            GlobalVariables.InterestAccountDesc = individual.InterestAccountDesc;
                            GlobalVariables.LTCGAccountCode = individual.LTCGAccountCode;
                            GlobalVariables.LTCGAccountDesc = individual.LTCGAccountDesc;
                            GlobalVariables.STCGAccountCode = individual.STCGAccountCode;
                            GlobalVariables.STCGAccountDesc = individual.STCGAccountDesc;
                            GlobalVariables.LTCLAccountCode = individual.LTCLAccountCode;
                            GlobalVariables.LTCLAccountDesc = individual.LTCLAccountDesc;
                            GlobalVariables.STCLAccountCode = individual.STCLAccountCode;
                            GlobalVariables.STCLAccountDesc = individual.STCLAccountDesc;
                            GlobalVariables.RetainedEarningsId = individual.RetainedEarningsId;

                            // Update the ViewModel
                            viewModel.PhotoFile = GlobalVariables.PictureFileName;
                            viewModel.PAN = GlobalVariables.PAN;

                            // Load the SelectYearView
                            var selectYearView = new SelectYearView
                            {
                                DataContext = new SelectYearViewModel()
                            };
                            ContentArea.Content = selectYearView;
                            
                            selectYearView.DataContext = new SelectYearViewModel();
                            ContentArea.Content = selectYearView;
                        }
                        break;


                    case "Close Books":
                        ContentArea.Content = null;
                        if (CheckYear() == false)
                        {
                            MessageBox.Show("Selected Year is not Current");
                            return;
                        }
                        var closeBooks = new CloseBooksUserControl()
                        {
                            DataContext = new CloseBooksViewModel()
                        };
                        ContentArea.Content = closeBooks;
                        break;

                    case "Ledger Accounts":
                        ContentArea.Content = null;
                        if (CheckYear() == false)
                        {
                            MessageBox.Show("Selected Year is not Current");
                            return;
                        }


                        // Set DataContext only here
                        var ledgerAccountsUserControl = new LedgerAccountUserControl
                        {
                            DataContext = new LedgerAccountViewModel() // Set the DataContext only here
                        };

                        ContentArea.Content = ledgerAccountsUserControl;

                        break;


                    case "Companies":
                        ContentArea.Content = null;
                        if (CheckYear() == false)
                        {
                            MessageBox.Show("Selected Year is not Current");
                            return;
                        }

                        var shareschartUserControl = new SharesChartUserControl
                        {
                            DataContext = new SharesChartViewModel() // Set the DataContext only here
                        };

                        ContentArea.Content =shareschartUserControl;

                        break;
                    case "Subledger":
                        ContentArea.Content = null;
                        if (CheckYear() == false)
                        {
                            MessageBox.Show("Selected Year is not Current");
                            return;
                        }

                        var subledgerChartUserControl = new SubLedgerChartUserControl
                        {
                            DataContext = new SubLedgerChartViewModel() // Set the DataContext only here
                        };

                        ContentArea.Content = subledgerChartUserControl;
                        break;

                    case "Receipts":
                        ContentArea.Content = null;
                        if (CheckYear() == false)
                        {
                            MessageBox.Show("Selected Year is not Current");
                            return;
                        }

                        var bankreceiptsUserControl = new BankReceiptsUserControl
                        {
                            DataContext = new BankReceiptViewModel()
                        };
                        ContentArea.Content= bankreceiptsUserControl;
                        break;
                        
                    case "Payments":
                        ContentArea.Content = null;
                        if (CheckYear() == false)
                        {
                            MessageBox.Show("Selected Year is not Current");
                            return;
                        }

                        var bankpaymentsUserControl = new BankPaymentsUserControl
                        {
                            DataContext = new BankPaymentViewModel()
                        };
                        ContentArea.Content = bankpaymentsUserControl;
                        break;


                    case "Journal":
                        ContentArea.Content = null;
                        if (CheckYear() == false)
                        {
                            MessageBox.Show("Selected Year is not Current");
                            return;
                        }

                        var journalEntryUserControl = new JournalEntryUserControl
                        {
                            DataContext= new JournalEntryViewModel()
                        };
                        ContentArea.Content = journalEntryUserControl;
                        break;
                    case "Share Purchase":
                        ContentArea.Content = null;
                        if (CheckYear() == false)
                        {
                            MessageBox.Show("Selected Year is not Current");
                            return;
                        }

                        var sharejournalUserControl = new ShareJournalUserControl
                        {
                            DataContext = new ShareJournalViewModel()
                        };
                        ContentArea.Content = sharejournalUserControl;
                        break;
                    case "Share Sales":
                        if (CheckYear() == false)
                        {
                            MessageBox.Show("Selected Year is not Current");
                            return;
                        }

                        ContentArea.Content = null;
                        var salessharejournalUserControl = new SalesShareJournalUserControl
                        {
                            DataContext = new SalesShareJournalViewModel()
                        };
                        ContentArea.Content = salessharejournalUserControl;
                        break;
                    case "Subledger Journal":
                        ContentArea.Content = null;
                        if (CheckYear() == false)
                        {
                            MessageBox.Show("Selected Year is not Current");
                            return;
                        }

                        var subledgerJournalUserControl = new SubledgerJournalUserControl
                        {
                            DataContext = new SubledgerJournalViewModel()

                        };
                        ContentArea.Content= subledgerJournalUserControl;
                        break;
                    case "Import Excel":
                        ContentArea.Content = null;
                        if (CheckYear() == false)
                        {
                            MessageBox.Show("Selected Year is not Current");
                            return;
                        }

                        var turboShares = new SharePurchaseImportUserControl
                        {
                            DataContext = new TurboSharePurchaseViewModel() // Assigning DataContext here
                        };
                        ContentArea.Content = turboShares;
                        break;
                    case "Trial Balance":
                        ContentArea.Content = null;
                        var reportsTrailBalanceControl = new ReportsTrialBalanceUserControl
                        {
                            DataContext = new ReportsTrialBalanceViewModel()
                        };
                        ContentArea.Content= reportsTrailBalanceControl;
                        break;
                    case "Report Ledger":
                        ContentArea.Content=null;
                        var reportLedger = new ReportLedgerUserControl
                        {
                            DataContext = new ReportLedgerViewModel()
                        };
                        ContentArea.Content = reportLedger;
                        break;
                    case "General Ledger":
                        ContentArea.Content = null;
                        var reportGL = new ReportGeneralLedgerUserControl
                        {
                            DataContext = new ReportGeneralLedgerViewModel()
                        };
                        ContentArea.Content = reportGL;
                        break;
                    case "Shares at Cost":
                        ContentArea.Content = null;
                        var shareCost = new ReportSharesAtCostUserControl()
                        {
                            DataContext = new ReportSharesAtCostViewModel()
                        };
                        ContentArea.Content = shareCost;
                        break;
                    case "Share Balance":
                        ContentArea.Content = null;
                        var shareCostBalance = new ReportShareBalanceUserControl()
                        {
                            DataContext = new ReportShareBalanceViewModel()
                        };
                        ContentArea.Content = shareCostBalance;
                        break;
                    case "Capital Gains":
                        ContentArea.Content = null  ;
                        var shareGain = new ReportCapitalGainStatementUserControl()
                        {
                            DataContext = new ReportCapitalGainStatmentViewModel()
                        };
                        ContentArea.Content = shareGain;
                        break;
                    case "Subledger TB":
                        ContentArea.Content = null;
                        var subLedger = new ReportSubledgerTrialBalanceUserControl()
                        {
                            DataContext = new ReportSubledgerTrialBalanceViewModel()
                        };
                        ContentArea.Content = subLedger;
                        break;

                }
            }
        }
        private bool CheckYear()
        {
            bool entriesPossible=true;
            if(GlobalVariables.MaxYearId==GlobalVariables.SelectedYearId)
                entriesPossible=true ;
            else
                entriesPossible=false ;

            return entriesPossible;

        }
    }
}
