// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Platform.Audio;

namespace Microsoft.Xna.Framework.Audio
{
    class XactClip
    {
        private readonly float _defaultVolume;
        private float _volumeScale;
        private float _volume;

        private readonly ClipEvent[] _events;
        private float _time;
        private int _nextEvent;

        internal readonly bool FilterEnabled;
        internal readonly FilterMode FilterMode;
        internal readonly float FilterQ;
        internal readonly ushort FilterFrequency;

        internal readonly bool UseReverb;

        public XactClip(SoundBank soundBank, BinaryReader clipReader, bool useReverb)
        {
#pragma warning disable 0219
            State = SoundState.Stopped;

            UseReverb = useReverb;

            float volumeDb = XactHelpers.ParseDecibels(clipReader.ReadByte());
            _defaultVolume = XactHelpers.ParseVolumeFromDecibels(volumeDb);
            uint clipOffset = clipReader.ReadUInt32();

            // Read the filter info.
            ushort filterQAndFlags = clipReader.ReadUInt16();
            FilterEnabled = (filterQAndFlags & 1) == 1;
            FilterMode = (FilterMode)((filterQAndFlags >> 1) & 3);
            FilterQ = (filterQAndFlags >> 3) * 0.01f;
            FilterFrequency = clipReader.ReadUInt16();

            long oldPosition = clipReader.BaseStream.Position;
            clipReader.BaseStream.Seek(clipOffset, SeekOrigin.Begin);
            
            byte numEvents = clipReader.ReadByte();
            _events = new ClipEvent[numEvents];
            
            for (int i = 0; i<numEvents; i++) 
            {
                uint eventInfo = clipReader.ReadUInt32();
                float randomOffset = clipReader.ReadUInt16() * 0.001f;

                // TODO: eventInfo still has 11 bits that are unknown!
                uint eventId = eventInfo & 0x1F;
                float timeStamp = ((eventInfo >> 5) & 0xFFFF) * 0.001f;
                uint unknown = eventInfo >> 21;

                switch (eventId)
                {
                case 0:
                    // Stop Event
                    throw new NotImplementedException("Stop event");

                case 1:
                {
                    // Unknown!
                    clipReader.ReadByte();

                    // Event flags
                    byte eventFlags = clipReader.ReadByte();
                    bool playRelease = (eventFlags & 0x01) == 0x01;
                    bool panEnabled = (eventFlags & 0x02) == 0x02;
                    bool useCenterSpeaker = (eventFlags & 0x04) == 0x04;

                    int trackIndex = clipReader.ReadUInt16();
                    int waveBankIndex = clipReader.ReadByte();					
                    byte loopCount = clipReader.ReadByte();
                    float panAngle = clipReader.ReadUInt16() / 100.0f;
                    float panArc = clipReader.ReadUInt16() / 100.0f;
                    
                    _events[i] = new PlayWaveEvent(
                        this,
                        timeStamp, 
                        randomOffset,
                        soundBank, 
                        new[] { waveBankIndex }, 
                        new[] { trackIndex },
                        null,
                        0,
                        VariationType.Ordered, 
                        null,
                        null,
                        null,
                        loopCount,
                        false);

                    break;
                }

                case 3:
                {
                    // Unknown!
                    clipReader.ReadByte();

                    // Event flags
                    byte eventFlags = clipReader.ReadByte();
                    bool playRelease = (eventFlags & 0x01) == 0x01;
                    bool panEnabled = (eventFlags & 0x02) == 0x02;
                    bool useCenterSpeaker = (eventFlags & 0x04) == 0x04;

                    byte loopCount = clipReader.ReadByte();
                    float panAngle = clipReader.ReadUInt16() / 100.0f;
                    float panArc = clipReader.ReadUInt16() / 100.0f;

                    // The number of tracks for the variations.
                    ushort numTracks = clipReader.ReadUInt16();

                    // Not sure what most of this is.
                    byte moreFlags = clipReader.ReadByte();
                    bool newWaveOnLoop = (moreFlags & 0x40) == 0x40;

                    // The variation playlist type seems to be 
                    // stored in the bottom 4bits only.
                    VariationType variationType = (VariationType)(moreFlags & 0x0F);

                    // Unknown!
                    clipReader.ReadBytes(5);

                    // Read in the variation playlist.
                    int[] waveBanks = new int[numTracks];
                    int[] tracks = new int[numTracks];
                    byte[] weights = new byte[numTracks];
                            int totalWeights = 0;
                    for (int j = 0; j < numTracks; j++)
                    {
                        tracks[j] = clipReader.ReadUInt16();
                        waveBanks[j] = clipReader.ReadByte();
                        byte minWeight = clipReader.ReadByte();
                        byte maxWeight = clipReader.ReadByte();
                        weights[j] = (byte)(maxWeight - minWeight);
                        totalWeights += weights[j];
                    }

                    _events[i] = new PlayWaveEvent(
                        this,
                        timeStamp,
                        randomOffset,
                        soundBank, 
                        waveBanks, 
                        tracks,
                        weights,
                        totalWeights,
                        variationType,
                        null,
                        null,
                        null,
                        loopCount,
                        newWaveOnLoop);

                    break;
                }

                case 4:
                {
                    // Unknown!
                    clipReader.ReadByte();

                    // Event flags
                    byte eventFlags = clipReader.ReadByte();
                    bool playRelease = (eventFlags & 0x01) == 0x01;
                    bool panEnabled = (eventFlags & 0x02) == 0x02;
                    bool useCenterSpeaker = (eventFlags & 0x04) == 0x04;

                    int trackIndex = clipReader.ReadUInt16();
                    int waveBankIndex = clipReader.ReadByte();
                    byte loopCount = clipReader.ReadByte();
                    float panAngle = clipReader.ReadUInt16() / 100.0f;
                    float panArc = clipReader.ReadUInt16() / 100.0f;

                    // Pitch variation range
                    float minPitch = clipReader.ReadInt16() / 1000.0f;
                    float maxPitch = clipReader.ReadInt16() / 1000.0f;

                    // Volume variation range
                    float minVolume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());
                    float maxVolume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());

                    // Filter variation
                    float minFrequency = clipReader.ReadSingle();
                    float maxFrequency = clipReader.ReadSingle();
                    float minQ = clipReader.ReadSingle();
                    float maxQ = clipReader.ReadSingle();

                    // Unknown!
                    clipReader.ReadByte();

                    byte variationFlags = clipReader.ReadByte();

                    // Enable pitch variation
                    Vector2? pitchVar = null;
                    if ((variationFlags & 0x10) == 0x10)
                        pitchVar = new Vector2(minPitch, maxPitch - minPitch);

                    // Enable volume variation
                    Vector2? volumeVar = null;
                    if ((variationFlags & 0x20) == 0x20)
                        volumeVar = new Vector2(minVolume, maxVolume - minVolume);

                    // Enable filter variation
                    Vector4? filterVar = null;
                    if ((variationFlags & 0x40) == 0x40)
                        filterVar = new Vector4(minFrequency, maxFrequency - minFrequency, minQ, maxQ - minQ);

                    _events[i] = new PlayWaveEvent(
                        this,
                        timeStamp,
                        randomOffset,
                        soundBank,
                        new[] { waveBankIndex },
                        new[] { trackIndex }, 
                        null,
                        0,
                        VariationType.Ordered,
                        volumeVar,
                        pitchVar, 
                        filterVar,
                        loopCount,
                        false);

                    break;
                }

                case 6:
                {
                    // Unknown!
                    clipReader.ReadByte();

                    // Event flags
                    byte eventFlags = clipReader.ReadByte();
                    bool playRelease = (eventFlags & 0x01) == 0x01;
                    bool panEnabled = (eventFlags & 0x02) == 0x02;
                    bool useCenterSpeaker = (eventFlags & 0x04) == 0x04;

                    byte loopCount = clipReader.ReadByte();
                    float panAngle = clipReader.ReadUInt16() / 100.0f;
                    float panArc = clipReader.ReadUInt16() / 100.0f;

                    // Pitch variation range
                    float minPitch = clipReader.ReadInt16() / 1000.0f;
                    float maxPitch = clipReader.ReadInt16() / 1000.0f;

                    // Volume variation range
                    float minVolume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());
                    float maxVolume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());

