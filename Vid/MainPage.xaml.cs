using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x412에 나와 있습니다.

namespace Vid
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SystemMediaTransportControls MediaControls { get; }
        public class AppFile
        {
            public string name { get; set; }
            public MediaSource mediaSource { get; set; }
        }
        public class ListButton : Button
        {
            MediaSource ms;
            MediaPlayerElement me;
            TextBlock label;
            AppFile item;
            public ListButton()
            {
                this.Width = 150;
                this.Height = 40;
                this.Margin = new Thickness(0, 0, 0, 8);
                this.Visibility = Visibility.Visible;
                label = new TextBlock();
                label.Visibility = Visibility.Visible;
                this.Click += OnClick;
                this.Content = label;
                label.Tapped += OnClick;
            }
            public ListButton(AppFile ms, MediaPlayerElement mpe) : this()
            {
                this.me = mpe;
                this.ms = ms.mediaSource;
                label.Text = ms.name;
            }
            public void OnClick(object sender, RoutedEventArgs e)
            {
                me.Source = ms;
                me.MediaPlayer.Play();
            }
        }
        List<AppFile> filelist;

        private void applyAcrylicAccent(Panel panel)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            _hostSprite = _compositor.CreateSpriteVisual();
            _hostSprite.Size = new Vector2((float)panel.ActualWidth, (float)panel.ActualHeight);

            ElementCompositionPreview.SetElementChildVisual(panel, _hostSprite);
            _hostSprite.Brush = _compositor.CreateHostBackdropBrush();
        }
        Compositor _compositor;
        SpriteVisual _hostSprite;
        public MainPage()
        {
            ApplicationViewTitleBar formattableTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            formattableTitleBar.ButtonBackgroundColor = Colors.Transparent;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            this.InitializeComponent();
            filelist = new List<AppFile>();
            applyAcrylicAccent(MainGrid);
            MediaPlayer mp = new MediaPlayer();
            mp.CurrentStateChanged += Mp_CurrentStateChangedAsync;
            player.SetMediaPlayer(mp);
        }

        private async void Mp_CurrentStateChangedAsync(MediaPlayer sender, object args)
        {
            switch (sender.PlaybackSession.PlaybackState)
            {
                case MediaPlaybackState.Playing:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        // TODO 재생 중일 때의 이벤트
                    });
                    break;
                case MediaPlaybackState.Paused:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        // TODO 일시 정지 중일 때의 이벤트
                    });
                    break;
            }
        }
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_hostSprite != null)
                _hostSprite.Size = e.NewSize.ToVector2();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await SetLocalMedia();
            updateList();
        }
        async private Task SetLocalMedia()
        {
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
            openPicker.FileTypeFilter.Add(".mp4");

            var file = await openPicker.PickSingleFileAsync();
            if(file!= null)
            {
                if (filelist.All(ap => ap.name != file.Name))
                {
                    AppFile ap = new AppFile();
                    ap.name = file.Name;
                    ap.mediaSource = MediaSource.CreateFromStorageFile(file);
                    filelist.Add(ap);
                }
            }
        } 

        private async void Grid_DropAsync(object sender, DragEventArgs e)
        {
            if (e.DragUIOverride != null)
            {
                if (e.DataView.Contains(StandardDataFormats.StorageItems))
                {
                    var item = await e.DataView.GetStorageItemsAsync();
                    if (item.Count > 0) { 
                        foreach (var file in item.OfType<StorageFile>())
                        {
                            if (filelist.All(ap => ap.name != file.Name))
                            { 
                                if (file.Name.Contains("mp4"))
                                { 
                                    AppFile ap = new AppFile();
                                    ap.name = file.Name;
                                    ap.mediaSource = MediaSource.CreateFromStorageFile(file);
                                    filelist.Add(ap);
                                }
                            }
                        }
                        updateList();
                    }
                }
            }
        }

        private void updateList()
        {
            vidList.Children.Clear();
            foreach (var f in filelist)
            {
                ListButton lb = new ListButton(f, player);
                lb.Visibility = Visibility.Visible;
                vidList.Children.Add(lb);
            }
        }
        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;

            if (e.DragUIOverride != null)
            {
                e.DragUIOverride.Caption = "Add file";
                e.DragUIOverride.IsContentVisible = true;
            }
        }
    }
}
