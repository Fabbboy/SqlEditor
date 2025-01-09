using System.ComponentModel;
using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls;
using sqleditor.Views;

namespace sqleditor
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private string _filePath;
        private string _username;
        private string _password;

        private string _filePathError;
        private string _usernameError;
        private string _passwordError;

        public string FilePath
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    ValidateFilePath();
                    OnPropertyChanged(nameof(FilePath));
                }
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    ValidateUsername();
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    ValidatePassword();
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        public string FilePathError
        {
            get => _filePathError;
            set
            {
                if (_filePathError != value)
                {
                    _filePathError = value;
                    OnPropertyChanged(nameof(FilePathError));
                    OnPropertyChanged(nameof(IsFilePathInvalid));
                }
            }
        }

        public string UsernameError
        {
            get => _usernameError;
            set
            {
                if (_usernameError != value)
                {
                    _usernameError = value;
                    OnPropertyChanged(nameof(UsernameError));
                    OnPropertyChanged(nameof(IsUsernameInvalid));
                }
            }
        }

        public string PasswordError
        {
            get => _passwordError;
            set
            {
                if (_passwordError != value)
                {
                    _passwordError = value;
                    OnPropertyChanged(nameof(PasswordError));
                    OnPropertyChanged(nameof(IsPasswordInvalid));
                }
            }
        }

        public bool IsFilePathInvalid => !string.IsNullOrEmpty(FilePathError);
        public bool IsUsernameInvalid => !string.IsNullOrEmpty(UsernameError);
        public bool IsPasswordInvalid => !string.IsNullOrEmpty(PasswordError);

        static Dictionary<DevicePlatform, IEnumerable<string>> customFileTypeDictionary = new()
        {
            { DevicePlatform.iOS, new[] { "public.database" } },
            { DevicePlatform.Android, new[] { "application/x-sqlite3" } },
            { DevicePlatform.WinUI, new[] { ".db", ".sqlite" } },
        };

        static FilePickerFileType customFileType = new FilePickerFileType(customFileTypeDictionary);

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            // Initialize fields
            _filePath = string.Empty;
            _username = string.Empty;
            _password = string.Empty;
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
                    FilePath = result.FullPath;
                }
            }
            catch (Exception ex)
            {
                FilePathError = $"Error: {ex.Message}";
            }
        }

        private async void OnConnectClicked(object sender, EventArgs e)
        {
            ValidateAllFields();

            if (IsFilePathInvalid || IsUsernameInvalid || IsPasswordInvalid)
            {
                return;
            }

            try
            {
                GlobalDatabase.OpenDatabase(FilePath, Username, Password);
               // await Shell.Current.GoToAsync("///Editor");
               await Navigation.PushAsync(new MainEditor());
            }
            catch (Exception ex)
            {
                FilePathError = $"Error: {ex.Message}";
            }
        }

        private void ValidateFilePath()
        {
            FilePathError = string.IsNullOrWhiteSpace(FilePath) ? "File path cannot be empty." : string.Empty;
        }

        private void ValidateUsername()
        {
            UsernameError = string.IsNullOrWhiteSpace(Username) ? "Username cannot be empty." : string.Empty;
        }

        private void ValidatePassword()
        {
            PasswordError = string.IsNullOrWhiteSpace(Password) ? "Password cannot be empty." : string.Empty;
        }

        private void ValidateAllFields()
        {
            ValidateFilePath();
            ValidateUsername();
            ValidatePassword();
        }

        private async void OnFAQTapped(object sender, EventArgs e)
        {
            //  await Shell.Current.GoToAsync("///FAQ");
            await Navigation.PushAsync(new FAQPage());
        }

        private void OnImprintTapped(object sender, EventArgs e)
        {
        }
    }
}
