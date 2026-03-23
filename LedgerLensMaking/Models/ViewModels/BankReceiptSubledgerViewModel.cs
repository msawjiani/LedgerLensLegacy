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
    public class BankReceiptSubledgerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand SfCommand { get; }
        public ICommand AddNew { get; }
        public ICommand CloseFdCommand { get; }
        private bool _isSfEnabled = false;
         
        private readonly Action<QrySubledgerFDBalance> _onFDSelectedCallback;

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
        private List<QrySubledgerFDBalance> _qrySubledgerFDBalances;
        public List<QrySubledgerFDBalance> QrySubledgerFDBalances
        {
            get => _qrySubledgerFDBalances;
            set
            {
                _qrySubledgerFDBalances = value;
                OnPropertyChanged();
            }
        }
        private QrySubledgerFDBalance _selectedFD;
        public QrySubledgerFDBalance SelectedFD
        {
            get => _selectedFD;
            set
            {
                _selectedFD = value;
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
        public BankReceiptSubledgerViewModel(int glAccountId, string glAccountName, Action<QrySubledgerFDBalance> onFDSelectedCallback)
        {
            GlAccountId = glAccountId;
            GlAccountName = glAccountName;
            _onFDSelectedCallback = onFDSelectedCallback;
            LoadFDBalances();
            CloseFdCommand = new RelayCommand(OnCloseFd);
            AddNew = new RelayCommand(OnAddNewClicked);
            
        }
        private void OnFDSelected(QrySubledgerFDBalance selectedFD)
        {
            _selectedFD = selectedFD;
            //_parentViewModel.HandleFDSelection(selectedFD);
        }
        
        private void OnCloseFd()
        {
            // Your saving logic goes here
            if (SelectedFD != null)
            {
                // Pass the selected FD to the BankReceiptViewModel (assuming a method exists to handle this)
                // You can call this method in your BankReceiptViewModel
                OnFDSelected(SelectedFD); // Example of passing the selected FD
                _onFDSelectedCallback?.Invoke(SelectedFD); // Call the callbac
                CloseWindow(Application.Current.Windows.OfType<BankReceiptSubledgerWindow>().FirstOrDefault()); // Close the specific window
            }

            else
            {
                MessageBoxResult result = MessageBox.Show("You have not selected an Account. Do you want to close?", "Not Selected", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    // User confirmed closing without selection
                    CloseWindow(Application.Current.Windows.OfType<BankReceiptSubledgerWindow>().FirstOrDefault()); // Close the specific window
                }
            }    

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
            // as of now this guy is not implemented
            bool hasErrors = false;
            return hasErrors;
        }

        private void LoadFDBalances()
        {
            // Fetch the list of bank accounts from the database
            
            QrySubledgerFDBalances = DatabaseHelper.GetFDBalances(GlAccountId);
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
