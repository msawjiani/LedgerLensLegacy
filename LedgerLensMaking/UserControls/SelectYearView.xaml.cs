using LedgerLensMaking.Models.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LedgerLensMaking.UserControls
{
    public partial class SelectYearView : UserControl
    {
        public SelectYearView()
        {
            InitializeComponent();
            MyYearsDataGrid.SelectedIndex = MyYearsDataGrid.Items.Count - 1;
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if an item is selected
            if (MyYearsDataGrid.SelectedItem != null)
            {
                // Cast the selected item to the appropriate type (assuming your items are of type AccountingYear)
                var selectedYear = MyYearsDataGrid.SelectedItem as AccountingYear;

                // Check if the cast was successful
                if (selectedYear != null)
                {
                    // Assign the value to the global variable
                    GlobalVariables.SelectedYearId = selectedYear.YearId;
                    GlobalVariables.SelectedEndDate = selectedYear.EndDate;
                    GlobalVariables.SelectedStartDate = selectedYear.StartDate;
                }
            }

            // Find the parent content control and remove the UserControl
            var parentContentControl = FindParentContentControl(this);
            if (parentContentControl != null)
            {
                // Navigate back to the default or home view
                parentContentControl.Content = new UserControl() ; // Replace 'HomeView' with your default view UserControl
            }
        }

        private ContentControl FindParentContentControl(DependencyObject child)
        {
            // Traverse the visual tree to find a ContentControl parent
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            ContentControl parent = parentObject as ContentControl;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return FindParentContentControl(parentObject);
            }
        }


        private void MyYearsDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (MyYearsDataGrid.Items.Count > 0)
            {
                MyYearsDataGrid.SelectedIndex = MyYearsDataGrid.Items.Count - 1;
                MyYearsDataGrid.ScrollIntoView(MyYearsDataGrid.SelectedItem);
            }
        }


    }
}
