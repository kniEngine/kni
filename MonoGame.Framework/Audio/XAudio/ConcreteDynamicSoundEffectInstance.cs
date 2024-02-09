// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using SharpDX;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace Microsoft.Xna.Platform.Audio
{
    public sealed class ConcreteDynamicSoundEffectInstance : ConcreteSoundEffectInstance
        , IDynamicSoundEffectInstanceStrategy
    {

        private static ByteBufferPool _bufferPool = new ByteBufferPool();

        private int _sampleRate;
        private int _channels;
        private List<QueuedBuffer> _queuedBuffers = new List<QueuedBuffer>();
        private int _uid;

        private readonly WeakReference _dynamicSoundEffectInstanceRef = new WeakReference(null);
        DynamicSoundEffectInstance IDynamicSoundEffectInstanceStrategy.DynamicSoundEffectInstance
        {
            get { return _dynamicSoundEffectInstanceRef.Target as DynamicSoundEffectInstance; }
            set { _dynamicSoundEffectInstanceRef.Target = value; }
        }

        internal ConcreteDynamicSoundEffectInstance(AudioServiceStrategy audioServiceStrategy, int sampleRate, int channels, float pan)
            : base(audioServiceStrategy, null, pan)
        {
            _sampleRate = sampleRate;
            _channels = channels;
            WaveFormat format = new WaveFormat(sampleRate, channels);

            _voice = new SourceVoice(ConcreteAudioService.Device, format, true);
            _voice.BufferEnd += OnBufferEnd;
        }

        public int BuffersNeeded { get; set; }

        public int DynamicPlatformGetPendingBufferCount()
        {
            return _queuedBuffers.Count;
        }

        public override void PlatformPause()
        {
            _voice.Stop();
        }

        public override void PlatformPlay(bool isLooped)
        {
            _voice.Start();
        }

        public override void PlatformResume(bool isLooped)
        {
            _voice.Start();
        }

        public override void PlatformStop()
        {
            _voice.Stop();
        }

        public override void PlatformRelease(bool isLooped)
        {
            System.Diagnostics.Debug.Assert(isLooped == false);

            // Dequeue all the submitted buffers
            _voice.FlushSourceBuffers();

        }

        public void DynamicPlatformSubmitBuffer(byte[] buffer, int offset, int count, SoundState state)
        {
            // we need to copy so datastream does not pin the buffer that the user might modify later
            byte[] dataBuffer = _bufferPool.Get(count);
            Buffer.BlockCopy(buffer, offset, dataBuffer, 0, count);

            DataStream stream = DataStream.Create(dataBuffer, true, false, 0, true);
            AudioBuffer audioBuffer = new AudioBuffer(stream);
            audioBuffer.AudioBytes = count;
            audioBuffer.Context = new IntPtr(unchecked(_uid++));

            _voice.SubmitSourceBuffer(audioBuffer, null);

            _queuedBuffers.Add(new QueuedBuffer(dataBuffer, audioBuffer));
        }

        public void DynamicPlatformClearBuffers()
        {
            // Dequeue all the submitted buffers
            _voice.FlushSourceBuffers();

            for (int i = _queuedBuffers.Count - 1; i >= 0; i--)
            {
                QueuedBuffer queuedBuffer = _queuedBuffers[i];
                _queuedBuffers.RemoveAt(i);

                queuedBuffer.AudioBuffer.Stream.Dispose();
                _bufferPool.Return(queuedBuffer.DataBuffer);
            }
        }

        public void DynamicPlatformUpdateBuffers()
        {
            // The XAudio implementation utilizes callbacks, so no work here.
        }


        private void OnBufferEnd(IntPtr context)
        {
            lock (base.AudioServiceSyncHandle)
            {
                // Release the queued buffer
                for (int i = 0; i < _queuedBuffers.Count; i++)
                {
                    if (_queuedBuffers[i].AudioBuffer.Context == context)
                    {
                        QueuedBuffer queuedBuffer = _queuedBuffers[i];
                        _queuedBuffers.RemoveAt(i);

                        queuedBuffer.AudioBuffer.Stream.Dispose();
                        _bufferPool.Return(queuedBuffer.DataBuffer);
                        break;
                    }
                }

                // Raise the event
                lock (base.AudioServiceSyncHandle)
                {
                    this.BuffersNeeded++;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = _queuedBuffers.Count - 1; i >= 0; i--)
                {
                    QueuedBuffer queuedBuffer = _queuedBuffers[i];
                    _queuedBuffers.RemoveAt(i);

                    queuedBuffer.AudioBuffer.Stream.Dispose();
                    _bufferPool.Return(queuedBuffer.DataBuffer);
                }
            }

            base.Dispose(disposing);
        }

        struct QueuedBuffer
        {
            public byte[] DataBuffer;
            public AudioBuffer AudioBuffer;

            public QueuedBuffer(byte[] dataBuffer, AudioBuffer audioBuffer)
            {
                this.DataBuffer = dataBuffer;
                this.AudioBuffer = audioBuffer;
            }
        }

        class ByteBufferPool
        {
            private const int _minBufferSize = 0;
            private const int _maxBuffers = int.MaxValue;

            private readonly List<byte[]> _bufferPool = new List<byte[]>();

            public ByteBufferPool()
            {
            }

            /// <summary>
            /// Get a buffer that is at least as big as size.
            /// </summary>
            public byte[] Get(int size)
            {
                if (size < _minBufferSize)
                    size = _minBufferSize;

                lock (_bufferPool)
                {
                    int index = FirstLargerThan(size);

                    if (index == -1)
                    {
                        if (_bufferPool.Count > 0)
                            _bufferPool.RemoveAt(_bufferPool.Count - 1);
                        return new byte[size];
                    }
                    else
                    {
                        byte[] result = _bufferPool[index];
                        _bufferPool.RemoveAt(index);
                        return result;
                    }
                }
            }

            /// <summary>
            /// Return the given buffer to the pool.
            /// </summary>
            public void Return(byte[] buffer)
            {
                lock (_bufferPool)
                {
                    if (_bufferPool.Count >= _maxBuffers)
                        return;
                    int index = FirstLargerThan(buffer.Length);
                    if (index == -1)
                        _bufferPool.Add(buffer);
                    else
                        _bufferPool.Insert(index, buffer);
                }
            }

            // Find the smallest buffer that is larger than or equally large as size or -1 if none exist
            private int FirstLargerThan(int size)
            {
                if (_bufferPool.Count == 0) return -1;

                int l = 0;
                int r = _bufferPool.Count - 1;

                while (l <= r)
                {
                    int m = (l + r) / 2;
                    byte[] buffer = _bufferPool[m];
                    if (buffer.Length < size)
                    {
                        l = m + 1;
                    }
                    else if (buffer.Length > size)
                    {
                        r = m;
                        if (l == r) return l;
                    }
                    else
                    {
                        return m;
                    }
                }

                return -1;
            }
        }
    }
}
