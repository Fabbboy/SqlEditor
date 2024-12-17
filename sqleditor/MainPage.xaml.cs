using System.ComponentModel;
using CommunityToolkit.Maui.Storage; 
using Microsoft.Maui.Controls;

namespace sqleditor
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private string _filePath;

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged();
                }
            }
        }


        static Dictionary<DevicePlatform, IEnumerable<string>> customFileTypeDictionary = new()
            {
                { DevicePlatform.iOS, new[] { "public.database" } }, // UTType for SQLite on iOS
                { DevicePlatform.Android, new[] { "application/x-sqlite3" } }, // MIME type for SQLite on Android
                { DevicePlatform.WinUI, new[] { ".db", ".sqlite" } }, // File extensions for SQLite on Windows
            };

        static FilePickerFileType customFileType = new FilePickerFileType(customFileTypeDictionary);

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private async void OnBrowseClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Please select a file",
                    FileTypes = customFileType
                });

                if (result != null)
                {
                   // FilePathEntry.Text = result.FullPath;
                   FilePath = result.FullPath;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions, such as permissions
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }


        private async void OnConnectClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                await DisplayAlert("Error", "Please select a file first", "OK");
                return;
            }
            try
            {
                using var stream = File.OpenRead(FilePath);
                using var reader = new StreamReader(stream);
                var text = await reader.ReadToEndAsync();
                await DisplayAlert("File Content", text, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
    }
}
