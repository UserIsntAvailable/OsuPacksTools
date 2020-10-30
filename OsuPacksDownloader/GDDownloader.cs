#define DEBUG

using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OsuPackUnpacker {

    /// <summary>
    /// Google Drive Downloader 
    /// </summary>
    public class GDDownloader : IPacksDownloader, IDisposable {

        #region Private Fields

        private readonly string _apiKey;

        private readonly HttpClient _httpClient;

        private readonly List<(string nameFile, string filePath)> _files;

        private const string GOOGLE_API_ROOT = "https://www.googleapis.com";
        #endregion

        #region Constructor

        /// <summary>
        /// Initialize a <see cref="GDDownloader"/> instance
        /// </summary>
        /// <param name="apiKey">Your google API key <see cref="https://developers.google.com/api-client-library/dotnet/get_started"/></param>
        public GDDownloader(string apiKey) {

            _apiKey = apiKey;

            _files = new();

            _httpClient = new();
        }
        #endregion

        #region IDispasable Implementation

        public void Dispose() {

            _httpClient.Dispose();
        }
        #endregion

        #region IPacksDownloader Implementation

        /// [WIP]
        /// <summary>
        /// Downloads a file of google drive 
        /// </summary>
        /// <param name="fileId">The Id of the google drive file</param>
        public async Task Download(string fileId) {

            var querry = $"id={fileId}&export=download";
#if DEBUG
            Console.WriteLine($"\nDownloading: {fileId}\n");
#endif
            var response = await _httpClient.SendAsync(new HttpRequestMessage() {

                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://drive.google.com/uc?{querry}"),
            });

            var downloadLink = Regex.Match(
                await response.Content.ReadAsStringAsync(), 
                @"/uc\?export=download&amp;confirm=[^""]*").Value;

            /// Then download file. (For later)
#if DEBUG
            Console.WriteLine($"File link: {downloadLink}");
#endif
        }

        /// <summary>
        /// Gets all the content of a google drive folder.
        /// </summary>
        /// <param name="googleDriveFolderId">The Id of the google drive folder</param>
        /// <returns>A tuple array of (name of the file, their id)</returns>
        public async Task<(string nameFile, string filePath)[]> ListFiles(string googleDriveFolderId) {

            var querry = $"key={_apiKey}&q='{googleDriveFolderId}'+in+parents";

            var response = await _httpClient.SendAsync(new HttpRequestMessage() {

                Method = HttpMethod.Get,
                RequestUri = new Uri($"{GOOGLE_API_ROOT}/drive/v3/files?{querry}"),
            });
#if DEBUG
            Console.WriteLine($"[ListFiles]Response\n{await response.Content.ReadAsStringAsync()}");
#endif
            using JsonDocument jsonDocument
                = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

            await Task.Run(async () => {

                var files = jsonDocument.RootElement.GetProperty("files")
                    .EnumerateArray();

                // For some reason Parallel.ForEach doesn't work
                //Parallel.ForEach(files, async (file) => {

                foreach (var file in files) {
#if DEBUG
                    Console.WriteLine(file);
#endif
                    var fileId = file.GetProperty("id").GetString();

                    if (!file.GetProperty("mimeType").GetString().Contains("folder")) {

                        _files.Add((file.GetProperty("name").GetString(), fileId));
                    }

                    else {

                        await this.ListFiles(fileId);
                    }
                }
                //});
            });

            return _files.ToArray();
        }
        #endregion
    }
}
