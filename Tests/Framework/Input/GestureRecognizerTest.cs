using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform.Input.Touch;
using MonoGame.Tests.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Input
{
    /// <summary>
    /// Tests the gesture recognition of the TouchPanelState class. (This will be split out in to another class in the future)
    /// </summary>
    [TestFixture]
    class GestureRecognizerTest
    {
        private const GestureType AllDrags = GestureType.DragComplete | GestureType.FreeDrag | GestureType.HorizontalDrag | GestureType.VerticalDrag;
        private const GestureType AllGestures =
            GestureType.DoubleTap | GestureType.DragComplete | GestureType.Flick | GestureType.FreeDrag | GestureType.Hold |
            GestureType.HorizontalDrag | GestureType.Pinch | GestureType.PinchComplete | GestureType.Tap | GestureType.VerticalDrag;

        private TimeSpan GameTimeForFrame(int frameNo)
        {
            return TimeSpan.FromSeconds(frameNo / 60D);
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GameWindow gameWindow = new MockWindow();

#if DESKTOPGL
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<ConcreteTouchPanel>().PrimaryWindow = gameWindow;
#endif
            TouchPanel.WindowHandle = gameWindow.Handle;
            TouchPanel.DisplayWidth = gameWindow.ClientBounds.Width;
            TouchPanel.DisplayHeight = gameWindow.ClientBounds.Height;
        }

        [TearDown]
        public void TearDown()
        {
            TouchPanel.EnabledGestures = GestureType.None;

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().InvalidateTouches();
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            TouchPanel.GetState();
        }

        [Test]
        [Order(0)]
        public void DoingNothingMakesNoGestures()
        {
            TouchPanel.EnabledGestures = AllGestures;

            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.False(TouchPanel.IsGestureAvailable);
        }

        [Test]
        [Order(1)]
        public void BasicTapGesture()
        {
            TouchPanel.EnabledGestures = GestureType.Tap;
            var pos = new Vector2(100, 150);

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.False(TouchPanel.IsGestureAvailable);

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Released, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.True(TouchPanel.IsGestureAvailable);
            var gesture = TouchPanel.ReadGesture();
            Assert.False(TouchPanel.IsGestureAvailable);

            Assert.AreEqual(GestureType.Tap, gesture.GestureType);
            Assert.AreEqual(pos, gesture.Position);
        }

        [Test]
        [Order(2)]
        [TestCase(true), TestCase(false)]
        public void BasicDoubleTapGesture(bool enableTap)
        {
            GestureSample gesture;

            TouchPanel.EnabledGestures = GestureType.DoubleTap;
            if (enableTap)
                TouchPanel.EnabledGestures |= GestureType.Tap;
            var pos = new Vector2(100, 150);

            //Do a first tap
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.False(TouchPanel.IsGestureAvailable);

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Released, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Will make a tap event if tap is enabled
            if (enableTap)
            {
                Assert.True(TouchPanel.IsGestureAvailable);
                gesture = TouchPanel.ReadGesture();
                Assert.False(TouchPanel.IsGestureAvailable);

                Assert.AreEqual(GestureType.Tap, gesture.GestureType);
                Assert.AreEqual(pos, gesture.Position);
            }
            else
            {
                Assert.False(TouchPanel.IsGestureAvailable);
            }

            //Now do the second tap in the same location, this will make a double tap on press (but no tap)
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(2, TouchLocationState.Pressed, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.True(TouchPanel.IsGestureAvailable);
            gesture = TouchPanel.ReadGesture();
            Assert.False(TouchPanel.IsGestureAvailable);

            Assert.AreEqual(GestureType.DoubleTap, gesture.GestureType);
            Assert.AreEqual(pos, gesture.Position);

            //This release should make no gestures
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(2, TouchLocationState.Released, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.False(TouchPanel.IsGestureAvailable);
        }

        [Test]
        [Order(3)]
        [Description("Do 2 quick taps, but make the second tap not near the first. Should not make a double tap")]
        public void DoubleTapTooFar()
        {
            TouchPanel.EnabledGestures = GestureType.DoubleTap;
            var pos1 = new Vector2(100, 150);
            var pos2 = new Vector2(100, 150 + TouchPanelStrategy.TapJitterTolerance + 1);

            //Do a first tap
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, pos1);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.False(TouchPanel.IsGestureAvailable);

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Released, pos1);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Now do the second tap in a different location
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(2, TouchLocationState.Pressed, pos2);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Shouldn't make a double tap
            Assert.False(TouchPanel.IsGestureAvailable);
        }

        [Test]
        [TestCase(GestureType.None), TestCase(GestureType.FreeDrag | GestureType.DragComplete)]
        [Description("Hold a finger down, then perform a tap and double tap with another, this should not make any tap gestures")]
        public void MultiFingerTap(GestureType otherEnabledGestures)
        {
            //TODO: This test is based on current behavior. We need to verify that XNA behaves the same
            //TODO: Need a test for how pinch and tap interact

            TouchPanel.EnabledGestures = otherEnabledGestures | GestureType.Tap | GestureType.DoubleTap;
            var pos = new Vector2(100, 150);

            //Place a finger down, this finger will never be released
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, new Vector2(10));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.False(TouchPanel.IsGestureAvailable);

            //Place a new finger down for a tap
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(2, TouchLocationState.Pressed, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.False(TouchPanel.IsGestureAvailable);

            //Release it, should not make a tap
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(2, TouchLocationState.Released, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.False(TouchPanel.IsGestureAvailable);

            //Press the finger down again, should not make a double tap
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(3, TouchLocationState.Pressed, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.False(TouchPanel.IsGestureAvailable);
        }

        [Test]
        [Description("Do 2 taps with a long time between. Should not make a double tap")]
        public void DoubleTapTooSlow()
        {
            TouchPanel.EnabledGestures = GestureType.DoubleTap;
            var pos = new Vector2(100, 150);

            //Do a first tap
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.False(TouchPanel.IsGestureAvailable);

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Released, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Now wait 500ms (we require it within 300ms)
            for (int frame = 3; frame < 33; frame++)
            {
                Thread.Sleep(GameTimeForFrame(1));
                FrameworkDispatcher.Update();
                Assert.False(TouchPanel.IsGestureAvailable);
            }

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(2, TouchLocationState.Pressed, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Shouldn't make a double tap
            Assert.False(TouchPanel.IsGestureAvailable);
        }

        [Test]
        public void BasicHold()
        {
            TouchPanel.EnabledGestures = GestureType.Hold;
            var pos = new Vector2(100, 150);

            //Place the finger down
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, pos);

            //We shouldn't generate the hold until the required time has passed
            TimeSpan gt = TimeSpan.Zero;
            do
            {
                Assert.False(TouchPanel.IsGestureAvailable);

                TimeSpan frameSpan = new TimeSpan(TouchPanelStrategy.TimeRequiredForHold.Ticks / 4);
                gt = gt + frameSpan;
                Thread.Sleep(frameSpan);
                FrameworkDispatcher.Update();
            } while (gt < TouchPanelStrategy.TimeRequiredForHold);

            //The last Update should have generated a hold
            Assert.True(TouchPanel.IsGestureAvailable);

            var gesture = TouchPanel.ReadGesture();
            Assert.AreEqual(GestureType.Hold, gesture.GestureType);
            Assert.AreEqual(pos, gesture.Position);

            Assert.False(TouchPanel.IsGestureAvailable);
        }

        [Test]
        [Description("Do a Tap, Double Tap, Hold using 2 taps. Should get gestures for each one")]
        public void TapDoubleTapHold()
        {
            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.DoubleTap | GestureType.Hold;

            var pos = new Vector2(100, 100);

            //Place the finger down
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.False(TouchPanel.IsGestureAvailable);

            //Release it, should make a tap
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Released, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.True(TouchPanel.IsGestureAvailable);

            var gesture = TouchPanel.ReadGesture();
            Assert.False(TouchPanel.IsGestureAvailable);
            Assert.AreEqual(GestureType.Tap, gesture.GestureType);

            //Place finger again, should make a double tap
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(2, TouchLocationState.Pressed, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.True(TouchPanel.IsGestureAvailable);

            gesture = TouchPanel.ReadGesture();
            Assert.False(TouchPanel.IsGestureAvailable);
            Assert.AreEqual(GestureType.DoubleTap, gesture.GestureType);

            //Now hold it for a while to make a hold gesture
            TimeSpan gt = TimeSpan.Zero;
            do
            {
                Assert.False(TouchPanel.IsGestureAvailable);

                TimeSpan frameSpan = new TimeSpan(TouchPanelStrategy.TimeRequiredForHold.Ticks / 4);
                gt = gt + frameSpan;
                Thread.Sleep(frameSpan);
                FrameworkDispatcher.Update();
            } 
            while (gt < (TouchPanelStrategy.TimeRequiredForHold));
            
            //The last Update should have generated a hold
            Assert.True(TouchPanel.IsGestureAvailable);
            gesture = TouchPanel.ReadGesture();
            Assert.False(TouchPanel.IsGestureAvailable);

            Assert.AreEqual(GestureType.Hold, gesture.GestureType);
        }

        [Test]
        [TestCase(AllDrags, GestureType.HorizontalDrag), TestCase(GestureType.HorizontalDrag, GestureType.HorizontalDrag)]
        [TestCase(AllDrags, GestureType.VerticalDrag), TestCase(GestureType.VerticalDrag, GestureType.VerticalDrag)]
        public void BasicDirectionalDrag(GestureType enabledGestures, GestureType direction)
        {
            TouchPanel.EnabledGestures = enabledGestures;
            var startPos = new Vector2(200, 200);
            Vector2 diffVec;

            if (direction == GestureType.HorizontalDrag)
                diffVec = new Vector2(10, -1);
            else //Vertical
                diffVec = new Vector2(1, -10);

            //Place the finger down
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, startPos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Move it until it should have made a drag
            int diff = 0;
            int frame = 1;
            while (diff * 10 < TouchPanelStrategy.TapJitterTolerance)
            {
                Assert.False(TouchPanel.IsGestureAvailable);

                diff ++;
                frame++;

                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Moved, startPos + diff * diffVec);
                Thread.Sleep(GameTimeForFrame(1));
                FrameworkDispatcher.Update();
            }

            //We should have a gesture now
            Assert.True(TouchPanel.IsGestureAvailable);
            var gesture = TouchPanel.ReadGesture();
            Assert.False(TouchPanel.IsGestureAvailable);

            //Should get the correct type at the new touch location, with the given delta
            Assert.AreEqual(direction, gesture.GestureType);
            Assert.AreEqual(startPos + diff * diffVec, gesture.Position);

            //Delta has only movement in the direction of the drag
            if (direction == GestureType.HorizontalDrag)
                Assert.AreEqual(new Vector2(10, 0), gesture.Delta);
            else //Vertical
                Assert.AreEqual(new Vector2(0, -10), gesture.Delta);

            //If all gestures are enabled (DragComplete is enabled), releasing our touch will generate a DragComplete gesture
            frame++;
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Released, startPos + diff * diffVec);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            if (enabledGestures == AllDrags)
            {
                Assert.True(TouchPanel.IsGestureAvailable);
                gesture = TouchPanel.ReadGesture();
                Assert.False(TouchPanel.IsGestureAvailable);

                Assert.AreEqual(GestureType.DragComplete, gesture.GestureType);
                Assert.AreEqual(Vector2.Zero, gesture.Position); //This is (0,0) in XNA too. It's weird though!
            }
            else
            {
                Assert.False(TouchPanel.IsGestureAvailable);
            }
        }

        [Test]
        [TestCase(AllDrags), TestCase(GestureType.FreeDrag | GestureType.DragComplete), TestCase(GestureType.FreeDrag)]
        [Description("Drag on an angle, it should generate a FreeDrag event, not a directional one")]
        public void BasicFreeDragTest(GestureType enabledGestures)
        {
            TouchPanel.EnabledGestures = enabledGestures;
            var startPos = new Vector2(200, 200);

            //Place the finger down
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, startPos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Move it until it should have made a drag
            int diff = 0;
            int frame = 1;
            while (new Vector2(diff).Length() < TouchPanelStrategy.TapJitterTolerance)
            {
                Assert.False(TouchPanel.IsGestureAvailable);

                diff += 5;
                frame++;

                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(diff));
                Thread.Sleep(GameTimeForFrame(1));
                FrameworkDispatcher.Update();
            }

            //We should have a gesture now
            Assert.True(TouchPanel.IsGestureAvailable);
            var gesture = TouchPanel.ReadGesture();
            Assert.False(TouchPanel.IsGestureAvailable);

            //Should get the correct type at the new touch location, with the given delta
            Assert.AreEqual(GestureType.FreeDrag, gesture.GestureType);
            Assert.AreEqual(startPos + new Vector2(diff), gesture.Position);

            Assert.AreEqual(new Vector2(5), gesture.Delta);

            //If DragComplete is enabled, releasing our touch will generate a DragComplete gesture
            frame++;
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Released, startPos + new Vector2(diff));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            if ((enabledGestures & GestureType.DragComplete) == GestureType.DragComplete)
            {
                Assert.True(TouchPanel.IsGestureAvailable);
                gesture = TouchPanel.ReadGesture();
                Assert.False(TouchPanel.IsGestureAvailable);

                Assert.AreEqual(GestureType.DragComplete, gesture.GestureType);
                Assert.AreEqual(Vector2.Zero, gesture.Position); //This is (0,0) in XNA too. It's weird though!
            }
            else
            {
                Assert.False(TouchPanel.IsGestureAvailable);
            }
        }

        [Test]
        [Description("If the user does a horizontal drag, this should be picked up as a free drag instead if horizontal is not enabled")]
        public void FreeDragIfDirectionalDisabled()
        {
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.VerticalDrag;
            var startPos = new Vector2(200, 200);

            //Place the finger down
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, startPos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Move it until it should have made a drag
            int diff = 0;
            int frame = 1;
            while (new Vector2(diff, 0).Length() < TouchPanelStrategy.TapJitterTolerance)
            {
                Assert.False(TouchPanel.IsGestureAvailable);

                diff += 5;
                frame++;

                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(diff, 0));
                Thread.Sleep(GameTimeForFrame(1));
                FrameworkDispatcher.Update();
            }

            //We should have a gesture now
            Assert.True(TouchPanel.IsGestureAvailable);
            var gesture = TouchPanel.ReadGesture();
            Assert.False(TouchPanel.IsGestureAvailable);

            //Should get the correct type at the new touch location, with the given delta
            Assert.AreEqual(GestureType.FreeDrag, gesture.GestureType);
            Assert.AreEqual(startPos + new Vector2(diff, 0), gesture.Position);

            Assert.AreEqual(new Vector2(5, 0), gesture.Delta);
        }

        [Test]
        [Description("Start a drag then disable gestures. Gestures events should just stop, no DragComplete gesture should happen")]
        public void DisableGesturesWhileDragging()
        {
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;
            var startPos = new Vector2(200, 200);


            //Place the finger down
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, startPos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.False(TouchPanel.IsGestureAvailable);

            //Drag it, should get a drag
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(40, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.True(TouchPanel.IsGestureAvailable);
            var gesture = TouchPanel.ReadGesture();
            Assert.False(TouchPanel.IsGestureAvailable);
            Assert.AreEqual(GestureType.FreeDrag, gesture.GestureType);

            //Disable gestures
            TouchPanel.EnabledGestures = GestureType.None;

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(80, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.False(TouchPanel.IsGestureAvailable);

            //Release that touch, should make no gesture
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Released, startPos + new Vector2(80, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.False(TouchPanel.IsGestureAvailable);


            //Enable both gestures again, just place the finger down and release it
            //Should make no gesture

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(2, TouchLocationState.Pressed, startPos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.False(TouchPanel.IsGestureAvailable);

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(2, TouchLocationState.Released, startPos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.False(TouchPanel.IsGestureAvailable);
        }

        [Test]
        [Description("Start a drag then disable gestures. Re-Enable them without releasing the finger. Releasing it then should make a DragComplete")]
        public void DisableGesturesWhileDragging2()
        {
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;
            var startPos = new Vector2(200, 200);


            //Place the finger down
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, startPos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.False(TouchPanel.IsGestureAvailable);

            //Drag it, should get a drag
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(40, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.True(TouchPanel.IsGestureAvailable);
            var gesture = TouchPanel.ReadGesture();
            Assert.False(TouchPanel.IsGestureAvailable);
            Assert.AreEqual(GestureType.FreeDrag, gesture.GestureType);

            //Disable gestures
            TouchPanel.EnabledGestures = GestureType.None;

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(80, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.False(TouchPanel.IsGestureAvailable);

            //Enable both gestures again, release the finger
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;

            //Release that touch, should make no gesture
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Released, startPos + new Vector2(80, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.True(TouchPanel.IsGestureAvailable);
            gesture = TouchPanel.ReadGesture();
            Assert.False(TouchPanel.IsGestureAvailable);

            Assert.AreEqual(GestureType.DragComplete, gesture.GestureType);
        }

        [Test]
        [Description("Start a drag then disable gestures. Release finger and replace, then re-enable gestures and release. Should not make a DragComplete")]
        public void DisableGesturesWhileDragging3()
        {
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;
            var startPos = new Vector2(200, 200);

            //Place the finger down
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, startPos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.False(TouchPanel.IsGestureAvailable);

            //Drag it, should get a drag
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(40, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.True(TouchPanel.IsGestureAvailable);
            var gesture = TouchPanel.ReadGesture();
            Assert.False(TouchPanel.IsGestureAvailable);
            Assert.AreEqual(GestureType.FreeDrag, gesture.GestureType);

            //Disable gestures
            TouchPanel.EnabledGestures = GestureType.None;

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(80, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.False(TouchPanel.IsGestureAvailable);

            //Release the finger, should make no gesture (gestures are disabled)
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Released, startPos + new Vector2(80, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.False(TouchPanel.IsGestureAvailable);

            //Press it down again
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(2, TouchLocationState.Pressed, startPos + new Vector2(80, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.False(TouchPanel.IsGestureAvailable);

            //Enable both gestures again
            TouchPanel.EnabledGestures = GestureType.FreeDrag | GestureType.DragComplete;

            //Release the second touch, should make no gesture
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(2, TouchLocationState.Released, startPos + new Vector2(80, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.False(TouchPanel.IsGestureAvailable);
        }

        [Test]
        [Description("Enable the tap gesture while dragging with no gestures enabled. No gestures should happen")]
        public void EnableTapWhileDragging()
        {
            //Based on https://github.com/mono/MonoGame/pull/1543#issuecomment-15004057
            
            var pos = new Vector2(10, 10);

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, pos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Drag it a bit
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Moved, pos + new Vector2(40, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            TouchPanel.EnabledGestures = GestureType.Tap;

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Moved, pos + new Vector2(80, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.False(TouchPanel.IsGestureAvailable);
        }

        [Test]
        public void BasicFlick()
        {
            TouchPanel.EnabledGestures = GestureType.Flick;
            var startPos = new Vector2(200, 200);

            //Place the finger down
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, startPos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Move it until it should have made a flick
            int diff = 0;
            int frame = 1;
            while (new Vector2(diff, 0).Length() < TouchPanelStrategy.TapJitterTolerance)
            {
                Assert.False(TouchPanel.IsGestureAvailable);

                diff += 30;
                frame++;

                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(diff, 0));
                Thread.Sleep(GameTimeForFrame(1));
                FrameworkDispatcher.Update();
            }
            Assert.False(TouchPanel.IsGestureAvailable);

            //Now release
            frame++;
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Released, startPos + new Vector2(diff, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Now we should have the flick
            Assert.True(TouchPanel.IsGestureAvailable);
            var gesture = TouchPanel.ReadGesture();
            Assert.False(TouchPanel.IsGestureAvailable);

            Assert.AreEqual(GestureType.Flick, gesture.GestureType);
            Assert.AreEqual(Vector2.Zero, gesture.Position);
            //Could check Delta here, it contains the flick velocity
        }

        [Test]
        [Description("Do a short movement within TapJitterTolerance, this should not make a flick even if it is quick")]
        public void ShortMovementDoesntMakeAFlick()
        {
            TouchPanel.EnabledGestures = GestureType.Flick;
            var startPos = new Vector2(200, 200);

            //Place the finger down
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, startPos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Then release it at the edge of the detection size
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Released, startPos + new Vector2(TouchPanelStrategy.TapJitterTolerance, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //This should not make a flick. If the distance is 1 greater it will.
            Assert.False(TouchPanel.IsGestureAvailable);
        }


        [Test]
        [Description("If Flick and FreeDrag are enabled, both events should be generated without impacting each other. " +
                     "There should be a flick and a DragComplete at the end in that order")]
        public void FlickAndFreeDrag()
        {
            TouchPanel.EnabledGestures = GestureType.Flick | GestureType.FreeDrag | GestureType.DragComplete;
            var startPos = new Vector2(200, 200);
            GestureSample gesture;

            //Place the finger down
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, startPos);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Move it until it should have made a flick
            int diff = 0;
            int frame = 1;
            while (frame < 4)
            {
                diff += 40;
                frame++;

                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Moved, startPos + new Vector2(diff, 0));
                Thread.Sleep(GameTimeForFrame(1));
                FrameworkDispatcher.Update();

                //Each drag should make a FreeDrag
                Assert.True(TouchPanel.IsGestureAvailable);
                gesture = TouchPanel.ReadGesture();
                Assert.False(TouchPanel.IsGestureAvailable);

                Assert.AreEqual(GestureType.FreeDrag, gesture.GestureType);
                Assert.AreEqual(startPos + new Vector2(diff, 0), gesture.Position);
            }
            Assert.False(TouchPanel.IsGestureAvailable);

            //Now release
            frame++;
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Released, startPos + new Vector2(diff, 0));
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Now we should have the flick
            Assert.True(TouchPanel.IsGestureAvailable);
            gesture = TouchPanel.ReadGesture();

            Assert.AreEqual(GestureType.Flick, gesture.GestureType);
            Assert.AreEqual(Vector2.Zero, gesture.Position);
            
            //And then the DragComplete
            Assert.True(TouchPanel.IsGestureAvailable);
            gesture = TouchPanel.ReadGesture();

            Assert.AreEqual(GestureType.DragComplete, gesture.GestureType);
            Assert.AreEqual(Vector2.Zero, gesture.Position);

            //And that should be it
            Assert.False(TouchPanel.IsGestureAvailable);
        }

        [Test]
        public void BasicPinch()
        {
            //TODO: This test is based on current behavior. We need to verify that XNA behaves the same

            TouchPanel.EnabledGestures = GestureType.Pinch | GestureType.PinchComplete;

            var pos1 = new Vector2(200, 200);
            var pos2 = new Vector2(400, 200);

            //Place a finger down
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Pressed, pos1);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.False(TouchPanel.IsGestureAvailable);

            //Place the other finger down
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(2, TouchLocationState.Pressed, pos2);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            //Now we should have a pinch
            Assert.True(TouchPanel.IsGestureAvailable);
            var gesture = TouchPanel.ReadGesture();

            Assert.AreEqual(GestureType.Pinch, gesture.GestureType);
            Assert.AreEqual(pos1, gesture.Position);
            Assert.AreEqual(pos2, gesture.Position2);

            //If we do nothing, we shouldn't get more pinch events
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            Assert.False(TouchPanel.IsGestureAvailable);

            //But if we move a finger, we should get an updated pinch
            pos2 += new Vector2(50, 0);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(2, TouchLocationState.Moved, pos2);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.True(TouchPanel.IsGestureAvailable);
            gesture = TouchPanel.ReadGesture();

            Assert.AreEqual(GestureType.Pinch, gesture.GestureType);
            Assert.AreEqual(pos1, gesture.Position);
            Assert.AreEqual(pos2, gesture.Position2);

            //Now releasing one of the fingers should make a pinch complete event
            pos1 -= new Vector2(0, 50);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent(1, TouchLocationState.Released, pos1);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();

            Assert.True(TouchPanel.IsGestureAvailable);
            gesture = TouchPanel.ReadGesture();

            Assert.AreEqual(GestureType.PinchComplete, gesture.GestureType);
            Assert.AreEqual(Vector2.Zero, gesture.Position);
            Assert.AreEqual(Vector2.Zero, gesture.Position2);

            //We should have no more events
            Assert.False(TouchPanel.IsGestureAvailable);
        }
    }
}
