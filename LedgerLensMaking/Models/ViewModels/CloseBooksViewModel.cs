using LedgerLensMaking;
using LedgerLensMaking.Models.Data;
using LedgerLensMaking.Models.UIModels;
using LedgerLensMaking.UtilityClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;

public class CloseBooksViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ReportLegerModel> LedgerAccounts { get; set; }

    public ICommand CloseCommand { get; }
    public ICommand CancelCommand { get; }


    private string _individualName;
    private List<Chart> _glAccounts;
    private Chart _selectedGLAccount;

    public List<Chart> GLAccounts
    {
        get => _glAccounts;
        set
        {
            _glAccounts = value;
            OnPropertyChanged();
        }
    }

    public Chart SelectedGL
    {
        get => _selectedGLAccount;
        set
        {
            _selectedGLAccount = value;
            OnPropertyChanged();
            if (_selectedGLAccount != null) // Ensure that an account is selected
            {
                PopulateGLData(); // Populate the data grid based on the selected account
            }
        }
    }
    private string _message1Input;
    public string Message1Input
    {
        get => _message1Input;
        set
        {
            _message1Input = value;
            OnPropertyChanged();
        }
    }
    private string _message2Input;
    public string Message2Input
    {
        get => _message2Input;
        set
        {
            _message2Input = value;
            OnPropertyChanged();
        }
    }

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
    private string _closingMessage;
    public string ClosingMessage
    {
        get => _closingMessage;
        set
        {
            _closingMessage = value;
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

    private decimal _totalDebits;
    public decimal TotalDebits
    {
        get => _totalDebits;
        set
        {
            _totalDebits = value;
            OnPropertyChanged();
        }
    }

    private decimal _totalCredits;
    public decimal TotalCredits
    {
        get => _totalCredits;
        set
        {
            _totalCredits = value;
            OnPropertyChanged();
        }
    }
    private bool _isCloseEnabled = false;
    public bool IsCloseEnabled
    {
        get => _isCloseEnabled;
        set
        {
            _isCloseEnabled = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public CloseBooksViewModel()
    {

        IndividualName = GlobalVariables.IndividualName + "'s Closing Books";
        PeriodLine = "For the Year Ended " + GlobalVariables.SelectedStartDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture) + " to  " + GlobalVariables.SelectedEndDate.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
        PrintDate = "Closing on " + DateTime.Today.ToString("dd/M/yyyy", CultureInfo.InvariantCulture);
        ClosingMessage = "";
        CloseCommand = new RelayCommand(CloseBooks);
        CancelCommand = new RelayCommand(CancelCloser);
        _isCloseEnabled=false;
        CheckClosingDate();
        


        // Load GL accounts when the ViewModel is initialized
        LoadAccountsForDropDown();

        // Calculate totals after population

    }
    private void CheckClosingDate()
    {
        DateTime today = DateTime.Today;
        if (today < GlobalVariables.SelectedEndDate)
        {
            ClosingMessage = $"The closing date must be after {GlobalVariables.SelectedEndDate:dd-MMM-yyyy}.";
            IsCloseEnabled=false ;
        }
        else
        {
            ClosingMessage = $"Enter the Closing Message. Make Sure you have backed up Database";
            IsCloseEnabled=true ;
        }
    }
    private void CloseBooks()
    {
        
        if (Message1Input == "YES" && Message2Input == "CLOSE")
            FurtherChecks();
        else
            MessageBox.Show("Permission Denied!");
    }
    private void FurtherChecks()
    {
        MessageBox.Show("Further Check is not Implemented, I will close the books anyways!! Trust your backup");
        YearEndProcessing endProcessing = new YearEndProcessing();
        endProcessing.StartYearEndProcessing();
    }
    private void CancelCloser()
    {
        Message1Input = string.Empty;
        Message2Input = string.Empty;

        // Optionally reset other properties or display a message
        ClosingMessage = "Closing operation was canceled. You can try again.";

        // Notify the UI that the inputs have been cleared
        OnPropertyChanged(nameof(Message1Input));
        OnPropertyChanged(nameof(Message2Input));
        OnPropertyChanged(nameof(ClosingMessage));

        MessageBox.Show("Closing process canceled and input fields cleared.");
    }

    private void OnExportToExcel()
    {
        //ReportLedgerAccountExportToExcel.ExportAccountToExcel(LedgerAccounts.ToList(), IndividualName, PeriodLine, PrintDate);
        ReporLedgersAccountExportToExcel.ExportAccountToExcel(LedgerAccounts.ToList(), IndividualName, PeriodLine, PrintDate);
    }

    private void LoadAccountsForDropDown()
    {
        // Load GL Accounts without depending on SelectedGL initially
        GLAccounts = DatabaseHelper.GetCreditAccountsForShareJournal("GetEverything");
        OnPropertyChanged(nameof(GLAccounts)); // Notify UI that GLAccounts is updated
    }
    private void PopulateGLData()
    {
        if (SelectedGL != null)
        {
            // Fetch the ledger data for the selected GL account
            LedgerAccounts = new ObservableCollection<ReportLegerModel>(
                DatabaseHelper.GetLedgerAccount(SelectedGL.AccountId, GlobalVariables.SelectedYearId, "Single")

            );
            

            // Notify the UI that LedgerAccounts has changed so it can update the grid
            OnPropertyChanged(nameof(LedgerAccounts));
        }
    }

   



    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
