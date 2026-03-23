using LedgerLensMaking.Models.ViewModels;
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
    /// Interaction logic for BankPaymentsUserControl.xaml
    /// </summary>
    public partial class BankPaymentsUserControl : UserControl
    {
        public BankPaymentsUserControl()
        {
            InitializeComponent();
        }

        private void RefTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Background = new SolidColorBrush(Colors.White);
        }

        private void NarrationTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Background = new SolidColorBrush(Colors.White);
        }

        private void DateTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Background = new SolidColorBrush(Colors.White);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Background = new SolidColorBrush(Colors.MistyRose);
        }

        private void AmountTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Background = new SolidColorBrush(Colors.White);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox.Name == "DateInputTextBox" && e.Key == Key.F2)
            {
                // Call IncrementDate when in DateInput TextBox
                if (DataContext is BankPaymentViewModel viewModel)
                {
                    viewModel.IncrementDate();
                }
                e.Handled = true;
            }
            else if (textBox.Name == "RefInputTextBox" && e.Key == Key.F3)
            {
                // Call IncrementRefInput when in RefInput TextBox
                if (DataContext is BankPaymentViewModel viewModel)
                {
                    viewModel.IncrementRefInput();
                }
                e.Handled = true;
            }
        }

    }
}
