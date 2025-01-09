
namespace sqleditor.Views
{
    public partial class FAQPage : ContentPage
    {
        public FAQPage()
        {
            InitializeComponent();
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
