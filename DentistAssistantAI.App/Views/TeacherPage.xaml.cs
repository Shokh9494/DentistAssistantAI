using DentistAssistantAI.App.ViewModels;

namespace DentistAssistantAI.App.Views;

public partial class TeacherPage : ContentPage
{
    public TeacherPage(TeacherPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
