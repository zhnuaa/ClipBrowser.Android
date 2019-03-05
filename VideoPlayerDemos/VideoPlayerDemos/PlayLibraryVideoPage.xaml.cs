using System;
using Xamarin.Forms;
using FormsVideoLibrary;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace ClipBrowser
{
    public partial class PlayLibraryVideoPage : ContentPage
    {
        public VideoListStatus status;
        public PlayLibraryVideoPage()
        {
            InitializeComponent();
            status = new VideoListStatus();            
            rootStack.BindingContext = status;
        }

       async void OnSelectVideoClicked(object sender, EventArgs args)
       {
            ImageButton btn = (ImageButton)sender;
            btn.IsEnabled = false;

            KeyValuePair<List<string>, int>? videoDict = await DependencyService.Get<IVideoPicker>().GetVideoFileAsync();
            status.CountReset();
            if (videoDict!=null)
            {
                status.VideoList = videoDict.Value.Key;
                status.Index = videoDict.Value.Value;
            }
            btn.IsEnabled = true;
       }

        private void OnPreviousClicked(object sender, EventArgs e)
        {
            ImageButton btn = (ImageButton)sender;
            btn.IsEnabled = false;
            if (status.Index == 0)
            {
                DisplayAlert(@"提示", @"已经到达第一个视频", @"确认");
            }
            else
            {
                status.Index--;
            }
            btn.IsEnabled = true;
        }

        private void OnNextClicked(object sender, EventArgs e)
        {
            ImageButton btn = (ImageButton)sender;
            btn.IsEnabled = false;
            if (status.Index == status.VideoList.Count - 1)
            {
                videoPlayer.Stop();
                DisplayAlert(@"提示",@"已经到达最后一个视频",@"确认");
            }
            else
            {
                status.Index++;
            }
            btn.IsEnabled = true;
        }

        private void OnMirrorScreenClicked(object sender, EventArgs e)
        {
            ImageButton btn = (ImageButton)sender;
            btn.IsEnabled = false;
            status.IsLeftHanded = !status.IsLeftHanded;
            btn.IsEnabled = true;
        }

        async private void OnDeleteClicked(object sender, EventArgs e)
        {            
            if (status.VideoList.Count > 0)
            {
                ImageButton btn = (ImageButton)sender;
                btn.IsEnabled = false;
                if (videoPlayer.Status == VideoStatus.Playing)
                {
                    videoPlayer.Stop();
                }
                await Task.Run(() => {
                    if (System.IO.File.Exists(status.CurrentVideo.File))
                    {
                        System.IO.File.Delete(status.CurrentVideo.File);
                        status.VideoList.RemoveAt(status.Index);
                    }
                });
                status.DeleteNum++;
                if (status.Index >= status.VideoList.Count)
                {
                    await DisplayAlert(@"提示", @"已经到达最后一个视频", @"确认");
                }
                else
                {
                    status.Index = status.Index;
                }
                btn.IsEnabled = true;
            }                       
        }
        async private void OnMarkClicked(object sender, EventArgs e)
        {
            if (status.VideoList.Count > 0)
            {
                ImageButton btn = (ImageButton)sender;
                btn.IsEnabled = false;
                if (videoPlayer.Status == VideoStatus.Playing)
                {
                    videoPlayer.Stop();
                }
                await Task.Run(() => {
                    var video = status.CurrentVideo.File;
                    if (System.IO.File.Exists(video))
                    {
                        var newVideoName = System.IO.Path.ChangeExtension(video, ".mark.mp4");
                        System.IO.File.Move(video, newVideoName);
                        status.VideoList[status.Index] = newVideoName;
                    }
                });
                status.MarkNum++;
                if (status.Index == status.VideoList.Count - 1)
                {
                    await DisplayAlert(@"提示", @"已经到达最后一个视频", @"确认");
                }
                else
                {
                    status.Index++;
                }
                btn.IsEnabled = true;
            }            
        }
    }


    public class VideoListStatus : System.ComponentModel.INotifyPropertyChanged
    {
        public int TotalNum { get; private set; }
        //total video number
        private List<string> videoList;
        public List<string> VideoList
    {
            get { return videoList; }
            set
            {
                videoList = value;
                TotalNum = videoList.Count;
                NotifyPropertyChanged("TotalNum");
                NotifyPropertyChanged("VideoList");
            }
        }
        //video number left in list
        public int LeftNum
        {
            get
            {
                if (videoList.Count > 0)
                {
                    return VideoList.Count - index - 1;
                }
                else
                {
                    return 0;
                }
            }
        }
        //video number marked
        private int markNum;
        public int MarkNum
        {
            get { return markNum; }
            set
            {
                markNum = value;
                NotifyPropertyChanged("MarkNum");
            }
        }
        //video number deleted
        private int deleteNum;
        public int DeleteNum
        {
            get { return deleteNum; }
            set
            {
                deleteNum = value;
                NotifyPropertyChanged("DeleteNum");
            }
        }
        
        //current video playing
        public string CurrentVideoName
        {
            get
            {
                if (videoList.Count > 0)
                {
                    return System.IO.Path.GetFileName(VideoList[index]);
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        public FileVideoSource CurrentVideo
        {
            get
            {
                if (videoList.Count > 0)
                {
                    return new FileVideoSource() { File = VideoList[index] };
                }
                else
                {
                    return null;
                }
            }
        }
        //index of video is playing
        private int index;        
        public int Index
        {
            get { return index; }
            set
            {
                index = value;
                NotifyPropertyChanged("Index");
                NotifyPropertyChanged("CurrentVideo");
                NotifyPropertyChanged("CurrentVideoName");
                NotifyPropertyChanged("LeftNum");
            }
        }
        //handed mode
        private bool isLeftHanded;
        public bool IsLeftHanded
        {
            get { return isLeftHanded; }
            set
            {
                isLeftHanded = value;
                NotifyPropertyChanged("IsLeftHanded");
                NotifyPropertyChanged("ScreenDirection");
            }
        }
        //screen flowdirection
        public FlowDirection ScreenDirection
        {
            get { return isLeftHanded ? FlowDirection.RightToLeft : FlowDirection.LeftToRight; }
        }
        //构造函数
        public VideoListStatus()
        {
            isLeftHanded = false;
            videoList = new List<string>();
            VideoList = new List<string>();            
            Index = 0;
            MarkNum = 0;
            DeleteNum = 0;
        }
        public void CountReset()
        {
            VideoList = new List<string>();            
            Index = 0;
            MarkNum = 0;
            DeleteNum = 0;
        }

        //注册属性改变事件，便于通告属性改变
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop));
        }
    }


}