// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
    {
        private VideoDecoderProcess _decoderProcess;
        private Texture2D _lastFrame; 

        public override MediaState State
        {
            get { return base.State; }
            protected set { base.State = value; }
        }

        public override bool IsLooped
        {
            get { return base.IsLooped; }
            set { base.IsLooped = value; }
        }

        public override bool IsMuted
        {
            get { return base.IsMuted; }
            set
            {
                base.IsMuted = value;
                throw new NotImplementedException();
            }
        }

        public override TimeSpan PlayPosition
        {
            get { throw new NotImplementedException(); }
        }

        public override float Volume
        {
            get { return base.Volume; }
            set
            {
                base.Volume = value;
                if (base.Video != null)
                    PlatformSetVolume();
            }
        }

        internal ConcreteVideoPlayerStrategy()
        {
            
        }

        public override Texture2D PlatformGetTexture()
        {
            if (_lastFrame != null)
            {
                if (_lastFrame.Width != base.Video.Width || _lastFrame.Height != base.Video.Height)
                {
                    _lastFrame.Dispose();
                    _lastFrame = null;
                }
            }
            if (_lastFrame == null)
                _lastFrame = new Texture2D(((IPlatformVideo)base.Video).Strategy.GraphicsDevice, base.Video.Width, base.Video.Height, false, SurfaceFormat.Color);

            // test. read next frame.
            if (false)
            {
                while (!_decoderProcess._isframeDataDirty)
                    _decoderProcess.ParseSegmentCluster2(_decoderProcess._binaryReader, (long)0);
            }

            if (_decoderProcess._isframeDataDirty)
            {
                _lastFrame.SetData(_decoderProcess._frameData);
                _decoderProcess._isframeDataDirty = false;
            }

            return _lastFrame;
        }

        protected override void PlatformUpdateState(ref MediaState state)
        {
            
        }

        public override void PlatformPlay(Video video)
        {
            base.Video = video;

            // Cleanup the last video first.
            if (State != MediaState.Stopped)
            {
                _decoderProcess.Dispose();
                _decoderProcess = null;
            }

            string ffmpegPath = "x64\\ffmpeg";
            string inputFile = "" + ((IPlatformVideo)base.Video).Strategy.FileName;

            _decoderProcess = new VideoDecoderProcess(base.Video, ffmpegPath, inputFile);

        }

        public override void PlatformPause()
        {
            throw new NotImplementedException();
        }

        public override void PlatformResume()
        {
            throw new NotImplementedException();
        }

        public override void PlatformStop()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetVolume()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_decoderProcess != null)
                {
                    _decoderProcess.Dispose();
                    _decoderProcess = null;
                }

            }


            base.Dispose(disposing);
        }
    }

    internal class VideoDecoderProcess : IDisposable
    {
        private Process _ffmpegProcess;
        private StreamReader _outputReader;
        internal PipeBinaryReader _binaryReader;

        internal byte[] _frameData;
        internal bool _isframeDataDirty;

        public VideoDecoderProcess(Video video, string ffmpegPath, string inputFile)
        {
            int frameSize = video.Width * video.Height * 4; // RBGA
            this._frameData = new byte[frameSize];

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = ffmpegPath;
            startInfo.Arguments = $"-i \"{inputFile}\" -c:v rawvideo -c:a pcm_s16le -ar 48000 -pix_fmt rgba -f matroska -";

            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            _ffmpegProcess = new Process();
            _ffmpegProcess.StartInfo = startInfo;
            _ffmpegProcess.Start();

            _outputReader = _ffmpegProcess.StandardOutput;
            _binaryReader = new PipeBinaryReader(_outputReader.BaseStream);

            ParseMKV(_binaryReader);
        }

        private MKVElement ReadIDInt(PipeBinaryReader reader)
        {
            byte first = reader.ReadByte();
            int length = 1;

            byte mask = 0x80;
            while ((first & mask) == 0)
            {
                mask >>= 1;
                length++;
            }

            ulong value = first;
            for (int i = 1; i < length; i++)
            {
                value = (value << 8) | reader.ReadByte();
            }
            return (MKVElement)value;
        }

        private ulong ReadVInt(PipeBinaryReader reader)
        {
            byte first = reader.ReadByte();
            int length = 1;

            int mask = 0x80;
            while ((first & mask) == 0)
            {
                mask >>= 1;
                length++;
            }

            ulong value = (ulong)(first & (mask - 1));
            for (int i = 1; i < length; i++)
            {
                value = (value << 8) | reader.ReadByte();
            }

            return value;
        }

        private ulong ReadUInt(PipeBinaryReader reader, int size)
        {
            ulong value = 0;
            for (int i = 0; i < size; i++)
            {
                value = (value << 8) | reader.ReadByte();
            }
            return value;
        }

        enum MKVElement
        {
            EBMLHeader  = 0x1A45DFA3,

            Segment     = 0x18538067,
            
            SeekHead    = 0x114D9B74,
            Info        = 0x1549A966,
            InfoTimestampScale = 0x2AD7B1,

            Tracks      = 0x1654AE6B,
            TrackEntry  = 0xAE,
            TrackNumber = 0xD7,
            TrackType   = 0x83,
            TrackUID    = 0x73C5,
            TrackLanguage = 0x22B59C,
            TrackCodecID  = 0x86,
            TrackDefaultDuration = 0x23E383,
            TrackVideoSettings = 0xE0,
            TrackAudioSettings = 0xE1,
            TrackCodecPrivate  = 0x63A2,

            Cluster     = 0x1F43B675,
            Timecode    = 0xE7,
            SimpleBlock = 0xA3,
        }

        enum MKVTrackType
        {
            Video    = 0x01,
            Audio    = 0x02,
            Subtitle = 0x11,
        }

        [Flags]
        enum SimpleBlockFlags
        {
            Keyframe    = 0x80,
            Invisible   = 0x08,
            LacingNone  = 0x00,
            LacingXiph  = 0x02,
            LacingFixed = 0x04,
            LacingEBML  = 0x06,
        };

        struct EBMLElementHeader
        {
            public MKVElement Id;
            public ulong Size;
        }

        private Dictionary<int, MKVTrackType> _tracks = new Dictionary<int, MKVTrackType>();


        private void ParseMKV(PipeBinaryReader reader)
        {
            ReadEBMLElement(_binaryReader);
            EBMLElementHeader segmentElementHdr = ReadSegmentStart(reader);
            EBMLElementHeader clusterHdr = ParseSegmentTracks(reader, (long)segmentElementHdr.Size);
            // parse first cluster
            if (clusterHdr.Id == MKVElement.Cluster)
                ParseCluster(reader, (long)clusterHdr.Size);

            while (!_isframeDataDirty)
                ParseSegmentCluster2(reader, (long)segmentElementHdr.Size);
        }

        private EBMLElementHeader ReadSegmentStart(PipeBinaryReader reader)
        {
            while (true)
            {
                EBMLElementHeader elementHdr = ReadElementHeader(reader);

                if (elementHdr.Id == MKVElement.Segment)
                {
                    return elementHdr;
                }
                else // unknown element
                {
                    reader.SkipBytes(elementHdr.Size);
                }
            }
        }

        private void ReadEBMLElement(PipeBinaryReader reader)
        {
            EBMLElementHeader emblElementHdr = ReadElementHeader(reader);
            if (emblElementHdr.Id != MKVElement.EBMLHeader)
                throw new InvalidDataException("Expected EBML Header (0x1A45DFA3)");
            reader.SkipBytes(emblElementHdr.Size);
        }

        private EBMLElementHeader ReadElementHeader(PipeBinaryReader reader)
        {
            EBMLElementHeader header;
            header.Id   = ReadIDInt(reader);
            header.Size = ReadVInt(reader);
            return header;
        }

        /// <returns>Cluster ElementHeader</returns>
        private EBMLElementHeader ParseSegmentTracks(PipeBinaryReader reader, long segmentSize)
        {
            long start = reader.Position;

            while (reader.Position < start + segmentSize)
            {
                EBMLElementHeader elementHdr = ReadElementHeader(reader);

                long dataPosition = reader.Position;

                Debug.WriteLine($"  Segment element ID: {elementHdr.Id:X}, Size: {elementHdr.Size}");

                switch (elementHdr.Id)
                {
                    case MKVElement.Info:
                        ParseTracksInfo(reader, (long)elementHdr.Size);
                        break;

                    case MKVElement.Tracks:
                        ParseTracks(reader, (long)elementHdr.Size);
                        break;
                    case MKVElement.Cluster:
                        return elementHdr;

                    case MKVElement.SeekHead:
                    default:
                        reader.SkipBytes(elementHdr.Size);
                        break;
                }
            }

            return new EBMLElementHeader();
        }

        private void ParseTracksInfo(PipeBinaryReader reader, long infoSize)
        {
            long start = reader.Position;

            while (reader.Position < start + infoSize)
            {
                EBMLElementHeader elementHdr = ReadElementHeader(reader);
                long dataPosition = reader.Position;

                switch (elementHdr.Id)
                {
                    case MKVElement.InfoTimestampScale:
                        reader.SkipBytes(elementHdr.Size);
                        break;

                    default:
                        reader.SkipBytes(elementHdr.Size);
                        break;
                }
            }
        }

        private void ParseTracks(PipeBinaryReader reader, long trackSize)
        {
            long start = reader.Position;

            while (reader.Position < start + trackSize)
            {
                EBMLElementHeader elementHdr = ReadElementHeader(reader);
                long dataPosition = reader.Position;

                switch (elementHdr.Id)
                {
                    case MKVElement.TrackEntry:
                        ParseTrackEntry(reader, (long)elementHdr.Size);
                        break;

                    default:
                        reader.SkipBytes(elementHdr.Size);
                        break;
                }
            }
        }

        private void ParseTrackEntry(PipeBinaryReader reader, long entrySize)
        {
            long start = reader.Position;

            int trackNumber = -1;
            MKVTrackType trackType = (MKVTrackType)(-1);

            while (reader.Position < start + entrySize)
            {
                EBMLElementHeader elementHdr = ReadElementHeader(reader);
                switch (elementHdr.Id)
                {
                    case MKVElement.TrackNumber:
                        trackNumber = (int)ReadUInt(reader, (int)elementHdr.Size);
                        break;
                    case MKVElement.TrackType:
                        Debug.Assert(elementHdr.Size == 1);
                        trackType = (MKVTrackType)reader.ReadByte();
                        break;

                    case MKVElement.TrackUID:
                    case MKVElement.TrackLanguage:
                    case MKVElement.TrackCodecID:
                    case MKVElement.TrackDefaultDuration:
                    case MKVElement.TrackVideoSettings:
                    case MKVElement.TrackAudioSettings:
                    case MKVElement.TrackCodecPrivate:
                    default:
                        reader.SkipBytes(elementHdr.Size);
                        break;
                }
            }

            if (trackNumber != -1)
                _tracks[trackNumber] = trackType;

            Debug.WriteLine($"Track Number: {trackNumber}, Type: {(trackType == MKVTrackType.Video ? "Video" : trackType == MKVTrackType.Audio ? "Audio" : "Other")}");
        }

        private void ParseSegmentCluster(PipeBinaryReader reader, long segmentSize)
        {
            long start = reader.Position;

            while (reader.Position < start + segmentSize)
            {
                EBMLElementHeader elementHdr = ReadElementHeader(reader);

                long dataPosition = reader.Position;

                Debug.WriteLine($"  Segment element ID: {elementHdr.Id:X}, Size: {elementHdr.Size}");

                switch (elementHdr.Id)
                {
                    case MKVElement.Cluster:
                        ParseCluster(reader, (long)elementHdr.Size);
                        break;

                    default:
                        reader.SkipBytes(elementHdr.Size);
                        break;
                }
            }
        }

        internal void ParseSegmentCluster2(PipeBinaryReader reader, long segmentSize)
        {
            long start = reader.Position;

            EBMLElementHeader elementHdr = ReadElementHeader(reader);

            long dataPosition = reader.Position;

            Debug.WriteLine($"  Segment element ID: {elementHdr.Id:X}, Size: {elementHdr.Size}");

            switch (elementHdr.Id)
            {
                case MKVElement.Cluster:
                    ParseCluster(reader, (long)elementHdr.Size);
                    break;

                default:
                    reader.SkipBytes(elementHdr.Size);
                    break;
            }
        }

        private void ParseCluster(PipeBinaryReader reader, long clusterSize)
        {
            long start = reader.Position;
            int clusterTimecode = 0;

            while (reader.Position < start + clusterSize)
            {
                EBMLElementHeader elementHdr = ReadElementHeader(reader);
                long dataPosition = reader.Position;

                if (elementHdr.Id == MKVElement.Timecode)
                {
                    clusterTimecode = (int)ReadUInt(reader, (int)elementHdr.Size);
                    Debug.WriteLine($"Cluster timecode: {clusterTimecode}");
                }
                else if (elementHdr.Id == MKVElement.SimpleBlock)
                {
                    ParseSimpleBlock(reader, (long)elementHdr.Size, clusterTimecode);
                }
                else
                {
                    reader.SkipBytes(elementHdr.Size);
                }
            }
        }

        private void ParseSimpleBlock(PipeBinaryReader reader, long blockSize, int clusterTimecode)
        {
            long blockStart = reader.Position;

            ulong trackNumber = ReadVInt(reader);
            short blockTimecode = reader.ReadInt16();
            SimpleBlockFlags flags = (SimpleBlockFlags)reader.ReadByte();

            long frameDataSize = blockSize -(reader.Position - blockStart);

            MKVTrackType trackType = _tracks[(int)trackNumber];
            switch (trackType)            
            {
                case MKVTrackType.Video:
                    {
                        Debug.Assert(frameDataSize == _frameData.Length);
                        ReadVideoFrame(_frameData);
                    }
                    break;
                case MKVTrackType.Audio:
                    {
                        byte[] frameData = reader.ReadBytes((int)frameDataSize);
                    }
                    break;

                case MKVTrackType.Subtitle:
                default:
                reader.SkipBytes((ulong)frameDataSize);
                    break;
            }

            int absoluteTimecode = clusterTimecode + blockTimecode;

            Debug.WriteLine($"  SimpleBlock -> Track: {trackNumber}, Time: {absoluteTimecode}, Keyframe: {(flags & SimpleBlockFlags.Keyframe) != 0}, Size: {frameDataSize}");
        }

        public void ReadVideoFrame(byte[] frameData)
        {
            int totalBytesRead = 0;
            while (totalBytesRead < frameData.Length)
            {
                int bytesRead = _binaryReader.Read(frameData, totalBytesRead, frameData.Length - totalBytesRead);
                totalBytesRead += bytesRead;
            }

            _isframeDataDirty = true;
        }

        int audioFrameSize = (44100 / 30) * 2 * 2;

        public bool ReadAudioFrame()
        {
            var st = Keyboard.GetState();
            if (st.IsKeyDown(Keys.O))
                audioFrameSize -= 1 * 2 ;
            if (st.IsKeyDown(Keys.P))
                audioFrameSize += 1 * 2 ;


            byte[] audioData = new byte[audioFrameSize];

            int totalBytesRead = 0;
            while (totalBytesRead < audioData.Length)
            {
                int bytesRead = _binaryReader.Read(audioData, totalBytesRead, audioData.Length - totalBytesRead);
                if (bytesRead == 0)
                    return false;
                totalBytesRead += bytesRead;
            }

            return true;
        }

        #region IDisposable Members

        ~VideoDecoderProcess()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            _ffmpegProcess.Kill();
            _ffmpegProcess.Dispose();

            _binaryReader.Dispose();
            _outputReader.Dispose();

            _frameData = null;
        }
        #endregion
    }
}
