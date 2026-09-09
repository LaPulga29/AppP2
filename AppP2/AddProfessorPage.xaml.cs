using AppP2.Models;
using AppP2.Services;
using Microsoft.Maui.Controls;

namespace AppP2.Views
{
    public partial class AddProfessorPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        private readonly int _userId;
        private List<string> _facultades;
        private Dictionary<string, List<string>> _materiasPorFacultad;

        public AddProfessorPage(int userId)
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _userId = userId;

            LoadPickerData();
        }

        private void LoadPickerData()
        {
            // Obtener facultades y materias
            var facultadesYMaterias = _databaseService.GetFacultadesYMaterias();
            _materiasPorFacultad = new Dictionary<string, List<string>>();

            FacultyPicker.Items.Clear();
            FacultyPicker.Items.Add("-- Selecciona una facultad --");

            foreach (var item in facultadesYMaterias)
            {
                FacultyPicker.Items.Add(item.Facultad);
                _materiasPorFacultad[item.Facultad] = item.Materias;
            }

            FacultyPicker.SelectedIndex = 0;
            SubjectPicker.IsEnabled = false;
        }

        private void OnFacultyPickerChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            var selectedFaculty = picker.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedFaculty) || selectedFaculty == "-- Selecciona una facultad --")
            {
                SubjectPicker.Items.Clear();
                SubjectPicker.Items.Add("-- Selecciona una facultad primero --");
                SubjectPicker.SelectedIndex = 0;
                SubjectPicker.IsEnabled = false;
                return;
            }

            // Cargar materias de la facultad seleccionada
            SubjectPicker.Items.Clear();
            SubjectPicker.Items.Add("-- Selecciona una materia --");

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

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            // Validaciones
            var name = NameEntry.Text?.Trim();
            var selectedFaculty = FacultyPicker.SelectedItem?.ToString();
            var selectedSubject = SubjectPicker.SelectedItem?.ToString();
            var bio = BioEditor.Text?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                ValidationMessage.Text = "Debes ingresar el nombre del profesor.";
                ValidationMessage.IsVisible = true;
                return;
            }

            if (string.IsNullOrEmpty(selectedFaculty) || selectedFaculty == "-- Selecciona una facultad --")
            {
                ValidationMessage.Text = "Debes seleccionar una facultad.";
                ValidationMessage.IsVisible = true;
                return;
            }

            if (string.IsNullOrEmpty(selectedSubject) || selectedSubject == "-- Selecciona una materia --")
            {
                ValidationMessage.Text = "Debes seleccionar una materia.";
                ValidationMessage.IsVisible = true;
                return;
            }

            ValidationMessage.IsVisible = false;

            // Crear el profesor
            var professor = new Professor
            {
                Name = name,
                Subject = selectedSubject,
                Faculty = selectedFaculty,
                Bio = bio ?? "Sin biografía disponible."
            };

            try
            {
                await _databaseService.SaveProfessor(professor);

                await DisplayAlert(
                    "Éxito",
                    $"El profesor {name} ha sido agregado correctamente.\n\n" +
                    $"Facultad: {selectedFaculty}\n" +
                    $"Materia: {selectedSubject}",
                    "OK"
                );

                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error al guardar: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            var confirm = await DisplayAlert(
                "Cancelar",
                "¿Estás seguro de que deseas cancelar?",
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