using System.Collections.Generic;
using LedgerLensMaking.Models.Data;
using LedgerLens.Models.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedgerLens.Models.ViewModels
{
    public class BaseViewModel  : INotifyPropertyChanged
    {
        // Base properties and methods
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
