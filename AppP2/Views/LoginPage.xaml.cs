using AppP2.Models;
using AppP2.Services;
using Microsoft.Maui.Controls;

namespace AppP2.Views
{
    public partial class LoginPage : ContentPage
    {
        private readonly DatabaseService _databaseService;

        public LoginPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();

            // Autocompletar con credenciales de prueba
            EmailEntry.Text = "juan@test.com";
            PasswordEntry.Text = "123456";
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            try
            {
                var email = EmailEntry.Text?.Trim();
                var password = PasswordEntry.Text;

                // ========== VALIDACIONES ==========
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    await DisplayAlert("Error", "Por favor ingresa todos los campos", "OK");
                    return;
                }

                // ========== VERIFICAR USUARIOS EN LA BASE DE DATOS ==========
                var allUsers = await _databaseService.GetAllUsers();
                System.Diagnostics.Debug.WriteLine($"Total usuarios en BD: {allUsers.Count}");
                foreach (var u in allUsers)
                {
                    System.Diagnostics.Debug.WriteLine($"  - {u.Email} / {u.Password}");
                }

                // ========== BUSCAR USUARIO ==========
                var user = await _databaseService.GetUserByEmail(email);

                if (user == null)
                {
                    // Mostrar mensaje con los usuarios disponibles
                    var userList = allUsers.Count > 0
                        ? string.Join("\n", allUsers.Select(u => $"{u.Email} - {u.Password}"))
                        : "No hay usuarios registrados. Regístrate primero.";

                    await DisplayAlert(
                        "Usuario No Encontrado",
                        $"El correo '{email}' no está registrado.\n\n" +
                        $"Usuarios disponibles:\n{userList}\n\n" +
                        "Usa: juan@test.com / 123456\n" +
                        "O regístrate para crear una cuenta nueva.",
                        "OK"
                    );
                    return;
                }

                // ========== VERIFICAR CONTRASEÑA ==========
                if (user.Password != password)
                {
                    await DisplayAlert(
                        "Contraseña Incorrecta",
                        $"La contraseña no coincide.\n\n" +
                        $"Email: {email}\n" +
                        $"Contraseña esperada: {user.Password}",
                        "OK"
                    );
                    return;
                }

                // ========== LOGIN EXITOSO ==========
                System.Diagnostics.Debug.WriteLine($"LOGIN EXITOSO: {user.Email}");
                await Navigation.PushAsync(new MainMenuPage(user.Id));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }

        private async void OnCreateTestUserClicked(object sender, EventArgs e)
        {
            try
            {
                var testEmail = "juan@test.com";
                var testPassword = "123456";

                // Verificar si ya existe
                var existingUser = await _databaseService.GetUserByEmail(testEmail);
                if (existingUser != null)
                {
                    await DisplayAlert(
                        "Usuario Existente",
                        "El usuario de prueba ya existe.\n\n" +
                        "Email: juan@test.com\n" +
                        "Contraseña: 123456",
                        "OK"
                    );
                    EmailEntry.Text = testEmail;
                    PasswordEntry.Text = testPassword;
                    return;
                }

                // Crear usuario de prueba
                var testUser = new User
                {
                    FullName = "Juan Pérez",
                    Email = testEmail,
                    Password = testPassword,
                    RegistrationDate = DateTime.Now
                };

                var result = await _databaseService.SaveUser(testUser);

                if (result > 0)
                {
                    await DisplayAlert(
                        "Éxito",
                        "Usuario de prueba creado correctamente.\n\n" +
                        "Email: juan@test.com\n" +
                        "Contraseña: 123456",
                        "OK"
                    );

                    EmailEntry.Text = testEmail;
                    PasswordEntry.Text = testPassword;
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo crear el usuario de prueba.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo crear el usuario: {ex.Message}", "OK");
            }
        }

        private async void OnRegisterTapped(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage());
        }

    }
}