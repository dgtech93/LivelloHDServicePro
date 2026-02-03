using System.Collections.Generic;
using System.Windows;
using LivelloHDServicePRO.Models;

namespace LivelloHDServicePRO.Views
{
    public partial class ColumnSelectionWindow : Window
    {
        public List<ColumnVisibilityModel> ColumnVisibilities { get; set; }

        public ColumnSelectionWindow(List<ColumnVisibilityModel> columnVisibilities)
        {
            InitializeComponent();
            ColumnVisibilities = columnVisibilities;
            ColumnsList.ItemsSource = ColumnVisibilities;
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var column in ColumnVisibilities)
            {
                column.IsVisible = true;
            }
        }

        private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var column in ColumnVisibilities)
            {
                column.IsVisible = false;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}