namespace ImageCrop.MobileApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }

        public static MemoryStream ImageStream { get; set; }
    }
}