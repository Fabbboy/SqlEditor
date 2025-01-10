using System.Collections.ObjectModel;

namespace sqleditor.Views
{
    public partial class AddEntity : ContentPage
    {
        private TableHandle tableHandle;
        private ObservableCollection<ColumnHandle> fields;

        public AddEntity(TableHandle tableHandle)
        {
            InitializeComponent();
            this.tableHandle = tableHandle;
            fields = new ObservableCollection<ColumnHandle>(tableHandle.Columns.Select(c => new ColumnHandle(c.ColumnName, c.ColumnType, "")));
            FieldsCollectionView.ItemsSource = fields;
        }

        private async void OnAddClicked(object sender, EventArgs e)
        {
            try
            {
                var columns = string.Join(", ", fields.Select(f => f.ColumnName));
                var values = string.Join(", ", fields.Select(f => "@" + f.ColumnName));
                var query = $"INSERT INTO {tableHandle.TableName} ({columns}) VALUES ({values})";

                var parameters = fields.ToDictionary(f => "@" + f.ColumnName, f => (object)f.ColumnValue);

                GlobalDatabase.ExecuteNonQuery(query, parameters);

                await DisplayAlert("Success", "Entity added successfully", "OK");
                await Navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to add entity: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}