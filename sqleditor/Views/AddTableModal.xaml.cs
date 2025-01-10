using System.Collections.ObjectModel;
using System.Windows.Input;

namespace sqleditor.Views
{
    public partial class AddTableModal : ContentPage
    {
        public ObservableCollection<ColumnViewModel> Columns { get; set; }
        public ICommand AddColumnCommand { get; private set; }
        public ICommand DeleteColumnCommand { get; private set; }

        public AddTableModal()
        {
            InitializeComponent();
            Columns = new ObservableCollection<ColumnViewModel>();
            AddColumnCommand = new Command(AddColumn);
            DeleteColumnCommand = new Command<ColumnViewModel>(DeleteColumn);
            BindingContext = this;
            ColumnsCollectionView.ItemsSource = Columns;
        }

        private void AddColumn()
        {
            Columns.Add(new ColumnViewModel());
        }

        private void DeleteColumn(ColumnViewModel column)
        {
            Columns.Remove(column);
        }

        private async void OnCreateClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TableNameEntry.Text))
            {
                await DisplayAlert("Error", "Please enter a table name.", "OK");
                return;
            }

            if (Columns.Count == 0)
            {
                await DisplayAlert("Error", "Please add at least one column.", "OK");
                return;
            }

            var tableHandle = new TableHandle(TableNameEntry.Text);
            foreach (var column in Columns)
            {
                if (string.IsNullOrWhiteSpace(column.ColumnName))
                {
                    await DisplayAlert("Error", "All columns must have a name.", "OK");
                    return;
                }
                tableHandle.Columns.Add(new ColumnHandle(column.ColumnName, column.SelectedColumnType, ""));
            }

            await Navigation.PopModalAsync();
            MessagingCenter.Send(this, "TableCreated", tableHandle);
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }

    public class ColumnViewModel : BindableObject
    {
        private string columnName;
        public string ColumnName
        {
            get => columnName;
            set
            {
                columnName = value;
                OnPropertyChanged();
            }
        }

        private ColumnType selectedColumnType;
        public ColumnType SelectedColumnType
        {
            get => selectedColumnType;
            set
            {
                selectedColumnType = value;
                OnPropertyChanged();
            }
        }

        public Array ColumnTypes => Enum.GetValues(typeof(ColumnType));

        public ColumnViewModel()
        {
            SelectedColumnType = ColumnType.Text;
        }
    }
}