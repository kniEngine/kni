// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;

namespace Microsoft.Xna.Framework.Content.Pipeline.Audio
{
    /// <summary>
    /// Encapsulates and provides operations, such as format conversions, on the 
    /// source audio. This type is produced by the audio importers and used by audio
    /// processors to produce compiled audio assets.
    /// </summary>
    /// <remarks>Note that AudioContent can load and process audio files that are not supported by the importers.</remarks>
    public class AudioContent : ContentItem, IDisposable
    {
        private bool _isDisposed;
        private readonly string _fileName;
        private readonly AudioFileType _fileType;
        private byte[] _data; 
        private TimeSpan _duration;
        private AudioFormat _format;
        private int _loopStart;
        private int _loopLength;

        private ReadOnlyCollection<byte> _readOnlyData;

        /// <summary>
        /// The name of the original source audio file.
        /// </summary>
        [ContentSerializer(AllowNull = false)]
        public string FileName { get { return _fileName; } }

        /// <summary>
        /// The type of the original source audio file.
        /// </summary>
        public AudioFileType FileType { get { return _fileType; } }

        /// <summary>
        /// The current raw audio data without header information.
        /// </summary>
        /// <remarks>
        /// This changes from the source data to the output data after conversion.
        /// For MP3 and WMA files this throws an exception to match XNA behavior.
        /// </remarks>
        public ReadOnlyCollection<byte> Data 
        {
            get
            {
                if (_isDisposed || _data == null)
                    throw new InvalidContentException("Could not read the audio data from file \"" + Path.GetFileName(_fileName) + "\".");

                if (_readOnlyData == null)
                    _readOnlyData = new ReadOnlyCollection<byte>(_data);

                return _readOnlyData;
            }
        }

        internal byte[] RawData
        {
            get { return _data; }
        }

        /// <summary>
        /// The duration of the audio data.
        /// </summary>
        public TimeSpan Duration { get { return _duration; } }

        /// <summary>
        /// The current format of the audio data.
        /// </summary>
        /// <remarks>This changes from the source format to the output format after conversion.</remarks>
        public AudioFormat Format { get { return _format; } }

        /// <summary>
        /// The current loop length in samples.
        /// </summary>
        /// <remarks>This changes from the source loop length to the output loop length after conversion.</remarks>
        public int LoopLength { get { return _loopLength; } }

        /// <summary>
        /// The current loop start location in samples.
        /// </summary>
        /// <remarks>This changes from the source loop start to the output loop start after conversion.</remarks>
        public int LoopStart { get { return _loopStart; } }

        /// <summary>
        /// Initializes a new instance of AudioContent.
        /// </summary>
        /// <param name="audioFileName">Name of the audio source file to be processed.</param>
        /// <param name="audioFileType">Type of the processed audio: WAV, MP3 or WMA.</param>
        /// <remarks>Constructs the object from the specified source file, in the format specified.</remarks>
        public AudioContent(string audioFileName, AudioFileType audioFileType)
        {
            _fileName = audioFileName;

            try
            {
                // Get the full path to the file.
                audioFileName = Path.GetFullPath(audioFileName);

                // Use probe to get the details of the file.
                AudioContent.ProbeFormat(audioFileName, out _fileType, out _format, out _duration, out _loopStart, out _loopLength);

                // Looks like XNA only cares about type mismatch when
                // the type is WAV... else it is ok.
                if (    (audioFileType == AudioFileType.Wav || _fileType == AudioFileType.Wav) &&
                        audioFileType != _fileType)
                    throw new ArgumentException("Incorrect file type!", "audioFileType");

                // Only provide the data for WAV files.
                if (audioFileType == AudioFileType.Wav)
                {
                    byte[] rawData;

                    // Must be opened in read mode otherwise it fails to open
                    // read-only files (found in some source control systems)
                    using (var fs = new FileStream(audioFileName, FileMode.Open, FileAccess.Read))
                    {
                        rawData = new byte[fs.Length];
                        fs.Read(rawData, 0, rawData.Length);
                    }

                    AudioFormat riffAudioFormat;
                    byte[] stripped = AudioContent.StripRiffWaveHeader(rawData, out riffAudioFormat);

                    if (riffAudioFormat != null)
                    {
                        if ((_format.Format != 2 && _format.Format != 17) && _format.BlockAlign != riffAudioFormat.BlockAlign)
                            throw new InvalidOperationException("Calculated block align does not match RIFF " + _format.BlockAlign + " : " + riffAudioFormat.BlockAlign);
                        if (_format.ChannelCount != riffAudioFormat.ChannelCount)
                            throw new InvalidOperationException("Probed channel count does not match RIFF: " + _format.ChannelCount + ", " + riffAudioFormat.ChannelCount);
                        if (_format.Format != riffAudioFormat.Format)
                            throw new InvalidOperationException("Probed audio format does not match RIFF: " + _format.Format + ", " + riffAudioFormat.Format);
                        if (_format.SampleRate != riffAudioFormat.SampleRate)
                            throw new InvalidOperationException("Probed sample rate does not match RIFF: " + _format.SampleRate + ", " + riffAudioFormat.SampleRate);
                    }

                    _data = stripped;
                }
            }
            catch (Exception ex)
            {
                var message = string.Format("Failed to open file {0}. Ensure the file is a valid audio file and is not DRM protected.", Path.GetFileNameWithoutExtension(audioFileName));
                throw new InvalidContentException(message, ex);
            }
        }

