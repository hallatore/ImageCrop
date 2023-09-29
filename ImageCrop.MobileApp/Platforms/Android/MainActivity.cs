using Android.App;
using Android.Content.PM;

namespace ImageCrop.MobileApp
{
    //[IntentFilter(
    //    new[] { "android.intent.action.SEND" },
    //    Categories = new[] { "android.intent.category.DEFAULT" },
    //    DataMimeTypes = new[] { "image/*" })]
    [Activity(
        Theme = "@style/Maui.SplashTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize |
                               ConfigChanges.Orientation |
                               ConfigChanges.UiMode |
                               ConfigChanges.ScreenLayout |
                               ConfigChanges.SmallestScreenSize |
                               ConfigChanges.Density,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : MauiAppCompatActivity
    {
        //protected override void OnCreate(Bundle savedInstanceState)
        //{
        //    //var intent = Intent;

        //    //if (intent != null && intent.Action == "android.intent.action.SEND" && intent.Type?.StartsWith("image/") == true)
        //    //{
        //    //    var clipData = intent.ClipData?.GetItemAt(0);
        //    //    ;

        //    //    base.OnCreate(savedInstanceState);
        //    //}
        //    //else
        //    //{
        //    //    base.OnCreate(savedInstanceState);
        //    //}
        //}
    }
}