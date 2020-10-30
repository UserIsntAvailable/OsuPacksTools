using System.Threading.Tasks;

namespace OsuPackUnpacker {

    // Just in case I want to change my file storage service
    public interface IPacksDownloader {

        public Task Download(string file);

        public Task<(string nameFile, string filePath)[]> ListFiles(string source);
    }
}
