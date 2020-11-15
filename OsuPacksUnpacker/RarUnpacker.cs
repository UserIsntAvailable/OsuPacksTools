using System;
using System.IO;
using System.Threading.Tasks;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Readers.Rar;

namespace OsuPacksUnpacker {
    public class RarUnpacker : IUnpacker {

        #region Private Fields

        private readonly string _osuSongsFolderPath;
        #endregion

        #region Constructor

        /// <summary>
        /// Initialize a <see cref="RarUnpacker"/> instance
        /// </summary>
        /// <param name="osuSongsFolderPath">The path of your osu songs folder</param>
        public RarUnpacker(string osuSongsFolderPath) => 
            _osuSongsFolderPath = osuSongsFolderPath;
        #endregion

        #region IUnpacker Implementation

        public event EventHandler<string> FileUnpacked;

        /// <summary>
        /// Unrar file content to yours osu songs folder
        /// </summary>
        /// <param name="stream">The RAR stream</param>
        public async Task Unpack(Stream stream) {

            await Task.Run(() => {

                using var reader = RarReader.Open(stream);

                while (reader.MoveToNextEntry()) {

                    var currentEntry = reader.Entry;

                    if (!currentEntry.IsDirectory) {

                        reader.WriteEntryToDirectory(
                            _osuSongsFolderPath,
                            new ExtractionOptions() {
                                ExtractFullPath = true,
                                Overwrite = true
                            });

                        OnFileUnpacked($"{_osuSongsFolderPath}/{currentEntry.Key}");
                    }
                }
            });
        }
        #endregion

        #region Protected Methods

        /// <summary>
        /// Notify the listeners that a file was unpacked
        /// </summary>
        /// <param name="filename">The name of the file</param>
        protected virtual void OnFileUnpacked(string filename)
            => FileUnpacked?.Invoke(this, filename);
        #endregion
    }
}
