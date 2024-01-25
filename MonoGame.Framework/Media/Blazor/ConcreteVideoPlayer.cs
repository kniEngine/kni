// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using nkast.Wasm.Canvas.WebGL;
using WasmDom = nkast.Wasm.Dom;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
    {
        private WasmDom.Video _player;

        private bool _isPlaying = false;
        private bool _frameAvailable;
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
                _player.Loop = value;

            }
        }

        public override bool IsMuted
        {
            get { return base.IsMuted; }
            set
            {
                base.IsMuted = value;
                _player.Muted = value;
            }
        }

        public override TimeSpan PlayPosition
        {
            get { throw new NotImplementedException(); }
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

        internal ConcreteVideoPlayerStrategy()
        {
            _player = new WasmDom.Video();
            _player.OnPlaying += Player_OnPlaying;
            _player.OnTimeUpdate += Player_OnTimeUpdate;
            _player.OnEnded += Player_OnEnded;

            return;
        }


        public override Texture2D PlatformGetTexture()
        {
            if (_lastFrame != null)
            {
                if (_lastFrame.Width != base.Video.Width || _lastFrame.Height != base.Video.Height)
                {
                    _lastFrame.Dispose();
                    _lastFrame = null;
                }
            }
            if (_lastFrame == null)
                _lastFrame = new Texture2D(base.Video.GraphicsDevice, base.Video.Width, base.Video.Height, false, SurfaceFormat.Color);

            if (_isPlaying && _frameAvailable)
            {
                var GL = ((IPlatformGraphicsContext) ((IPlatformGraphicsDevice)base.Video.GraphicsDevice).Strategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

                ConcreteTexture2D texture2D = ((IPlatformTexture)_lastFrame).GetTextureStrategy<ConcreteTexture2D>();

                System.Diagnostics.Debug.Assert(texture2D._glTexture != null);
                ((IPlatformTextureCollection)texture2D.GraphicsDeviceStrategy.CurrentContext.Textures).Strategy.Dirty(0);
                GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + 0);
                GL.CheckGLError();
                GL.BindTexture(WebGLTextureTarget.TEXTURE_2D, texture2D._glTexture);
                GL.CheckGLError();

                GL.PixelStore(WebGLPixelParameter.UNPACK_ALIGNMENT, Math.Min(texture2D.Format.GetSize(), 8));
                GL.CheckGLError();

                GL.TexImage2D(WebGLTextureTarget.TEXTURE_2D, 0,
                    texture2D._glInternalFormat, texture2D._glFormat, texture2D._glType,
                    _player);
                GL.CheckGLError();

                //GL.Finish();
                //GL.CheckGLError();
            }

            return _lastFrame;
        }


        protected override void PlatformUpdateState(ref MediaState state)
        {
        }

        public override void PlatformPlay(Video video)
        {
            _player.Pause();
            _player.Src = "";

            _isPlaying = false;
            _frameAvailable = false;

            base.Video = video;

            ConcreteVideoStrategy videoStrategy = ((IPlatformVideo)video).Strategy.ToConcrete<ConcreteVideoStrategy>();

            _player.Src = videoStrategy.FileName;
            _player.Load();
            _player.Play();

            State = MediaState.Playing;
        }

        public override void PlatformPause()
        {
            _player.Pause();
            _isPlaying = false;
            _frameAvailable = false;

            State = MediaState.Paused;
        }

        public override void PlatformResume()
        {
            _player.Play();

            State = MediaState.Playing;
        }
        
        public override void PlatformStop()
        {
            _player.Pause();
            _player.Src = "";

            _isPlaying = false;
            _frameAvailable = false;

            State = MediaState.Stopped;
        }

        private void PlatformSetVolume()
        {
            _player.Volume = Volume;
        }

        private void Player_OnPlaying(object sender, EventArgs e)
        {
            _isPlaying = true;
        }

        private void Player_OnTimeUpdate(object sender, EventArgs e)
        {
            _frameAvailable = true;
        }

        private void Player_OnEnded(object sender, EventArgs e)
        {
            _isPlaying = false;
            _frameAvailable = false;

            State = MediaState.Stopped;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _player.OnPlaying -= Player_OnPlaying;
                _player.OnTimeUpdate -= Player_OnTimeUpdate;
                _player.OnEnded -= Player_OnEnded;
            }

            base.Dispose(disposing);
        }
    }
}
