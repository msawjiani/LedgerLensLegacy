using LedgerLensMaking.Models.Data;
using LedgerLensMaking.Models.UIModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using LedgerLensMaking.UtilityClasses;
using LedgerLensMaking.Windows;
using System.Linq;

namespace LedgerLensMaking.Models.ViewModels
{
    public class BankReceiptSharesViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand SfCommand { get; }
        public ICommand AddNew { get; }
        public ICommand CloseSharesCommand { get; }
        private bool _isSfEnabled = false;

        private readonly Action<SelectedShareDetails> _onShareSelectedCallback;

        private string _selectedGlAccount;
        public string SelectedGlAccount
        {
            get => _selectedGlAccount;
            set
            {
                _selectedGlAccount = value;
                OnPropertyChanged();
            }
        }
        private List<QryShareBalanceNett> _qryShareBalances;
        public List<QryShareBalanceNett> QryShareBalancesNetts
        {
            get => _qryShareBalances;
            set
            {
                _qryShareBalances = value;
                OnPropertyChanged();
            }
        }
        private QryShareBalanceNett _selectedShare;
        public QryShareBalanceNett SelectedShare
        {
            get => _selectedShare;
            set
            {
                _selectedShare = value;
                OnPropertyChanged();
            }
        }

        public bool IsSfEnabled
        {
            get => _isSfEnabled;
            set
            {
                _isSfEnabled = value;
                OnPropertyChanged();
            }
        }
        private bool _isGridEnabled;

        public bool IsGridEnabled
        {
            get => _isGridEnabled;
            set
            {
                _isGridEnabled = value;
                OnPropertyChanged();
            }
        }
        private int _glAccountId;
        public int GlAccountId
        {
            get => _glAccountId;
            set
            {
                _glAccountId = value;
                OnPropertyChanged();
            }
        }

        private string _glAccountName;
        public string GlAccountName
        {
            get => _glAccountName;
            set
            {
                _glAccountName = value;
                OnPropertyChanged();
            }
        }
        private string _qtyInput;
        public string QtyInput
        {
            get => _qtyInput;
            set
            {
                _qtyInput = value;
                OnPropertyChanged();
            }
        }
        private string _rateInput;
        public string RateInput
        {
            get => _rateInput;
            set
            {
                _rateInput = value;
                OnPropertyChanged();
            }
        }
        public BankReceiptSharesViewModel(int glAccountId, string glAccountName, Action<SelectedShareDetails> onShareSelectedCallback)
        {
            GlAccountId = glAccountId;
            GlAccountName = glAccountName;
            _onShareSelectedCallback = onShareSelectedCallback;
            LoadSharesBalances();
            IsGridEnabled = true;
            CloseSharesCommand = new RelayCommand(OnCloseShare);
            AddNew = new RelayCommand(OnAddNewClicked);

        }
        private void OnShareSelectedReceipt(QryShareBalanceNett selectedShare)
        {
            var selectedShareDetails = new SelectedShareDetails
            {
                SelectedShare = selectedShare,
                Quantity = decimal.Parse(QtyInput),
                Rate = decimal.Parse(RateInput)
            };

            // Invoke the callback and pass the selected share details
            _onShareSelectedCallback?.Invoke(selectedShareDetails);
             
        }
        private void OnCloseShare()
        {
            if (SelectedShare != null)
            {
                // Call OnShareSelected to pass the SelectedShareDetails object


                // Close the window if form inputs are valid
                if (FormInputsAreValid())
                {
                    OnShareSelectedReceipt(SelectedShare);
                    CloseWindow(Application.Current.Windows.OfType<BankReceiptsSharesWindow>().FirstOrDefault()); // Close the specific window
                }
            }
            else
            {
                // Show confirmation dialog if no account is selected
                MessageBoxResult result = MessageBox.Show("You have not selected an Account. Do you want to close?", "Not Selected", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // User confirmed closing without selection
                    CloseWindow(Application.Current.Windows.OfType<BankReceiptsSharesWindow>().FirstOrDefault()); // Close the specific window
                }
            }


        }

        private bool FormInputsAreValid()
        {
            bool hasErrors = false;

            // Validate Quantity
            if (string.IsNullOrWhiteSpace(QtyInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a valid Qty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                // Check if the input is a valid integer
                if (decimal.TryParse(QtyInput, out decimal qtyInput))
                {
                    if (qtyInput > SelectedShare.TS)
                    {
                        hasErrors = true;
                        MessageBox.Show("Qty cannot exceed total quantity.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    hasErrors = true;
                    MessageBox.Show("Please enter a valid numeric value for Qty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Validate Rate
            if (string.IsNullOrWhiteSpace(RateInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a valid Rate.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                // Check if the rate input is a valid decimal
                if (!decimal.TryParse(RateInput, out decimal rate))
                {
                    hasErrors = true;
                    MessageBox.Show("Please enter a valid numeric value for Rate.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            return !hasErrors;
        }


        private void CloseWindow(Window window)
        {
            if (window != null)
            {
                window.Close();
            }

        }
        private void OnAddNewClicked()
        {

        }
        private bool CheckForFormErrors()
        {
            
            bool hasErrors = false;
            if (SelectedShare == null)
            {
                hasErrors = true;
                MessageBox.Show("Please select a Share from the Grid.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (string.IsNullOrWhiteSpace(QtyInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter Quantity.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (string.IsNullOrWhiteSpace(RateInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a Rate.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }





            return hasErrors;
        }

        private void LoadSharesBalances()
        {
            // Fetch the list of bank accounts from the database

            QryShareBalancesNetts = DatabaseHelper.GetShareBalances(GlAccountId);
            IsGridEnabled = QryShareBalancesNetts != null && QryShareBalancesNetts.Count > 0;
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
