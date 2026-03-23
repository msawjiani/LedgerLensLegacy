using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using LedgerLensMaking.Models.Data;
using LedgerLensMaking;

namespace LedgerLensMaking.Models.ViewModels
{
    public class SelectYearViewModel : INotifyPropertyChanged
    {
        private string _individualName;
        public string IndividualName
        {
            get => _individualName;
            set { _individualName = value; OnPropertyChanged(); }
        }

        private List<AccountingYear> _accountingYears;
        public List<AccountingYear> AccountingYears
        {
            get => _accountingYears;
            set { _accountingYears = value; OnPropertyChanged(); }
        }

        private AccountingYear _selectedYear;
        public AccountingYear SelectedYear
        {
            get => _selectedYear;
            set
            {
                _selectedYear = value;
                OnPropertyChanged();

                // 🔸 Keep your global state in sync whenever selection changes
                if (_selectedYear != null)
                {
                    GlobalVariables.SelectedYearId = _selectedYear.YearId;
                    GlobalVariables.SelectedStartDate = _selectedYear.StartDate;
                    GlobalVariables.SelectedEndDate = _selectedYear.EndDate;
                }
            }
        }

        public SelectYearViewModel()
        {
            IndividualName = GlobalVariables.IndividualName + "'s Accounts - Select a Year";
            LoadAccountingYears();
        }

        private void LoadAccountingYears()
        {
            var years = DatabaseHelper.GetAccountingYears();
            if (years == null) years = new List<AccountingYear>();

            // Oldest → Newest by EndDate
            AccountingYears = new List<AccountingYear>(years);
            AccountingYears.Sort((a, b) => a.EndDate.CompareTo(b.EndDate));

            if (AccountingYears.Count > 0)
            {
                // programmatically select the newest (last) AND update globals via setter
                SelectedYear = AccountingYears[AccountingYears.Count - 1];

                // (Optional) If you maintain MaxYearId globally:
                GlobalVariables.MaxYearId = AccountingYears[AccountingYears.Count - 1].YearId;
            }
            else
            {
                // Clear globals if there are no years
                GlobalVariables.SelectedYearId = 0;
                GlobalVariables.SelectedStartDate = default(DateTime);
                GlobalVariables.SelectedEndDate = default(DateTime);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
