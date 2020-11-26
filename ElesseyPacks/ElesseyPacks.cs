using System;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.IO;
using System.IO.Compression;
using OsuPacksStorage;
using OsuPacksUnpacker;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Framework.Platform;

namespace ElesseyPacks {
    public class ElesseyPacks {

        #region Private Fields

        private string _osuSongFolder;

        private string _currentStreamName;

        private string _collectionFilePath;

        private const string COLLECTION_NAME = "collection.db";

        private CollectionManager _collectionManager;

        private readonly IStorage _storage;

        private readonly IUnpacker _unpacker;
        #endregion

        #region Constructor

        public ElesseyPacks(string apiKey, string osuFolder) {

            if (string.IsNullOrEmpty(apiKey)) {

                apiKey = Environment.GetEnvironmentVariable("GoogleApiKey");

                if (string.IsNullOrEmpty(apiKey))
                    throw new ArgumentException("You forgot to use the api-key option." +
                        " If you don't want to write it each time that you use the program, set it as an environment variable", nameof(apiKey));
            }
            if (string.IsNullOrEmpty(osuFolder)) {

                var paths = new string[] {
                @$"C:\Users\{Environment.UserName}\AppData\Local\osu!",
                @"C:\Program Files\osu!",
                @"C:\Program Files(x86)\osu!" }
                    .Where(p => Directory.Exists(p));

                osuFolder = paths.Count() switch {

                    1 => paths.First(),
                    0 => throw new ArgumentException("You forgot to use the osu-folder option." +
                         " Your osu folder isn't on the default installation path.", nameof(osuFolder)),
                    _ => throw new ArgumentException("You have more than 1 osu folder on your PC. Write wich one you want to use", nameof(osuFolder))
                };
            }

            _osuSongFolder = $"{osuFolder}/Songs";
            _collectionFilePath = $"{osuFolder}/{COLLECTION_NAME}";

            _storage = new GDDrive(apiKey);
            _unpacker = new RarUnpacker(_osuSongFolder);
        }
        #endregion

        #region Public Methods

        public async Task Start(bool addToCollection, bool openOsu, string regexPattern, string[] osuModes, string[] packsToDownload) {

            if (addToCollection) {

                if (!File.Exists(_collectionFilePath)) File.Create(_collectionFilePath);

                /// This doesn't do nothing at all, it only to initialize a <see cref="CollectionManager"/>
                _collectionManager = new CollectionManager(new NativeStorage(""));
                await Task.Run(() => _collectionManager.ImportStableCollection(File.OpenRead(_collectionFilePath)));

                _unpacker.FileUnpacked += OnFileUnpacked;
            }

            if (osuModes is null) osuModes = new string[] { "standard" };

            foreach (var osuMode in osuModes) {

                string gdFolderId = osuMode switch {

                    "o" or "osu" or "s" or "standard" => "13VvNppE_QvqFXOKhXG2hvewZU9PojpRh",
                    "c" or "catch" => "1BW-t1TMPlhZgtnxu39QtTwgBR2IFljy_",
                    "m" or "mania" => "1QEC59oSjJ25PlXvWxsG2Z08Y2NjM0aaO",
                    "t" or "taiko" => "17TKiSouvG-dQ4xwhdjnOC7Ey16JbsKiC",
                    _ => throw new ArgumentException($"This osu gamemode isn't avalaible: {osuMode}", nameof(osuModes)),
                };

                var packs = await _storage.ListFiles(gdFolderId);

                if (packsToDownload is not null && packsToDownload.Any()) {

                    packs = packs
                        .Where(p => packsToDownload.Contains(p.filename[..4]))
                        .ToArray();

                    if (!packs.Any()) {

                        throw new ArgumentException("The pack or packs that you select are not available", nameof(packsToDownload));
                    }
                }
                if (!string.IsNullOrEmpty(regexPattern)) {

                    packs = packs
                        .Where(p => Regex.IsMatch(p.filename, regexPattern))
                        .ToArray();

                    if (!packs.Any()) {

                        throw new ArgumentException("Your regex pattern didn't match any pack", nameof(regexPattern));
                    }
                }

                foreach (var (name, id) in packs) {

                    _currentStreamName = name.Replace(".rar", "");
#if DEBUG
                    try {
#endif
                        var stream = await _storage.GetFileAsStream(id);
                        await _unpacker.Unpack(stream);
#if DEBUG
                    }

                    catch (Exception e) {

                        Console.WriteLine("I just will skip. I want to test the program\n");

                        if (e.Message != "You already used your downlaod quota for this file. See Readme for more information about this.")
                            throw;
                    }
#endif
                }
            }

            if (openOsu) {
                try {
                    Process.Start(new ProcessStartInfo() {
                        FileName = Directory.GetFiles(_osuSongFolder,
                            "*.osz")[0],
                        UseShellExecute = true
                    });
                }

                catch (Win32Exception e) {
                    if (e.Message.Contains("associated"))
                        Console.WriteLine(".osz files have not been configured to open with osu!.exe on this system.\n" +
                            "To fix this, go to your osu songs folder, right click any .osz file, click properties, beside Opens with... click Change..., and select to osu!");
                }
            }

            _collectionManager?.SaveToFile(_collectionFilePath);
        }
        #endregion

        #region Private Methods

        private void OnFileUnpacked(object sender, string oszFilePath) {

            using ZipArchive zipArchive = new ZipArchive(File.OpenRead(oszFilePath));

            var beatmaps = zipArchive.Entries
                .Where(e => e.Name.Contains(".osu"))
                .Select(s => new BeatmapInfo() {
                    MD5Hash = string.Join(
                        "",
                        MD5.Create().ComputeHash(s.Open())
                            .Select(b => b.ToString("x2")))
                });

            _collectionManager.AddBeatmapCollection(_currentStreamName, beatmaps);
        }
        #endregion
    }
}