        /// <summary>
        /// Transcodes the source audio to the target format and quality.
        /// </summary>
        /// <param name="formatType">Format to convert this audio to.</param>
        /// <param name="quality">Quality of the processed output audio. For streaming formats, it can be one of the following: Low (96 kbps), Medium (128 kbps), Best (192 kbps).  For WAV formats, it can be one of the following: Low (11kHz ADPCM), Medium (22kHz ADPCM), Best (44kHz PCM)</param>
        /// <param name="saveToFile">
        /// The name of the file that the converted audio should be saved into.  This is used for SongContent, where
        /// the audio is stored external to the XNB file.  If this is null, then the converted audio is stored in
        /// the Data property.
        /// </param>
        public void ConvertFormat(ConversionFormat formatType, ConversionQuality quality, string saveToFile)
        {
            string temporaryOutput = Path.GetTempFileName();
            try
            {
                string ffmpegCodecName, ffmpegMuxerName;
                //int format;
                switch (formatType)
                {
                    case ConversionFormat.Adpcm:
                        // ADPCM Microsoft 
                        ffmpegCodecName = "adpcm_ms";
                        ffmpegMuxerName = "wav";
                        //format = 0x0002; /* WAVE_FORMAT_ADPCM */
                        break;
                    case ConversionFormat.Pcm:
                        // XNA seems to preserve the bit size of the input
                        // format when converting to PCM.
                        if (this.Format.BitsPerSample == 8)
                            ffmpegCodecName = "pcm_u8";
                        else if (this.Format.BitsPerSample == 32 && this.Format.Format == 3)
                            ffmpegCodecName = "pcm_f32le";
                        else
                            ffmpegCodecName = "pcm_s16le";
                        ffmpegMuxerName = "wav";
                        //format = 0x0001; /* WAVE_FORMAT_PCM */
                        break;
                    case ConversionFormat.WindowsMedia:
                        // Windows Media Audio 2
                        ffmpegCodecName = "wmav2";
                        ffmpegMuxerName = "asf";
                        //format = 0x0161; /* WAVE_FORMAT_WMAUDIO2 */
                        break;
                    case ConversionFormat.Xma:
                        throw new NotSupportedException(
                            "XMA is not a supported encoding format. It is specific to the Xbox 360.");
                    case ConversionFormat.ImaAdpcm:
                        // ADPCM IMA WAV
                        ffmpegCodecName = "adpcm_ima_wav";
                        ffmpegMuxerName = "wav";
                        //format = 0x0011; /* WAVE_FORMAT_IMA_ADPCM */
                        break;
                    case ConversionFormat.Aac:
                        // AAC (Advanced Audio Coding)
                        // Requires -strict experimental
                        ffmpegCodecName = "aac";
                        ffmpegMuxerName = "ipod";
                        //format = 0x0000; /* WAVE_FORMAT_UNKNOWN */
                        break;
                    case ConversionFormat.Vorbis:
                        // Vorbis
                        ffmpegCodecName = "libvorbis";
                        ffmpegMuxerName = "ogg";
                        //format = 0x0000; /* WAVE_FORMAT_UNKNOWN */
                        break;
                    case ConversionFormat.Mp3:
                        // Vorbis
                        ffmpegCodecName = "libmp3lame";
                        ffmpegMuxerName = "mp3";
                        //format = 0x0000; /* WAVE_FORMAT_UNKNOWN */
                        break;
                    default:
                        // Unknown format
                        throw new NotSupportedException();
                }

                ConversionQuality finalQuality = quality;
                string ffmpegStdout, ffmpegStderr;
                int ffmpegExitCode;
                do
                {
                    ffmpegExitCode = ExternalTool.Run(
                        "ffmpeg",
                        string.Format(
                            "-y -i \"{0}\" -vn -c:a {1} -b:a {2} -ar {3} -f:a {4} -strict experimental \"{5}\"",
                            this.FileName,
                            ffmpegCodecName,
                            AudioContent.QualityToBitRate(finalQuality),
                            AudioContent.QualityToSampleRate(finalQuality, this.Format.SampleRate),
                            ffmpegMuxerName,
                            temporaryOutput),
                        out ffmpegStdout,
                        out ffmpegStderr);
                    if (ffmpegExitCode != 0)
                        finalQuality--;
                }
                while (finalQuality >= 0 && ffmpegExitCode != 0);

                if (ffmpegExitCode != 0)
                    throw new InvalidOperationException("ffmpeg exited with non-zero exit code: \n" + ffmpegStdout + "\n" + ffmpegStderr);

                byte[] rawData;
                using (FileStream fs = new FileStream(temporaryOutput, FileMode.Open, FileAccess.Read))
                {
                    rawData = new byte[fs.Length];
                    fs.Read(rawData, 0, rawData.Length);
                }

                if (saveToFile != null)
                {
                    using (FileStream fs = new FileStream(saveToFile, FileMode.Create, FileAccess.Write))
                        fs.Write(rawData, 0, rawData.Length);
                }

                // Use probe to get the final format and information on the converted file.
                AudioFileType audioFileType;
                AudioFormat audioFormat;
                TimeSpan duration;
                int loopStart, loopLength;
                AudioContent.ProbeFormat(temporaryOutput, out audioFileType, out audioFormat, out duration, out loopStart, out loopLength);

                AudioFormat riffAudioFormat;
                byte[] data = AudioContent.StripRiffWaveHeader(rawData, out riffAudioFormat);

                // deal with adpcm
                if (audioFormat.Format == 2 || audioFormat.Format == 17)
                {
                    // riff contains correct blockAlign
                    audioFormat = riffAudioFormat;

                    // fix loopLength -> has to be multiple of sample per block
                    // see https://msdn.microsoft.com/de-de/library/windows/desktop/ee415711(v=vs.85).aspx
                    int samplesPerBlock = AudioContent.SampleAlignment(audioFormat);
                    loopLength = (int)(audioFormat.SampleRate * duration.TotalSeconds);
                    int remainder = loopLength % samplesPerBlock;
                    loopLength += samplesPerBlock - remainder;
                }

                this.SetData(data, audioFormat, duration, loopStart, loopLength);
            }
            finally
            {
                ExternalTool.DeleteFile(temporaryOutput);
            }

            //return quality;
        }

