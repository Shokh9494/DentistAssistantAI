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

            var chatItem = new FlyoutItem { Title = "Chat", Icon = "icon_chat.svg" };
            chatItem.Items.Add(new ShellContent { ContentTemplate = new DataTemplate(typeof(MainPage)) });

            var teacherItem = new FlyoutItem { Title = "Teacher", Icon = "icon_teacher.svg" };
            teacherItem.Items.Add(new ShellContent { ContentTemplate = new DataTemplate(typeof(TeacherPage)) });

            var studentItem = new FlyoutItem { Title = "Student", Icon = "icon_student.svg" };
            studentItem.Items.Add(new ShellContent { ContentTemplate = new DataTemplate(typeof(StudentPage)) });

            var patientsItem = new FlyoutItem { Title = "Patients", Icon = "icon_patients.svg" };
            patientsItem.Items.Add(new ShellContent { ContentTemplate = new DataTemplate(typeof(PatientsPage)) });

            Items.Add(chatItem);
            Items.Add(teacherItem);
            Items.Add(studentItem);
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
                Icon = "icon_chat.svg",
                ContentTemplate = new DataTemplate(typeof(MainPage))
            });
            tabBar.Items.Add(new ShellContent
            {
                Title = "Teacher",
                Icon = "icon_teacher.svg",
                ContentTemplate = new DataTemplate(typeof(TeacherPage))
            });
            tabBar.Items.Add(new ShellContent
            {
                Title = "Student",
                Icon = "icon_student.svg",
                ContentTemplate = new DataTemplate(typeof(StudentPage))
            });
            tabBar.Items.Add(new ShellContent
            {
                Title = "Patients",
                Icon = "icon_patients.svg",
                ContentTemplate = new DataTemplate(typeof(PatientsPage))
            });

            Items.Add(tabBar);
        }
    }
}
