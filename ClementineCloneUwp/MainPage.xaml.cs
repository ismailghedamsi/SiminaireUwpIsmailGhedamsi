
using Microsoft.Toolkit.Uwp.UI.Controls;
using NAudio.Wave;
using Syncfusion.Data.Extensions;
using Syncfusion.XForms.AvatarView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input.ForceFeedback;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Xamarin.Forms;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ClementineCloneUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {

        private ObservableCollection<Song> Songs;
        private FolderPicker picker;
        private ObservableCollection<StorageFile> allSongsStorageFiles;
        private StorageFolder folder = KnownFolders.MusicLibrary;
        private MediaPlayer player;
        private static int currentPlayingSongIndex;
        private bool manuallySeek;
        Timer timer;


        private async Task RetrieveSongMetadata(ObservableCollection<StorageFile> listsongStorage, ObservableCollection<Song> listSong)
        {
            foreach(var item in listsongStorage)
            {
                MusicProperties metaData = await item.Properties.GetMusicPropertiesAsync();
                listSong.Add(new Song(metaData.Title,metaData.Artist,metaData.Album, Math.Round(metaData.Duration.TotalMinutes,2), metaData.Genre,item.Path));  
            }
        }

        private async Task RetreiveFilesInFolders(ObservableCollection<StorageFile> list,StorageFolder parent)
        {
            foreach(var item in await parent.GetFilesAsync())
            {
                if(item.FileType == ".mp3")
                {
                    list.Add(item);
                }
            }

            foreach(var item in await parent.GetFoldersAsync())
            {
                await RetreiveFilesInFolders(list, item);
            }
        }


        public MainPage()
        {
            this.InitializeComponent();
        
            allSongsStorageFiles = new ObservableCollection<StorageFile>();
            Songs = new ObservableCollection<Song>();
            player = new MediaPlayer();
            player.MediaEnded += PlayNewSong_MediaEnded;
            volumeSlider.Value = player.Volume * 100;
            currentPlayingSongIndex = 0;
       
        }

        private  void OpenCloseSplitView_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = !splitView.IsPaneOpen;
        }

        private void Continue_playing(object sender,RoutedEventArgs e)
        {
            player.Play();
        }



        private async void PlaySongFromGrid_DoubleClick(object sender, DoubleTappedRoutedEventArgs ev)
        {

            string paths = ((Song)dataGrid.SelectedItem).Path;
            StorageFile file = await StorageFile.GetFileFromPathAsync(paths);
            player.Dispose();
            player = new MediaPlayer();
            player.MediaEnded += PlayNewSong_MediaEnded;
            player.SetFileSource(file);

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
             () =>
             {
                 player.Play();
             }
             );

            currentPlayingSongIndex = dataGrid.SelectedIndex;
            seekPositionSlider.Value = 0;
            seekPositionSlider.ManipulationCompleted += SeekPositionSlider_ManipulationCompleted;
            UpdateTimelineSlider();
        }

        private void UpdateTimelineSlider()
        {

                timer = new System.Threading.Timer(async (e) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                   () =>
                   {

                       if (!Double.IsInfinity(99.0 / player.PlaybackSession.NaturalDuration.TotalSeconds))
                       {
                           seekPositionSlider.Value += (100.0 / player.PlaybackSession.NaturalDuration.TotalSeconds);
                       }
                   }
                 );
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }



        private void SeekPositionSlider_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var seekPosition = seekPositionSlider.Value / 100;
            var playFrom = player.PlaybackSession.NaturalDuration * seekPosition;
            player.PlaybackSession.Position = playFrom;
         
        }


        private void dataGrid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "drop a sound";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;

        }

        public string FormatTrackDuration(double minutes)
        {
            string secondsPart = minutes.ToString().Split(",")[1];
            string minutesPart = minutes.ToString().Split(",")[0];
            int nbMinuteFromSeconds = int.Parse(secondsPart) / 60;
            int restOfSeconds = int.Parse(secondsPart) % 60;
            return "";
        }

        private async void dataGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Any())
                {
                    var storageFile = items[0] as StorageFile;
                    var contentType = storageFile.ContentType;
                    StorageFolder folder = ApplicationData.Current.LocalFolder;
                    StorageFile newFile = await storageFile.CopyAsync(folder, storageFile.Name, NameCollisionOption.GenerateUniqueName);
                    MusicProperties metaData = await newFile.Properties.GetMusicPropertiesAsync();
                    player.SetFileSource(storageFile);
                    Songs.Add(new Song(metaData.Title, metaData.Artist, metaData.Album, Math.Round(metaData.Duration.TotalMinutes, 2), metaData.Genre.Count == 0 ? "" : metaData.Genre[0], newFile.Path));
                    dataGrid.ItemsSource = null;
                    dataGrid.Columns.Clear();
                    dataGrid.ItemsSource = Songs;


                }

            }
        }

        private async void PlayNewSong_MediaEnded(MediaPlayer sender, object args)
        {
            Console.WriteLine("a new song with be played");

            player.Dispose();
            timer.Dispose();

            player = new MediaPlayer();
            player.MediaEnded += PlayNewSong_MediaEnded;
            currentPlayingSongIndex++;
            if(allSongsStorageFiles.Count > currentPlayingSongIndex)
            {
                player.SetFileSource(allSongsStorageFiles[currentPlayingSongIndex]);
            }

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                dataGrid.SelectedIndex = currentPlayingSongIndex;
                seekPositionSlider.Value = 0;
            }
            );
            player.Play();
            
       
        }
  

        public void GenerateColumnHeaderManually()
        {

            DataGridTextColumn idCol = new DataGridTextColumn();
            idCol.Header = "Id";

            DataGridTextColumn titleCol = new DataGridTextColumn();
            titleCol.Header = "Title";

            DataGridTextColumn artistCol = new DataGridTextColumn();
            artistCol.Header = "Artist";

            DataGridTextColumn albumCol = new DataGridTextColumn();
            albumCol.Header = "Album";

            DataGridTextColumn durationCol = new DataGridTextColumn();
            durationCol.Header = "Duration";

            DataGridTextColumn genreCol = new DataGridTextColumn();
            durationCol.Header = "Genre";

            DataGridTextColumn pathCol = new DataGridTextColumn();
            durationCol.Header = "Path";

            dataGrid.Columns.Add(idCol);
            dataGrid.Columns.Add(titleCol);
            dataGrid.Columns.Add(artistCol);
            dataGrid.Columns.Add(albumCol);
            dataGrid.Columns.Add(durationCol);
            dataGrid.Columns.Add(genreCol);
            dataGrid.Columns.Add(pathCol);
        }

        private  void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateColumnHeaderManually();
        }

        private void DG1_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {

            e.Column.Visibility = Visibility.Visible;
        }

        private void showPlaylistButton_Click(object sender,RoutedEventArgs e)
        {
            dataGrid.ItemsSource = null;
            dataGrid.Columns.Clear();
            GenerateColumnHeaderManually();
        }

        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            player.Pause();
        }

        private  async void Button_Click_Library(object sender, RoutedEventArgs e)
        {
            await RetreiveFilesInFolders(allSongsStorageFiles, folder);
            await RetrieveSongMetadata(allSongsStorageFiles, Songs);
            if(Songs.Count == 0 || dataGrid.ItemsSource == null)
            {
                dataGrid.Columns.Clear();
            }
            dataGrid.ItemsSource = Songs;
        }

        private void volumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            player.Volume = volumeSlider.Value / 100;
            player.MediaEnded += PlayNewSong_MediaEnded;
        }

        private async void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<StorageFile> folderFiles = new ObservableCollection<StorageFile>();
            ObservableCollection<Song> songs = new ObservableCollection<Song>();
            picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.FileTypeFilter.Add("*");
            var folder = await picker.PickSingleFolderAsync();
            await RetreiveFilesInFolders(folderFiles, folder);
            await RetrieveSongMetadata(folderFiles, songs);
            dataGrid.ItemsSource = null;
            dataGrid.Columns.Clear();
            dataGrid.ItemsSource = songs;
        }


        private void PlayNextSongButton_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.SelectedItem = currentPlayingSongIndex;
        }
    }
}
