using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OsuPackUnpacker {

    /// <summary>
    /// Google Drive Downloader 
    /// </summary>
    public class GDDownloader : IPacksDownloader {

        #region Private Fields

        private readonly string _apiKey;

        private readonly HttpClient _httpClient;

        private const string GOOGLE_API_ROOT = "https://www.googleapis.com";
        #endregion

        #region Constructor

        /// <summary>
        /// Initialize a <see cref="GDDownloader"/> instance
        /// </summary>
        /// <param name="apiKey">Your google API key <see cref="https://developers.google.com/api-client-library/dotnet/get_started"/></param>
        public GDDownloader(string apiKey) {

            _apiKey = apiKey;

            _httpClient = new();
        }
        #endregion

        #region IPacksDownloader Implementation

        public async Task<Stream> GetFileAsStream(string fileId) {

            var querry = $"key={_apiKey}&alt=media";
#if DEBUG
            Console.WriteLine($"\nDownloading: {fileId}\n");
#endif
            var response = await _httpClient.SendAsync(new HttpRequestMessage() {

                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://www.googleapis.com/drive/v3/files/{fileId}?{querry}"),
            }, HttpCompletionOption.ResponseHeadersRead);
#if DEBUG
            Console.WriteLine($"{response}\n");
#endif  
            return await response.Content.ReadAsStreamAsync();
        }

        /// <summary>
        /// Gets all the content of a google drive folder.
        /// </summary>
        /// <param name="googleDriveFolderId">The Id of the google drive folder</param>
        /// <returns>A tuple array of (name of the file, their id)</returns>
        public async Task<(string filename, string fileId)[]> ListFiles(string googleDriveFolderId) {

            var files = new List<(string nameFile, string filePath)>();

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

                var jsonFiles = jsonDocument.RootElement.GetProperty("files")
                    .EnumerateArray();

                foreach (var file in jsonFiles) {
#if DEBUG
                    Console.WriteLine(file);
#endif
                    var fileId = file.GetProperty("id").GetString();

                    if (!file.GetProperty("mimeType").GetString().Contains("folder")) {

                        files.Add((file.GetProperty("name").GetString(), fileId));
                    }

                    else {

                        files.AddRange(await this.ListFiles(fileId));
                    }
                }
            });

            return files.ToArray();
        }
        #endregion
    }
}
