using AppP2.Models;
using AppP2.Services;

namespace AppP2.Views
{
    public partial class ProfessorListPage : ContentPage
    {
        private DatabaseService _dbService;

        public ProfessorListPage()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            LoadProfessors();
        }

        private async void LoadProfessors()
        {
            var professors = await _dbService.GetProfessors();
            // Usar FindByName para acceder al CollectionView
            var collectionView = this.FindByName<CollectionView>("ProfessorsCollectionView");
            if (collectionView != null)
            {
                collectionView.ItemsSource = professors;
            }
        }

        private async void OnProfessorSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Professor selected)
            {
                await Navigation.PushAsync(new AddReviewPage(selected));
            }
        }
    }
}