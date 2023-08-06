using ABI.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public interface IPackable
    {
        public byte[] Pack();

        [RequiresPreviewFeatures]
        public abstract static IPackable Unpack(ReadOnlySpan<byte> bytes);
    }
}
