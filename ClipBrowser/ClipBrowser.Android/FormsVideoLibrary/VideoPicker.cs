using System;
using System.Threading.Tasks;
using Android.Content;
using Xamarin.Forms;

// Need application's MainActivity
using ClipBrowser.Droid;
using System.Collections.Generic;

[assembly: Dependency(typeof(FormsVideoLibrary.Droid.VideoPicker))]

namespace FormsVideoLibrary.Droid
{
    public class VideoPicker : IVideoPicker
    {
        public Task<KeyValuePair<List<string>, int>?> GetVideoFileAsync()
        {
            // Define the Intent for getting images
            Intent intent = new Intent();
            intent.SetType("video/*");
            intent.SetAction(Intent.ActionGetContent);
            
            // Get the MainActivity instance
            MainActivity activity = MainActivity.Current;

            // Start the picture-picker activity (resumes in MainActivity.cs)
            activity.StartActivityForResult(
                Intent.CreateChooser(intent, "Select Video"),
                MainActivity.PickImageId);

            // Save the TaskCompletionSource object as a MainActivity property
            activity.PickImageTaskCompletionSource = new TaskCompletionSource<KeyValuePair<List<string>, int>?>();

            // Return Task object
            return activity.PickImageTaskCompletionSource.Task;
        }
    }
}