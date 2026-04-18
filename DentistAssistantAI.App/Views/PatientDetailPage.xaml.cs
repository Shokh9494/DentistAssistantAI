using DentistAssistantAI.App.ViewModels;

namespace DentistAssistantAI.App.Views
{
    public partial class PatientDetailPage : ContentPage
    {
        public PatientDetailPage(PatientDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private async void OnBackClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("..");
    }
}
