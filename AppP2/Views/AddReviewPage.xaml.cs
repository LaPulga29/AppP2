using AppP2.Models;
using AppP2.Services;
using Microsoft.Maui.Controls;

namespace AppP2.Views
{
    public partial class AddReviewPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly int _professorId;
        private readonly int _userId;
        private int _selectedRating = 0;
        private Professor _professor;
        private bool _isSubmitting = false;

        public AddReviewPage(int professorId, int userId)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _professorId = professorId;
            _userId = userId;

            LoadProfessorInfo();
        }

        private async void LoadProfessorInfo()
        {
            try
            {
                _professor = await _databaseService.GetProfessorWithReviews(_professorId);
                if (_professor != null)
                {
                    ProfessorNameLabel.Text = _professor.Name;
                    SubjectLabel.Text = _professor.Subject;

                    var hasReviewed = await _databaseService.UserHasReviewedProfessor(_userId, _professorId);
                    if (hasReviewed)
                    {
                        SubmitButton.IsEnabled = false;
                        await DisplayAlert("Aviso", "Ya has reseñado a este profesor anteriormente. Solo puedes enviar una reseña por profesor.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar la información: {ex.Message}", "OK");
            }
        }

        private void OnStarClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            if (int.TryParse(button.ClassId, out int starNumber))
            {
                _selectedRating = starNumber;
                UpdateStars();
            }
        }

        private void UpdateStars()
        {
            var starButtons = new[] { Star1, Star2, Star3, Star4, Star5 };
            string[] ratingDescriptions = { "", "Muy Malo", "Malo", "Regular", "Bueno", "Excelente" };

            for (int i = 0; i < starButtons.Length; i++)
            {
                if (starButtons[i] != null)
                {
                 
                    if (i < _selectedRating)
                    {
                        starButtons[i].Text = "★";
                        starButtons[i].TextColor = Color.FromArgb("#FFD700"); 
                        starButtons[i].BackgroundColor = Colors.Transparent;  
                    }
                    else
                    {
                        starButtons[i].Text = "☆";
                        starButtons[i].TextColor = Colors.Gray;           
                        starButtons[i].BackgroundColor = Colors.Transparent; 
                    }
                }
            }

            var starsText = new string('★', _selectedRating);
            RatingLabel.Text = _selectedRating > 0
                ? $"{starsText} {_selectedRating} - {ratingDescriptions[_selectedRating]}"
                : "Selecciona una calificación";
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            if (_isSubmitting) return;
            _isSubmitting = true;
            SubmitButton.IsEnabled = false;

            try
            {
                if (_selectedRating == 0)
                {
                    ValidationMessage.Text = "⚠️ Debes seleccionar una calificación.";
                    ValidationMessage.IsVisible = true;
                    _isSubmitting = false;
                    SubmitButton.IsEnabled = true;
                    return;
                }

                if (string.IsNullOrWhiteSpace(CommentEditor.Text))
                {
                    ValidationMessage.Text = "⚠️ Debes escribir un comentario.";
                    ValidationMessage.IsVisible = true;
                    _isSubmitting = false;
                    SubmitButton.IsEnabled = true;
                    return;
                }

                if (CommentEditor.Text.Length < 10)
                {
                    ValidationMessage.Text = "⚠️ El comentario debe tener al menos 10 caracteres.";
                    ValidationMessage.IsVisible = true;
                    _isSubmitting = false;
                    SubmitButton.IsEnabled = true;
                    return;
                }

                ValidationMessage.IsVisible = false;

                var hasReviewed = await _databaseService.UserHasReviewedProfessor(_userId, _professorId);
                if (hasReviewed)
                {
                    await DisplayAlert("Error", "Ya has reseñado a este profesor.", "OK");
                    _isSubmitting = false;
                    SubmitButton.IsEnabled = true;
                    return;
                }

                var review = new Review
                {
                    ProfessorId = _professorId,
                    UserId = _userId,
                    Rating = _selectedRating,
                    Comment = CommentEditor.Text.Trim(),
                    Date = DateTime.Now
                };

                var result = await _databaseService.SaveReview(review);

                if (result > 0)
                {
                    await DisplayAlert("✅ Éxito", "¡Tu reseña ha sido publicada correctamente!", "OK");
                    MessagingService.NotifyProfessorsUpdated();
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo guardar la reseña. Intenta nuevamente.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error al guardar la reseña: {ex.Message}", "OK");
            }
            finally
            {
                _isSubmitting = false;
                SubmitButton.IsEnabled = true;
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert(
                "Cancelar",
                "¿Estás seguro de que deseas cancelar? Los cambios no se guardarán.",
                "Sí, cancelar",
                "Continuar"
            );

            if (confirm)
            {
                await Navigation.PopAsync();
            }
        }
    }
}