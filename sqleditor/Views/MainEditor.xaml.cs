using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace sqleditor.Views
{
    public partial class MainEditor : ContentPage
    {
        private List<TableHandle> tableHandles;
        public ObservableCollection<string> TableNames { get; set; }

        private TableHandle? activeTableHandle;
        public TableHandle? ActiveTableHandle
        {
            get => activeTableHandle;
            set
            {
                activeTableHandle = value;
                ActiveTableName = activeTableHandle?.TableName ?? "No Table Selected";
            }
        }

        private string activeTableName = "No Table Selected";
        public string ActiveTableName
        {
            get => activeTableName;
            set
            {
                activeTableName = value;
                OnPropertyChanged(nameof(ActiveTableName));
            }
        }

        public MainEditor()
        {
            InitializeComponent();

            TableNames = new ObservableCollection<string>();
            tableHandles = new List<TableHandle>();
            BindingContext = this;

            LoadTables();
        }

        private void LoadTables()
        {
            // Clear existing data to prevent duplicates
            tableHandles.Clear();
            TableNames.Clear();

            // Fetch updated table list
            tableHandles = GlobalDatabase.GetAllTablesWithColumns();

            Console.WriteLine("Num: " + tableHandles.Count);

            // Populate the ObservableCollection with table names
            foreach (var tableHandle in tableHandles)
            {
                TableNames.Add(tableHandle.TableName);
            }
        }


        private void OnTableSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is string tableName)
            {
                ActiveTableHandle = tableHandles.FirstOrDefault(t => t.TableName == tableName);
                ((ListView)sender).SelectedItem = null;
            }
        }

        private async void OnLogout(object sender, EventArgs e)
        {
            Debug.WriteLine("Logout button pressed");
            GlobalDatabase.Connection?.Close();
            await Shell.Current.GoToAsync("///MainPage");
        }

        private void RefreshPressed(object sender, EventArgs e)
        {
            LoadTables();
        }

        private async void DeleteTable(object sender, EventArgs e)
        {
            try
            {
                if (activeTableHandle != null)
                {
                    GlobalDatabase.DeleteTable(activeTableHandle);
                    tableHandles.Remove(activeTableHandle);
                    TableNames.Remove(activeTableHandle.TableName);
                    ActiveTableHandle = null;
                }
                else
                {
                    await DisplayAlert("Error", "No table selected to delete.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
