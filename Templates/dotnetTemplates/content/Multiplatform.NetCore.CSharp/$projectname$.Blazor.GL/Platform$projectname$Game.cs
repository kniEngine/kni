using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
//-:cnd:noEmit
#if WebXR
using Microsoft.Xna.Framework.XR;
using Microsoft.Xna.Framework.Input.XR;
#endif
//+:cnd:noEmit

namespace $safeprojectname$
{
    /// <inheritdoc/>
    public class Platform$safeprojectname$Game : $safeprojectname$Game
    {
//-:cnd:noEmit
#if WebXR
        XRDevice xrDevice;
#endif
//+:cnd:noEmit

        public Platform$safeprojectname$Game() : base()
        {
            // TODO: Add platform specific initialization logic here

//-:cnd:noEmit
#if WebXR
            // WebXR requires at least WebGL2.
            graphics.GraphicsProfile = GraphicsProfile.HiDef;

            IsFixedTimeStep = true;
            // 90Hz Frame rate for WebXR
            TargetElapsedTime = TimeSpan.FromTicks(111111);

            // We don't care is the main window is Focuses or not
            // because we render on the XR rendertargets.
            InactiveSleepTime = TimeSpan.FromSeconds(0);

            xrDevice = new XRDevice("$safeprojectname$", this);
#endif
//+:cnd:noEmit
        }

        /// <inheritdoc/>
        protected override void Initialize()
        {
            base.Initialize();
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
//-:cnd:noEmit
#if WebXR
            var ms = Mouse.GetState();
            var ts = TouchPanel.GetState();
            TouchLocation tl = default;
            if (ts.Count > 0)
                tl = ts[0];

            if (ms.LeftButton == ButtonState.Pressed
            ||  tl.State == TouchLocationState.Pressed)
            {
                if (xrDevice.DeviceState == XRDeviceState.Disabled
                ||  xrDevice.DeviceState == XRDeviceState.NoPermissions)
                {
                    try
                    {
                        // Initialize XR device and Select VR/AR mode.
                        int beginSessionResult = xrDevice.BeginSessionAsync(XRSessionMode.VR);
                    }
                    catch (Exception ovre)
                    {
                        System.Diagnostics.Debug.WriteLine(ovre.Message);
                    }
                }
            }

            GamePadState touchControllerState = TouchController.GetState(TouchControllerType.Touch);
#endif
//+:cnd:noEmit

            base.Update(gameTime);
        }

        /// <inheritdoc/>
        protected override void Draw(GameTime gameTime)
        {
            float aspect = GraphicsDevice.Viewport.AspectRatio;
            Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 0.05f, 1000);

//-:cnd:noEmit
#if WebXR
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
                            if (rt == null)
                                continue;

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
#endif
//+:cnd:noEmit

            base.Draw(gameTime);
        }
    }
}
