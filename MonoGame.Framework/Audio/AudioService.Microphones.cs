// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    /// <summary>
    /// Handles the buffer events of all DynamicSoundEffectInstance instances.
    /// </summary>
    public sealed partial class AudioService
    {
        internal Microphone _defaultMicrophone = null;
        internal List<Microphone> _microphones = new List<Microphone>();

        private void UpdateMicrophones()
        {
            // querying all running microphones for new samples available
            for (int i = 0; i < _microphones.Count; i++)
                _microphones[i].UpdateBuffer();
        }

        private void StopMicrophones()
        {
            // stopping all running microphones before shutting down audio devices
            for (int i = 0; i < _microphones.Count; i++)
                _microphones[i].Stop();
        }
    }
}
