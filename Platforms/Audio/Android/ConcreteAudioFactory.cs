// Copyright (C)2022 Nick Kastellanos

namespace Microsoft.Xna.Platform.Audio
{
    public sealed class ConcreteAudioFactory : AudioFactory
    {
        public override AudioServiceStrategy CreateAudioServiceStrategy()
        {
            return new ConcreteAudioService();
        }

        public override MicrophoneStrategy CreateMicrophoneStrategy()
        {
            return new ConcreteMicrophone();
        }

        public override SoundEffectStrategy CreateSoundEffectStrategy()
        {
            return new ConcreteSoundEffect();
        }
    }
}
