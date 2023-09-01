// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Platform.Audio;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Microphone state. 
    /// </summary>
    public enum MicrophoneState
    {
        Started,
        Stopped
    }

    /// <summary>
    /// Provides microphones capture features. 
    /// </summary>
    public sealed class Microphone
    {
        MicrophoneStrategy _strategy { get; set; }

        #region Internal Constructors
        
        internal Microphone() : this(null)
        {
        }

        internal Microphone(string name)
        {
            Name = name;
            _strategy = AudioFactory.Current.CreateMicrophoneStrategy();
        }

        #endregion

        #region Public Fields

        /// <summary>
        /// Returns the friendly name of the microphone.
        /// </summary>
        public readonly string Name;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the capture buffer duration. This value must be greater than 100 milliseconds, lower than 1000 milliseconds, and must be 10 milliseconds aligned (BufferDuration % 10 == 10).
        /// </summary>
        public TimeSpan BufferDuration
        {
            get { return _strategy.BufferDuration; }
            set
            {
                if (value.TotalMilliseconds < 100 || value.TotalMilliseconds > 1000)
                    throw new ArgumentOutOfRangeException("Buffer duration must be a value between 100 and 1000 milliseconds.");
                if (value.TotalMilliseconds % 10 != 0)
                    throw new ArgumentOutOfRangeException("Buffer duration must be 10ms aligned (BufferDuration % 10 == 0)");

                _strategy.BufferDuration = value;
            }
        }

        /// <summary>
        /// Determines if the microphone is a wired headset.
        /// Note: XNA could know if a headset microphone was plugged in an Xbox 360 controller but MonoGame can't.
        /// </summary>
        public bool IsHeadset
        {
            get { return _strategy.PlatformIsHeadset(); }
        }

        /// <summary>
        /// Returns the sample rate of the captured audio.
        /// Note: default value is 44100hz
        /// </summary>
        public int SampleRate
        {
            get { return _strategy.SampleRate; }
        }

        private MicrophoneState _state = MicrophoneState.Stopped;

        /// <summary>
        /// Returns the state of the Microphone. 
        /// </summary>
        public MicrophoneState State
        {
            get { return _state; }
        }

        #endregion

        #region Static Members

        private static ReadOnlyCollection<Microphone> _readOnlyMicrophones = null;

        /// <summary>
        /// Returns all compatible microphones.
        /// </summary>
        public static ReadOnlyCollection<Microphone> All
        {
            get
            {
                if (_readOnlyMicrophones == null)
                    _readOnlyMicrophones = new ReadOnlyCollection<Microphone>(AudioService.Current._microphones);
                return _readOnlyMicrophones;
            }
        }
        
        /// <summary>
        /// Returns the default microphone.
        /// </summary>
        public static Microphone Default
        {
            get { return AudioService.Current._defaultMicrophone; }
        }       

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the duration based on the size of the buffer (assuming 16-bit PCM data).
        /// </summary>
        /// <param name="sizeInBytes">Size, in bytes</param>
        /// <returns>TimeSpan of the duration.</returns>
        public TimeSpan GetSampleDuration(int sizeInBytes)
        {
            return _strategy.GetSampleDuration(sizeInBytes);
        }

        /// <summary>
        /// Returns the size, in bytes, of the array required to hold the specified duration of 16-bit PCM data. 
        /// </summary>
        /// <param name="duration">TimeSpan of the duration of the sample.</param>
        /// <returns>Size, in bytes, of the buffer.</returns>
        public int GetSampleSizeInBytes(TimeSpan duration)
        {
            // this should be 10ms aligned
            // this assumes 16bit mono data
            return _strategy.GetSampleSizeInBytes(duration);
        }

        /// <summary>
        /// Starts microphone capture.
        /// </summary>
        public void Start()
        { 
            var state = State;
            switch (state)
            {
                case MicrophoneState.Started:
                    return;
                case MicrophoneState.Stopped:
                    {
                        _strategy.PlatformStart(Name);
                        _state = MicrophoneState.Started;
                    }
                    return;
            }
        }

        /// <summary>
        /// Stops microphone capture.
        /// </summary>
        public void Stop()
        {
            var state = State;
            switch (state)
            {
                case MicrophoneState.Started:
                    {
                        _strategy.PlatformStop();
                        _state = MicrophoneState.Stopped;
                    }
                    return;
                case MicrophoneState.Stopped:
                    return;
            }
        }

        /// <summary>
        /// Gets the latest available data from the microphone.
        /// </summary>
        /// <param name="buffer">Buffer, in bytes, of the captured data (16-bit PCM).</param>
        /// <returns>The buffer size, in bytes, of the captured data.</returns>
        public int GetData(byte[] buffer)
        {
            return GetData(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Gets the latest available data from the microphone.
        /// </summary>
        /// <param name="buffer">Buffer, in bytes, of the captured data (16-bit PCM).</param>
        /// <param name="offset">Byte offset.</param>
        /// <param name="count">Amount, in bytes.</param>
        /// <returns>The buffer size, in bytes, of the captured data.</returns>
        public int GetData(byte[] buffer, int offset, int count)
        {
            if (_state == MicrophoneState.Stopped || BufferReady == null)
                return 0;

            return _strategy.PlatformGetData(buffer, offset, count);
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Event fired when the audio data are available.
        /// </summary>
        public event EventHandler<EventArgs> BufferReady;

        #endregion


        internal void UpdateBuffer()
        {
            var state = _state;
            switch (state)
            {
                case MicrophoneState.Started:
                    {
                        var handler = BufferReady;
                        if (handler == null)
                            return;

                        if (_strategy.PlatformUpdate())
                            handler.Invoke(this, EventArgs.Empty);
                    }
                    return;
                case MicrophoneState.Stopped:
                    return;
            }
        }

        #region Static Methods


        #endregion
    }
}
