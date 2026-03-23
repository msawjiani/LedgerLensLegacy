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
    public class SubLedgerChartViewModel : INotifyPropertyChanged
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
                LoadSubledgersForSelectedAccount(); // Update the DataGrid based on the selected account
            }
        }
        private SubledgerChart _selectedSubledger;
        public SubledgerChart SelectedSubledger
        {
            get => _selectedSubledger;
            set
            {
                _selectedSubledger = value;
                OnPropertyChanged();
                UpdateSubledgerName(); // Update the TextBox when a subaccount is selected
            }
        }


        private string _subaccountName;
        public string SubaccountName
        {
            get => _subaccountName;
            set
            {
                _subaccountName = value;
                OnPropertyChanged();
            }
        }

        private void UpdateSubledgerName()
        {
            if (SelectedSubledger != null)
            {
                SubaccountName = SelectedSubledger.Subaccount;
            }
            else
            {
                SubaccountName = string.Empty; // Clear the text box if nothing is selected
            }
        }

        private List<SubledgerChart> _subledgerCharts;
        public List<SubledgerChart> SubledgerCharts
        {
            get => _subledgerCharts;
            set
            {
                _subledgerCharts = value;
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
        public SubLedgerChartViewModel()
        {
            IndividualName = GlobalVariables.IndividualName + "'s Add/Modify Subledgers";
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

            SubaccountName = string.Empty;
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
                        DatabaseHelper.UpdateSubledger(new SubledgerChart
                        {
                            SubledgerId = SelectedSubledger.SubledgerId, // Use the existing ID to update
                            Subaccount = SubaccountName,
                            AccountId = SelectedSubledger.AccountId
                        });
                    }
                    else
                    {
                        // Implement add logic here
                        DatabaseHelper.SaveSubledger(new SubledgerChart
                        {
                            Subaccount = SubaccountName,
                            AccountId = SelectedAccount.AccountId
                        });
                    }

                    LoadSubledgersForSelectedAccount(); // Reload the data grid to reflect changes
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
            return !string.IsNullOrEmpty(SubaccountName) && SelectedAccount != null;
        }

        private void OnCancel()
        {
            ResetState();
        }

        private void ResetState()
        {
            IsAddEnabled = true;
            IsModifyEnabled = SelectedSubledger != null; // Enable Modify if a company is selected
            IsSaveEnabled = false;
            IsCancelEnabled = false;

            // Optionally reset CompanyName or any other fields if necessary
            SubaccountName = SelectedSubledger?.Subaccount ?? string.Empty;
        }

        private void LoadGLAccountsForLedgerCharts()
        {
            GLAccounts = DatabaseHelper.GetGLAccountForSubledgerChart();
        }

        private void LoadSubledgersForSelectedAccount()
        {
            if (SelectedAccount != null)
            {
                SubledgerCharts= DatabaseHelper.GetSubledgerForGLAccount(SelectedAccount.AccountId);
            }
            else
            {
                SubledgerCharts = new List<SubledgerChart>(); // Clear the DataGrid if no account is selected
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
