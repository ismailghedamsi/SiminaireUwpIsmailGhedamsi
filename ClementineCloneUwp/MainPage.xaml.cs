
using Syncfusion.Data.Extensions;
using Syncfusion.XForms.AvatarView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        MediaElement element;
        ObservableCollection<Song> Songs;
        Windows.Storage.Pickers.FolderPicker picker;
        ObservableCollection<StorageFile> allSongsStorageFiles;
        StorageFolder folder = KnownFolders.MusicLibrary;


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
            element = new MediaElement();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            splitView.IsPaneOpen = !splitView.IsPaneOpen;
            StorageFolder folder = KnownFolders.MusicLibrary;
            var allSongs = new ObservableCollection<StorageFile>();
            await RetreiveFilesInFolders(allSongs, folder);

        }

        private void Continue_playing(object sender,RoutedEventArgs e)
        {
            element.Play();
        }

        public async void PickMusicFolder_Click()
        {
            picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.FileTypeFilter.Add("*");
            var folder = await picker.PickSingleFolderAsync();
        }

        private async void test(object sender, DoubleTappedRoutedEventArgs e)
        {
            await new MessageDialog("Double clicked").ShowAsync();
            string paths = ((Song)dataGrid.SelectedItem).Path;
            StorageFile file = await StorageFile.GetFileFromPathAsync(paths);
            element.SetSource(await file.OpenAsync(FileAccessMode.Read), file.ContentType);
            element.Play();

        }

        private void dataGrid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "drop a sound";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;

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
                    element.SetSource(await storageFile.OpenAsync(FileAccessMode.Read), contentType);
                    element.Play();
                    element.MediaEnded += new RoutedEventHandler(playNewSong);
                 
                    Songs.Add(new Song(metaData.Title, metaData.Artist, metaData.Album, Math.Round(metaData.Duration.TotalMinutes, 2), metaData.Genre.Count == 0 ? "" : metaData.Genre[0], newFile.Path));
                    allSongsStorageFiles.Add(newFile);
                    dataGrid.ItemsSource = null;
                    dataGrid.ItemsSource = Songs;


                }

            }
        }

        private void playNewSong(object sender, RoutedEventArgs e)
        {
            MessageDialog dialog = new MessageDialog("A new song with be played");
            dialog.ShowAsync();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await RetreiveFilesInFolders(allSongsStorageFiles, folder);
            await RetrieveSongMetadata(allSongsStorageFiles, Songs);
            dataGrid.ItemsSource = null;
            dataGrid.ItemsSource = Songs;
        }

        private void Button_Click_Stop(object sender, RoutedEventArgs e)
        {
            element.Pause();
        }
    }
}
