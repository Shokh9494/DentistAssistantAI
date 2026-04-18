using DentistAssistantAI.App.ViewModels;

namespace DentistAssistantAI.App.Views;

public partial class StudentPage : ContentPage
{
    public StudentPage(StudentPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
