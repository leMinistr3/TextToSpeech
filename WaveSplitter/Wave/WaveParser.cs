using System.Timers;
using WaveSplitter.Extention;

namespace WaveSplitter.Wave
{
    public class WaveParser
    {
        public uint waveSize { get; private set; }
        public byte[] audioBytes { get; private set; }
        public WaveFormat? waveFormat { get; private set; }
        public virtual TimeSpan totalTime
        {
            get
            {
                if (waveFormat != null)
                {
                    return TimeSpan.FromSeconds((double)dataChunkLength / waveFormat.AverageBytesPerSecond);
                }
                return TimeSpan.Zero;
            }
        }

        private bool missingFileSize { get; set; }
        private readonly int datachunkId = BitConverter.ToInt32([(byte)'d', (byte)'a', (byte)'t', (byte)'a'], 0);
        private readonly int formatChunkId = BitConverter.ToInt32([(byte)'f', (byte)'m', (byte)'t', (byte)' '], 0);
        private long dataChunkPosition { get; set; }
        private long dataChunkLength { get; set; }
        private long formatChunkPosition { get; set; }

        public WaveParser(byte[] audioBytes, bool automaticRepair = true) : this(new MemoryStream(audioBytes), automaticRepair) { }
        public WaveParser(Stream stream, bool automaticRepair = true) : this(streamToMemoryStream(stream), automaticRepair) { }
        public WaveParser(MemoryStream ms, bool automaticRepair = true)
        {
            ms.Seek(0, SeekOrigin.Begin);

            using (BinaryReader br = new BinaryReader(ms))
            {
                // Read RIFF Header
                if (br.ReadInt32() != BitConverter.ToInt32([(byte)'R', (byte)'I', (byte)'F', (byte)'F'], 0))
                    throw new FormatException("Not a RIFF file");

                waveSize = br.ReadUInt32(); // File size from header

                if (br.ReadInt32() != BitConverter.ToInt32([(byte)'W', (byte)'A', (byte)'V', (byte)'E'], 0))
                    throw new FormatException("Not a WAVE file");

                dataChunkPosition = -1;
                formatChunkPosition = -1;
                bool formatFound = false;
                bool dataFound = false;
                // Find fromat and Data Chunk
                while (ms.Position <= ms.Length - 8)
                {
                    Int32 chunkID = br.ReadInt32();
                    Int32 chunkSize = br.ReadInt32();

                    if (chunkID == formatChunkId)
                    {
                        formatChunkPosition = ms.Position;
                        waveFormat = WaveFormat.FromFormatChunk(br, chunkSize);
                        ms.Position = formatChunkPosition;
                        formatFound = true;
                    }
                    if (chunkID == datachunkId)
                    {
                        dataChunkPosition = ms.Position;
                        dataChunkLength = chunkSize;
                        dataFound = true;
                    }
                    if (formatFound && dataFound)
                    {
                        break;
                    }

                    ms.Position += chunkSize;
                }

                if (formatChunkPosition == -1)
                    throw new FormatException("No format chunk found");
                if (dataChunkPosition == -1)
                    throw new FormatException("No data chunk found");

                ms.Seek(0, SeekOrigin.Begin);
                if (automaticRepair && (waveSize == uint.MaxValue || dataChunkLength == -1))
                {

                    audioBytes = RepairPipeWave(ms);
                }
                else
                {
                    audioBytes = br.ReadBytes((int)ms.Length);
                }
            }
        }

        public byte[] AppendSilence(double seconde, WaveAppend waveAppend)
        {
            if (waveFormat == null)
                throw new FormatException("No Wave format found");

            long silenceLength = (long)(seconde * waveFormat.AverageBytesPerSecond);

            byte[] header = audioBytes.BigTake(dataChunkPosition).ToArray();
            byte[] data = audioBytes.BigSkip(dataChunkPosition).BigTake(dataChunkLength).ToArray();
            byte[] silence = new byte[(seconde < 0) ? 0 : silenceLength];

            if (silenceLength < 0) // If silence lengh is negative renmove audio data
            {
                silenceLength = Math.Abs(silenceLength);

                data = (waveAppend == WaveAppend.AtTheBeginning) ?
                    data.BigSkip(silenceLength).ToArray() : // To remove begginning skip byte
                    data.BigTake(data.Length - silenceLength).ToArray(); // To remove at the end take first byte
            }

            MemoryStream ms = new MemoryStream();

            if (waveAppend == WaveAppend.AtTheBeginning)
            {
                ms = CombineBytes(header, silence, data);
            }
            else
            {
                ms = CombineBytes(header, data, silence);
            }
            audioBytes = RepairPipeWave(ms);

            ms.Dispose();

            return audioBytes;
        }

        private byte[] RepairPipeWave(MemoryStream ms)
        {
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                // Add FileSize in the Wave Header
                ms.Seek(0, SeekOrigin.End);
                uint fileSize = (uint)(ms.Length - 8);
                ms.Seek(4, SeekOrigin.Begin);
                bw.Write(fileSize);
                waveSize = fileSize;

                // Add Data Chunk Size in the Chunk Header
                // Calculate Correct Data Chunk Length
                long correctDataChunkLength = ms.Length - dataChunkPosition;
                // Overwrite Data Chunk Length
                ms.Seek(dataChunkPosition - 4, SeekOrigin.Begin);
                bw.Write((int)correctDataChunkLength);
                dataChunkLength = correctDataChunkLength;

                return ms.ToArray();
            }
        }

        private static MemoryStream streamToMemoryStream(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms;
        }

        private static MemoryStream CombineBytes(params byte[][] arrays)
        {
            MemoryStream ms = new MemoryStream();

            foreach (var array in arrays)
            {
                ms.Write(array, 0, array.Length);
            }
            return ms;

        }
    }
}
