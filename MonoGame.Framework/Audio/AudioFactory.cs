// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    public abstract class AudioFactory
    {
        private volatile static AudioFactory _current;

        internal static AudioFactory Current
        {
            get
            {
                AudioFactory current = _current;
                if (current != null)
                    return current;

                AudioFactory audioFactory = CreateAudioFactory();
                AudioFactory.RegisterAudioFactory(audioFactory);

                return _current;
            }
        }

        private static AudioFactory CreateAudioFactory()
        {
            return new ConcreteAudioFactory();
        }

        public static void RegisterAudioFactory(AudioFactory audioFactory)
        {
            if (audioFactory == null)
                throw new NullReferenceException("audioFactory");

            lock (AudioService.SyncHandle)
            {
                if (_current == null)
                    _current = audioFactory;
                else
                    throw new InvalidOperationException("AudioFactory allready registered.");
            }
        }

        public abstract AudioServiceStrategy CreateAudioServiceStrategy();
        public abstract MicrophoneStrategy CreateMicrophoneStrategy();
        public abstract SoundEffectStrategy CreateSoundEffectStrategy();
    }

}

