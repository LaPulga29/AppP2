using System.Collections.ObjectModel;
using AppP2.Models;
using AppP2.Services;
using Microsoft.Maui.Controls;

namespace AppP2.Views
{
    public partial class ProfessorListPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private List<Professor> _allProfessors;
        private readonly int _currentUserId;
        private readonly bool _isSelectionMode;
        private Dictionary<string, List<string>> _materiasPorFacultad;
        private string _selectedFaculty = string.Empty;

        public ObservableCollection<Professor> Professors { get; set; } = new();

        public ProfessorListPage(int userId) : this(userId, false)
        {
        }

        public ProfessorListPage(int userId, bool isSelectionMode)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _currentUserId = userId;
            _isSelectionMode = isSelectionMode;

            if (_isSelectionMode)
            {
                Title = "Seleccionar Profesor";
            }

            LoadPickerData();
            LoadProfessors();

            MessagingService.ProfessorsUpdated += OnProfessorsUpdated;
        }

        private void LoadPickerData()
        {
            try
            {
                var facultadesYMaterias = _databaseService.GetFacultadesYMaterias();
                _materiasPorFacultad = new Dictionary<string, List<string>>();

                FacultyPicker.Items.Clear();
                FacultyPicker.Items.Add("-- Todas las facultades --");

                foreach (var item in facultadesYMaterias)
                {
                    FacultyPicker.Items.Add(item.Facultad);
                    _materiasPorFacultad[item.Facultad] = item.Materias;
                }

                FacultyPicker.SelectedIndex = 0;
                SubjectPicker.IsEnabled = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en LoadPickerData: {ex.Message}");
            }
        }

        private async 
        Task