                    // Filter variation range
                    float minFrequency = clipReader.ReadSingle();
                    float maxFrequency = clipReader.ReadSingle();
                    float minQ = clipReader.ReadSingle();
                    float maxQ = clipReader.ReadSingle();

                    // Unknown!
                    clipReader.ReadByte();

                    // TODO: Still has unknown bits!
                    byte variationFlags = clipReader.ReadByte();

                    // Enable pitch variation
                    Vector2? pitchVar = null;
                    if ((variationFlags & 0x10) == 0x10)
                        pitchVar = new Vector2(minPitch, maxPitch - minPitch);

                    // Enable volume variation
                    Vector2? volumeVar = null;
                    if ((variationFlags & 0x20) == 0x20)
                        volumeVar = new Vector2(minVolume, maxVolume - minVolume);

                    // Enable filter variation
                    Vector4? filterVar = null;
                    if ((variationFlags & 0x40) == 0x40)
                        filterVar = new Vector4(minFrequency, maxFrequency - minFrequency, minQ, maxQ - minQ);

                    // The number of tracks for the variations.
                    ushort numTracks = clipReader.ReadUInt16();

                    // Not sure what most of this is.
                    byte moreFlags = clipReader.ReadByte();
                    bool newWaveOnLoop = (moreFlags & 0x40) == 0x40;

                    // The variation playlist type seems to be 
                    // stored in the bottom 4bits only.
                    VariationType variationType = (VariationType)(moreFlags & 0x0F);

