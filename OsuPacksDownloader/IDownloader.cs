using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OsuPacksDownloader {

    // Just in case I want to change my file storage service
    public interface IDownloader {

        public async Task Download(string fileId, string path, CancellationToken cT = default) {

            await (await GetFileAsStream(fileId)).CopyToAsync(File.Create(path), cT);
        }

        public Task<Stream> GetFileAsStream(string fileId);

        public Task<(string filename, string fileId)[]> ListFiles(string source);
    }
}
