// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Platform.Audio;
using Microsoft.Xna.Platform.Media;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class Song : SongStrategy
    {
        private OggStream _stream;
        private float _volume = 1f;
        private readonly object _sourceMutex = new object();

        internal override void PlatformInitialize(string fileName)
        {
            // init OpenAL if need be
            var audioService = AudioService.Current;

            _stream = new OggStream(fileName, OnFinishedPlaying);
            _stream.Prepare();

            _duration = _stream.GetLength();
        }
        
        internal void SetEventHandler(FinishedPlayingHandler handler)
        {

        }

        internal void OnFinishedPlaying()
        {
            MediaPlayer.Strategy.OnSongFinishedPlaying(null, null);
        }
		
        internal override void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
                lock (_sourceMutex)
                {
                    if (_stream != null)
                        _stream.Dispose();

                    _stream = null;
                }
            }
        }

        internal void Play()
        {
            if (_stream == null)
                return;

            _stream.Play();
            _playCount++;
        }

        internal void Resume()
        {
            if (_stream == null)
                return;

            _stream.Resume();
        }

        internal void Pause()
        {
            if (_stream == null)
                return;

            _stream.Pause();
        }

        internal void Stop()
        {
            if (_stream == null)
                return;

            _stream.Stop();
            _playCount = 0;
        }

        internal float Volume
        {
            get
            {
                if (_stream == null)
                    return 0.0f;
                return _volume; 
            }
            set
            {
                _volume = value;
                if (_stream != null)
                    _stream.Volume = _volume;
            }
        }

        internal TimeSpan Position
        {
            get
            {
                if (_stream == null)
                    return TimeSpan.FromSeconds(0.0);
                return _stream.GetPosition();
            }
        }

        internal override Album PlatformGetAlbum()
        {
            return null;
        }

        internal override void PlatformSetAlbum(Album album)
        {
            
        }

        internal override Artist PlatformGetArtist()
        {
            return null;
        }

        internal override Genre PlatformGetGenre()
        {
            return null;
        }

        internal override TimeSpan PlatformGetDuration()
        {
            return _duration;
        }

        internal override bool PlatformIsProtected()
        {
            return false;
        }

        internal override bool PlatformIsRated()
        {
            return false;
        }

        internal override string PlatformGetName()
        {
            return Path.GetFileNameWithoutExtension(_name);
        }

        internal override int PlatformGetPlayCount()
        {
            return _playCount;
        }

        internal override int PlatformGetRating()
        {
            return 0;
        }

        internal override int PlatformGetTrackNumber()
        {
            return 0;
        }
    }
}

