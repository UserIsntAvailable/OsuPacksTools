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
        public async Task CopyToAsync(string file, string path, IProgress<long> progress = default, CancellationToken cT = default)
            => await CopyToAsync(await GetFileAsStream(file), File.Create(path), progress, cT);
        #endregion

        #region Private Methods

        private static async Task CopyToAsync(Stream source, Stream destination, IProgress<long> progress, CancellationToken cT) {

            var buffer = new byte[0x1000];
            int bytesRead;
            long totalRead = 0;

            while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cT)) > 0) {

                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cT);
                cT.ThrowIfCancellationRequested();
                totalRead += bytesRead;
                Thread.Sleep(10);
                progress?.Report(totalRead);
            }
        }
        #endregion
    }
}
