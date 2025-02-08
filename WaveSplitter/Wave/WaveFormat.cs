using System.Diagnostics;

namespace WaveSplitter.Wave
{
    public class WaveFormat
    {
        /// <summary>format type</summary>
        protected WaveFormatEncoding waveFormatTag;
        /// <summary>number of channels</summary>
        protected short channels;
        /// <summary>sample rate</summary>
        protected int sampleRate;
        /// <summary>for buffer estimation</summary>
        protected int averageBytesPerSecond;
        /// <summary>block size of data</summary>
        protected short blockAlign;
        /// <summary>number of bits per sample of mono data</summary>
        protected short bitsPerSample;
        /// <summary>number of following bytes</summary>
        protected short extraSize;

        /// <summary>
        /// Creates a new PCM 44.1Khz stereo 16 bit format
        /// </summary>
        public WaveFormat() : this(44100, 16, 2)
        {

        }

        /// <summary>
        /// Reads a new WaveFormat object from a stream
        /// </summary>
        /// <param name="br">A binary reader that wraps the stream</param>
        public WaveFormat(BinaryReader br)
        {
            int formatChunkLength = br.ReadInt32();
            ReadWaveFormat(br, formatChunkLength);
        }

        /// <summary>
        /// Creates a new 16 bit wave format with the specified sample
        /// rate and channel count
        /// </summary>
        /// <param name="sampleRate">Sample Rate</param>
        /// <param name="channels">Number of channels</param>
        public WaveFormat(int sampleRate, int channels)
            : this(sampleRate, 16, channels)
        {
        }

        /// <summary>
        /// Creates a new PCM format with the specified sample rate, bit depth and channels
        /// </summary>
        public WaveFormat(int rate, int bits, int channels)
        {
            if (channels < 1)
            {
                throw new ArgumentOutOfRangeException("Channels must be 1 or greater", "channels");
            }
            // minimum 16 bytes, sometimes 18 for PCM
            waveFormatTag = WaveFormatEncoding.Pcm;
            this.channels = (short)channels;
            sampleRate = rate;
            bitsPerSample = (short)bits;
            extraSize = 0;

            blockAlign = (short)(channels * (bits / 8));
            averageBytesPerSecond = sampleRate * blockAlign;
        }

        /// <summary>
        /// Reads in a WaveFormat (with extra data) from a fmt chunk (chunk identifier and
        /// length should already have been read)
        /// </summary>
        /// <param name="br">Binary reader</param>
        /// <param name="formatChunkLength">Format chunk length</param>
        /// <returns>A WaveFormatExtraData</returns>
        public static WaveFormat FromFormatChunk(BinaryReader br, int formatChunkLength)
        {
            WaveFormatExtraData waveFormat = new WaveFormatExtraData();
            waveFormat.ReadWaveFormat(br, formatChunkLength);
            waveFormat.ReadExtraData(br);
            return waveFormat;
        }

        private void ReadWaveFormat(BinaryReader br, int formatChunkLength)
        {
            if (formatChunkLength < 16)
                throw new ApplicationException("Invalid WaveFormat Structure");
            waveFormatTag = (WaveFormatEncoding)br.ReadUInt16();
            channels = br.ReadInt16();
            sampleRate = br.ReadInt32();
            averageBytesPerSecond = br.ReadInt32();
            blockAlign = br.ReadInt16();
            bitsPerSample = br.ReadInt16();
            if (formatChunkLength > 16)
            {
                extraSize = br.ReadInt16();
                if (extraSize != formatChunkLength - 18)
                {
                    Debug.WriteLine("Format chunk mismatch");
                    extraSize = (short)(formatChunkLength - 18);
                }
            }
        }

        /// <summary>
        /// Reports this WaveFormat as a string
        /// </summary>
        /// <returns>String describing the wave format</returns>
        public override string ToString()
        {
            switch (waveFormatTag)
            {
                case WaveFormatEncoding.Pcm:
                case WaveFormatEncoding.Extensible:
                    // extensible just has some extra bits after the PCM header
                    return string.Format("{0} bit PCM: {1}kHz {2} channels",
                        bitsPerSample, sampleRate / 1000, channels);
                default:
                    return waveFormatTag.ToString();
            }
        }

        /// <summary>
        /// Compares with another WaveFormat object
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True if the objects are the same</returns>
        public override bool Equals(object obj)
        {
            WaveFormat? other = obj as WaveFormat;
            if (other != null)
            {
                return waveFormatTag == other.waveFormatTag &&
                    channels == other.channels &&
                    sampleRate == other.sampleRate &&
                    averageBytesPerSecond == other.averageBytesPerSecond &&
                    blockAlign == other.blockAlign &&
                    bitsPerSample == other.bitsPerSample;
            }
            return false;
        }

        /// <summary>
        /// Provides a Hashcode for this WaveFormat
        /// </summary>
        /// <returns>A hashcode</returns>
        public override int GetHashCode()
        {
            return (int)waveFormatTag ^
                channels ^
                sampleRate ^
                averageBytesPerSecond ^
                blockAlign ^
                bitsPerSample;
        }

        /// <summary>
        /// Returns the encoding type used
        /// </summary>
        public WaveFormatEncoding Encoding
        {
            get
            {
                return waveFormatTag;
            }
        }

        /// <summary>
        /// Writes this WaveFormat object to a stream
        /// </summary>
        /// <param name="writer">the output stream</param>
        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(18 + extraSize); // wave format length
            writer.Write((short)Encoding);
            writer.Write((short)Channels);
            writer.Write(SampleRate);
            writer.Write(AverageBytesPerSecond);
            writer.Write((short)BlockAlign);
            writer.Write((short)BitsPerSample);
            writer.Write(extraSize);
        }

        /// <summary>
        /// Returns the number of channels (1=mono,2=stereo etc)
        /// </summary>
        public int Channels
        {
            get
            {
                return channels;
            }
        }

        /// <summary>
        /// Returns the sample rate (samples per second)
        /// </summary>
        public int SampleRate
        {
            get
            {
                return sampleRate;
            }
        }

        /// <summary>
        /// Returns the average number of bytes used per second
        /// </summary>
        public int AverageBytesPerSecond
        {
            get
            {
                return averageBytesPerSecond;
            }
        }

        /// <summary>
        /// Returns the block alignment
        /// </summary>
        public virtual int BlockAlign
        {
            get
            {
                return blockAlign;
            }
        }

        /// <summary>
        /// Returns the number of bits per sample (usually 16 or 32, sometimes 24 or 8)
        /// Can be 0 for some codecs
        /// </summary>
        public int BitsPerSample
        {
            get
            {
                return bitsPerSample;
            }
        }

        /// <summary>
        /// Returns the number of extra bytes used by this waveformat. Often 0,
        /// except for compressed formats which store extra data after the WAVEFORMATEX header
        /// </summary>
        public int ExtraSize
        {
            get
            {
                return extraSize;
            }
        }
    }
}
