using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Maui.Controls;

namespace sqleditor.Views
{
    public partial class MainEditor : ContentPage
    {
        public ObservableCollection<Dictionary<string, string>> TableData { get; set; }
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
            TableData = new ObservableCollection<Dictionary<string, string>>();
            BindingContext = this;

            LoadTables();
        }

        private void LoadTables()
        {
            tableHandles.Clear();
            TableNames.Clear();

            tableHandles = GlobalDatabase.GetAllTablesWithColumns();

            Console.WriteLine("Num: " + tableHandles.Count);

            foreach (var tableHandle in tableHandles)
            {
                TableNames.Add(tableHandle.TableName);
            }


        }


        private void UpdateTableHeaders(List<ColumnHandle> columns)
        {
            var headerStack = this.FindByName<HorizontalStackLayout>("HeaderStack");
            headerStack.Children.Clear();

            foreach (var column in columns)
            {
                headerStack.Children.Add(new Label
                {
                    Text = column.ColumnName,
                    TextColor = Application.Current.Resources["TailwindNeutral100"] as Color,
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    Margin = new Thickness(8, 0)
                });
            }
        }

        private void OnTableSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is string tableName)
            {
                ActiveTableHandle = tableHandles.FirstOrDefault(t => t.TableName == tableName);
                ((ListView)sender).SelectedItem = null;

                var columns = GlobalDatabase.GetTableColumns(tableName);
                UpdateTableHeaders(columns);

                var data = GlobalDatabase.GetTableData(tableName);
                TableData.Clear();
                foreach (var row in data)
                {
                    TableData.Add(row);
                }

                // Update the DataTemplate for the CollectionView
                var dataCollection = this.FindByName<CollectionView>("DataCollection");
                dataCollection.ItemTemplate = new DataTemplate(() =>
                {
                    var stackLayout = new HorizontalStackLayout
                    {
                        Padding = new Thickness(12, 8)
                    };

                    foreach (var column in columns)
                    {
                        var label = new Label
                        {
                            TextColor = Application.Current.Resources["TailwindNeutral300"] as Color,
                            FontSize = 14,
                            Margin = new Thickness(8, 0)
                        };
                        label.SetBinding(Label.TextProperty, new Binding($"[{column.ColumnName}]"));
                        stackLayout.Children.Add(label);
                    }

                    return new ScrollView
                    {
                        Orientation = ScrollOrientation.Horizontal,
                        Content = stackLayout
                    };
                });
            }
        }

        private async void OnLogout(object sender, EventArgs e)
        {
            Debug.WriteLine("Logout button pressed");
            GlobalDatabase.Connection?.Close();
            await Navigation.PopAsync();
            //await Shell.Current.GoToAsync("///MainPage");
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
