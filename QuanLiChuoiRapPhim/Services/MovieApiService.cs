using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace QuanLiChuoiRapPhim.Services
{
    /// <summary>
    /// TMDB API Integration Service for fetching movie data
    /// API Documentation: https://developer.themoviedb.org/docs
    /// </summary>
    public class MovieApiService
    {
        // TMDB API Configuration
        // NOTE: Replace with your own API key from https://www.themoviedb.org/settings/api
        private const string API_KEY = "YOUR_TMDB_API_KEY";
        private const string BASE_URL = "https://api.themoviedb.org/3";
        private const string IMAGE_BASE_URL = "https://image.tmdb.org/t/p/w500";
        private const string LANGUAGE = "vi-VN"; // Vietnamese language for results

        private readonly JavaScriptSerializer _serializer;
        private readonly string _cacheFolder;

        public MovieApiService()
        {
            _serializer = new JavaScriptSerializer();
            _cacheFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads", "movie_cache");
            
            if (!Directory.Exists(_cacheFolder))
            {
                Directory.CreateDirectory(_cacheFolder);
            }
        }

        #region Search Movies

        /// <summary>
        /// Search movies by title from TMDB
        /// </summary>
        /// <param name="query">Search keyword</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <returns>List of MovieSearchResult</returns>
        public List<MovieSearchResult> SearchMovies(string query, int page = 1)
        {
            var results = new List<MovieSearchResult>();

            try
            {
                string url = $"{BASE_URL}/search/movie?api_key={API_KEY}&language={LANGUAGE}&query={Uri.EscapeDataString(query)}&page={page}";
                string response = MakeRequest(url);

                if (!string.IsNullOrEmpty(response))
                {
                    var data = _serializer.Deserialize<Dictionary<string, object>>(response);
                    if (data.ContainsKey("results"))
                    {
                        var movies = data["results"] as System.Collections.ArrayList;
                        foreach (Dictionary<string, object> movie in movies)
                        {
                            results.Add(ParseMovieSearchResult(movie));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MovieApiService] SearchMovies Error: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Get movies now playing in theaters
        /// </summary>
        public List<MovieSearchResult> GetNowPlayingMovies(int page = 1)
        {
            var results = new List<MovieSearchResult>();

            try
            {
                string url = $"{BASE_URL}/movie/now_playing?api_key={API_KEY}&language={LANGUAGE}&page={page}&region=VN";
                string response = MakeRequest(url);

                if (!string.IsNullOrEmpty(response))
                {
                    var data = _serializer.Deserialize<Dictionary<string, object>>(response);
                    if (data.ContainsKey("results"))
                    {
                        var movies = data["results"] as System.Collections.ArrayList;
                        foreach (Dictionary<string, object> movie in movies)
                        {
                            results.Add(ParseMovieSearchResult(movie));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MovieApiService] GetNowPlayingMovies Error: {ex.Message}");
            }

            return results;
        }

        /// <summary>
        /// Get upcoming movies
        /// </summary>
        public List<MovieSearchResult> GetUpcomingMovies(int page = 1)
        {
            var results = new List<MovieSearchResult>();

            try
            {
                string url = $"{BASE_URL}/movie/upcoming?api_key={API_KEY}&language={LANGUAGE}&page={page}&region=VN";
                string response = MakeRequest(url);

                if (!string.IsNullOrEmpty(response))
                {
                    var data = _serializer.Deserialize<Dictionary<string, object>>(response);
                    if (data.ContainsKey("results"))
                    {
                        var movies = data["results"] as System.Collections.ArrayList;
                        foreach (Dictionary<string, object> movie in movies)
                        {
                            results.Add(ParseMovieSearchResult(movie));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MovieApiService] GetUpcomingMovies Error: {ex.Message}");
            }

            return results;
        }

        #endregion

        #region Movie Details

        /// <summary>
        /// Get detailed movie information by TMDB ID
        /// </summary>
        /// <param name="tmdbId">TMDB Movie ID</param>
        /// <returns>MovieDetail object</returns>
        public MovieDetail GetMovieDetails(int tmdbId)
        {
            try
            {
                // Get movie details with credits (cast & crew)
                string url = $"{BASE_URL}/movie/{tmdbId}?api_key={API_KEY}&language={LANGUAGE}&append_to_response=credits,videos";
                string response = MakeRequest(url);

                if (!string.IsNullOrEmpty(response))
                {
                    var data = _serializer.Deserialize<Dictionary<string, object>>(response);
                    return ParseMovieDetail(data);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MovieApiService] GetMovieDetails Error: {ex.Message}");
            }

            return null;
        }

        #endregion

        #region Image Handling

        /// <summary>
        /// Download and cache movie poster
        /// </summary>
        /// <param name="posterPath">Poster path from TMDB</param>
        /// <returns>Local file path or null if failed</returns>
        public string DownloadPoster(string posterPath)
        {
            if (string.IsNullOrEmpty(posterPath))
                return null;

            try
            {
                string fileName = Path.GetFileName(posterPath);
                string localPath = Path.Combine(_cacheFolder, fileName);

                // Return cached file if exists
                if (File.Exists(localPath))
                    return localPath;

                // Download from TMDB
                string imageUrl = IMAGE_BASE_URL + posterPath;
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(imageUrl, localPath);
                }

                return localPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MovieApiService] DownloadPoster Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get poster as Image object
        /// </summary>
        public Image GetPosterImage(string posterPath)
        {
            string localPath = DownloadPoster(posterPath);
            if (!string.IsNullOrEmpty(localPath) && File.Exists(localPath))
            {
                try
                {
                    return Image.FromFile(localPath);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        #endregion

        #region Helper Methods

        private string MakeRequest(string url)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Timeout = 10000; // 10 seconds timeout
                request.UserAgent = "CGV Cinema Management System";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MovieApiService] WebException: {ex.Message}");
                return null;
            }
        }

        private MovieSearchResult ParseMovieSearchResult(Dictionary<string, object> movie)
        {
            var result = new MovieSearchResult
            {
                TmdbId = Convert.ToInt32(movie["id"]),
                Title = movie.ContainsKey("title") ? movie["title"]?.ToString() : "",
                OriginalTitle = movie.ContainsKey("original_title") ? movie["original_title"]?.ToString() : "",
                Overview = movie.ContainsKey("overview") ? movie["overview"]?.ToString() : "",
                PosterPath = movie.ContainsKey("poster_path") ? movie["poster_path"]?.ToString() : "",
                BackdropPath = movie.ContainsKey("backdrop_path") ? movie["backdrop_path"]?.ToString() : "",
                VoteAverage = movie.ContainsKey("vote_average") ? Convert.ToDouble(movie["vote_average"]) : 0
            };

            // Parse release date
            if (movie.ContainsKey("release_date") && !string.IsNullOrEmpty(movie["release_date"]?.ToString()))
            {
                DateTime releaseDate;
                if (DateTime.TryParse(movie["release_date"].ToString(), out releaseDate))
                {
                    result.ReleaseDate = releaseDate;
                }
            }

            // Parse genre IDs
            if (movie.ContainsKey("genre_ids"))
            {
                var genreIds = movie["genre_ids"] as System.Collections.ArrayList;
                if (genreIds != null)
                {
                    foreach (var id in genreIds)
                    {
                        result.GenreIds.Add(Convert.ToInt32(id));
                    }
                }
            }

            return result;
        }

        private MovieDetail ParseMovieDetail(Dictionary<string, object> movie)
        {
            var detail = new MovieDetail
            {
                TmdbId = Convert.ToInt32(movie["id"]),
                Title = movie.ContainsKey("title") ? movie["title"]?.ToString() : "",
                OriginalTitle = movie.ContainsKey("original_title") ? movie["original_title"]?.ToString() : "",
                Overview = movie.ContainsKey("overview") ? movie["overview"]?.ToString() : "",
                PosterPath = movie.ContainsKey("poster_path") ? movie["poster_path"]?.ToString() : "",
                BackdropPath = movie.ContainsKey("backdrop_path") ? movie["backdrop_path"]?.ToString() : "",
                VoteAverage = movie.ContainsKey("vote_average") ? Convert.ToDouble(movie["vote_average"]) : 0,
                Runtime = movie.ContainsKey("runtime") ? Convert.ToInt32(movie["runtime"]) : 0,
                Status = movie.ContainsKey("status") ? movie["status"]?.ToString() : ""
            };

            // Parse release date
            if (movie.ContainsKey("release_date") && !string.IsNullOrEmpty(movie["release_date"]?.ToString()))
            {
                DateTime releaseDate;
                if (DateTime.TryParse(movie["release_date"].ToString(), out releaseDate))
                {
                    detail.ReleaseDate = releaseDate;
                }
            }

            // Parse genres
            if (movie.ContainsKey("genres"))
            {
                var genres = movie["genres"] as System.Collections.ArrayList;
                if (genres != null)
                {
                    foreach (Dictionary<string, object> genre in genres)
                    {
                        detail.Genres.Add(genre["name"]?.ToString() ?? "");
                    }
                }
            }

            // Parse credits (cast & crew)
            if (movie.ContainsKey("credits"))
            {
                var credits = movie["credits"] as Dictionary<string, object>;
                if (credits != null)
                {
                    // Get top 5 cast members
                    if (credits.ContainsKey("cast"))
                    {
                        var cast = credits["cast"] as System.Collections.ArrayList;
                        if (cast != null)
                        {
                            int count = 0;
                            foreach (Dictionary<string, object> member in cast)
                            {
                                if (count++ >= 5) break;
                                detail.Cast.Add(member["name"]?.ToString() ?? "");
                            }
                        }
                    }

                    // Get director
                    if (credits.ContainsKey("crew"))
                    {
                        var crew = credits["crew"] as System.Collections.ArrayList;
                        if (crew != null)
                        {
                            foreach (Dictionary<string, object> member in crew)
                            {
                                if (member["job"]?.ToString() == "Director")
                                {
                                    detail.Director = member["name"]?.ToString() ?? "";
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // Parse trailer (videos)
            if (movie.ContainsKey("videos"))
            {
                var videos = movie["videos"] as Dictionary<string, object>;
                if (videos != null && videos.ContainsKey("results"))
                {
                    var results = videos["results"] as System.Collections.ArrayList;
                    if (results != null)
                    {
                        foreach (Dictionary<string, object> video in results)
                        {
                            if (video["type"]?.ToString() == "Trailer" && video["site"]?.ToString() == "YouTube")
                            {
                                detail.TrailerKey = video["key"]?.ToString();
                                break;
                            }
                        }
                    }
                }
            }

            return detail;
        }

        #endregion

        #region API Key Validation

        /// <summary>
        /// Check if API key is configured and valid
        /// </summary>
        public bool IsApiKeyConfigured()
        {
            return !string.IsNullOrEmpty(API_KEY) && API_KEY != "YOUR_TMDB_API_KEY";
        }

        /// <summary>
        /// Test API connection
        /// </summary>
        public bool TestConnection()
        {
            if (!IsApiKeyConfigured())
                return false;

            try
            {
                string url = $"{BASE_URL}/configuration?api_key={API_KEY}";
                string response = MakeRequest(url);
                return !string.IsNullOrEmpty(response);
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }

    #region Data Models

    /// <summary>
    /// Movie search result from TMDB
    /// </summary>
    public class MovieSearchResult
    {
        public int TmdbId { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string Overview { get; set; }
        public string PosterPath { get; set; }
        public string BackdropPath { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public double VoteAverage { get; set; }
        public List<int> GenreIds { get; set; } = new List<int>();

        public string PosterUrl => !string.IsNullOrEmpty(PosterPath) 
            ? $"https://image.tmdb.org/t/p/w500{PosterPath}" 
            : "";

        public string Year => ReleaseDate?.Year.ToString() ?? "N/A";

        public string Rating => VoteAverage > 0 ? $"⭐ {VoteAverage:F1}" : "Chưa có đánh giá";
    }

    /// <summary>
    /// Detailed movie information from TMDB
    /// </summary>
    public class MovieDetail : MovieSearchResult
    {
        public int Runtime { get; set; }
        public string Status { get; set; }
        public string Director { get; set; }
        public string TrailerKey { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
        public List<string> Cast { get; set; } = new List<string>();

        public string GenresString => string.Join(", ", Genres);
        public string CastString => string.Join(", ", Cast);
        public string RuntimeFormatted => Runtime > 0 ? $"{Runtime} phút" : "N/A";
        public string TrailerUrl => !string.IsNullOrEmpty(TrailerKey) 
            ? $"https://www.youtube.com/watch?v={TrailerKey}" 
            : "";
    }

    #endregion
}
