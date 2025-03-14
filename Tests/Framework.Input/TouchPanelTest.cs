﻿using System;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform.Input.Touch;
using Kni.Tests.Framework;
using NUnit.Framework;

namespace Kni.Tests.Input
{
    [TestFixture]
    internal class TouchPanelTest
    {
        private TimeSpan GameTimeForFrame(int frameNo)
        {
            return TimeSpan.FromSeconds(frameNo / 60D);
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Game game = new Game();

            TouchPanel.WindowHandle  = game.Window.Handle;
            TouchPanel.DisplayWidth  = game.Window.ClientBounds.Width;
            TouchPanel.DisplayHeight = game.Window.ClientBounds.Height;
        }

        [TearDown]
        public void TearDown()
        {
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().TestReleaseAllTouches();
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            TouchPanel.GetState();
        }

        [Test]
        [Order(0)]
        public void InitiallyHasNoTouches()
        {
            var state = TouchPanel.GetState();

            Assert.AreEqual(0, state.Count);
            Assert.AreEqual(true, state.IsReadOnly);
        }

        [Test]
        [Order(1)]
        public void PressedStartsATouch()
        {
            var pos = new Vector2(100, 50);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(1, pos);

            var state = TouchPanel.GetState();

            Assert.AreEqual(1, state.Count);

            var touch = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, touch.State);
            Assert.AreEqual(pos, touch.Position);
        }

