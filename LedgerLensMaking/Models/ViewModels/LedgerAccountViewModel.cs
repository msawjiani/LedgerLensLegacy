using LedgerLensMaking.Models.Data;
using LedgerLensMaking.UtilityClasses;
using LedgerLensMaking;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Linq;
using System.Windows;
using System;

public class LedgerAccountViewModel : INotifyPropertyChanged
{
    private string _individualName;
    public List<string> Categories { get; set; } = new List<string> { "BANK", "BS", "PL" ,"RE"};
    public List<string> SubLedgerFlags { get; set; } = new List<string> { "NO", "SH", "SL" };
    private bool _isAddEnabled = true;
    private bool _isModifyEnabled = true;
    private bool _isSaveEnabled = false;
    private bool _isCancelEnabled = false;
    private bool _isGridEnabled = true;
    private bool _isDataGridEnabled = true;
    

    public string IndividualName
    {
        get => _individualName;
        set
        {
            _individualName = value;
            OnPropertyChanged();
        }
    }

    private List<Chart> _charts;
    public List<Chart> Charts
    {
        get => _charts;
        set
        {
            _charts = value;
            OnPropertyChanged();
        }
    }

    private Chart _selectedChart;
    public Chart SelectedChart
    {
        get => _selectedChart;
        set
        {
            _selectedChart = value;
            OnPropertyChanged();
            UpdateFields();
        }
    }

    private string _account;
    public string Account
    {
        get => _account;
        set
        {
            _account = value;
            OnPropertyChanged();
        }
    }

    private string _category;
    public string Category
    {
        get => _category;
        set
        {
            _category = value;
            OnPropertyChanged();
        }
    }

    private string _subLedgerFlag;
    public string SubLedgerFlag
    {
        get => _subLedgerFlag;
        set
        {
            _subLedgerFlag = value;
            OnPropertyChanged();
        }
    }

    // New Properties for Button and DataGrid States

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


    public bool IsGridEnabled
    {
        get => _isGridEnabled;
        set
        {
            _isGridEnabled = value;
            OnPropertyChanged();
        }
    }

    public bool IsDataGridEnabled
    {
        get => _isDataGridEnabled;
        set
        {
            _isDataGridEnabled = value;
            OnPropertyChanged();
        }
    }

    private bool _isInModifyMode = false;

    public bool IsInModifyMode
    {
        get => _isInModifyMode;
        set
        {
            _isInModifyMode = value;
            OnPropertyChanged();
        }
    }



    // Commands
    public ICommand AddCommand { get; }
    public ICommand ModifyCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
 

    public LedgerAccountViewModel()
    {
        IndividualName = GlobalVariables.IndividualName + "'s Add/Modify Ledger Account";
        LoadLedgerCharts();

        AddCommand = new RelayCommand(OnAdd, () => IsAddEnabled);
        ModifyCommand = new RelayCommand(OnModify, () => IsModifyEnabled);
        SaveCommand = new RelayCommand(OnSave, () => IsSaveEnabled);
        CancelCommand = new RelayCommand(OnCancel, () => IsCancelEnabled);
    }

    private void LoadLedgerCharts()
    {
        Charts = DatabaseHelper.GetCharts();
    }

    private void UpdateFields()
    {
        if (SelectedChart != null)
        {
            Account = SelectedChart.Account;
            Category = SelectedChart.Category;
            SubLedgerFlag = SelectedChart.SubledgerFlag;

            if (!SubLedgerFlags.Contains(SubLedgerFlag))
            {
                SubLedgerFlag = SubLedgerFlags.FirstOrDefault();
            }

            OnPropertyChanged(nameof(Category));
            OnPropertyChanged(nameof(SubLedgerFlag));
        }
    }

    // Command Handlers
    private void OnAdd()
    {
        // Disable Add and Modify, enable Save and Cancel
        IsAddEnabled = false;
        IsModifyEnabled = false;
        IsSaveEnabled = true;
        IsCancelEnabled = true;

        // Disable the grid to prevent interaction
        IsGridEnabled = false;

        // Clear the fields for new input
        Account = string.Empty;
        Category = Categories.FirstOrDefault();
        SubLedgerFlag = SubLedgerFlags.FirstOrDefault();
    }

    private void OnModify()
    {
        if (SelectedChart != null)
        {
            // Set to Modify mode
            IsInModifyMode = true;

            // Disable Add and Modify, enable Save and Cancel
            IsAddEnabled = false;
            IsModifyEnabled = false;
            IsSaveEnabled = true;
            IsCancelEnabled = true;

            // Disable the grid to prevent interaction
            IsGridEnabled = false;
        }
    }

    private void OnSave()
    {
        if (ValidateInputs())
        {
            try
            {
                if (IsInModifyMode)
                {
                    // Modify the selected chart
                    SelectedChart.Account = Account;
                    SelectedChart.Category = Category;
                    SelectedChart.SubledgerFlag = SubLedgerFlag;

                    DatabaseHelper.UdateChart(SelectedChart);   
                }
                else
                {
                    // Add a new chart
                    DatabaseHelper.SaveChart(new Chart
                    {
                        Account = Account,
                        Category = Category,
                        SubledgerFlag = SubLedgerFlag
                    });
                }

                // Reload the data grid to reflect changes
                LoadLedgerCharts();

                // Reset the form and state
                ResetState();
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



    private void Save(object parameter)
    {
        if (ValidateInputs())
        {
            try
            {
                DatabaseHelper.SaveChart(new Chart
                {
                    Account = Account,
                    Category = Category,
                    SubledgerFlag = SubLedgerFlag
                });

                LoadLedgerCharts(); // Reload data to reflect the new entry
                ResetForm(); // Clear form fields and reset buttons
            }
            catch (Exception ex)
            {
                // Handle exceptions, such as UNIQUE INDEX constraint violation
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
        return !string.IsNullOrEmpty(Account) &&
                       Categories.Contains(Category) &&
                       SubLedgerFlags.Contains(SubLedgerFlag);
    }


    private void ResetForm()
    {
        // Clear the form and reset buttons
        Account = string.Empty;
        Category = Categories.FirstOrDefault();
        SubLedgerFlag = SubLedgerFlags.FirstOrDefault();
        EnableAllButtons();
    }

    private void EnableAllButtons()
    {
        IsAddEnabled = true;
        IsModifyEnabled = true;
        IsSaveEnabled = false;
        IsCancelEnabled = false;
    }
    private bool CanSave()
    {
        return !string.IsNullOrEmpty(Account) &&
               !string.IsNullOrEmpty(Category) &&
               !string.IsNullOrEmpty(SubLedgerFlag);
    }


    private void OnCancel()
    {
        // Logic for canceling the operation

        // Reset the state to initial
        ResetState();
    }

    private void ResetState()
    {
        // Reset the form fields
        Account = string.Empty;
        Category = Categories.FirstOrDefault();
        SubLedgerFlag = SubLedgerFlags.FirstOrDefault();

        // Enable Add and Modify, disable Save and Cancel
        IsAddEnabled = true;
        IsModifyEnabled = true;
        IsSaveEnabled = false;
        IsCancelEnabled = false;

        // Enable the grid
        IsGridEnabled = true;

        // Reset the operation mode
        IsInModifyMode = false;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