                    // Unknown!
                    clipReader.ReadBytes(5);

                    // Read in the variation playlist.
                    int[] waveBanks = new int[numTracks];
                    int[] tracks = new int[numTracks];
                    byte[] weights = new byte[numTracks];
                    int totalWeights = 0;
                    for (int j = 0; j < numTracks; j++)
                    {
                        tracks[j] = clipReader.ReadUInt16();
                        waveBanks[j] = clipReader.ReadByte();
                        byte minWeight = clipReader.ReadByte();
                        byte maxWeight = clipReader.ReadByte();
                        weights[j] = (byte)(maxWeight - minWeight);
                        totalWeights += weights[j];
                    }

                    _events[i] = new PlayWaveEvent(
                        this,
                        timeStamp,
                        randomOffset,
                        soundBank,
                        waveBanks,
                        tracks,
                        weights,
                        totalWeights,
                        variationType,
                        volumeVar,
                        pitchVar, 
                        filterVar,
                        loopCount,
                        newWaveOnLoop);

                    break;
                }

                case 7:
                    // Pitch Event
                    throw new NotImplementedException("Pitch event");

                case 8:
                {
                    // Unknown!
                    clipReader.ReadBytes(2);

                    // Event flags
                    byte eventFlags = clipReader.ReadByte();
                    bool isAdd = (eventFlags & 0x01) == 0x01;

                    // The replacement or additive volume.
                    float decibles = clipReader.ReadSingle() / 100.0f;
                    float volume = XactHelpers.ParseVolumeFromDecibels(decibles + (isAdd ? volumeDb : 0));

                    // Unknown!
                    clipReader.ReadBytes(9);

                    _events[i] = new VolumeEvent(   this, 
                                                    timeStamp, 
                                                    randomOffset, 
                                                    volume);
                    break;
                }

                case 17:
                    // Volume Repeat Event
                    throw new NotImplementedException("Volume repeat event");

                case 9:
                    // Marker Event
                    throw new NotImplementedException("Marker event");

                default:
                    throw new NotSupportedException("Unknown event " + eventId);
                }
            }
            
            clipReader.BaseStream.Seek(oldPosition, SeekOrigin.Begin);
#pragma warning restore 0219
        }

        internal void Update(float dt)
        {
            if (State != SoundState.Playing)
                return;

            _time += dt;

            // Play the next event.
            while (_nextEvent < _events.Length)
            {
                ClipEvent evt = _events[_nextEvent];
                if (_time < evt.TimeStamp)
                    break;

                evt.Play();
                ++_nextEvent;
            }

            // Update all the active events.
            bool isPlaying = _nextEvent < _events.Length;
            for (int i = 0; i < _nextEvent; i++)
            {
                ClipEvent evt = _events[i];
                isPlaying |= evt.Update(dt);
            }

            // Update the state.
            if (!isPlaying)
                State = SoundState.Stopped;
        }

        internal void SetFade(float fadeInDuration, float fadeOutDuration)
        {
            foreach (ClipEvent evt in _events)
            {
                if (evt is PlayWaveEvent)
                    evt.SetFade(fadeInDuration, fadeOutDuration);
            }
        }
        
        internal void UpdateState(float volume, float pitch, float reverbMix, float? filterFrequency, float? filterQFactor)
        {
            _volumeScale = volume;
            float trackVolume = _volume * _volumeScale;

            foreach (ClipEvent evt in _events)
                evt.SetState(trackVolume, pitch, reverbMix, filterFrequency, filterQFactor);
        }

        public void Play()
        {
            _time = 0.0f;
            _nextEvent = 0;
            SetVolume(_defaultVolume);
            State = SoundState.Playing; 
            Update(0);
        }

        public void Resume()
        {
            foreach (ClipEvent evt in _events)
                evt.Resume();

            State = SoundState.Playing;
        }
        
        public void Stop()
        {
            foreach (ClipEvent evt in _events)
                evt.Stop();

            State = SoundState.Stopped;
        }
        
        public void Pause()
        {
            foreach (ClipEvent evt in _events)
                evt.Pause();

            State = SoundState.Paused;
        }

        public SoundState State { get; private set; }

        /// <summary>
        /// Set the combined volume scale from the parent objects.
        /// </summary>
        /// <param name="volume">The volume scale.</param>
        public void SetVolumeScale(float volume)
        {
            _volumeScale = volume;
            UpdateVolumes();
        }

        /// <summary>
        /// Set the volume for the clip.
        /// </summary>
        /// <param name="volume">The volume level.</param>
        public void SetVolume(float volume)
        {
            _volume = volume;
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            float volume = _volume * _volumeScale;
            foreach (ClipEvent evt in _events)
                evt.SetTrackVolume(volume);
        }

        public void SetPan(float pan)
        {
            foreach (ClipEvent evt in _events)
                evt.SetTrackPan(pan);
        }
    }
}

