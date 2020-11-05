using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Xunit;
using OsuPackUnpacker;
using RichardSzalay.MockHttp;

namespace OsuPacksDownloader.Tests {
    public class GDDownloaderTests {

        [Fact]
        public async Task ListFiles_Should_Work() {

            var moqApiKey = Guid.NewGuid().ToString();
            var moqMainFolderId = Guid.NewGuid().ToString();
            var moqPackFolderIds = new string[4] {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString() };

            var moqHttp = new MockHttpMessageHandler();

            var mainFolderResquest = moqHttp
                .Expect("https://www.googleapis.com/drive/v3/files")
                .WithExactQueryString($"key={moqApiKey}&q='{moqMainFolderId}'+in+parents")
                .Respond("application/json", @$"{{
""kind"": ""drive#fileList"",
""incompleteSearch"": false, 
""files"": [
		{{
		  ""kind"": ""drive#file"",
		  ""id"": ""{moqPackFolderIds[1]}"",
		  ""name"": ""2020 (osu!)"",
		  ""mimeType"": ""application/vnd.google-apps.folder""
		}},
		{{
		  ""kind"": ""drive#file"",
		  ""id"": ""{moqPackFolderIds[0]}"",
		  ""name"": ""2019 (osu!)"",
		  ""mimeType"": ""application/vnd.google-apps.folder""
		}},
		{{
		  ""kind"": ""drive#file"",
		  ""id"": ""{moqPackFolderIds[3]}"",
		  ""name"": ""2018 (osu!)"",
		  ""mimeType"": ""application/vnd.google-apps.folder""
		}},
		{{
		  ""kind"": ""drive#file"",
		  ""id"": ""{moqPackFolderIds[2]}"",
		  ""name"": ""2017 (osu!)"",
		  ""mimeType"": ""application/vnd.google-apps.folder""
		}}
	]
}}");

            var subFolderRequest = moqHttp
                .When("https://www.googleapis.com/drive/v3/files")
                .With(req => moqPackFolderIds.Contains(Regex.Match(
                    req.RequestUri.Query,
                    "q='([^']*)").Groups[1].Value))
                .Respond("application/json", @$"{{
""kind"": ""drive#fileList"",
""incompleteSearch"": false, 
""files"": [
		{{
		  ""kind"": ""drive#file"",
		  ""id"": ""{Guid.NewGuid()}"",
		  ""name"": ""2020-10 (osu!).rar"",
		  ""mimeType"": ""application/rar""
		}},
		{{
		  ""kind"": ""drive#file"",
		  ""id"": ""{Guid.NewGuid()}"",
		  ""name"": ""2020-09 (osu!).rar"",
		  ""mimeType"": ""application/rar""
		}}
	]
}}");

            var files = await new GDDownloader(moqApiKey, moqHttp).ListFiles(moqMainFolderId);

            Assert.Equal(1, moqHttp.GetMatchCount(mainFolderResquest));
            Assert.Equal(moqPackFolderIds.Length, moqHttp.GetMatchCount(subFolderRequest));
            Assert.Equal(8, files.Length);
        }

        [Fact]
        public async Task ListFiles_Throw_ArgumentException_ApiKey() {

            var moqHttp = new MockHttpMessageHandler();
            moqHttp.Fallback
                .Respond("application/json", @"{
""error"": {
	""errors"": [
		{
		 ""domain"": ""usageLimits"",
		 ""reason"": ""keyInvalid"",
		 ""message"": ""Bad Request""
		}
	],
	""code"": 400,
	""message"": ""Bad Request""
	}
}");

            var ex = Assert.ThrowsAsync<ArgumentException>(
                async () =>
                    await new GDDownloader(Guid.NewGuid().ToString(), moqHttp)
                        .ListFiles(Guid.NewGuid().ToString()));

            Assert.Equal("The api key passed to the constructor is invalid (Parameter 'apiKey')", (await ex).Message);
        }

        [Fact]
        public async Task ListFiles_Throw_ArgumentException_GoogleDriveFolderId() {

            var moqHttp = new MockHttpMessageHandler();
            moqHttp.Fallback
                .Respond("application/json", @"{
""error"": {
	""errors"": [
		{
		 ""domain"": ""global"",
		 ""reason"": ""notFound"",
		 ""message"": ""File not found: ."",
		 ""locationType"": ""parameter"",
		 ""location"": ""fileId""
		}
	],
	""code"": 400,
	""message"": ""Bad Request""
	}
}");

            var ex = Assert.ThrowsAsync<ArgumentException>(
                async () =>
                    await new GDDownloader(Guid.NewGuid().ToString(), moqHttp)
                        .ListFiles(Guid.NewGuid().ToString()));

            Assert.Equal("The google drive folder id was not found (Parameter 'googleDriveFolderId')", (await ex).Message);
        }

        [Fact]
        public async Task ListFiles_Throw_Exception_UnexpectedReason() {

            var errorJsonResponse = @"{
""error"": {
	""errors"": [
		{
		 ""domain"": ""local"",
		 ""reason"": ""unexpected"",
		 ""message"": ""Bad Request""
		}
	],
	""code"": 400,
	""message"": ""Bad Request""
	}
}";

            var moqHttp = new MockHttpMessageHandler();
            moqHttp.Fallback
                .Respond("application/json", errorJsonResponse);

            var ex = Assert.ThrowsAsync<Exception>(
                async () =>
                    await new GDDownloader(Guid.NewGuid().ToString(), moqHttp)
                        .ListFiles(Guid.NewGuid().ToString()));

			/*There are some weird indentation problems with JsonDocument,
              I can't compare the two strings*/
            Assert.True(
				(await ex).Message.Contains(@"""reason"": ""unexpected""")
				&& 
				errorJsonResponse.Contains(@"""reason"": ""unexpected"""));
        }
    }
}
