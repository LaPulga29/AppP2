using System.Collections.ObjectModel;
using AppP2.Models;
using AppP2.Services;
using Microsoft.Maui.Controls;

namespace AppP2.Views
{
    public partial class ProfilePage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly int _userId;
        private User _currentUser;
        private List<Review> _allUserReviews = new List<Review>();
        private bool _isLoading = false;
        private bool _isFirstLoad = true;

        public ObservableCollection<Review> UserReviews { get; set; } = new();

        public ProfilePage(int userId)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _userId = userId;
            BindingContext = this;

            LoadProfileData();
        }

        private async void LoadProfileData()
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                // Cargar información del usuario
                _currentUser = await _databaseService.GetUserById(_userId);
                if (_currentUser != null)
                {
                    UserNameLabel.Text = _currentUser.FullName;
                    UserEmailLabel.Text = _currentUser.Email;
                }

                await LoadUserReviews();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar perfil: {ex.Message}", "OK");
            }
            finally
            {
                _isLoading = false;
                _isFirstLoad = false;
            }
        }

        private async Task LoadUserReviews()
        {
            try
            {
                //LIMPIAR LA LISTA
                UserReviews.Clear();

                //OBTENER RESEÑAS DEL USUARIO
                _allUserReviews = await _databaseService.GetReviewsByUser(_userId);

                System.Diagnostics.Debug.WriteLine($"Reseñas encontradas en BD: {_allUserReviews.Count}");

                //ELIMINAR DUPLICADOS POR ID
                var uniqueReviews = _allUserReviews
                    .GroupBy(r => r.Id)
                    .Select(g => g.First())
                    .ToList();

                _allUserReviews = uniqueReviews;

                //AGREGAR A LA UI
                foreach (var review in _allUserReviews)
                {
                    var professor = await _databaseService.GetProfessor(review.ProfessorId);
                    review.ProfessorName = professor?.Name ?? "Profesor eliminado";
                    UserReviews.Add(review);
                }

                System.Diagnostics.Debug.WriteLine($"Reseñas cargadas en UI: {UserReviews.Count}");

                //ACTUALIZAR ESTADÍSTICAS DESPUÉS DE CARGAR
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en LoadUserReviews: {ex.Message}");
            }
        }

        //MÉTODO PARA ACTUALIZAR ESTADÍSTICAS
        private void UpdateStatistics()
        {
            try
            {
                var reviews = _allUserReviews ?? new List<Review>();
                var reviewCount = reviews.Count;

                System.Diagnostics.Debug.WriteLine($"Actualizando estadísticas: {reviewCount} reseñas");

                //Total de reseñas
                if (TotalReviewsLabel != null)
                    TotalReviewsLabel.Text = reviewCount.ToString();

                //Profesores únicos
                var uniqueProfessors = reviews.Select(r => r.ProfessorId).Distinct().Count();
                if (ProfessorsCountLabel != null)
                    ProfessorsCountLabel.Text = uniqueProfessors.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en UpdateStatistics: {ex.Message}");
            }
        }

        private async void OnDeleteReviewClicked(object sender, EventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button == null) return;

                var reviewId = (int)button.CommandParameter;
                var review = UserReviews.FirstOrDefault(r => r.Id == reviewId);

                if (review == null) return;

                var confirm = await DisplayAlert(
                    "Confirmar Eliminación",
                    $"¿Estás seguro de que deseas eliminar esta reseña?\n\n" +
                    $"Profesor: {review.ProfessorName}\n" +
                    $"Calificación: {review.Rating} ?\n" +
                    $"Comentario: {review.Comment}",
                    "Sí, Eliminar",
                    "Cancelar"
                );

                if (confirm)
                {
                    await _databaseService.DeleteReview(reviewId);

                    UserReviews.Remove(review);
                    _allUserReviews.Remove(review);

                    UpdateStatistics();
                    MessagingService.NotifyProfessorsUpdated();

                    await DisplayAlert("Éxito", "La reseña ha sido eliminada correctamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al eliminar: {ex.Message}", "OK");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!_isFirstLoad)
            {
                await LoadUserReviews();
                //
            }
        }
    }
}