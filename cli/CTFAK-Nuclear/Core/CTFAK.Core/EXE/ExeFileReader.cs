using CTFAK.CCN;
using CTFAK.EXE;
using CTFAK.Memory;
using CTFAK.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CTFAK.CCN.Chunks;

namespace CTFAK.FileReaders
{
    public class ExeFileReader : IFileReader
    {
        public string Name => "Normal EXE";
        public GameData game;
        public Dictionary<int, Bitmap> Icons = new Dictionary<int, Bitmap>();

        public void LoadGame(string gamePath)
        {
            CTFAKCore.currentReader = this;
            Settings.gameType = Settings.GameType.NORMAL;

            var reader = new ByteReader(gamePath, FileMode.Open);
            ReadHeader(reader);
            PackData packData = null;
            if (Settings.Old)
            {
                Settings.Unicode = false;
                if (reader.PeekInt32() != 1162690896)
                {
                    while (true)
                    {
                        if (reader.Tell() >= reader.Size()) break;
                        var ID = reader.ReadInt16();
                        var flag = reader.ReadInt16();
                        var size = reader.ReadInt32();
                        reader.ReadBytes(size);
                        if (ID == 32639) break;
                    }
                }
            }
            else
            {
                packData = new PackData();
                packData.Read(reader);
            }

            game = new GameData();
            game.Read(reader);
        }

        public int ReadHeader(ByteReader reader)
        {
            var entryPoint = CalculateEntryPoint(reader);
            reader.Seek(0);
            byte[] exeHeader = reader.ReadBytes(entryPoint);
            var firstShort = reader.PeekUInt16();
            if (firstShort == 0x7777) Settings.gameType = Settings.GameType.NORMAL;
            else Settings.gameType = Settings.GameType.MMF15;
            return (int)reader.Tell();
        }

        public int CalculateEntryPoint(ByteReader exeReader)
        {
            var sig = exeReader.ReadAscii(2);
            if (sig != "MZ") Logger.Log("Invalid executable signature", true, ConsoleColor.Red);

            exeReader.Seek(60);
            var hdrOffset = exeReader.ReadUInt16();
            exeReader.Seek(hdrOffset);
            var peHdr = exeReader.ReadAscii(2);
            exeReader.Skip(4);
            var numOfSections = exeReader.ReadUInt16();
            exeReader.Skip(16);
            var optionalHeader = 28 + 68;
            var dataDir = 16 * 8;
            exeReader.Skip(optionalHeader + dataDir);

            var possition = 0;
            for (var i = 0; i < numOfSections; i++)
            {
                var entry = exeReader.Tell();
                var sectionName = exeReader.ReadAscii();
                if (sectionName == ".extra")
                {
                    exeReader.Seek(entry + 20);
                    possition = (int)exeReader.ReadUInt32();
                    break;
                }
                if (i >= numOfSections - 1)
                {
                    exeReader.Seek(entry + 16);
                    var size = exeReader.ReadUInt32();
                    var address = exeReader.ReadUInt32();
                    possition = (int)(address + size);
                    break;
                }
                exeReader.Seek(entry + 40);
            }
            exeReader.Seek(possition);
            return (int)exeReader.Tell();
        }

        public GameData getGameData() => game;
        public Dictionary<int, Bitmap> getIcons() => Icons;
        public void PatchMethods() { }
        public IFileReader Copy()
        {
            var reader = new ExeFileReader();
            reader.game = game;
            return reader;
        }
    }
}
