using System;
using Xamarin.Forms;
using FormsVideoLibrary;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using System.IO;
using System.ComponentModel;

namespace ClipBrowser
{    
    public partial class PlayLibraryVideoPage : ContentPage
    {
        private string configFile;
        private AppConfig config;
        public VideoListStatus status;

        public PlayLibraryVideoPage()
        {
            InitializeComponent();
            configFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "config.xml");
            config = AppConfig.LoadFromDisk(configFile);
            status = new VideoListStatus();            
            rootStack.BindingContext = status;
            status.IsLeftHanded = config.IsLeftHanded;
            status.PropertyChanged += UpdateInfoToConfig;
        }

        private void UpdateInfoToConfig(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Index")
            {
                config.Index = status.Index;
            }
            else if (e.PropertyName == "VideoList")
            {
                config.VideoList = status.VideoList;
            }
            else if (e.PropertyName == "IsLeftHanded")
            {
                config.IsLeftHanded = status.IsLeftHanded;
            }
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
                status.IsEdited = true;
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
                status.IsEdited = true;
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
                        string newVideoName;
                        if (video.EndsWith(".mark.mp4"))
                        {
                            newVideoName = video.Replace(".mark.mp4", ".mp4");
                        }
                        else
                        {
                            newVideoName = System.IO.Path.ChangeExtension(video, ".mark.mp4");
                        }                        
                        System.IO.File.Move(video, newVideoName);
                        status.VideoList[status.Index] = newVideoName;
                    }
                });
                status.IsEdited = true;
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
         private void SaveConfigOnDisappearing(object sender, EventArgs e)
        {
            config.SaveToDisk();
        }

        async private void LoadConfigOnAppearing(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("路径:{0}", configFile));
            //if browse record in config is valid,try to load it
            if (config.VideoList != null)
            {
                if (config.VideoList.Count > 0 && config.VideoList.Count > config.Index)
                {
                    string currentVideo = config.VideoList[config.Index];
                    if (File.Exists(currentVideo))
                    {
                        string videoDir = Path.GetDirectoryName(currentVideo);
                        List<string> videoList = await Task<List<string>>.Run(() =>
                        {
                            return new List<string>(Directory.GetFiles(videoDir, "*.mp4", SearchOption.TopDirectoryOnly));
                        });
                        var action = await DisplayActionSheet("发现浏览记录", null, null, "忽略记录", "继续浏览");
                        if (action == "继续浏览")
                        {
                            status.VideoList = videoList;
                            status.Index = videoList.IndexOf(currentVideo);
                            status.IsEdited = true;
                        }
                    }
                }
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
                    return VideoList.Count - index;
                }
                else
                {
                    return 0;
                }
            }
        }
        //video number marked
        public int MarkNum
        {
            get { return videoList.FindAll(video => { return video.EndsWith(".mark.mp4"); }).Count; }            
        }
        //video number deleted
        public int DeleteNum
        {
            get { return TotalNum - videoList.Count; }
        }
        private bool isEdited;
        public bool IsEdited
        {
            get { return isEdited; }
            set
            {
                isEdited = value;
                NotifyPropertyChanged("MarkNum");
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
                NotifyPropertyChanged("MarkImage");
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
        //mark button image
        public string MarkImage
        {
            get
            {
                return CurrentVideoName.EndsWith(".mark.mp4") ? "marked.png" : "mark.png";
            }
        }
        //构造函数
        public VideoListStatus()
        {
            isLeftHanded = false;
            videoList = new List<string>();
            VideoList = new List<string>();            
            Index = 0;
        }
        public void CountReset()
        {
            VideoList = new List<string>();            
            Index = 0;
        }

        //注册属性改变事件，便于通告属性改变
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(prop));
        }
    }

    [Serializable]
    public class AppConfig
    {  
        public string ConfigFile { get; set; }
        public bool IsLeftHanded { get; set; }
        public List<string> VideoList { get; set; }
        public int Index { get; set; }

        public AppConfig()
        {
            ConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "config.xml");
            IsLeftHanded = false;
            VideoList = new List<string>();
            Index = 0;          
        }
        //保存到硬盘
        public void SaveToDisk()
        {
            FileStream fs = new FileStream(ConfigFile, FileMode.Create);
            XmlSerializer xs = new XmlSerializer(this.GetType());
            xs.Serialize(fs, this);
            fs.Close();
        }

        //从硬盘上读取设置文件
        public static AppConfig LoadFromDisk(string configFile)
        {
            AppConfig settings = new AppConfig();
            if (System.IO.File.Exists(configFile))
            {
                FileStream fs = new FileStream(configFile, FileMode.Open);
                XmlSerializer xs = new XmlSerializer(typeof(AppConfig));
                settings = xs.Deserialize(fs) as AppConfig;
                fs.Close();
            }
            settings.ConfigFile = configFile;//更正配置文件地址
            return settings;
        }
    }
}