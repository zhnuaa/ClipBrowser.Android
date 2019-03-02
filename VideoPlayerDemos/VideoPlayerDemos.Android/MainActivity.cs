using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;
using System.Collections.Generic;
using Android.Provider;

namespace ClipBrowser.Droid
{
    [Activity(Label = "ClipBrowser", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            Current = this;

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            SetTheme(Android.Resource.Style.ThemeNoTitleBarFullScreen);
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        // Field, properties, and method for Video Picker
        public static MainActivity Current { private set; get; }

        public static readonly int PickImageId = 1000;

        public TaskCompletionSource<KeyValuePair<List<string>, int>?> PickImageTaskCompletionSource { set; get; }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == PickImageId)
            {
                if ((resultCode == Result.Ok) && (data != null))
                {                    
                    var imagePicked = AndroidNetUriToPath.GetPath(this,data.Data);
                    System.Diagnostics.Debug.WriteLine(imagePicked);
                    string imageDir = System.IO.Path.GetDirectoryName(imagePicked);
                    var imageList = new List<string>(System.IO.Directory.GetFiles(imageDir, "*.mp4"));
                    int index = imageList.IndexOf(imagePicked);                  
                    KeyValuePair<List<string>, int>? result = new KeyValuePair<List<string>, int>(imageList, index);
                    PickImageTaskCompletionSource.SetResult(result);
                }
                else
                {
                    PickImageTaskCompletionSource.SetResult(null);
                }
            }
        }
    }
}

