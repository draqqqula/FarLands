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
        public IEnumerable<byte> Pack(ContentStorage contentStorage);

    }

    public class ByteKeyAttribute : Attribute
    {
        public byte value;

        public ByteKeyAttribute(byte value)
        {
            this.value = value;
        }
    }
}