LoadProfessors()
        {
            try
            {
                FacultyPicker.IsVisible = false;
                SubjectPicker.IsVisible = false;
                ApplyFilterButton.IsVisible = false;
                SearchEntry.Text = string.Empty;

                _allProfessors = await _databaseService.GetProfessorsWithReviews();
                await UpdateProfessorsList(_allProfessors);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar profesores: {ex.Message}", "OK");
            }
        }

        private async Task UpdateProfessorsList(List<Professor> filteredProfessors)
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Professors.Clear();
                    foreach (var professor in filteredProfessors)
                    {
                        Professors.Add(professor);
                    }

                    OnPropertyChanged(nameof(Professors));

                    if (ProfessorsCollectionView != null)
                    {
                        ProfessorsCollectionView.ItemsSource = null;
                        ProfessorsCollectionView.ItemsSource = Professors;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en UpdateProfessorsList: {ex.Message}");
            }
        }

        // ==================== ELIMINAR PROFESOR ====================
        private async void OnDeleteProfessorClicked(object sender, EventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button == null) return;

                var professorId = (int)button.CommandParameter;
                var professor = _allProfessors.FirstOrDefault(p => p.Id == professorId);

                if (professor == null) return;

                // Confirmar eliminación
                var confirm = await DisplayAlert(
                    "Eliminar Profesor",
                    $"¿Estás seguro de que deseas eliminar a {professor.Name}?\n\n" +
                    $"Materia: {professor.Subject}\n" +
                    $"Facultad: {professor.Faculty}\n" +
                    $"{professor.Reviews.Count} reseñas asociadas serán eliminadas.",
                    "Sí, Eliminar",
                    "Cancelar"
                );

                if (!confirm) return;

                // Eliminar el profesor (esto también eliminará las reseñas asociadas)
                var result = await _databaseService.DeleteProfessorWithReviews(professorId);

                if (result > 0)
                {
                    await DisplayAlert("Éxito", $"El profesor {professor.Name} ha sido eliminado correctamente.", "OK");

                    // Recargar la lista
                    _allProfessors = await _databaseService.GetProfessorsWithReviews();
                    await UpdateProfessorsList(_allProfessors);

                    // Notificar actualización
                    MessagingService.NotifyProfessorsUpdated();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo eliminar el profesor.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al eliminar: {ex.Message}", "OK");
            }
        }

        // ==================== EVENTOS DE FILTROS ====================

        private void OnFacultyPickerChanged(object sender, EventArgs e)
        {
            try
            {
                var picker = (Picker)sender;
                var selectedFaculty = picker.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(selectedFaculty) || selectedFaculty == "-- Todas las facultades --")
                {
                    SubjectPicker.Items.Clear();
                    SubjectPicker.Items.Add("-- Selecciona una facultad primero --");
                    SubjectPicker.SelectedIndex = 0;
                    SubjectPicker.IsEnabled = false;
                    _selectedFaculty = string.Empty;
                    return;
                }

                _selectedFaculty = selectedFaculty;

                SubjectPicker.Items.Clear();
                SubjectPicker.Items.Add("-- Todas las materias --");

                if (_materiasPorFacultad.ContainsKey(selectedFaculty))
                {
                    foreach (var materia in _materiasPorFacultad[selectedFaculty])
                    {
                        SubjectPicker.Items.Add(materia);
                    }
                    SubjectPicker.IsEnabled = true;
                    SubjectPicker.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en OnFacultyPickerChanged: {ex.Message}");
            }
        }

        private async void OnApplyFilterClicked(object sender, EventArgs e)
        {
            try
            {
                _allProfessors = await _databaseService.GetProfessorsWithReviews();

                List<Professor> filteredProfessors = new List<Professor>(_allProfessors);

                if (!string.IsNullOrEmpty(_selectedFaculty))
                {
                    filteredProfessors = filteredProfessors
                        .Where(p => p.Faculty == _selectedFaculty)
                        .ToList();
                }

                var selectedSubject = SubjectPicker.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedSubject) && selectedSubject != "-- Todas las materias --")
                {
                    filteredProfessors = filteredProfessors
                        .Where(p => p.Subject == selectedSubject)
                        .ToList();
                }

                await UpdateProfessorsList(filteredProfessors);

                FacultyPicker.IsVisible = false;
                SubjectPicker.IsVisible = false;
                ApplyFilterButton.IsVisible = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al aplicar filtro: {ex.Message}", "OK");
            }
        }

        private void OnFilterByFacultyClicked(object sender, EventArgs e)
        {
            FacultyPicker.IsVisible = true;
            SubjectPicker.IsVisible = true;
            ApplyFilterButton.IsVisible = true;
            SubjectPicker.IsEnabled = false;
            FacultyPicker.SelectedIndex = 0;
        }

        private void OnFilterBySubjectClicked(object sender, EventArgs e)
        {
            FacultyPicker.IsVisible = true;
            SubjectPicker.IsVisible = true;
            ApplyFilterButton.IsVisible = true;
            SubjectPicker.IsEnabled = false;
            FacultyPicker.SelectedIndex = 0;
        }

        private async void OnProfessorSelected(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedProfessor = e.CurrentSelection.FirstOrDefault() as Professor;
                if (selectedProfessor != null)
                {
                    if (_isSelectionMode)
                    {
                        await Navigation.PushAsync(new AddReviewPage(selectedProfessor.Id, _currentUserId));
                    }
                    else
                    {
                        await Navigation.PushAsync(new ProfessorDetailPage(selectedProfessor.Id, _currentUserId));
                    }
                    ((CollectionView)sender).SelectedItem = null;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al seleccionar profesor: {ex.Message}", "OK");
            }
        }

        // ==================== BÚSQUEDA ====================

        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(e.NewTextValue))
                {
                    _allProfessors = await _databaseService.GetProfessorsWithReviews();
                    await UpdateProfessorsList(_allProfessors);
                }
                else
                {
                    var filtered = await _databaseService.SearchProfessorsByName(e.NewTextValue);
                    await UpdateProfessorsList(filtered);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en OnSearchTextChanged: {ex.Message}");
            }
        }

        private async void OnSearchClicked(object sender, EventArgs e)
        {
            try
            {
                var searchTerm = SearchEntry.Text?.Trim();
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    _allProfessors = await _databaseService.GetProfessorsWithReviews();
                    await UpdateProfessorsList(_allProfessors);
                }
                else
                {
                    var filtered = await _databaseService.SearchProfessorsByName(searchTerm);
                    await UpdateProfessorsList(filtered);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error en búsqueda: {ex.Message}", "OK");
            }
        }

        private async void OnShowAllClicked(object sender, EventArgs e)
        {
            try
            {
                FacultyPicker.IsVisible = false;
                SubjectPicker.IsVisible = false;
                ApplyFilterButton.IsVisible = false;
                SearchEntry.Text = string.Empty;

                _allProfessors = await _databaseService.GetProfessorsWithReviews();
                await UpdateProfessorsList(_allProfessors);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al mostrar todos: {ex.Message}", "OK");
            }
        }

        // ==================== ACTUALIZAR CUANDO SE AGREGA UN PROFESOR ====================
        private async void OnProfessorsUpdated()
        {
            // ? Usar async void para eventos
            await LoadProfessors();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                _allProfessors = await _databaseService.GetProfessorsWithReviews();
                await UpdateProfessorsList(_allProfessors);

                FacultyPicker.IsVisible = false;
                SubjectPicker.IsVisible = false;
                ApplyFilterButton.IsVisible = false;
                SearchEntry.Text = string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en OnAppearing: {ex.Message}");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MessagingService.ProfessorsUpdated -= OnProfessorsUpdated;
        }
    }
}