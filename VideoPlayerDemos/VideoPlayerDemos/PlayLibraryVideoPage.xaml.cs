using System;
using Xamarin.Forms;
using FormsVideoLibrary;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClipBrowser
{
    public partial class PlayLibraryVideoPage : ContentPage
    {
        private int currentIndex;
        public List<string> videoList;

        public PlayLibraryVideoPage()
        {
            InitializeComponent();
            currentIndex = 0;
            videoList = new List<string>();
            videoName.Text = "";
        }

       async void OnSelectVideoClicked(object sender, EventArgs args)
       {
            Button btn = (Button)sender;
            btn.IsEnabled = false;

            KeyValuePair<List<string>, int>? videoDict = await DependencyService.Get<IVideoPicker>().GetVideoFileAsync();

            if (videoDict!=null)
            {
                currentIndex = videoDict.Value.Value;
                videoList = videoDict.Value.Key;
                videoPlayer.Source = new FileVideoSource
                {
                    File = videoList[currentIndex]
                };
                videoName.Text = System.IO.Path.GetFileName(videoList[currentIndex]);
            }
            btn.IsEnabled = true;
       }

        private void OnPreviousClicked(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.IsEnabled = false;
            if (currentIndex-1 >= 0)
            {
                currentIndex--;
                if (videoPlayer.Status == VideoStatus.Playing)
                {
                    videoPlayer.Stop();
                }
                videoPlayer.Source = new FileVideoSource
                {
                    File = videoList[currentIndex]
                };
                videoName.Text = System.IO.Path.GetFileName(videoList[currentIndex]);
                System.Diagnostics.Debug.WriteLine(videoList[currentIndex]);
            }
            else
            {
                videoPlayer.Stop();
            }
            btn.IsEnabled = true;
        }

        private void OnNextClicked(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.IsEnabled = false;
            if (currentIndex + 1 < videoList.Count)
            {
                currentIndex++;
                if(videoPlayer.Status==VideoStatus.Playing)
                {
                    videoPlayer.Stop();
                }
                videoPlayer.Source = new FileVideoSource
                {
                    File = videoList[currentIndex]
                };
                videoName.Text = System.IO.Path.GetFileName(videoList[currentIndex]);
                System.Diagnostics.Debug.WriteLine(videoList[currentIndex]);
            }
            else
            {
                videoPlayer.Stop();
            }
            btn.IsEnabled = true;
        }
        async private void OnDeleteClicked(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.IsEnabled = false;
            if (videoPlayer.Status == VideoStatus.Playing)
            {
                videoPlayer.Stop();
            }
            if (currentIndex + 1 < videoList.Count)
            {
                videoPlayer.Source = new FileVideoSource
                {
                    File = videoList[currentIndex+1]
                };
                videoName.Text = System.IO.Path.GetFileName(videoList[currentIndex]);
            }
            await Task.Run(() => {
                if (System.IO.File.Exists(videoList[currentIndex]))
                {
                    System.IO.File.Delete(videoList[currentIndex]);
                    videoList.RemoveAt(currentIndex);
                }
            });
            btn.IsEnabled = true;
        }
        async private void OnMarkClicked(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.IsEnabled = false;
            if (videoPlayer.Status == VideoStatus.Playing)
            {
                videoPlayer.Stop();
            }
            await Task.Run(() => {
                var video = videoList[currentIndex];
                if (System.IO.File.Exists(video))
                {
                    var newVideoName = System.IO.Path.ChangeExtension(video, ".mark.mp4");
                    System.IO.File.Move(video, newVideoName);
                    videoList[currentIndex]=newVideoName;
                }
            });
            if (currentIndex + 1 < videoList.Count)
            {
                currentIndex++;
                videoPlayer.Source = new FileVideoSource
                {
                    File = videoList[currentIndex]
                };
                videoName.Text = System.IO.Path.GetFileName(videoList[currentIndex]);
            }
            btn.IsEnabled = true;
        }
    }
}