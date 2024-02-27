// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Reflection;
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

                Console.WriteLine("AudioFactory not found.");
                Console.WriteLine("Initialize audio with 'AudioFactory.RegisterAudioFactory(new ConcreteAudioFactory());'.");
                AudioFactory audioFactory = CreateAudioFactory();
                AudioFactory.RegisterAudioFactory(audioFactory);

                return _current;
            }
        }

        private static AudioFactory CreateAudioFactory()
        {
            Console.WriteLine("Registering Concrete AudioFactoryStrategy through reflection.");

            // find and create Concrete AudioFactoryStrategy through reflection.
            Assembly currentAsm = typeof(AudioFactory).Assembly;

            // seach in current Assembly
            foreach (Type type in currentAsm.GetExportedTypes())
                if (type.IsSubclassOf(typeof(AudioFactory)) && !type.IsAbstract)
                    return (AudioFactory)Activator.CreateInstance(type);

            // seach in loaded Assemblies
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (AssemblyName refAsm in asm.GetReferencedAssemblies())
                {
                    if (refAsm.FullName == currentAsm.FullName)
                    {
                        foreach (Type type in asm.GetExportedTypes())
                            if (type.IsSubclassOf(typeof(AudioFactory)) && !type.IsAbstract)
                                return (AudioFactory)Activator.CreateInstance(type);
                    }
                }
            }

            return null;
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

