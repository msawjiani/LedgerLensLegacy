using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedgerLensMaking.VewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _photoFile;

        public MainWindowViewModel()
        {
            // Set the default image
            _photoFile = "Assets/Logo1.png";
        }

        public string PhotoFile
        {
            get => _photoFile;
            set
            {
                _photoFile = value;
                OnPropertyChanged();
            }
        }

        public string PAN { get; set; }
        public string IndividualName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
