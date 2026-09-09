using AppP2.Services;
using AppP2.Views;
using Microsoft.Maui.Controls;

namespace AppP2.Views
{
    public partial class MainMenuPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly int _userId;

        public MainMenuPage(int userId)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _userId = userId;
        }

        private async void OnViewProfessorsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfessorListPage(_userId));
        }

        private async void OnAddReviewClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfessorListPage(_userId, true));
        }

        private async void OnAddProfessorClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddProfessorPage(_userId));
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfilePage(_userId));
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert(
                "Cerrar Sesión",
                "¿Estás seguro de que deseas cerrar sesión?",
                "Sí",
                "No"
            );

            if (confirm)
            {
                await Navigation.PopToRootAsync();
            }
        }
    }
}