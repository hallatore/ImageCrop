using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace ImageCrop.MobileApp
{
    [IntentFilter(
        new[] {"android.intent.action.SEND"},
        Categories = new[] {"android.intent.category.DEFAULT"},
        DataMimeTypes = new[] {"image/*"})]
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        LaunchMode = LaunchMode.SingleInstance,
        ConfigurationChanges = ConfigChanges.ScreenSize |
                               ConfigChanges.Orientation |
                               ConfigChanges.UiMode |
                               ConfigChanges.ScreenLayout |
                               ConfigChanges.SmallestScreenSize |
                               ConfigChanges.Density,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Intent?.Action == Intent.ActionSend && Intent.ClipData != null)
            {
                LoadIntent(Intent);
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            LoadIntent(intent);
        }

        private void LoadIntent(Intent intent)
        {
            if (intent?.Action != Intent.ActionSend || intent.ClipData == null)
            {
                return;
            }

            var file = intent.ClipData.GetItemAt(0);

            if (file?.Uri == null)
            {
                return;
            }

            var fileStream = ContentResolver?.OpenInputStream(file.Uri);

            if (fileStream == null)
            {
                return;
            }

            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            if (Shell.Current.CurrentPage is MainPage mainPage)
            {
                Dispatcher.GetForCurrentThread()!.DispatchAsync(() => mainPage.LoadFromStream(memoryStream));
            }
            else
            {
                App.ImageStream = memoryStream;
            }
        }
    }
}