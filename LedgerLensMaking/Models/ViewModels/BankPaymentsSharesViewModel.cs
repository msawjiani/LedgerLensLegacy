using LedgerLensMaking.Models.Data;
using LedgerLensMaking.Models.UIModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using LedgerLensMaking.UtilityClasses;
using LedgerLensMaking.Windows;
using System.Linq;

namespace LedgerLensMaking.Models.ViewModels
{
    public class BankPaymentsSharesViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand SfCommand { get; }
        public ICommand AddNew { get; }
        public ICommand CloseSharesCommand { get; }
        private bool _isSfEnabled = false;

        private readonly Action<PaymentsSelectedShareDetails> _onShareSelectedCallback;

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

        // Change to QryShareBalanceNett
        private List<SharesChart> _sharesCharts;
        public List<SharesChart> SharesCharts
        {
            get => _sharesCharts;
            set
            {
                _sharesCharts = value;
                OnPropertyChanged();
            }
        }

        // Change to QryShareBalanceNett
        private SharesChart _selectedShare;
        public SharesChart SelectedShare
        {
            get => _selectedShare;
            set
            {
                _selectedShare = value;
                OnPropertyChanged();
            }
        }

        private PaymentsSelectedShareDetails _selectedShareDetails;
        public PaymentsSelectedShareDetails SelectedShareDetails
        {
            get => _selectedShareDetails;
            set
            {
                _selectedShareDetails = value;
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

        public BankPaymentsSharesViewModel(int glAccountId, string glAccountName, Action<PaymentsSelectedShareDetails> onShareSelectedCallback)
        {
            GlAccountId = glAccountId;
            GlAccountName = glAccountName;
            _onShareSelectedCallback = onShareSelectedCallback;
            LoadSharesBalances();
            CloseSharesCommand = new RelayCommand(OnCloseShare);
            AddNew = new RelayCommand(OnAddNewClicked);
        }

        private void OnShareSelected(SharesChart selectedShare)
        {
            var selectedShareDetails = new PaymentsSelectedShareDetails
            {
                SelectedShare = selectedShare,  // Assuming you update SelectedShare to QryShareBalanceNett
                Quantity = decimal.Parse(QtyInput),
                Rate = decimal.Parse(RateInput)
            };

            _onShareSelectedCallback?.Invoke(selectedShareDetails);
        }

        private void OnCloseShare()
        {
            // Check if form inputs are valid before processing the share selection
  
            if (SelectedShare != null)
            {
                // If inputs are valid, proceed with share selection
                
                if (!FormInputsAreValid())
                {
                    // Inputs are not valid, so don't proceed further
                    return;
                }
                OnShareSelected(SelectedShare);
                CloseWindow(Application.Current.Windows.OfType<BankPaymentSharesWindow>().FirstOrDefault());
            }
            else
            {
                // Handle the case where no share is selected
                MessageBoxResult result = MessageBox.Show("You have not selected an Account. Do you want to close?", "Not Selected", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    CloseWindow(Application.Current.Windows.OfType<BankPaymentSharesWindow>().FirstOrDefault());
                }
            }
        }

        private bool FormInputsAreValid()
        {
            bool hasErrors = false;

            if (string.IsNullOrWhiteSpace(QtyInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a valid Qty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            if (string.IsNullOrWhiteSpace(RateInput))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a valid Rate.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (!decimal.TryParse(RateInput, out decimal rate))
            {
                hasErrors = true;
                MessageBox.Show("Please enter a valid numeric value for Rate.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            // Add new logic
        }

        private void LoadSharesBalances()
        {
            // Fetch the list of share balances as QryShareBalanceNett
            SharesCharts = DatabaseHelper.GetShares(GlAccountId);
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