        [Test]
        [Order(2)]
        [Description("In XNA if you press then move your finger before GetState is called, the initial touch position is the latest position of the touch")]
        public void PressedThenMoveStartsATouchAtNewPosition()
        {
            var pos = new Vector2(101, 50);
            var pos2 = new Vector2(101, 100);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(1, pos);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddMovedEvent(1, pos2);

            var state = TouchPanel.GetState();

            Assert.AreEqual(1, state.Count);

            var touch = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, touch.State);
            Assert.AreEqual(pos2, touch.Position);
        }

        [Test]
        [Order(3)]
        public void MovedDoesntStartATouch(TouchLocationState providedState)
        {
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddMovedEvent(1, new Vector2(102, 50));

            var state = TouchPanel.GetState();

            Assert.AreEqual(0, state.Count);
        }

        [Test]
        [Order(3)]
        public void ReleasedDoesntStartATouch()
        {
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddReleasedEvent(1, new Vector2(102, 50));

            var state = TouchPanel.GetState();

            Assert.AreEqual(0, state.Count);
        }

        [Test]
        [Order(3)]
        public void CanceledDoesntStartATouch()
        {
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddCanceledEvent(1, new Vector2(102, 50));

            var state = TouchPanel.GetState();

            Assert.AreEqual(0, state.Count);
        }

        [Test]
        [Order(4)]
        public void PressedAgesToMovedAfterGetState()
        {
            var pos = new Vector2(103, 50);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(1, pos);

            var initialState = TouchPanel.GetState();
            var initialTouch = initialState[0];

            var state = TouchPanel.GetState();

            Assert.AreEqual(1, state.Count);

            var touch = state[0];
            Assert.AreEqual(TouchLocationState.Moved, touch.State);
            Assert.AreEqual(pos, touch.Position);
            Assert.AreEqual(initialTouch.Id, touch.Id);
        }

        [Test]
        [Order(5)]
        public void MovingTouchUpdatesPosition()
        {
            var pos1 = new Vector2(104, 50);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(1, pos1);

            var state = TouchPanel.GetState();
            Assert.AreEqual(1, state.Count);

            var touch = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, touch.State);
            Assert.AreEqual(pos1, touch.Position);

            var pos2 = new Vector2(104, 50);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddMovedEvent(1, pos2);

            state = TouchPanel.GetState();
            Assert.AreEqual(1, state.Count);

            touch = state[0];
            Assert.AreEqual(TouchLocationState.Moved, touch.State);
            Assert.AreEqual(pos2, touch.Position); //Location should be updated
        }

        [Test]
        [Order(6)]
        [TestCase(true)]
        [TestCase(false)]
        public void ReleasingTouchMakesItGoAway(bool moveInBetween)
        {
            //Touch the screen, we should get one touch with the given location in the pressed state
            var pos = new Vector2(105, 50);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(1, pos);
            
            var state = TouchPanel.GetState();
            Assert.AreEqual(1, state.Count);

            var initialTouch = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, initialTouch.State);
            Assert.AreEqual(pos, initialTouch.Position);

            //Now optionally move the touch, should give the same touch in the new location in the moved state
            if (moveInBetween)
            {
                var pos2 = new Vector2(105, 100);
                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddMovedEvent(1, pos2);

                var movedState = TouchPanel.GetState();
                Assert.AreEqual(1, movedState.Count);

                var touch = movedState[0];
                Assert.AreEqual(TouchLocationState.Moved, touch.State);
                Assert.AreEqual(pos2, touch.Position);
                Assert.AreEqual(initialTouch.Id, touch.Id);
            }

            //Release the touch, it should then show up as released touch
            var pos3 = new Vector2(105, 150);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddReleasedEvent(1, pos3);

            var endState = TouchPanel.GetState();
            Assert.AreEqual(1, endState.Count);

            var endTouch = endState[0];
            Assert.AreEqual(TouchLocationState.Released, endTouch.State);
            Assert.AreEqual(pos3, endTouch.Position);
            Assert.AreEqual(initialTouch.Id, endTouch.Id);

            //Finally get the TouchState again, we should now have no touches
            var finalState = TouchPanel.GetState();
            Assert.AreEqual(0, finalState.Count);
        }

        [Test]
        [Order(7)]
        [TestCase(true)]
        [TestCase(false)]
        [Description("In XNA if you press and release your finger over multiple frames where GetState is not called, the touch never shows up")]
        public void TouchBetweenGetStateCallsMakesNoTouch(bool moveInBetween)
        {
            var pos = new Vector2(106, 50);
            var pos2 = new Vector2(106, 150);
            int frame = 0;

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(1, pos);
            if (moveInBetween) //Moving shouldn't change the behavior
            {
                Thread.Sleep(GameTimeForFrame(1));
                FrameworkDispatcher.Update();
                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddMovedEvent(1, pos2);
            }

            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddReleasedEvent(1, pos2);

            var state = TouchPanel.GetState();
            Assert.AreEqual(0, state.Count); //Should miss the touch that happened between
        }

        [Test]
        [Order(8)]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        [Description("If you press and release a touch on the same frame we need to show it as pressed that frame and released in the next")]
        public void SameFrameTouchAndReleaseMakesTouch(bool moveInBetween, bool waitAFrameForNextState)
        {
            var pos = new Vector2(107, 50);
            var pos2 = new Vector2(107, 150);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(1, pos);
            if (moveInBetween) //Moving shouldn't change the behavior
                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddMovedEvent(1, pos2);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddReleasedEvent(1, pos2);

            var state = TouchPanel.GetState();
            Assert.AreEqual(1, state.Count); //Should get the touch that happened between

            var touch = state[0];
            Assert.AreEqual(pos2, touch.Position);
            Assert.AreEqual(TouchLocationState.Pressed, touch.State);

            if (waitAFrameForNextState)
            {
                Thread.Sleep(GameTimeForFrame(1));
                FrameworkDispatcher.Update();
            }

            state = TouchPanel.GetState();
            Assert.AreEqual(1, state.Count); //Touch should still be there, but as released

            touch = state[0];
            Assert.AreEqual(pos2, touch.Position);
            Assert.AreEqual(TouchLocationState.Released, touch.State);


            if (waitAFrameForNextState)
            {
                Thread.Sleep(GameTimeForFrame(1));
                FrameworkDispatcher.Update();
            }
           
            state = TouchPanel.GetState();
            Assert.AreEqual(0, state.Count); //Touch should be gone now
        }

        [Test]
        [Order(9)]
        [TestCase(true)]
        [TestCase(false)]
        [Description("Press and release our finger on the same frame. Don't call GetState that frame, but do the next. We should not get the touch")]
        public void SameFrameTouchAndReleaseMissedIfWaitAFrameToGetState(bool moveInBetween)
        {
            var pos = new Vector2(108, 50);
            var pos2 = new Vector2(108, 150);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(1, pos);
            if (moveInBetween) //Moving shouldn't change the behavior
                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddMovedEvent(1, pos2);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddReleasedEvent(1, pos2);
            Thread.Sleep(GameTimeForFrame(1));
            FrameworkDispatcher.Update();
            var state = TouchPanel.GetState();
            Assert.AreEqual(0, state.Count); //Shouldn't get the touch that happened last frame
        }

        [Test]
        [Order(10)]
        [Description("Do multiple touches and check they behave as expected")]
        public void SimpleMultiTouchTest()
        {
            //Start with one touch
            var pos = new Vector2(109, 50);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(1, pos);

            var state = TouchPanel.GetState();
            Assert.AreEqual(1, state.Count);

            var initialTouch1 = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, initialTouch1.State);
            Assert.AreEqual(pos, initialTouch1.Position);


            //Start a second touch
            var pos2 = new Vector2(150, 100);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(2, pos2);

            state = TouchPanel.GetState();
            Assert.AreEqual(2, state.Count);

            //First touch should now be moved, same location
            var touch1 = state.First(x => x.Id == initialTouch1.Id);
            Assert.AreEqual(TouchLocationState.Moved, touch1.State);
            Assert.AreEqual(pos, touch1.Position);

            //Second touch should be pressed in its position
            var initialTouch2 = state.First(x => x.Id != initialTouch1.Id);
            Assert.AreEqual(TouchLocationState.Pressed, initialTouch2.State);
            Assert.AreEqual(pos2, initialTouch2.Position);


            //Move the second touch
            var pos3 = new Vector2(150, 150);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddMovedEvent(2, pos3);
            
            state = TouchPanel.GetState();
            Assert.AreEqual(2, state.Count);

            //touch1 should be the same
            touch1 = state.First(x => x.Id == initialTouch1.Id);
            Assert.AreEqual(TouchLocationState.Moved, touch1.State);
            Assert.AreEqual(pos, touch1.Position);

            //touch2 should be moved in its new location
            var touch2 = state.First(x => x.Id == initialTouch2.Id);
            Assert.AreEqual(TouchLocationState.Moved, touch2.State);
            Assert.AreEqual(pos3, touch2.Position);


            //Release the second touch
            var pos4 = new Vector2(150, 200);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddReleasedEvent(2, pos4);

            state = TouchPanel.GetState();
            Assert.AreEqual(2, state.Count);

            //touch1 should be the same
            touch1 = state.First(x => x.Id == initialTouch1.Id);
            Assert.AreEqual(TouchLocationState.Moved, touch1.State);
            Assert.AreEqual(pos, touch1.Position);

            //touch2 should be released in its new location
            touch2 = state.First(x => x.Id == initialTouch2.Id);
            Assert.AreEqual(TouchLocationState.Released, touch2.State);
            Assert.AreEqual(pos4, touch2.Position);


            //Move the first touch, second touch shouldn't be there any more
            var pos5 = new Vector2(100, 200);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddMovedEvent(1, pos5);

            state = TouchPanel.GetState();
            Assert.AreEqual(1, state.Count);

            //touch1 should be moved to the new position
            touch1 = state.First(x => x.Id == initialTouch1.Id);
            Assert.AreEqual(TouchLocationState.Moved, touch1.State);
            Assert.AreEqual(pos5, touch1.Position);

            //No more touch2


            //Release the first touch
            var pos6 = new Vector2(100, 250);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddReleasedEvent(1, pos6);

            state = TouchPanel.GetState();
            Assert.AreEqual(1, state.Count);

            //touch1 should be released at the new position
            touch1 = state.First(x => x.Id == initialTouch1.Id);
            Assert.AreEqual(TouchLocationState.Released, touch1.State);
            Assert.AreEqual(pos6, touch1.Position);


            //Now we should have no touches
            state = TouchPanel.GetState();
            Assert.AreEqual(0, state.Count);
        }

        [Test]
        [Order(11)]
        [Description("Internally TouchPanelState uses a queue of TouchLocation updating state. If it gets longer than 100, it throws away the first ones, which is bad")]
        public void TooManyEventsLosesOldOnes()
        {
            //To test this, we will start a touch, read the state.
            //Then release the touch, start a new touch and move it around lots
            //Then read the state, the first touch should be released

            //Start a touch
            var pos = new Vector2(1);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(1, pos);

            var state = TouchPanel.GetState();
            Assert.AreEqual(1, state.Count);

            var initialTouch = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, initialTouch.State);
            Assert.AreEqual(pos, initialTouch.Position);


            //Release the touch, make a new one and move it around lots
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddReleasedEvent(1, pos);

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(2, new Vector2(2));
            for (var i = 3; i < 200; i++)
                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddMovedEvent(2, new Vector2(i));

            //We should now have the first touch in the release state and the second touch in the pressed state at 199,199
            state = TouchPanel.GetState();
            Assert.AreEqual(2, state.Count);

            var newInitialTouch = state.First(x => x.Id == initialTouch.Id);
            Assert.AreEqual(TouchLocationState.Released, newInitialTouch.State);
            Assert.AreEqual(pos, newInitialTouch.Position);

            var secondTouch = state.First(x => x.Id != initialTouch.Id);
            Assert.AreEqual(TouchLocationState.Pressed, secondTouch.State);
            Assert.AreEqual(new Vector2(199), secondTouch.Position);
        }

        [Test]
        [Order(12)]
        [TestCase(false)]
        [TestCase(true)]
        public void ReleaseAllTouchesTest(bool testBetween)
        {
            //Create multiple touches in different states

            //Start a touch
            Vector2 pos = new Vector2(2);
            Vector2 pos2 = new Vector2(3);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(1, pos);

            var state = TouchPanel.GetState();
            Assert.AreEqual(1, state.Count);

            TouchLocation initialTouch = state[0];
            Assert.AreEqual(TouchLocationState.Pressed, initialTouch.State);
            Assert.AreEqual(pos, initialTouch.Position);

            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(2, pos2);

            if (testBetween)
            {
                state = TouchPanel.GetState();
                Assert.AreEqual(2, state.Count);
                TouchLocation touch = state.First(x => x.Id == initialTouch.Id);
                TouchLocation touch2 = state.First(x => x.Id != initialTouch.Id);

                Assert.AreEqual(TouchLocationState.Moved, touch.State);
                Assert.AreEqual(TouchLocationState.Pressed, touch2.State);
            }

            // Release touches
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(1, pos);
            ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddPressedEvent(2, pos2);

            //If we saw the second touch happen then we should see it be released, otherwise it will be in pressed, then in released next time
            state = TouchPanel.GetState();
            Assert.AreEqual(2, state.Count);

            Assert.AreEqual(testBetween ? TouchLocationState.Released : TouchLocationState.Pressed, state.Single(p => p.Id != initialTouch.Id).State);
            Assert.AreEqual(TouchLocationState.Released, state.Single(p => p.Id == initialTouch.Id).State);

            if (!testBetween)
            {
                state = TouchPanel.GetState();
                Assert.AreEqual(1, state.Count);

                Assert.AreEqual(TouchLocationState.Released, state[0].State);
            }

            //Then it should be empty
            state = TouchPanel.GetState();
            Assert.AreEqual(0, state.Count);
        }
    }
}
