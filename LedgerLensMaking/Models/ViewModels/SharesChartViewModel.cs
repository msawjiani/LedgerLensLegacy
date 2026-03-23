using LedgerLensMaking.Models.Data;
using LedgerLensMaking.UtilityClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Windows;
using System.Windows.Input;

namespace LedgerLensMaking.Models.ViewModels
{
    public class SharesChartViewModel : INotifyPropertyChanged
    {
        // Commands
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ModifyCommand { get; }
        public ICommand CancelCommand { get; }

        // Properties for managing UI states
        private bool _isAddEnabled = true;
        private bool _isModifyEnabled = false;
        private bool _isSaveEnabled = false;
        private bool _isCancelEnabled = false;
        private bool _isInModifyMode = false;

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

        private List<Chart> _glaccounts;
        public List<Chart> GLAccounts
        {
            get => _glaccounts;
            set
            {
                _glaccounts = value;
                OnPropertyChanged();
            }
        }

        private Chart _selectedAccount;
        public Chart SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                _selectedAccount = value;
                OnPropertyChanged();
                LoadCompaniesForSelectedAccount(); // Update the DataGrid based on the selected account
            }
        }

        private SharesChart _selectedCompany;
        public SharesChart SelectedCompany
        {
            get => _selectedCompany;
            set
            {
                _selectedCompany = value;
                OnPropertyChanged();
                UpdateCompanyName(); // Update the TextBox when a company is selected
            }
        }

        private void UpdateCompanyName()
        {
            if (SelectedCompany != null)
            {
                CompanyName = SelectedCompany.Company;
                IsModifyEnabled = true; // Enable Modify when a company is selected
            }
            else
            {
                CompanyName = string.Empty; // Clear the text box if nothing is selected
                IsModifyEnabled = false; // Disable Modify if no company is selected
            }
        }

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

        private string _companyName;
        public string CompanyName
        {
            get => _companyName;
            set
            {
                _companyName = value;
                OnPropertyChanged();
            }
        }

        public bool IsAddEnabled
        {
            get => _isAddEnabled;
            set
            {
                _isAddEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsModifyEnabled
        {
            get => _isModifyEnabled;
            set
            {
                _isModifyEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsSaveEnabled
        {
            get => _isSaveEnabled;
            set
            {
                _isSaveEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsCancelEnabled
        {
            get => _isCancelEnabled;
            set
            {
                _isCancelEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool IsInModifyMode
        {
            get => _isInModifyMode;
            set
            {
                _isInModifyMode = value;
                OnPropertyChanged();
            }
        }

        // Constructor
        public SharesChartViewModel()
        {
            IndividualName = GlobalVariables.IndividualName + "'s Add/Modify Shares";
            LoadGLAccountsForLedgerCharts(); // Populate ComboBox

            AddCommand = new RelayCommand(OnAdd, () => IsAddEnabled);
            ModifyCommand = new RelayCommand(OnModify, () => IsModifyEnabled);
            SaveCommand = new RelayCommand(OnSave, () => IsSaveEnabled);
            CancelCommand = new RelayCommand(OnCancel, () => IsCancelEnabled);
        }

        private void OnAdd()
        {
            IsAddEnabled = false;
            IsModifyEnabled = false;
            IsSaveEnabled = true;
            IsCancelEnabled = true;

            CompanyName = string.Empty;
            IsInModifyMode = false; // Ensure we are in "Add" mode
        }


        private void EnableAllButtons()
        {
            IsAddEnabled = true;
            IsModifyEnabled = true;
            IsSaveEnabled = false;
            IsCancelEnabled = false;
        }

        private void OnModify()
        {
            IsAddEnabled = false;
            IsModifyEnabled = false;
            IsSaveEnabled = true;
            IsCancelEnabled = true;

            IsInModifyMode = true; // Set to "Modify" mode
        }

        private void OnSave()
        {
            if (ValidateInputs())
            {
                try
                {
                    if (IsInModifyMode)
                    {
                        // Implement modify logic here
                        DatabaseHelper.UpdateCompany(new SharesChart
                        {
                            ShareId = SelectedCompany.ShareId, // Use the existing ID to update
                            Company = CompanyName,
                            AccountId = SelectedAccount.AccountId
                        });
                    }
                    else
                    {
                        // Implement add logic here
                        DatabaseHelper.SaveCompany(new SharesChart
                        {
                            Company = CompanyName,
                            AccountId = SelectedAccount.AccountId
                        });
                    }

                    LoadCompaniesForSelectedAccount(); // Reload the data grid to reflect changes
                    ResetState();
                    EnableAllButtons();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving data: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Invalid input. Please check your entries.");
            }
        }

        private bool ValidateInputs()
        {
            return !string.IsNullOrEmpty(CompanyName) && SelectedAccount != null;
        }

        private void OnCancel()
        {
            ResetState();
        }

        private void ResetState()
        {
            IsAddEnabled = true;
            IsModifyEnabled = SelectedCompany != null; // Enable Modify if a company is selected
            IsSaveEnabled = false;
            IsCancelEnabled = false;

            // Optionally reset CompanyName or any other fields if necessary
            CompanyName = SelectedCompany?.Company ?? string.Empty;
        }

        private void LoadGLAccountsForLedgerCharts()
        {
            GLAccounts = DatabaseHelper.GetGLAccountForSharesChart();
        }

        private void LoadCompaniesForSelectedAccount()
        {
            if (SelectedAccount != null)
            {
                SharesCharts = DatabaseHelper.GetCompaniesForGLAccount(SelectedAccount.AccountId);
            }
            else
            {
                SharesCharts = new List<SharesChart>(); // Clear the DataGrid if no account is selected
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
