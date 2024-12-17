using CommunityToolkit.Maui.Storage; 
using Microsoft.Maui.Controls;

namespace sqleditor
{
    public partial class MainPage : ContentPage
    {
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
                    // Set the selected file path in the Entry
                    FilePathEntry.Text = result.FullPath;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions, such as permissions
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

    }
}
