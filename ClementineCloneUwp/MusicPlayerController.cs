using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Core;

namespace ClementineCloneUwp
{
    public static class MusicPlayerController
    {
        public static async Task PlayAsync(MediaPlayer player)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
             () =>
             {
                 try
                 {
                     player.Play();
                 }
                 catch (Exception e)
                 {
                     Debug.WriteLine(e.Message);
                 }

             }
             );
        }

        public static void ReinitiatePlayer(ref MediaPlayer player)
        {
            player.Dispose();
            player = new MediaPlayer();
        }

        public static void PausePlayer(MediaPlayer player)
        {
            player.Pause();
        }


            public static void SelectAnotherTrack(MediaPlayer player, StorageFile file)
        {
            player.SetFileSource(file);
        }
    }
}
