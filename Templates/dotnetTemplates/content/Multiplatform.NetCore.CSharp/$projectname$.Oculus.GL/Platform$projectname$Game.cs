using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input.XR;
using Microsoft.Xna.Framework.XR;

namespace $safeprojectname$
{
    /// <inheritdoc/>
    public class Platform$safeprojectname$Game : $safeprojectname$Game
    {
        XRDevice xrDevice;

        public Platform$safeprojectname$Game()
        {
            // TODO: Add platform specific initialization logic here

            graphics.IsFullScreen = true;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;

            // OXR requires at least feature level 9.3.
            graphics.GraphicsProfile = GraphicsProfile.HiDef;

            // Syncronize to the headset refresh rate.
            graphics.SynchronizeWithVerticalRetrace = true;
            IsFixedTimeStep = false;
            // 72Hz Frame rate for oculus.
            TargetElapsedTime = TimeSpan.FromTicks(138888);

            // We don't care is the main window is Focuses or not
            // because we render on the XR rendertargets.
            InactiveSleepTime = TimeSpan.FromSeconds(0);

            xrDevice = new XRDevice("$safeprojectname$", this.Services);
        }

        /// <inheritdoc/>
        protected override void Initialize()
        {
            base.Initialize();

            // Initialize XR device and Select VR/AR mode.
            xrDevice.BeginSessionAsync(XRSessionMode.VR);
            xrDevice.TrackFloorLevelAsync(true);
        }

        /// <inheritdoc/>
        protected override void LoadContent()
        {
            base.LoadContent();
        }

        /// <inheritdoc/>
        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        /// <inheritdoc/>
        protected override void Update(GameTime gameTime)
        {
            GamePadState touchControllerState = TouchController.GetState(TouchControllerType.Touch);

            base.Update(gameTime);
        }

        /// <inheritdoc/>
        protected override void Draw(GameTime gameTime)
        {
            float aspect = GraphicsDevice.Viewport.AspectRatio;
            Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 0.05f, 1000);

            if (xrDevice.DeviceState == XRDeviceState.Enabled)
            {
                // Draw on XR headset.
                int beginResult = xrDevice.BeginFrame();
                if (beginResult >= 0)
                {
                    try
                    {
                        HeadsetState headsetState = xrDevice.GetHeadsetState();

                        // Draw each eye on a rendertarget.
                        foreach (XREye eye in xrDevice.GetEyes())
                        {
                            RenderTarget2D rt = xrDevice.GetEyeRenderTarget(eye);
                            GraphicsDevice.SetRenderTarget(rt);

                            // Get XR view and projection.
                            view = headsetState.GetEyeView(eye);
                            projection = xrDevice.CreateProjection(eye, 0.05f, 1000);

                            // Draw eye rendertarget.
                            if (xrDevice.SessionMode == XRSessionMode.AR)
                                GraphicsDevice.Clear(Color.Transparent);
                            else
                                GraphicsDevice.Clear(Color.CornflowerBlue);

                            base.Draw(gameTime);

                            // Resolve eye rendertarget.
                            GraphicsDevice.SetRenderTarget(null);
                            // Submit eye rendertarget.
                            xrDevice.CommitRenderTarget(eye, rt);
                        }
                    }
                    finally
                    {
                        // Submit XR frame.
                        int result = xrDevice.EndFrame();
                    }

                    return;
                }
            }

            base.Draw(gameTime);
        }
    }
}
