using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OsuPacksStorage {
    public static class StreamExtentions {

        public async static Task CopyToAsync(this Stream source, Stream destination, IProgress<long> progress = null, CancellationToken cT = default) {

            var buffer = new byte[81920];
            int bytesRead;
            long totalRead = 0;

            while ((bytesRead = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cT)) > 0) {

                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cT);
                cT.ThrowIfCancellationRequested();
                totalRead += bytesRead;
                progress?.Report(totalRead);
            }
        }
    }
}
