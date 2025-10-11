// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    internal class VideoDecoderProcess : IDisposable
    {
        private Process _ffmpegProcess;
        private StreamReader _outputReader;
        internal PipeBinaryReader _binaryReader;

        internal byte[] _videoFrameData;
        internal bool _isVideoFrameDataDirty;

        internal byte[] _audioFrameData;

        public VideoDecoderProcess(Video video, string ffmpegPath, string inputFile)
        {
            int frameSize = video.Width * video.Height * 4; // RBGA
            this._videoFrameData = new byte[frameSize];

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

        private unsafe double ReadFInt(PipeBinaryReader reader, ulong size)
        {
            switch (size)
            {
                case 8:
                    ulong raw64 = ReadUInt(reader, 8);
                    return *((double*)&raw64);
                case 4:
                    uint raw32 = (uint)ReadUInt(reader, 4);
                    return *((float*)&raw32);

                default:
                    throw new InvalidOperationException();
            }
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

            TrackAudioSettingsChannels = 0x9F,
            TrackAudioSettingsSamplingFrequency = 0xB5,

            TrackVideoSettingsPixelWidth    = 0xB0,
            TrackVideoSettingsPixelHeight   = 0xBA,
            TrackVideoSettingsDisplayWidth  = 0x54B0,
            TrackVideoSettingsDisplayHeight = 0x54BA,
            TrackVideoSettingsUncompressedFourCC = 0x2EB524,

            Cluster     = 0x1F43B675,
            Timecode    = 0xE7,
            SimpleBlock = 0xA3,
        }

        internal enum MKVTrackType
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

        public struct MKVAudioSettings
        {
            public int Channels;
            public double SamplingFrequency;
        }

        public struct MKVVideoSettings
        {
            public int PixelWidth;
            public int PixelHeight;
            public int DisplayWidth;
            public int DisplayHeight;
            public int FourCC;
        }

        internal class MKVAudioTrack : MKVTrack
        {
            public readonly MKVAudioSettings AudioSettings;

            public MKVAudioTrack(MKVTrackType type, MKVAudioSettings audioSettings) : base(type)
            {
                AudioSettings = audioSettings;
            }
        }

        internal class MKVVideoTrack : MKVTrack
        {
            public readonly MKVVideoSettings VideoSettings;

            public MKVVideoTrack(MKVTrackType type, MKVVideoSettings videoSettings) : base(type)
            {
                VideoSettings = videoSettings;
            }
        }        

        internal class MKVTrack
        {
            public readonly MKVTrackType Type;

            public MKVTrack(MKVTrackType type)
            {
                Type = type;
            }
        }

        struct EBMLElementHeader
        {
            public MKVElement Id;
            public ulong Size;
        }

        internal Dictionary<int, MKVTrack> _tracks = new Dictionary<int, MKVTrack>();


        internal void ParseMKV()
        {
            ReadEBMLElement(_binaryReader);
            EBMLElementHeader segmentElementHdr = ReadSegmentStart(_binaryReader);
            EBMLElementHeader clusterHdr = ParseSegmentTracks(_binaryReader, (long)segmentElementHdr.Size);

            // parse first cluster
            if (clusterHdr.Id == MKVElement.Cluster)
                ParseCluster(_binaryReader, (long)clusterHdr.Size);

            while (!_isVideoFrameDataDirty)
                ParseSegmentCluster2(_binaryReader, (long)segmentElementHdr.Size);
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
            MKVAudioSettings audioSettings;
            audioSettings.Channels = 1;
            audioSettings.SamplingFrequency = 8000;
            MKVVideoSettings videoSettings = default;

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
                    case MKVElement.TrackAudioSettings:
                        audioSettings = ParseTrackAudioSettings(reader, (long)elementHdr.Size);
                        break;
                    case MKVElement.TrackVideoSettings:
                        videoSettings = ParseTrackVideoSettings(reader, (long)elementHdr.Size);
                        break;
                    case MKVElement.TrackDefaultDuration:
                        // Number of nanoseconds per frame, expressed in Matroska Ticks
                        int defaultDuration = (int)ReadUInt(reader, (int)elementHdr.Size);
                        break;

                    case MKVElement.TrackUID:
                    case MKVElement.TrackLanguage:
                    case MKVElement.TrackCodecID:
                    case MKVElement.TrackCodecPrivate:
                    default:
                        reader.SkipBytes(elementHdr.Size);
                        break;
                }
            }

            if (trackNumber != -1)
            {
                switch (trackType)
                {
                    case MKVTrackType.Audio:
                        _tracks[trackNumber] = new MKVAudioTrack(trackType, audioSettings);
                        break;

                    default:
                        _tracks[trackNumber] = new MKVVideoTrack(trackType, videoSettings);
                        break;
                }
            }

            Debug.WriteLine($"Track Number: {trackNumber}, Type: {(trackType == MKVTrackType.Video ? "Video" : trackType == MKVTrackType.Audio ? "Audio" : "Other")}");
        }

        private MKVAudioSettings ParseTrackAudioSettings(PipeBinaryReader reader, long size)
        {
            long start = reader.Position;

            MKVAudioSettings audioSettings;
            audioSettings.Channels = 1;
            audioSettings.SamplingFrequency = 8000;

            while (reader.Position < start + size)
            {
                EBMLElementHeader elementHdr = ReadElementHeader(reader);

                switch (elementHdr.Id)
                {
                    case MKVElement.TrackAudioSettingsChannels:
                        audioSettings.Channels = (int)ReadUInt(reader, (int)elementHdr.Size);
                        break;
                    case MKVElement.TrackAudioSettingsSamplingFrequency:
                        audioSettings.SamplingFrequency = ReadFInt(reader, elementHdr.Size);
                        break;

                    default:
                        reader.SkipBytes(elementHdr.Size);
                        break;
                }
            }

            return audioSettings;
        }

        private MKVVideoSettings ParseTrackVideoSettings(PipeBinaryReader reader, long size)
        {
            long start = reader.Position;

            MKVVideoSettings videoSettings = default;

            while (reader.Position < start + size)
            {
                EBMLElementHeader elementHdr = ReadElementHeader(reader);

                switch (elementHdr.Id)
                {
                    case MKVElement.TrackVideoSettingsPixelWidth: 
                        videoSettings.PixelWidth = (int)ReadUInt(reader, (int)elementHdr.Size);
                        break;
                    case MKVElement.TrackVideoSettingsPixelHeight:
                        videoSettings.PixelHeight = (int)ReadUInt(reader, (int)elementHdr.Size);
                        break;
                    case MKVElement.TrackVideoSettingsDisplayWidth:
                        videoSettings.DisplayWidth = (int)ReadUInt(reader, (int)elementHdr.Size);
                        break;
                    case MKVElement.TrackVideoSettingsDisplayHeight:
                        videoSettings.DisplayHeight = (int)ReadUInt(reader, (int)elementHdr.Size);
                        break;
                    case MKVElement.TrackVideoSettingsUncompressedFourCC:
                        videoSettings.FourCC = (int)ReadUInt(reader, (int)elementHdr.Size);
                        break;

                    default:
                        reader.SkipBytes(elementHdr.Size);
                        break;
                }
            }

            return videoSettings;
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

                switch (elementHdr.Id)
                {
                    case MKVElement.Timecode:
                        clusterTimecode = (int)ReadUInt(reader, (int)elementHdr.Size);
                        Debug.WriteLine($"Cluster timecode: {clusterTimecode}");
                        break;
                    case MKVElement.SimpleBlock:
                        ParseSimpleBlock(reader, (long)elementHdr.Size, clusterTimecode);
                        break;

                    default:
                        reader.SkipBytes(elementHdr.Size);
                        break;
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

            MKVTrack track = _tracks[(int)trackNumber];
            switch (track.Type)
            {
                case MKVTrackType.Video:
                    {
                        MKVVideoTrack videoTrack = (MKVVideoTrack)track;
                        ReadVideoFrame(frameDataSize, videoTrack.VideoSettings, _videoFrameData);
                    }
                    break;
                case MKVTrackType.Audio:
                    {
                        MKVAudioTrack audioTrack = (MKVAudioTrack)track;
                        ReadAudioFrame(frameDataSize, audioTrack.AudioSettings);
                    }
                    break;

                case MKVTrackType.Subtitle:
                default:
                    reader.SkipBytes((ulong)frameDataSize);
                    break;
            }

            int absoluteTimecode = clusterTimecode + blockTimecode;

            Debug.WriteLine($"  SimpleBlock -> Track: {trackNumber}, TrackType: {track.Type}, AbsTime: {absoluteTimecode}, blockTimecode: {blockTimecode}, Keyframe: {(flags & SimpleBlockFlags.Keyframe) != 0}, Size: {frameDataSize}");
        }

        public void ReadVideoFrame(long videoFrameDataSize, MKVVideoSettings videoSettings, byte[] frameData)
        {
            Debug.Assert(_videoFrameData.Length == videoFrameDataSize);

            int totalBytesRead = 0;
            while (totalBytesRead < frameData.Length)
            {
                int bytesRead = _binaryReader.Read(frameData, totalBytesRead, frameData.Length - totalBytesRead);
                totalBytesRead += bytesRead;
            }

            _isVideoFrameDataDirty = true;
        }

        public void ReadAudioFrame(long audioFrameDataSize, MKVAudioSettings audioSettings)
        {
            _audioFrameData = _binaryReader.ReadBytes((int)audioFrameDataSize);

            return;

            int audioFrameSize = ((int)audioSettings.SamplingFrequency / 30) * (int)audioSettings.Channels * 2;

            _audioFrameData = new byte[audioFrameSize];

            int totalBytesRead = 0;
            while (totalBytesRead < _audioFrameData.Length)
            {
                int bytesRead = _binaryReader.Read(_audioFrameData, totalBytesRead, _audioFrameData.Length - totalBytesRead);
                if (bytesRead == 0)
                    return;
                totalBytesRead += bytesRead;
            }

            return;
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

            _videoFrameData = null;
        }
        #endregion
    }
}
