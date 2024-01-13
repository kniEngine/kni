// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Platform.Media
{
    public interface IPlatformVideo
    {
        VideoStrategy Strategy { get; }
    }

    public class VideoStrategy : IDisposable
    {
        public VideoStrategy(GraphicsDevice graphicsDevice, string fileName, TimeSpan duration)
        {
            this.GraphicsDevice = graphicsDevice;
            this.FileName = fileName;
            this.Duration = duration;
        }

        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// I actually think this is a file PATH...
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets the duration of the Video.
        /// </summary>
        public TimeSpan Duration { get; internal set; }

        /// <summary>
        /// Gets the frame rate of this video.
        /// </summary>
        public float FramesPerSecond { get; internal set; }

        /// <summary>
        /// Gets the width of this video, in pixels.
        /// </summary>
        public int Width { get; internal set; }

        /// <summary>
        /// Gets the height of this video, in pixels.
        /// </summary>
        public int Height { get; internal set; }

        /// <summary>
        /// Gets the VideoSoundtrackType for this video.
        /// </summary>
        public VideoSoundtrackType VideoSoundtrackType { get; internal set; }

        public T ToConcrete<T>() where T : VideoStrategy
        {
            return (T)this;
        }

        #region IDisposable Members
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            GraphicsDevice = null;
        }

        #endregion
    }
}
