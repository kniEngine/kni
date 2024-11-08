// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    public sealed partial class AudioService
    {
        private LinkedList<SoundEffectInstance> _pooledInstances = new LinkedList<SoundEffectInstance>();

        /// <summary>
        /// Add the specified instance to the pool if it is a pooled instance and removes it from the
        /// list of playing instances.
        /// </summary>
        /// <param name="inst">The SoundEffectInstance</param>
        internal void AddPooledInstance(SoundEffectInstance inst)
        {
            if (inst.PooledInstancesNode == null)
                return;

            int maxPooledInstances = Math.Min(512, MAX_PLAYING_INSTANCES) * 2;
            if (_pooledInstances.Count >= maxPooledInstances)
            {
                var firstNode = _pooledInstances.First;
                firstNode.Value.Dispose();
                _pooledInstances.Remove(firstNode);
            }

            _pooledInstances.AddLast(inst.PooledInstancesNode);
        }

        /// <summary>
        /// Returns a pooled SoundEffectInstance if one is available, or allocates a new
        /// SoundEffectInstance if the pool is empty.
        /// </summary>
        /// <returns>The SoundEffectInstance.</returns>
        private SoundEffectInstance GetPooledInstance(SoundEffect effect, bool forXAct = false)
        {
            // search for an instance of effect
            for (var node = _pooledInstances.First; node != null; node = node.Next)
            {
                if (ReferenceEquals(node.Value._effect, effect))
                {
                    SoundEffectInstance inst = node.Value;
                    _pooledInstances.Remove(inst.PooledInstancesNode);

                    inst._strategy.IsXAct = forXAct;
                    inst.Reset();
                    return inst;
                }
            }

            return null;
        }
    }
}
