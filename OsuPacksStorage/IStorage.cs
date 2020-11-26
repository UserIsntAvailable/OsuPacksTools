using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OsuPacksStorage {

    public interface IStorage {

        /// <summary>
        /// Copy a file from a <see cref="IStorage"/> to a file in the computer
        /// </summary>
        /// <param name="file">The file that you want to copy</param>
        /// <param name="path">Where the file will be saved</param>
        /// <param name="cT">A cancellation token</param>
        public async Task CopyTo(string file, string path, CancellationToken cT = default) 
            => await (await GetFileAsStream(file)).CopyToAsync(File.Create(path), cT);

        public Task<Stream> GetFileAsStream(string file);

        public Task<(string filename, string fileId)[]> ListFiles(string path);
    }
}
