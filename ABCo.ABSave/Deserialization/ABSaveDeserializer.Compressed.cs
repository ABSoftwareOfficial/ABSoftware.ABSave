﻿namespace ABCo.ABSave.Deserialization
{
    // ===============================================
    // NOTE: Full details about the "compressed numerical" structure can be seen in the TXT file: CompressedPlan.txt in the project root
    // ===============================================
    // This is just an implementation of everything shown there.
    public sealed partial class ABSaveDeserializer
    {
        public uint ReadCompressedInt()
        {
            var source = new BitSource(this);
            return ReadCompressedInt(ref source);
        }

        public uint ReadCompressedInt(ref BitSource source) => (uint)ReadCompressed(false, ref source);

        public ulong ReadCompressedLong()
        {
            var source = new BitSource(this);
            return ReadCompressedLong(ref source);
        }

        public ulong ReadCompressedLong(ref BitSource source) => ReadCompressed(true, ref source);

        public ulong ReadCompressed(bool canBeLong, ref BitSource source)
        {
            if (source.FreeBits == 0) source.MoveToNewByte();

            if (Settings.LazyCompressedWriting)
                return ReadCompressedLazyFast(canBeLong, ref source);
            else
                return ReadCompressedSlow(ref source);
        }

        public ulong ReadCompressedLazyFast(bool canBeLong, ref BitSource source)
        {
            if ((source.FinishByte() & 1) > 0)
                return ReadByte();
            else if (canBeLong)
                return (ulong)ReadInt64();
            else
                return (ulong)ReadInt32();
        }

        public ulong ReadCompressedSlow(ref BitSource source)
        {
            // Process header
            byte preHeaderCapacity = source.FreeBits;
            (byte noContBytes, byte headerLen) = ReadNoContBytes(ref source);

            byte bitsToGo = (byte)(8 * noContBytes);
            ulong res = ReadFirstByteData(ref source, headerLen, bitsToGo, preHeaderCapacity);

            while (bitsToGo > 0)
            {
                bitsToGo -= 8;
                res |= (ulong)ReadByte() << bitsToGo;
            }

            return res;
        }

        static ulong ReadFirstByteData(ref BitSource source, byte headerLen, byte noContBits, byte preHeaderCapacity)
        {
            bool isExtended = preHeaderCapacity < 4;
            ulong res = 0;

            // For an extended first byte (yyy-xxxxxxxx)
            if (isExtended)
            {
                // If there are still "y" bits left, get them.
                if (headerLen < preHeaderCapacity) res = (ulong)source.ReadInteger(source.FreeBits) << noContBits << 8;

                // Make sure we're ready to read "x"s. There will always be "x"es as the header can not physically take them all up.
                if (source.FreeBits == 0) source.MoveToNewByte();
            }

            return res | ((ulong)source.ReadInteger(source.FreeBits) << noContBits);
        }

        (byte noContBytes, byte headerLen) ReadNoContBytes(ref BitSource source)
        {
            for (byte i = 0; i < 8; i++)
            {
                if (!source.ReadBit()) return (i, (byte)(i + 1));
            }

            return (8, 8);
        }
    }
}
