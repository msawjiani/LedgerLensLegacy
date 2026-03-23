using LedgerLensMaking.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public partial class BankReceiptsUserControl : UserControl
    {
        public BankReceiptsUserControl()
        {
            InitializeComponent();
        }

        private void DateTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (DateTime.TryParse(textBox.Text, out DateTime enteredDate))
            {
                if (enteredDate < GlobalVariables.SelectedStartDate || enteredDate > GlobalVariables.SelectedEndDate)
                {
                    MessageBox.Show($"Please enter a date between {GlobalVariables.SelectedStartDate:dd-MMM-yyyy} and {GlobalVariables.SelectedEndDate:dd-MMM-yyyy}.", "Invalid Date", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Delay setting focus to avoid re-triggering LostFocus immediately
                    Dispatcher.BeginInvoke(new Action(() => textBox.Focus()), System.Windows.Threading.DispatcherPriority.Input);
                }
                else
                {
                    textBox.Background = new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid date.", "Invalid Date", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Delay setting focus to avoid re-triggering LostFocus immediately
                Dispatcher.BeginInvoke(new Action(() => textBox.Focus()), System.Windows.Threading.DispatcherPriority.Input);
            }
        }



        private void RefTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length   >12 )
            {

                    MessageBox.Show("Length Longer than 12 Characters","Length Too Long",MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Delay setting focus to avoid re-triggering LostFocus immediately
                    Dispatcher.BeginInvoke(new Action(() => textBox.Focus()), System.Windows.Threading.DispatcherPriority.Input);
            }
            else
            {
                textBox.Background= new SolidColorBrush(Colors.White);  
            }

        }

        private void NarrationTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Background = new SolidColorBrush(Colors.LightBlue);

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {

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
    
    



