using AppP2.Models;
using AppP2.Services;

namespace AppP2.Views
{
    public partial class RegisterPage : ContentPage
    {
        private DatabaseService _dbService;

        public RegisterPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var name = NameEntry?.Text;
            var email = EmailEntry?.Text;
            var password = PasswordEntry?.Text;
            var confirmPassword = ConfirmPasswordEntry?.Text;

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Error", "Todos los campos son obligatorios", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                return;
            }

            // Crear usuario (versión simplificada)
            var user = new User
            {
                Name = name,
                Email = email,
                Password = password
            };

            await _dbService.InsertUser(user);
            await DisplayAlert("Éxito", "Usuario registrado correctamente", "OK");
            await Navigation.PopAsync();
        }

        private async void OnBackToLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}