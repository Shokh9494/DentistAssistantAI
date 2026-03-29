using DentistAssistantAI.App.ViewModels;
using System.Collections.Specialized;

namespace DentistAssistantAI.App
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;

        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
            _viewModel.Messages.CollectionChanged += OnMessagesChanged;
        }

        private void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add || _viewModel.Messages.Count == 0)
                return;

            Dispatcher.Dispatch(() =>
                MessagesView.ScrollTo(_viewModel.Messages[^1], animate: true));
        }
    }
}
