using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClementineCloneUwp
{
    public class Song
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public string Artist{ get; set; }

        public string Album { get; set; }

        public TimeSpan Duration { get; set; }

        public string Genre { get; set; }

        public string Path { get; set; }

        public Song(string title, string artist, string album, TimeSpan duration, string genre, string path)
        {
            Title = title;
            Artist = artist;
            Album = album;
            Duration = duration;
            Genre = genre;
            Path = path;
        }





        //metaData.Album
        //metaData.Artist
        //metaData.Duration
        //metaData.Genre
        //metaData.Title


    }
}
