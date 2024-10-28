// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using GLPixelFormat = Microsoft.Xna.Platform.Graphics.OpenGL.PixelFormat;
using Android.Views;
using Android.Graphics;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
    {
        private const int GL_TEXTURE_EXTERNAL_OES = (0x00008D65);
        
        private Android.Media.MediaPlayer _player;

        private SurfaceTexture _surfaceTexture;
        private Surface _surface;
        private GraphicsDeviceStrategy _graphicsDeviceStrategy;
        private int _glVideoSurfaceTexture;

        private bool _frameAvailable;
        private byte[] _frameData;
        private Texture2D _lastFrame;

        public override MediaState State
        {
            get { return base.State; }
            protected set { base.State = value; }
        }

        public override bool IsLooped
        {
            get { return base.IsLooped; }
            set
            {
                base.IsLooped = value;

                _player.Looping = true;
            }
        }

        public override bool IsMuted
        {
            get { return base.IsMuted; }
            set
            {
                base.IsMuted = value;
                throw new NotImplementedException();
            }
        }

        public override TimeSpan PlayPosition
        {
            get { return TimeSpan.FromMilliseconds(_player.CurrentPosition); }
        }

        public override float Volume
        {
            get { return base.Volume; }
            set
            {
                base.Volume = value;
                
                PlatformSetVolume();
            }
        }

        public override Video Video
        {
            get { return base.Video; }
            protected set
            {
                base.Video = value;
                if (value != null)
                    CreateGLVideoSurfaceTexture(((IPlatformGraphicsDevice)((IPlatformVideo)value).Strategy.GraphicsDevice).Strategy);
            }
        }


        internal ConcreteVideoPlayerStrategy()
        {
            _player = new Android.Media.MediaPlayer();

        }
        
        private void CreateGLVideoSurfaceTexture(GraphicsDeviceStrategy graphicsDeviceStrategy)
        {
            if (_graphicsDeviceStrategy != null)
            {
                if (_graphicsDeviceStrategy != graphicsDeviceStrategy)
                    throw new InvalidOperationException();

                return;
            }

            _graphicsDeviceStrategy = graphicsDeviceStrategy;

            var GL = ((IPlatformGraphicsContext)_graphicsDeviceStrategy.MainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;


            _glVideoSurfaceTexture = GL.GenTexture();
            GL.CheckGLError();
            ((IPlatformTextureCollection)_graphicsDeviceStrategy.MainContext.Textures).Strategy.Dirty(0);
            GL.ActiveTexture(TextureUnit.Texture0 + 0);
            GL.CheckGLError();
            GL.BindTexture((TextureTarget)GL_TEXTURE_EXTERNAL_OES, _glVideoSurfaceTexture);
            GL.CheckGLError();
            GL.TexParameter((TextureTarget)GL_TEXTURE_EXTERNAL_OES, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.CheckGLError();
            GL.TexParameter((TextureTarget)GL_TEXTURE_EXTERNAL_OES, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.CheckGLError();
            GL.TexParameter((TextureTarget)GL_TEXTURE_EXTERNAL_OES, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.CheckGLError();
            GL.TexParameter((TextureTarget)GL_TEXTURE_EXTERNAL_OES, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.CheckGLError();

            _surfaceTexture = new SurfaceTexture(_glVideoSurfaceTexture);
            _surface = new Surface(_surfaceTexture);

            _player.SetSurface(_surface);
            _surfaceTexture.FrameAvailable += _surfaceTexture_FrameAvailable;
        }

        public override Texture2D PlatformGetTexture()
        {
            if (_lastFrame != null)
            {
                if (_lastFrame.Width != this.Video.Width || _lastFrame.Height != this.Video.Height)
                {
                    _lastFrame.Dispose();
                    _lastFrame = null;
                }
            }
            if (_lastFrame == null)
                _lastFrame = new Texture2D(((IPlatformVideo)base.Video).Strategy.GraphicsDevice, this.Video.Width, this.Video.Height, false, SurfaceFormat.Color);

            var GL = ((IPlatformGraphicsContext)_graphicsDeviceStrategy.MainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            if (_frameAvailable)
            {
                _frameAvailable = false;

                // Calculate the buffer size for RGBA format
                int frameBufferSize = this.Video.Width * this.Video.Height * 4;

                // Allocate memory for the frame data if needed
                if (_frameData == null || _frameData.Length != frameBufferSize)
                    _frameData = new byte[frameBufferSize];

                // Update the surface texture
                _surfaceTexture.UpdateTexImage();

                // Create a framebuffer
                int framebuffer = GL.GenFramebuffer();
                GL.CheckGLError();

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                GL.CheckGLError();

                // Attach the texture to the framebuffer
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, (TextureTarget)GL_TEXTURE_EXTERNAL_OES, _glVideoSurfaceTexture, 0);
                GL.CheckGLError();

                // Read the pixel data from the framebuffer
                GCHandle dataHandle = GCHandle.Alloc(_frameData, GCHandleType.Pinned);
                try
                {
                    IntPtr dataPtr = dataHandle.AddrOfPinnedObject();
                    GL.ReadPixels(0, 0, this.Video.Width, this.Video.Height, GLPixelFormat.Rgba, PixelType.UnsignedByte, dataPtr);
                    GL.CheckGLError();
                }
                finally
                {
                    dataHandle.Free();
                }

                // Dettach framebuffer
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.CheckGLError();

                // cleanup
                GL.DeleteFramebuffer(framebuffer);
                GL.CheckGLError();

                _lastFrame.SetData(_frameData, 0, frameBufferSize);
            }

            return _lastFrame;
        }
        
        protected override void PlatformUpdateState(ref MediaState state)
        {
        }


        public override void PlatformPlay(Video video)
        {
            MediaState state = State;
            if (state == MediaState.Playing || state == MediaState.Paused)
            {
                _player.Stop();
                _player.Reset();
            }

            this.Video = video;

            Android.Content.Res.AssetFileDescriptor afd = Android.App.Application.Context.Assets.OpenFd(((IPlatformVideo)this.Video).Strategy.FileName);
            if (afd == null)
                return;

            _player.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
            afd.Close();
            _player.Prepare();

            _player.Start();

            State = MediaState.Playing;
        }

        private void _surfaceTexture_FrameAvailable(object sender, SurfaceTexture.FrameAvailableEventArgs e)
        {
            _frameAvailable = true;
        }

        public override void PlatformPause()
        {
            _player.Pause();
            State = MediaState.Paused;
        }

        public override void PlatformResume()
        {
            _player.Start();
            State = MediaState.Playing;
        }

        public override void PlatformStop()
        {
            _player.Stop();
            _player.Reset();

            State = MediaState.Stopped;
        }

        private void PlatformSetVolume()
        {
            float logVolume = (float)Math.Pow(Volume, 2);
            _player.SetVolume(logVolume, logVolume);
        }

        protected override void Dispose(bool disposing)
        {

            if (disposing)
            {
                if (_lastFrame != null)
                    _lastFrame.Dispose();
                _lastFrame = null;

                if (_player != null)
                    _player.Dispose();
                _player = null;

                if (_surfaceTexture != null)
                {
                    _surfaceTexture.FrameAvailable -= _surfaceTexture_FrameAvailable;
                    _surfaceTexture.Dispose();
                }
                _surfaceTexture = null;

                if (_surface != null)
                    _surface.Dispose();
                _surface = null;
            }

            if (_graphicsDeviceStrategy != null)
            {
                var GL = ((IPlatformGraphicsContext)_graphicsDeviceStrategy.MainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

                if (_glVideoSurfaceTexture != 0)
                {
                    GL.DeleteTexture(_glVideoSurfaceTexture);
                    _glVideoSurfaceTexture = 0;
                }
            }
            _graphicsDeviceStrategy = null;

            base.Dispose(disposing);
        }
    }    
}
