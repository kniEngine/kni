// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Media;
using Foundation;
using AVFoundation;
using RectF = CoreGraphics.CGRect;
using AppKit;
using CoreAnimation;


namespace Microsoft.Xna.Framework.Media
{
    public sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
    {
        NSSDLWindow nsWindow;
        AVPlayerLayer layer;
        NSView view;

        public override MediaState State
        {
            get { return base.State; }
            protected set { base.State = value; }
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

        public override bool IsLooped
        {
            get { return base.IsLooped; }
            set { base.IsLooped = value; }
        }

        public override float Volume
        {
            get { return base.Volume; }
            set
            {
                base.Volume = value;
                if (base.Video != null)
                    PlatformSetVolume();
            }
        }

        public ConcreteVideoPlayerStrategy()
        {
            Sdl.Window.SDL_SysWMinfo sys = new Sdl.Window.SDL_SysWMinfo();
            Sdl.Window.GetWindowWMInfo(Game.Instance.Window.Handle, ref sys);
            nsWindow = new NSSDLWindow(sys.window);
        }

        public override Texture2D PlatformGetTexture()
        {
            throw new NotImplementedException();
        }

        protected override void PlatformUpdateState(ref MediaState state)
        {
        }

        public override void PlatformPause()
        {
            base.Video.Player.Pause();
            State = MediaState.Paused;
        }

        public override void PlatformResume()
        {
            base.Video.Volume = _volume;
            base.Video.Player.Play();
            State = MediaState.Playing;
        }

        public override void PlatformPlay(Video video)
        {
            base.Video = video;

            layer = AVPlayerLayer.FromPlayer(base.Video.Player);
            view = new NSView(nsWindow.ContentView.Frame);
            view.WantsLayer = true;
            view.Layer = layer;
            layer.Frame = nsWindow.ContentView.Bounds;
            nsWindow.ContentView.AddSubview(view);
           
            NSNotificationCenter.DefaultCenter.AddObserver(AVPlayerItem.DidPlayToEndTimeNotification,
                                                           notification =>
            {
                Stop();

                if (IsLooped)
                    PlatformPlay();

            });

            base.Video.Volume = _volume;
            base.Video.Player.Play();

            State = MediaState.Playing;
        }

        public override void PlatformStop()
        {
            var movieView = base.Video.Player;
            movieView.Pause();
            movieView.Seek(CoreMedia.CMTime.Zero);

            nsWindow.ContentView.WillRemoveSubview(view);
            view.RemoveFromSuperview();
            view.Dispose();
            view = null;
            layer.Dispose();
            layer = null;

            State = MediaState.Stopped;
        }

        public override TimeSpan PlatformGetPlayPosition()
        {
            return base.Video.CurrentPosition;
        }

        private void PlatformSetVolume()
        {
            base.Video.Volume = _volume;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }

    internal class NSSDLWindow : AppKit.NSWindow
    {
        public NSSDLWindow(IntPtr handle) : base(handle)
        {
        }
    }
}
