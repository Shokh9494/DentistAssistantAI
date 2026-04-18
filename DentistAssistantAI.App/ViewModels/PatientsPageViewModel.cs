using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentistAssistantAI.Core.Interfaces;
using DentistAssistantAI.Core.Models;
using System.Collections.ObjectModel;

namespace DentistAssistantAI.App.ViewModels
{
    public partial class PatientsPageViewModel : ObservableObject
    {
        private readonly IPatientService _patientService;

        [ObservableProperty]
        private bool _isBusy;

        public ObservableCollection<Patient> Patients { get; } = [];

        public PatientsPageViewModel(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [RelayCommand]
        private async Task LoadPatientsAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var patients = await _patientService.GetAllAsync();
                Patients.Clear();
                foreach (var p in patients)
                    Patients.Add(p);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task AddPatientAsync()
        {
            string[] names = ["Smith John", "Brown Anna", "Taylor Mike", "Davis Sara", "Wilson Alex"];
            var patient = new Patient
            {
                Name = names[Random.Shared.Next(names.Length)],
                LastVisit = DateTime.Now
            };
            await _patientService.AddAsync(patient);
            Patients.Insert(0, patient);
        }

        [RelayCommand]
        private static async Task SelectPatientAsync(Patient patient)
        {
            await Shell.Current.GoToAsync("patientdetail", new Dictionary<string, object>
            {
                { "patient", patient }
            });
        }
    }
}
