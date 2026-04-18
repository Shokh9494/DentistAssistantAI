using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentistAssistantAI.App.Models;
using DentistAssistantAI.Core.Interfaces;
using DentistAssistantAI.Core.Models;
using System.Collections.ObjectModel;

namespace DentistAssistantAI.App.ViewModels
{
    [QueryProperty(nameof(Patient), "patient")]
    public partial class PatientDetailViewModel : ObservableObject
    {
        private readonly IVisitService _visitService;

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(
            nameof(PatientInitials),
            nameof(PatientName),
            nameof(PatientPhone),
            nameof(PatientDob),
            nameof(HasPhone),
            nameof(HasDob))]
        public partial Patient? Patient { get; set; }

        public ObservableCollection<VisitItem> Visits { get; } = [];

        public string PatientInitials => Patient?.Initials ?? string.Empty;
        public string PatientName     => Patient?.Name ?? string.Empty;
        public string PatientPhone    => Patient?.Phone ?? string.Empty;
        public string PatientDob      => Patient?.DateOfBirthText ?? string.Empty;
        public bool   HasPhone        => !string.IsNullOrEmpty(Patient?.Phone);
        public bool   HasDob          => Patient?.DateOfBirth.HasValue ?? false;

        public PatientDetailViewModel(IVisitService visitService)
        {
            _visitService = visitService;
        }

        partial void OnPatientChanged(Patient? value)
        {
            if (value is not null)
                LoadVisitsCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadVisitsAsync()
        {
            if (Patient is null) return;
            IsBusy = true;
            try
            {
                var records = await _visitService.GetByPatientIdAsync(Patient.Id);
                Visits.Clear();
                foreach (var r in records)
                    Visits.Add(new VisitItem(r));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private Task NewVisitAsync() => Task.CompletedTask;
    }
}
