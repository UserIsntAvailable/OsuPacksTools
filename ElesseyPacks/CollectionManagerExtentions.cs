using System.IO;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.IO.Legacy;
using osu.Game.Collections;
using System.Collections.Generic;

namespace ElesseyPacks {

    /// <summary>
    /// Why <see cref="CollectionManager"/> needs a <see cref="BeatmapManager"/> to work.....
    /// </summary>
    public static class CollectionManagerExtentions {

        /// <summary>
        /// Database version in stable-compatible YYYYMMDD format.
        /// </summary>
        private const int database_version = 30000000;

        /// <summary>
        /// Save this <see cref="CollectionManager"/> 
        /// </summary>
        /// <param name="collection">This collection</param>
        /// <param name="path">The file where the collection will be saved</param>
        public static void SaveToFile(this CollectionManager collection, string path) {

            using var sw = new SerializationWriter(File.OpenWrite(path));

            sw.Write(database_version);
            sw.Write(collection.Collections.Count);

            foreach (var c in collection.Collections) {

                sw.Write(c.Name.Value);
                sw.Write(c.Beatmaps.Count);

                foreach (var b in c.Beatmaps)
                    sw.Write(b.MD5Hash);
            }
        }

        /// <summary>
        /// Import a collection.db from osu-stable ( without the need of a BeatmapManager : / )
        /// </summary>
        /// <param name="collection">This collection</param>
        /// <param name="stream">The collection.db stream</param>
        public static void ImportStableCollection(this CollectionManager collection, Stream stream) {

            using var sr = new SerializationReader(stream);
            sr.ReadInt32(); // Version

            int collectionCount = sr.ReadInt32();

            for (int i = 0; i < collectionCount; i++) {

                var bmpCollection = new BeatmapCollection { Name = { Value = sr.ReadString() } };
                int mapCount = sr.ReadInt32();

                for (int j = 0; j < mapCount; j++) {

                    string checksum = sr.ReadString();

                    bmpCollection.Beatmaps.Add(new BeatmapInfo() { MD5Hash = checksum });
                }

                collection.Collections.Add(bmpCollection);
            }
        }

        /// <summary>
        /// Add a <see cref="BeatmapCollection"/> entry into the <see cref="CollectionManager.Collections"/>
        /// </summary>
        /// <param name="collection">This collection</param>
        /// <param name="bmpCollectionName">The name of the new ( or existing ) <see cref="BeatmapCollection"/></param>
        /// <param name="beatmaps">The beatmaps that will be added to the <see cref="BeatmapCollection"/></param>
        public static void AddBeatmapCollection(this CollectionManager collection, string bmpCollectionName, IEnumerable<BeatmapInfo> beatmaps) {

            var existing = collection.Collections
                .FirstOrDefault(c => c.Name.Value == bmpCollectionName);

            if (existing is null) {

                collection.Collections
                    .Add(existing = new BeatmapCollection {
                        Name = { Value = bmpCollectionName }
                    });
            }

            // Just remove duplicates
            beatmaps = beatmaps.Except(existing.Beatmaps);

            existing.Beatmaps.AddRange(beatmaps);
            
        }
    }
}
