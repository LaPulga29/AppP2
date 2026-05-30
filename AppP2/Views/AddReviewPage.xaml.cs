using AppP2.Models;
using AppP2.Services;

namespace AppP2.Views
{
    public partial class AddReviewPage : ContentPage
    {
        private DatabaseService _dbService;
        private Professor _professor;

        public AddReviewPage(Professor professor)
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _professor = professor;

            if (ProfessorNameLabel != null)
            {
                ProfessorNameLabel.Text = professor.Name;
            }
        }

        private async void OnSubmitReviewClicked(object sender, EventArgs e)
        {
            var ratingText = RatingPicker?.SelectedItem?.ToString();
            var comment = CommentEditor?.Text;

            if (ratingText == null)
            {
                await DisplayAlert("Error", "Selecciona una calificación", "OK");
                return;
            }

            // Extraer el número de la calificación
            int rating = ratingText.Count(c => c == '?');

            var review = new Review
            {
                ProfessorId = _professor.Id,
                UserEmail = "usuario@ejemplo.com", // Temporal
                Rating = rating,
                Comment = comment ?? string.Empty,
                Date = DateTime.Now
            };

            await _dbService.InsertReview(review);
            await DisplayAlert("Éxito", "Reseña enviada correctamente", "OK");
            await Navigation.PopAsync();
        }
    }
}