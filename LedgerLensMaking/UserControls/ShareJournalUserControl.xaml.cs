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
    /// Interaction logic for ShareJournalUserControl.xaml
    /// </summary>
    public partial class ShareJournalUserControl : UserControl
    {
        public ShareJournalUserControl()
        {
            InitializeComponent();
        }

        private void DateTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Background = new SolidColorBrush(Colors.White);
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

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Background = new SolidColorBrush(Colors.LightBlue);
        }
        private void GenericTextBox_LostFocus(Object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.Background = new SolidColorBrush(Colors.White);
        }
    }
}
