using System.Collections.ObjectModel;
using AppP2.Models;
using AppP2.Services;
using Microsoft.Maui.Controls;

namespace AppP2.Views
{
    public partial class ProfessorDetailPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private Professor _professor;
        private readonly int _professorId;
        private readonly int _currentUserId;
        private bool _isLoading = false;

        public ObservableCollection<Review> Reviews { get; set; } = new();

        public ProfessorDetailPage(int professorId, int userId)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _professorId = professorId;
            _currentUserId = userId;
            BindingContext = this;
            LoadProfessorData();
        }

        private async void LoadProfessorData()
        {
            if (_isLoading) return;
            _isLoading = true;

            try
            {
                _professor = await _databaseService.GetProfessorWithReviews(_professorId);

                if (_professor != null)
                {
                    ProfessorNameLabel.Text = _professor.Name;
                    SubjectLabel.Text = $" {_professor.Subject}";
                    FacultyLabel.Text = $" {_professor.Faculty}";
                    BioLabel.Text = _professor.Bio ?? "Sin biografía disponible.";

                    UpdateRatingDisplay();
                    await LoadReviews();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar: {ex.Message}", "OK");
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void UpdateRatingDisplay()
        {
            var average = _professor.AverageRating;
            AverageRatingLabel.Text = average > 0 ? average.ToString("F1") : "Sin calificaciones";

            var stars = GetStarsString(average);
            StarsLabel.Text = stars;

            ReviewsCountLabel.Text = $"{_professor.Reviews.Count} reseñas";
        }
        private string GetStarsString(double rating)
        {
            var fullStars = (int)Math.Floor(rating);
            var halfStar = rating - fullStars >= 0.5;
            var emptyStars = 5 - fullStars - (halfStar ? 1 : 0);

            var stars = new string('⭐', fullStars);
            if (halfStar) stars += "½"; 
            stars += new string('⭐', emptyStars);

            if (rating == 0) stars = "⭐";
            return stars;
        }

        private async Task LoadReviews()
        {
            try
            {
                Reviews.Clear();

                var reviews = await _databaseService.GetReviewsByProfessor(_professorId);

                foreach (var review in reviews)
                {
                    var user = await _databaseService.GetUserById(review.UserId);
                    review.UserName = user?.FullName ?? "Usuario desconocido";
                    review.IsOwnReview = review.UserId == _currentUserId;
                    Reviews.Add(review);
                }

                NoReviewsLabel.IsVisible = !Reviews.Any();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Error en LoadReviews: {ex.Message}");
            }
        }

        private async void AddReviewButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddReviewPage(_professorId, _currentUserId));
        }

        private async void DeleteReview_Clicked(object sender, EventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button == null) return;

                var reviewId = (int)button.CommandParameter;
                var review = Reviews.FirstOrDefault(r => r.Id == reviewId);

                if (review != null && review.UserId == _currentUserId)
                {
                    var confirm = await DisplayAlert(
                        "Confirmar Eliminación",
                        "¿Estás seguro de que deseas eliminar esta reseña?",
                        "Sí, Eliminar",
                        "Cancelar"
                    );

                    if (confirm)
                    {
                        await _databaseService.DeleteReview(reviewId);

                        //Eliminar de la UI
                        Reviews.Remove(review);

                        // Actualizar promedio
                        _professor = await _databaseService.GetProfessorWithReviews(_professorId);
                        UpdateRatingDisplay();

                        await DisplayAlert("Éxito", "La reseña ha sido eliminada correctamente.", "OK");

                        // Notificar actualización
                        MessagingService.NotifyProfessorsUpdated();
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No tienes permiso para eliminar esta reseña.", "OK");
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

            if (_professor != null)
            {
                _professor = await _databaseService.GetProfessorWithReviews(_professorId);
                UpdateRatingDisplay();
                await LoadReviews();
            }
        }
    }
}