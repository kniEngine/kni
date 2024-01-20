// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos
﻿
using System;
using System.IO;
using Microsoft.Xna.Platform.Audio;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Represents a loaded sound resource.</summary>
    /// <remarks>
    /// <para>A SoundEffect represents the buffer used to hold audio data and metadata. SoundEffectInstances are used to play from SoundEffects. Multiple SoundEffectInstance objects can be created and played from the same SoundEffect object.</para>
    /// <para>The only limit on the number of loaded SoundEffects is restricted by available memory. When a SoundEffect is disposed, all SoundEffectInstances created from it will become invalid.</para>
    /// <para>SoundEffect.Play() can be used for 'fire and forget' sounds. If advanced playback controls like volume or pitch is required, use SoundEffect.CreateInstance().</para>
    /// </remarks>
    public sealed class SoundEffect : IDisposable
    {
        #region Internal Audio Data

        private string _name = string.Empty;
        
        private bool _isDisposed = false;
        private readonly TimeSpan _duration;

        #endregion

        internal SoundEffectStrategy _strategy { get; set; }

        #region Internal Constructors

        // Only used from SoundEffect.FromStream.
        private SoundEffect(Stream stream)
        {
            /*
              The Stream object must point to the head of a valid PCM wave file. Also, this wave file must be in the RIFF bitstream format.
              The audio format has the following restrictions:
              Must be a PCM wave file
              Can only be mono or stereo
              Must be 8 or 16 bit
              Sample rate must be between 8,000 Hz and 48,000 Hz
            */

            _strategy = AudioFactory.Current.CreateSoundEffectStrategy();
            _strategy.PlatformLoadAudioStream(stream, out _duration);
        }

        // Only used from SoundEffectReader.
        internal SoundEffect(byte[] header, byte[] buffer, int bufferSize, int durationMs, int loopStart, int loopLength)
        {
            _duration = TimeSpan.FromMilliseconds(durationMs);

            // Peek at the format... handle regular PCM data.
            short format = BitConverter.ToInt16(header, 0);

            switch (format)
            {
                case 1:
                    {
                        short channels = BitConverter.ToInt16(header, 2);
                        int sampleRate = BitConverter.ToInt32(header, 4);
                        short bitsPerSample = BitConverter.ToInt16(header, 14);
                        _strategy = AudioFactory.Current.CreateSoundEffectStrategy();
                        _strategy.PlatformInitializePcm(buffer, 0, bufferSize, bitsPerSample, sampleRate, channels, loopStart, loopLength);
                        return;
                    }
            }

            // Everything else is platform specific.
            _strategy = AudioFactory.Current.CreateSoundEffectStrategy();
            _strategy.PlatformInitializeFormat(header, buffer, 0, bufferSize, loopStart, loopLength);
        }

        // Only used from XACT WaveBank.
        internal SoundEffect(MiniFormatTag codec, byte[] buffer, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength)
        {
            _strategy = AudioFactory.Current.CreateSoundEffectStrategy();

            switch(codec)
            {
                // Handle the common case... the rest is platform specific.
                case MiniFormatTag.Pcm:
                    _duration = TimeSpan.FromSeconds((float)buffer.Length / (sampleRate * blockAlignment));
                    _strategy.PlatformInitializePcm(buffer, 0, buffer.Length, 16, sampleRate, channels, loopStart, loopLength);
                    return;

                case MiniFormatTag.Adpcm:
                    _duration = TimeSpan.FromSeconds((float)loopLength / sampleRate);
                    _strategy.PlatformInitializeXactAdpcm(buffer, 0, buffer.Length, channels, sampleRate, blockAlignment, loopStart, loopLength);
                    return;

                default:
                    throw new NotSupportedException("Unsupported sound format!");
            }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Create a sound effect.
        /// </summary>
        /// <param name="buffer">The buffer with the sound data.</param>
        /// <param name="sampleRate">The sound data sample rate in hertz.</param>
        /// <param name="channels">The number of channels in the sound data.</param>
        /// <remarks>This only supports uncompressed 16bit PCM wav data.</remarks>
        public SoundEffect(byte[] buffer, int sampleRate, AudioChannels channels)
             : this(buffer, 0, buffer != null ? buffer.Length : 0, sampleRate, channels, 0, 0)
        {
        }

        /// <summary>
        /// Create a sound effect.
        /// </summary>
        /// <param name="buffer">The buffer with the sound data.</param>
        /// <param name="offset">The offset to the start of the sound data in bytes.</param>
        /// <param name="count">The length of the sound data in bytes.</param>
        /// <param name="sampleRate">The sound data sample rate in hertz.</param>
        /// <param name="channels">The number of channels in the sound data.</param>
        /// <param name="loopStart">The position where the sound should begin looping in samples.</param>
        /// <param name="loopLength">The duration of the sound data loop in samples.</param>
        /// <remarks>This only supports uncompressed 16bit PCM wav data.</remarks>
        public SoundEffect(byte[] buffer, int offset, int count, int sampleRate, AudioChannels channels, int loopStart, int loopLength)
        {
            if (sampleRate < 8000 || sampleRate > 48000)
                throw new ArgumentOutOfRangeException("sampleRate");
            if ((int)channels != 1 && (int)channels != 2)
                throw new ArgumentOutOfRangeException("channels");

            if (buffer == null || buffer.Length == 0)
                throw new ArgumentException("Ensure that the buffer length is non-zero.", "buffer");

            int blockAlign = (int)channels * 2;
            if (count <= 0)
                throw new ArgumentException("Ensure that the count is greater than zero.", "count");
            if ((count % blockAlign) != 0)
                throw new ArgumentException("Ensure that the count meets the block alignment requirements for the number of channels.", "count");

            if (offset < 0)
                throw new ArgumentException("The offset cannot be negative.", "offset");
            if (((ulong)count + (ulong)offset) > (ulong)buffer.Length)
                throw new ArgumentException("Ensure that the offset+count region lines within the buffer.", "offset");

            int totalSamples = count / blockAlign;

            if (loopStart < 0)
                throw new ArgumentException("The loopStart cannot be negative.", "loopStart");
            if (loopStart > totalSamples)
                throw new ArgumentException("The loopStart cannot be greater than the total number of samples.", "loopStart");

            if (loopLength == 0)
                loopLength = totalSamples - loopStart;

            if (loopLength < 0)
                throw new ArgumentException("The loopLength cannot be negative.", "loopLength");
            if (((ulong)loopStart + (ulong)loopLength) > (ulong)totalSamples)
                throw new ArgumentException("Ensure that the loopStart+loopLength region lies within the sample range.", "loopLength");

            _duration = AudioService.GetSampleDuration(count, sampleRate, channels);

            _strategy = AudioFactory.Current.CreateSoundEffectStrategy();
            _strategy.PlatformInitializePcm(buffer, offset, count, 16, sampleRate, (int)channels, loopStart, loopLength);
        }

        #endregion

        #region Finalizer

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Microsoft.Xna.Framework.Audio.SoundEffect"/> is reclaimed by garbage collection.
        /// </summary>
        ~SoundEffect()
        {
            Dispose(false);
        }

        #endregion

        #region Additional SoundEffect/SoundEffectInstance Creation Methods

        /// <summary>
        /// Creates a new SoundEffectInstance for this SoundEffect.
        /// </summary>
        /// <returns>A new SoundEffectInstance for this SoundEffect.</returns>
        /// <remarks>Creating a SoundEffectInstance before calling SoundEffectInstance.Play() allows you to access advanced playback features, such as volume, pitch, and 3D positioning.</remarks>
        public SoundEffectInstance CreateInstance()
        {
            return new SoundEffectInstance(AudioService.Current, this);
        }

        /// <summary>
        /// Creates a new SoundEffect object based on the specified data stream.
        /// </summary>
        /// <param name="stream">A stream containing the wave data.</param>
        /// <returns>A new SoundEffect object.</returns>
        /// <remarks>The stream must point to the head of a valid wave file in the RIFF bitstream format.  The formats supported are:
        /// <list type="bullet">
        /// <item>
        /// <description>8-bit unsigned PCM</description>
        /// <description>16-bit signed PCM</description>
        /// <description>24-bit signed PCM</description>
        /// <description>32-bit IEEE float PCM</description>
        /// <description>MS-ADPCM 4-bit compressed</description>
        /// <description>IMA/ADPCM (IMA4) 4-bit compressed</description>
        /// </item>
        /// </list>
        /// </remarks>
        public static SoundEffect FromStream(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            return new SoundEffect(stream);
        }
        #endregion

        #region Play

        /// <summary>Gets an internal SoundEffectInstance and plays it.</summary>
        /// <returns>True if a SoundEffectInstance was successfully played, false if not.</returns>
        /// <remarks>
        /// <para>Play returns false if more SoundEffectInstances are currently playing then the platform allows.</para>
        /// <para>To loop a sound or apply 3D effects, call SoundEffect.CreateInstance() and SoundEffectInstance.Play() instead.</para>
        /// <para>SoundEffectInstances used by SoundEffect.Play() are pooled internally.</para>
        /// </remarks>
        public bool Play()
        {
            return AudioService.Current.Play(this);
        }

        /// <summary>Gets an internal SoundEffectInstance and plays it with the specified volume, pitch, and panning.</summary>
        /// <returns>True if a SoundEffectInstance was successfully created and played, false if not.</returns>
        /// <param name="volume">Volume, ranging from 0.0 (silence) to 1.0 (full volume). Volume during playback is scaled by SoundEffect.MasterVolume.</param>
        /// <param name="pitch">Pitch adjustment, ranging from -1.0 (down an octave) to 0.0 (no change) to 1.0 (up an octave).</param>
        /// <param name="pan">Panning, ranging from -1.0 (left speaker) to 0.0 (centered), 1.0 (right speaker).</param>
        /// <remarks>
        /// <para>Play returns false if more SoundEffectInstances are currently playing then the platform allows.</para>
        /// <para>To apply looping or simulate 3D audio, call SoundEffect.CreateInstance() and SoundEffectInstance.Play() instead.</para>
        /// <para>SoundEffectInstances used by SoundEffect.Play() are pooled internally.</para>
        /// </remarks>
        public bool Play(float volume, float pitch, float pan)
        {
            return AudioService.Current.Play(this, volume, pitch,  pan);
        }


        #endregion

        #region Public Properties

        /// <summary>Gets the duration of the SoundEffect.</summary>
        public TimeSpan Duration
        {
            get
            {
                if (_duration.Ticks == 0)
                    throw new NotSupportedException();
                return _duration;
            }
        }

        /// <summary>Gets or sets the asset name of the SoundEffect.</summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        #endregion

        #region Static Members

        static float _masterVolume = 1.0f;
        /// <summary>
        /// Gets or sets the master volume scale applied to all SoundEffectInstances.
        /// </summary>
        /// <remarks>
        /// <para>Each SoundEffectInstance has its own Volume property that is independent to SoundEffect.MasterVolume. During playback SoundEffectInstance.Volume is multiplied by SoundEffect.MasterVolume.</para>
        /// <para>This property is used to adjust the volume on all current and newly created SoundEffectInstances. The volume of an individual SoundEffectInstance can be adjusted on its own.</para>
        /// </remarks>
        public static float MasterVolume 
        { 
            get { return _masterVolume; }
            set
            {
                if (value < 0.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException();

                if (_masterVolume == value)
                    return;
                
                _masterVolume = value;
                AudioService.UpdateMasterVolume();
            }
        }

        static float _distanceScale = 1.0f;
        /// <summary>
        /// Gets or sets the scale of distance calculations.
        /// </summary>
        /// <remarks> 
        /// <para>DistanceScale defaults to 1.0 and must be greater than 0.0.</para>
        /// <para>Higher values reduce the rate of falloff between the sound and listener.</para>
        /// </remarks>
        public static float DistanceScale
        {
            get { return _distanceScale; }
            set
            {
                if (value <= 0f)
                    throw new ArgumentOutOfRangeException("value", "value of DistanceScale");

                _distanceScale = value;
            }
        }

        static float _dopplerScale = 1f;
        /// <summary>
        /// Gets or sets the scale of Doppler calculations applied to sounds.
        /// </summary>
        /// <remarks>
        /// <para>DopplerScale defaults to 1.0 and must be greater or equal to 0.0</para>
        /// <para>Affects the relative velocity of emitters and listeners.</para>
        /// <para>Higher values more dramatically shift the pitch for the given relative velocity of the emitter and listener.</para>
        /// </remarks>
        public static float DopplerScale
        {
            get { return _dopplerScale; }
            set
            {
                // As per documenation it does not look like the value can be less than 0
                //   although the documentation does not say it throws an error we will anyway
                //   just so it is like the DistanceScale
                if (value < 0.0f)
                    throw new ArgumentOutOfRangeException("value", "value of DopplerScale");

                _dopplerScale = value;
            }
        }

        static float speedOfSound = 343.5f;
        /// <summary>Returns the speed of sound used when calculating the Doppler effect..</summary>
        /// <remarks>
        /// <para>Defaults to 343.5. Value is measured in meters per second.</para>
        /// <para>Has no effect on distance attenuation.</para>
        /// </remarks>
        public static float SpeedOfSound
        {
            get { return speedOfSound; }
            set
            {
                if (value <= 0.0f)
                    throw new ArgumentOutOfRangeException();

                speedOfSound = value;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>Indicates whether the object is disposed.</summary>
        public bool IsDisposed { get { return _isDisposed; } }

        /// <summary>Releases the resources held by this <see cref="Microsoft.Xna.Framework.Audio.SoundEffect"/>.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the resources held by this <see cref="Microsoft.Xna.Framework.Audio.SoundEffect"/>.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c>, Dispose was called explicitly.</param>
        /// <remarks>If the disposing parameter is true, the Dispose method was called explicitly. This
        /// means that managed objects referenced by this instance should be disposed or released as
        /// required.  If the disposing parameter is false, Dispose was called by the finalizer and
        /// no managed objects should be touched because we do not know if they are still valid or
        /// not at that time.  Unmanaged resources should always be released.</remarks>
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (AudioService.SyncHandle)
                {
                    if (IsDisposed) return;

                    // Clean up managed objects

                    // Clean up unmanaged resources
                    AudioService.OnEffectDisposed(this, disposing);

                    _strategy.Dispose();
                    _strategy = null;

                    _isDisposed = true;
                }
            }
            else
            {
                lock (AudioService.SyncHandle)
                {
                    if (IsDisposed) return;

                    // Clean up unmanaged resources
                    AudioService.OnEffectDisposed(this, disposing);
                }
            }
        }

        #endregion

    }
}
