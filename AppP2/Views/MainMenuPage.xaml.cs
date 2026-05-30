namespace AppP2.Views
{
    public partial class MainMenuPage : ContentPage
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        private async void OnProfessorsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ProfessorListPage());
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}