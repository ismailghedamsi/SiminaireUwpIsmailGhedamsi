using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClementineCloneUwp
{
    public class Song
    {
        public string SongName { get; set; }
        public string ArtistName { get; set; }

        public string Path { get; set; }

        public Song(string songName, string artistName, string path)
        {
            SongName = songName;
            ArtistName = artistName;
            Path = path;
        }

        //public Song(string songName, string artistName, string path)
        //{
        //    SongName = songName;
        //    ArtistName = artistName;
        //    Path = path;
        //}
    }
}
