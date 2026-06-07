using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
//-:cnd:noEmit
#if OculusOVR
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
#if OculusOVR
        XRDevice xrDevice;
#endif
//+:cnd:noEmit

        public Platform$safeprojectname$Game() : base()
        {
            // TODO: Add platform specific initialization logic here

//-:cnd:noEmit
#if OculusOVR
            // OVR requires at least DX feature level 11.0
            graphics.GraphicsProfile = GraphicsProfile.FL11_0;

            graphics.SynchronizeWithVerticalRetrace = false;
            IsFixedTimeStep = true;
            // 90Hz Frame rate for oculus
            TargetElapsedTime = TimeSpan.FromTicks(111111);

            // We don't care is the main window is Focuses or not
            // because we render on the VR rendertargets.
            InactiveSleepTime = TimeSpan.FromSeconds(0);

            xrDevice = new XRDevice("$safeprojectname$", this.Services);
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
#if OculusOVR
            if (xrDevice.DeviceState == XRDeviceState.Disabled)
            {
                try
                {
                    // Initialize VR device.
                    xrDevice.BeginSessionAsync(XRSessionMode.VR);
                }
                catch (Exception ovre)
                {
                    System.Diagnostics.Debug.WriteLine(ovre.Message);
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
//-:cnd:noEmit
#if OculusOVR
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

                    // Draw on PC screen.
                    GraphicsDevice.SetRenderTarget(null);
                    base.Draw(gameTime);

                    return;
                }
            }
#endif
//+:cnd:noEmit

            base.Draw(gameTime);
        }
    }
}
