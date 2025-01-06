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
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class $safeprojectname$Game : Game
    {
        GraphicsDeviceManager graphics;
        XRDevice xrDevice;
        SpriteBatch spriteBatch;

        public $safeprojectname$Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

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

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            xrDevice.BeginSessionAsync(XRSessionMode.VR);
            xrDevice.TrackFloorLevelAsync(true);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: Use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            MouseState mouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            GamePadState touchControllerState = TouchController.GetState(TouchControllerType.Touch);

            if (keyboardState.IsKeyDown(Keys.Escape) ||
                keyboardState.IsKeyDown(Keys.Back) ||
                gamePadState.Buttons.Back == ButtonState.Pressed)
            {
                try { Exit(); }
                catch (PlatformNotSupportedException) { /* ignore */ }
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            float aspect = GraphicsDevice.Viewport.AspectRatio;
            Matrix view = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, aspect, 0.05f, 1000);

            if (xrDevice.DeviceState == XRDeviceState.Enabled)
            {

                // Draw on XR headset.
                xrDevice.BeginFrame();
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

                        // TODO: Add your drawing code here
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

            // Draw on the backbuffer.
            GraphicsDevice.SetRenderTarget(null);

            // TODO: Add your drawing code here
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}
