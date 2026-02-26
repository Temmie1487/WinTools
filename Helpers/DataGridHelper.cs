using System.Collections;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace WinTools.Helpers;

public static class DataGridHelper
{
    public static void EnableSorting(DataGrid dataGrid)
    {
        dataGrid.Sorting += (s, e) =>
        {
            var dg = s as DataGrid;
            if (dg == null || dg.ItemsSource == null) return;

            var view = CollectionViewSource.GetDefaultView(dg.ItemsSource);
            if (view == null) return;

            ListSortDirection direction = (e.Column.SortDirection != ListSortDirection.Ascending)
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;

            e.Column.SortDirection = direction;

            var propertyName = e.Column.SortMemberPath;
            if (!string.IsNullOrEmpty(propertyName))
            {
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(propertyName, direction));
            }
        };
    }

    public static void EnableDoubleClickSelect(DataGrid dataGrid, Action<object> onDoubleClick)
    {
        dataGrid.MouseDoubleClick += (s, e) =>
        {
            if (dataGrid.SelectedItem != null)
            {
                onDoubleClick(dataGrid.SelectedItem);
            }
        };
    }
}