        public void SetData(byte[] data, AudioFormat format, TimeSpan duration, int loopStart, int loopLength)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (format == null)
                throw new ArgumentNullException("format");

            _data = data;
            _format = format;
            _duration = duration;
            _loopStart = loopStart;
            _loopLength = loopLength;
        }


        public static void ProbeFormat(string sourceFile, out AudioFileType audioFileType, out AudioFormat audioFormat, out TimeSpan duration, out int loopStart, out int loopLength)
        {
            string ffprobeStdout, ffprobeStderr;
            int ffprobeExitCode = ExternalTool.Run(
                "ffprobe",
                string.Format("-i \"{0}\" -show_format -show_entries streams -v quiet -of flat", sourceFile),
                out ffprobeStdout,
                out ffprobeStderr);
            if (ffprobeExitCode != 0)
                throw new InvalidOperationException("ffprobe exited with non-zero exit code.");

            // Set default values if information is not available.
            int averageBytesPerSecond = 0;
            int bitsPerSample = 0;
            int blockAlign = 0;
            int channelCount = 0;
            int sampleRate = 0;
            int format = 0;
            string sampleFormat = null;
            double durationInSeconds = 0;
            string formatName = string.Empty;

            try
            {
                NumberFormatInfo numberFormat = CultureInfo.InvariantCulture.NumberFormat;
                foreach (string line in ffprobeStdout.Split(new[] { '\r', '\n', '\0' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] kv = line.Split(new[] { '=' }, 2);

                    switch (kv[0])
                    {
                        case "streams.stream.0.sample_rate":
                            sampleRate = int.Parse(kv[1].Trim('"'), numberFormat);
                            break;
                        case "streams.stream.0.bits_per_sample":
                            bitsPerSample = int.Parse(kv[1].Trim('"'), numberFormat);
                            break;
                        case "streams.stream.0.start_time":
                            {
                                double seconds;
                                if (double.TryParse(kv[1].Trim('"'), NumberStyles.Any, numberFormat, out seconds))
                                    durationInSeconds += seconds;
                                break;
                            }
                        case "streams.stream.0.duration":
                            durationInSeconds += double.Parse(kv[1].Trim('"'), numberFormat);
                            break;
                        case "streams.stream.0.channels":
                            channelCount = int.Parse(kv[1].Trim('"'), numberFormat);
                            break;
                        case "streams.stream.0.sample_fmt":
                            sampleFormat = kv[1].Trim('"').ToLowerInvariant();
                            break;
                        case "streams.stream.0.bit_rate":
                            averageBytesPerSecond = (int.Parse(kv[1].Trim('"'), numberFormat) / 8);
                            break;
                        case "format.format_name":
                            formatName = kv[1].Trim('"').ToLowerInvariant();
                            break;
                        case "streams.stream.0.codec_tag":
                            {
                                string hex = kv[1].Substring(3, kv[1].Length - 4);
                                format = int.Parse(hex, NumberStyles.HexNumber);
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse ffprobe output.", ex);
            }

            // XNA seems to use the sample format for the bits per sample
            // in the case of non-PCM formats like MP3 and WMA.
            if (bitsPerSample == 0 && sampleFormat != null)
            {
                switch (sampleFormat)
                {
                    case "u8":
                    case "u8p":
                        bitsPerSample = 8;
                        break;
                    case "s16":
                    case "s16p":
                        bitsPerSample = 16;
                        break;
                    case "s32":
                    case "s32p":
                    case "flt":
                    case "fltp":
                        bitsPerSample = 32;
                        break;
                    case "dbl":
                    case "dblp":
                        bitsPerSample = 64;
                        break;
                }
            }

            // Figure out the file type.
            int durationMs = (int)Math.Floor(durationInSeconds * 1000.0);
            if (formatName == "wav")
            {
                audioFileType = AudioFileType.Wav;
            }
            else if (formatName == "mp3")
            {
                audioFileType = AudioFileType.Mp3;
                format = 1;
                durationMs = (int)Math.Ceiling(durationInSeconds * 1000.0);
                bitsPerSample = Math.Min(bitsPerSample, 16);
            }
            else if (formatName == "wma" || formatName == "asf")
            {
                audioFileType = AudioFileType.Wma;
                format = 1;
                durationMs = (int)Math.Ceiling(durationInSeconds * 1000.0);
                bitsPerSample = Math.Min(bitsPerSample, 16);
            }
            else if (formatName == "ogg")
            {
                audioFileType = AudioFileType.Ogg;
                format = 1;
                durationMs = (int)Math.Ceiling(durationInSeconds * 1000.0);
                bitsPerSample = Math.Min(bitsPerSample, 16);
            }
            else
                audioFileType = (AudioFileType)(-1);

            // XNA seems to calculate the block alignment directly from 
            // the bits per sample and channel count regardless of the 
            // format of the audio data.
            // ffprobe doesn't report blockAlign for ADPCM and we cannot calculate it like this
            if (bitsPerSample > 0 && (format != 2 && format != 17))
                blockAlign = (bitsPerSample * channelCount) / 8;

            // XNA seems to only be accurate to the millisecond.
            duration = TimeSpan.FromMilliseconds(durationMs);

            // Looks like XNA calculates the average bps from
            // the sample rate and block alignment.
            if (blockAlign > 0)
                averageBytesPerSecond = sampleRate * blockAlign;

            audioFormat = new AudioFormat(
                averageBytesPerSecond,
                bitsPerSample,
                blockAlign,
                channelCount,
                format,
                sampleRate);

            // Loop start and length in number of samples.  For some
            // reason XNA doesn't report loop length for non-WAV sources.
            loopStart = 0;
            if (audioFileType != AudioFileType.Wav)
                loopLength = 0;
            else
                loopLength = (int)Math.Floor(sampleRate * durationInSeconds);
        }

        internal static byte[] StripRiffWaveHeader(byte[] data, out AudioFormat audioFormat)
        {
            audioFormat = null;

            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                string signature = new string(reader.ReadChars(4));
                if (signature != "RIFF")
                    return data;

                reader.ReadInt32(); // riff_chunck_size

                string wformat = new string(reader.ReadChars(4));
                if (wformat != "WAVE")
                    return data;

                // Look for the data chunk.
                while (true)
                {
                    string chunkSignature = new string(reader.ReadChars(4));
                    if (chunkSignature.ToLowerInvariant() == "data")
                        break;
                    if (chunkSignature.ToLowerInvariant() == "fmt ")
                    {
                        int fmtLength = reader.ReadInt32();
                        short formatTag = reader.ReadInt16();
                        short channels = reader.ReadInt16();
                        int sampleRate = reader.ReadInt32();
                        int avgBytesPerSec = reader.ReadInt32();
                        short blockAlign = reader.ReadInt16();
                        short bitsPerSample = reader.ReadInt16();
                        audioFormat = new AudioFormat(avgBytesPerSec, bitsPerSample, blockAlign, channels, formatTag, sampleRate);

                        fmtLength -= 2 + 2 + 4 + 4 + 2 + 2;
                        if (fmtLength < 0)
                            throw new InvalidOperationException("riff wave header has unexpected format");
                        reader.BaseStream.Seek(fmtLength, SeekOrigin.Current);
                    }
                    else
                    {
                        reader.BaseStream.Seek(reader.ReadInt32(), SeekOrigin.Current);
                    }
                }

                int dataSize = reader.ReadInt32();
                data = reader.ReadBytes(dataSize);
            }

            return data;
        }

        public static void WritePcmFile(AudioContent content, string saveToFile, int bitRate = 192000, int? sampeRate = null)
        {
            string ffmpegStdout, ffmpegStderr;
            int ffmpegExitCode = ExternalTool.Run(
                "ffmpeg",
                string.Format(
                    "-y -i \"{0}\" -vn -c:a pcm_s16le -b:a {2} {3} -f:a wav -strict experimental \"{1}\"",
                    content.FileName,
                    saveToFile,
                    bitRate,
                    sampeRate != null ? "-ar " + sampeRate.Value : ""
                    ),
                out ffmpegStdout,
                out ffmpegStderr);
            if (ffmpegExitCode != 0)
                throw new InvalidOperationException("ffmpeg exited with non-zero exit code: \n" + ffmpegStdout + "\n" + ffmpegStderr);
        }

        // Converts block alignment in bytes to sample alignment, primarily for compressed formats
        // Calculation of sample alignment from http://kcat.strangesoft.net/openal-extensions/SOFT_block_alignment.txt
        private static int SampleAlignment(AudioFormat format)
        {
            switch (format.Format)
            {
                case 2:     // MS-ADPCM
                    return (format.BlockAlign / format.ChannelCount - 7) * 2 + 2;
                case 17:    // IMA/ADPCM
                    return (format.BlockAlign / format.ChannelCount - 4) / 4 * 8 + 1;

                default:
                    return 0;
            }
        }

        private static int QualityToSampleRate(ConversionQuality quality, int sourceSampleRate)
        {
            switch (quality)
            {
                case ConversionQuality.Low:
                    return Math.Max(8000, (int)Math.Floor(sourceSampleRate / 2.0));
                case ConversionQuality.Medium:
                    return Math.Max(8000, (int)Math.Floor((sourceSampleRate / 4.0) * 3));
                case ConversionQuality.Best:
                    return Math.Max(8000, sourceSampleRate);

                default:
                    return Math.Max(8000, sourceSampleRate);
            }
        }

        private static int QualityToBitRate(ConversionQuality quality)
        {
            switch (quality)
            {
                case ConversionQuality.Low:
                    return 96000;
                case ConversionQuality.Medium:
                    return 128000;
                case ConversionQuality.Best:
                    return 192000;

                default:
                    return 192000;
            }
        }

        public void Dispose()
        {
            _data = null;
            _readOnlyData = null;

            _isDisposed = true;
        }
    }
}
