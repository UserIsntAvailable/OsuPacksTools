using System;
using System.IO;
using System.Threading.Tasks;

namespace OsuPacksUnpacker {
    public interface IUnpacker {

        public event EventHandler<string> FileUnpacked;

        public Task Unpack(Stream stream);
    }
}
