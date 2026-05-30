using AppP2.Services;
using Microsoft.Maui.Controls;

namespace AppP2.Views
{
    public partial class LoginPage : ContentPage
    {
        private DatabaseService _dbService;

        public LoginPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Acceder directamente a los controles
            var email = this.FindByName<Entry>("EmailEntry")?.Text;
            var password = this.FindByName<Entry>("PasswordEntry")?.Text;
            var messageLabel = this.FindByName<Label>("MessageLabel");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                if (messageLabel != null)
                {
                    messageLabel.Text = "Por favor complete todos los campos";
                    messageLabel.IsVisible = true;
                }
                return;
            }

            // Versión demo - redirigir directamente
            await Shell.Current.GoToAsync("//MainMenuPage");
        }

        private async void OnRegisterTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }
}