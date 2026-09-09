using AppP2.Models;
using AppP2.Services;
using Microsoft.Maui.Controls;

namespace AppP2.Views
{
    public partial class RegisterPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public RegisterPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            try
            {
                // Obtener valores
                var fullName = FullNameEntry.Text?.Trim();
                var email = EmailEntry.Text?.Trim();
                var password = PasswordEntry.Text;
                var confirmPassword = ConfirmPasswordEntry.Text;

                // ========== VALIDACIONES ==========
                if (string.IsNullOrEmpty(fullName))
                {
                    await DisplayAlert("Error", "Por favor ingresa tu nombre completo", "OK");
                    return;
                }

                if (string.IsNullOrEmpty(email))
                {
                    await DisplayAlert("Error", "Por favor ingresa tu correo electrónico", "OK");
                    return;
                }

                if (string.IsNullOrEmpty(password))
                {
                    await DisplayAlert("Error", "Por favor ingresa una contraseña", "OK");
                    return;
                }

                if (password != confirmPassword)
                {
                    await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                    return;
                }

                if (password.Length < 6)
                {
                    await DisplayAlert("Error", "La contraseña debe tener al menos 6 caracteres", "OK");
                    return;
                }

                // ========== VERIFICAR SI EL USUARIO YA EXISTE ==========
                var existingUser = await _databaseService.GetUserByEmail(email);
                if (existingUser != null)
                {
                    await DisplayAlert("Error", "El correo electrónico ya está registrado", "OK");
                    return;
                }

                // ========== CREAR USUARIO ==========
                var user = new User
                {
                    FullName = fullName,
                    Email = email,
                    Password = password,
                    RegistrationDate = DateTime.Now
                };

                // Guardar usuario
                var result = await _databaseService.SaveUser(user);

                // VERIFICAR QUE SE GUARDÓ CORRECTAMENTE
                if (result > 0)
                {
                    // Verificar que el usuario existe en la base de datos
                    var savedUser = await _databaseService.GetUserByEmail(email);

                    if (savedUser != null)
                    {
                        await DisplayAlert(
                            "Éxito",
                            $"Cuenta creada correctamente.\n\n" +
                            $"Email: {email}\n" +
                            $"Nombre: {fullName}\n\n" +
                            "Ya puedes iniciar sesión.",
                            "OK"
                        );

                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "El usuario no se guardó correctamente. Intenta nuevamente.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo crear la cuenta. Intenta nuevamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al registrar: {ex.Message}", "OK");
            }
        }

        private async void OnBackToLoginTapped(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}