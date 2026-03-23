using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LedgerLensMaking.UserControls
{
    /// <summary>
    /// Interaction logic for CloseBooksUserControl.xaml
    /// </summary>
    public partial class CloseBooksUserControl : UserControl
    {
        public CloseBooksUserControl()
        {
            InitializeComponent();
        }

        private void Password1Changed(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            var viewModel = (CloseBooksViewModel)DataContext;
            if (viewModel != null)
            {
                viewModel.Message1Input = passwordBox.Password; // Set the password to your ViewModel property
            }
        }

        private void Password2Changed(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            var viewModel = (CloseBooksViewModel)DataContext;
            if (viewModel != null)
            {
                viewModel.Message2Input = passwordBox.Password; // Set the password to your ViewModel property
            }

        }
    }
}
