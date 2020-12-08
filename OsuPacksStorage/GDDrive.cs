using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OsuPacksStorage {

    /// <summary>
    /// Google Drive cloud storage API
    /// </summary>
    public class GDDrive : IStorage {

        #region Private Fields

        private readonly string _apiKey;

        private readonly HttpClient _httpClient;

        private const string GOOGLE_API_ROOT = "https://www.googleapis.com";
        #endregion

        #region Constructor

        /// <summary>
        /// Initialize a <see cref="GDDrive"/> instance
        /// </summary>
        /// <param name="apiKey">Your google API key <see cref="https://developers.google.com/api-client-library/dotnet/get_started"/></param>
        public GDDrive(string apiKey) : this(apiKey, new HttpClientHandler()) { }

        /// <summary>
        /// Initialize a <see cref="GDDrive"/> instance for Unit testing purposes
        /// </summary>
        internal GDDrive(string moqApiKey, HttpMessageHandler handler) {

            _apiKey = moqApiKey;

            _httpClient = new(handler);
        }
        #endregion

        #region IStorage Implementation

        /// <summary>
        /// Gets a file stream data
        /// </summary>
        /// <param name="fileId">The Id of the google drive file</param>
        public async Task<Stream> GetFileAsStream(string fileId) {

            var querry = $"key={_apiKey}&alt=media";
#if DEBUG
            Console.WriteLine($"\nDownloading: {fileId}\n");
#endif
            using var httpReqMsg = new HttpRequestMessage() {

                Method = HttpMethod.Get,
                RequestUri = new Uri($"{GOOGLE_API_ROOT}/drive/v3/files/{fileId}?{querry}"),
            };
            using var response = await _httpClient.SendAsync(
                httpReqMsg,
                HttpCompletionOption.ResponseHeadersRead);

            var stream = await response.Content.ReadAsStreamAsync();
#if DEBUG
            Console.WriteLine($"{response}\n");
#endif
            if (response.Content.Headers.ContentType.ToString() == "application/json")
                ThrowExceptionIfRequestFailed(await JsonDocument.ParseAsync(stream), nameof(fileId));

            return stream;
        }

        /// <summary>
        /// Gets all the files of a folder ( including subdirectories ).
        /// </summary>
        /// <param name="folderId">The Id of the google drive folder</param>
        /// <returns>A tuple array of (name of the file, his id)</returns>
        public async Task<(string filename, string fileId)[]> ListFiles(string folderId) {

            var files = new List<(string nameFile, string filePath)>();

            var querry = $"key={_apiKey}&q='{folderId}'+in+parents";

            using var httpReqMsg = new HttpRequestMessage() {

                Method = HttpMethod.Get,
                RequestUri = new Uri($"{GOOGLE_API_ROOT}/drive/v3/files?{querry}"),
            };
            using var response = await _httpClient.SendAsync(httpReqMsg);
#if DEBUG
            Console.WriteLine($"[ListFiles]Response\n{await response.Content.ReadAsStringAsync()}");
#endif
            using JsonDocument jsonDocument
                = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

            ThrowExceptionIfRequestFailed(jsonDocument, nameof(folderId));

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

        #region Private Methods

        private static void ThrowExceptionIfRequestFailed(JsonDocument reqJson, string notFoundArgument) {

            if (reqJson.RootElement.TryGetProperty("error", out JsonElement errorJson)) {

                var reason = errorJson.GetProperty("errors")
                    .EnumerateArray()
                    .ElementAt(0)
                    .GetProperty("reason")
                    .ToString();

                reqJson.Dispose();

                throw reason switch {

                    "keyInvalid" => new ArgumentException("The api key passed to the constructor is invalid", nameof(_apiKey)[1..]),
                    "notFound" => new ArgumentException("The google drive folder/file id was not found", notFoundArgument),
                    "downloadQuotaExceeded" => new Exception("You already used your downlaod quota for this file. See Readme for more information about this."),
                    _ => new Exception($"Unexpected reason\n {errorJson}"),
                };
            }
        }
        #endregion
    }
}
