using DentistAssistantAI.App.ViewModels;

namespace DentistAssistantAI.App.Views
{
    public partial class PatientsPage : ContentPage
    {
        private readonly PatientsPageViewModel _viewModel;

        public PatientsPage(PatientsPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.Patients.Count == 0)
                _viewModel.LoadPatientsCommand.Execute(null);
        }
    }
}
