using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OsuPacksStorage {

    public interface IStorage {

        #region Abstract Methods

        public Task<Stream> GetFileAsStream(string file);

        public Task<(string filename, string fileId)[]> ListFiles(string path);
        #endregion

        #region Default Methods

        /// <summary>
        /// Copy a file from a <see cref="IStorage"/> to a file in this system
        /// </summary>
        /// <param name="file">The file that you want to copy</param>
        /// <param name="path">Where the file will be saved</param>
        /// <param name="progress">An instance of a <see cref="IProgress{long}"/></param>
        /// <param name="cT">A cancellation token</param>
        public async Task CopyToAsync(string file, string path, IProgress<long> progress = default, CancellationToken cT = default) {

            await using var fileStream = File.Create(path);
            await using var storageStream = await GetFileAsStream(file);

            await storageStream.CopyToAsync(fileStream, progress, cT);
        }
        #endregion
    }
}
