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
            tableHandles = GlobalDatabase.GetAllTablesWithColumns();

            Console.WriteLine("Num: " + tableHandles.Count);

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


    }
}
