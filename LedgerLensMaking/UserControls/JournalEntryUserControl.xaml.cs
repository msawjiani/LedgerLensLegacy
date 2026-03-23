using LedgerLensMaking.Models.UIModels;
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
    /// Interaction logic for JournalEntryUserControl.xaml
    /// </summary>
    public partial class JournalEntryUserControl : UserControl
    {
        public JournalEntryUserControl()
        {
            InitializeComponent();
        }
        private void MenuEdit_Click(object sender, RoutedEventArgs e)
        {
            // Get the MenuItem that was clicked
            var menuItem = sender as MenuItem;

            // Get the ContextMenu to which the MenuItem belongs
            var contextMenu = menuItem?.Parent as ContextMenu;

            // Find the DataGridRow that contains the ContextMenu
            var dataGridRow = contextMenu?.PlacementTarget as DataGridRow;

            if (dataGridRow != null)
            {
                // Get the bound item (JournalModel) for this row
                var selectedJournalEntry = dataGridRow.Item as JournalModel;

                if (selectedJournalEntry != null)
                {
                    MessageBox.Show($"Editing: {selectedJournalEntry.Account}");
                }
            }
        }

        private void MenuDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var dataGridRow = contextMenu?.PlacementTarget as DataGridRow;

            if (dataGridRow != null)
            {
                var selectedJournalEntry = dataGridRow.Item as JournalModel;

                if (selectedJournalEntry != null)
                {
                    MessageBox.Show($"Deleting: {selectedJournalEntry.Account}");

                    // Assuming you have access to the view model
                    var viewModel = DataContext as JournalEntryViewModel;
                    if (viewModel != null)
                    {
                        viewModel.JournalEntries.Remove(selectedJournalEntry);
                        viewModel.CalculateTotals(); // Recalculate totals after deletion
                    }
                }
            }
        }






    }
}
