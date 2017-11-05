using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyTVWeb
{

    public class MoviesBase
    {
        public string RootURL { get; set; }
        public string PageNumber { get; set; }
        public string Search { get; set; }
        public virtual List<Movie> GetMovies() { return null; }
        public virtual List<Movie> GetMovie(string url) { return null; }
    }

    public class Movie
    {
        public bool Success { get; set; }
        public int IsVideoURL { get; set; }
        public string MovieName { get; set; }
        public string MovieURL { get; set; }
        public string CookieString { get; set; }
        public string MovieImage { get; set; }
        public bool Embed { get; set; }
    }

}