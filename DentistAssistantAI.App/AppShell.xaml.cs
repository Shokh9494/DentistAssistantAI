using DentistAssistantAI.App.Views;

namespace DentistAssistantAI.App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("patientdetail", typeof(PatientDetailPage));

            if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
                BuildDesktopNavigation();
            else
                BuildMobileNavigation();
        }

        private void BuildDesktopNavigation()
        {
            FlyoutBehavior = FlyoutBehavior.Locked;

            var chatItem = new FlyoutItem { Title = "Chat", Icon = "icon_chat.png" };
            chatItem.Items.Add(new ShellContent
            {
                ContentTemplate = new DataTemplate(typeof(MainPage))
            });

            var patientsItem = new FlyoutItem { Title = "Patients", Icon = "icon_patients.png" };
            patientsItem.Items.Add(new ShellContent
            {
                ContentTemplate = new DataTemplate(typeof(PatientsPage))
            });

            Items.Add(chatItem);
            Items.Add(patientsItem);

            FlyoutIsPresented = true;
        }

        private void BuildMobileNavigation()
        {
            FlyoutBehavior = FlyoutBehavior.Disabled;

            var tabBar = new TabBar();
            tabBar.Items.Add(new ShellContent
            {
                Title = "Chat",
                Icon = "icon_chat.png",
                ContentTemplate = new DataTemplate(typeof(MainPage))
            });
            tabBar.Items.Add(new ShellContent
            {
                Title = "Patients",
                Icon = "icon_patients.png",
                ContentTemplate = new DataTemplate(typeof(PatientsPage))
            });

            Items.Add(tabBar);
        }
    }
}
