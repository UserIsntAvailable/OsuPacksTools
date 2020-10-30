using System;
using System.Diagnostics;
using System.Threading.Tasks;
using OsuPackUnpacker;

const string testFolderId = "13VvNppE_QvqFXOKhXG2hvewZU9PojpRh";

await GoogleAPITest();

static async Task GoogleAPITest() {

    Stopwatch stopwatch = new();
    stopwatch.Start();

    using var gdD = new GDDownloader(Environment.GetEnvironmentVariable("GoogleApiKey"));

    var files = await gdD.ListFiles(testFolderId);

    foreach (var file in files) {

        await gdD.Download(file.filePath);
    }

    // Just for testing
    Console.WriteLine(stopwatch.ElapsedMilliseconds);
}