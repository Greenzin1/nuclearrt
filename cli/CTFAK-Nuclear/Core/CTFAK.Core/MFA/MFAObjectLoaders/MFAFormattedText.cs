using CTFAK.Memory;
using System.Collections.Generic;

namespace CTFAK.MFA.MFAObjectLoaders
{
    public class MFAFormattedText : ObjectLoader
    {
        public uint Width;
        public uint Height;
        public uint Flags;

        public override void Read(ByteReader reader)
        {
            base.Read(reader);
            Width = reader.ReadUInt32();
            Height = reader.ReadUInt32();
            Flags = reader.ReadUInt32();
            var dataLen = reader.ReadInt32();
            if (dataLen > 0)
                reader.ReadBytes(dataLen);
        }
    }
}
