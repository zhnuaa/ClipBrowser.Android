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
            resetInfo();
        }

       async void OnSelectVideoClicked(object sender, EventArgs args)
       {
            Button btn = (Button)sender;
            btn.IsEnabled = false;

            KeyValuePair<List<string>, int>? videoDict = await DependencyService.Get<IVideoPicker>().GetVideoFileAsync();

            if (videoDict!=null)
            {
                resetInfo();
                currentIndex = videoDict.Value.Value;
                videoList = videoDict.Value.Key;
                videoPlayer.Source = new FileVideoSource
                {
                    File = videoList[currentIndex]
                };
                updateInfo(videoList.Count, currentIndex, 0, 0);
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
                updateInfo(0, -1, 0, 0);
                System.Diagnostics.Debug.WriteLine(videoList[currentIndex]);
            }
            else
            {
                if (videoPlayer.Status == VideoStatus.Playing)
                {
                    videoPlayer.Pause();
                }
                else if (videoPlayer.Status == VideoStatus.Paused)
                {
                    videoPlayer.Play();
                }
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
                updateInfo(0, 1, 0, 0);
                System.Diagnostics.Debug.WriteLine(videoList[currentIndex]);
            }
            else
            {
                if (videoPlayer.Status == VideoStatus.Playing)
                {
                    videoPlayer.Pause();
                    updateInfo(0, 1, 0, 0);
                }       
                else if (videoPlayer.Status == VideoStatus.Paused)
                {
                    videoPlayer.Play();
                    updateInfo(0, -1, 0, 0);
                }
            }            
            btn.IsEnabled = true;
        }

        async private void OnDeleteClicked(object sender, EventArgs e)
        {            
            if (videoList.Count > 0)
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
                        File = videoList[currentIndex + 1]
                    };
                }
                updateInfo(0, 1, 1, 0);
                await Task.Run(() => {
                    if (System.IO.File.Exists(videoList[currentIndex]))
                    {
                        System.IO.File.Delete(videoList[currentIndex]);
                        videoList.RemoveAt(currentIndex);
                    }
                });
                btn.IsEnabled = true;
            }                       
        }
        async private void OnMarkClicked(object sender, EventArgs e)
        {
            if (videoList.Count > 0)
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
                        videoList[currentIndex] = newVideoName;
                    }
                });
                if (currentIndex + 1 < videoList.Count)
                {
                    currentIndex++;
                    videoPlayer.Source = new FileVideoSource
                    {
                        File = videoList[currentIndex]
                    };
                }
                updateInfo(0, 1, 0, 1);
                btn.IsEnabled = true;
            }            
        }
        private void updateInfo(int deltaTotal=0,int deltaLeft=0,int deltaDelete=0,int deltaMark=0)
        {
            videoName.Text = System.IO.Path.GetFileName(videoList[currentIndex]);
            int num = 0;
            totalNum.Text = string.Format("{0}:总数", int.TryParse(totalNum.Text.Split(':')[0], out num) ? (num + deltaTotal).ToString() : deltaTotal.ToString());
            leftNum.Text = string.Format("{0}:剩余", int.TryParse(leftNum.Text.Split(':')[0], out num) ? (num - deltaLeft).ToString() : (int.Parse(totalNum.Text.Split(':')[0])-deltaLeft).ToString());
            deleteNum.Text = string.Format("{0}:删除", int.TryParse(deleteNum.Text.Split(':')[0], out num) ? (num + deltaDelete).ToString() : deltaDelete.ToString());
            markNum.Text = string.Format("{0}:标记", int.TryParse(markNum.Text.Split(':')[0], out num) ? (num + deltaMark).ToString() : deltaMark.ToString());
        }
        private void resetInfo()
        {
            videoName.Text = "";
            totalNum.Text = "";
            leftNum.Text = "";
            deleteNum.Text = "";
            markNum.Text = "";
        }
    }
}