using System;
using System.Diagnostics;
using System.Threading.Tasks;
using OsuPackUnpacker;

const string testFolderId = "13VvNppE_QvqFXOKhXG2hvewZU9PojpRh";

await GoogleAPITest();

static async Task GoogleAPITest() {

    var apiKey = Environment.GetEnvironmentVariable("GoogleApiKey");

    Stopwatch stopwatch = new();
    stopwatch.Start();

    IPacksDownloader gdD = new GDDownloader(apiKey);

    var files = await gdD.ListFiles(testFolderId);

    Console.WriteLine(files.Length);

    //foreach (var file in files) {

    //    await gdD.GetFileAsStream(file.fileId);
    //}

    // Just for testing
    Console.WriteLine(stopwatch.ElapsedMilliseconds);
}