// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Platform.Input.Touch
{
    public abstract partial class TouchPanelStrategy
    {
        /// The value 1 is reserved for the mouse touch point.
        private const int StartingTouchId = 2;

        /// <summary>
        /// The current touch state.
        /// </summary>
        private readonly List<TouchLocationData> _touchStates = new List<TouchLocationData>();

        /// <summary>
        /// The next touch location identifier.
        /// </summary>
        private int _nextTouchId = StartingTouchId;

        private TimeSpan _currentTimestamp;
        private int _currentFramestamp;

        /// <summary>
        /// The current timestamp that we use for setting the timestamp of new TouchLocations
        /// </summary>
        internal TimeSpan CurrentTimestamp
        {
            get { return _currentTimestamp; }
        }

        private void UpdateCurrentTimestamp()
        {
            _currentTimestamp = _stopwatch.Elapsed;
            _currentFramestamp++;
        }

        /// <summary>
        /// The mapping between platform specific touch ids
        /// and the touch ids we assign to touch locations.
        /// 
        /// Different platforms return different touch identifiers
        /// based on the specifics of their implementation and the
        /// system drivers.
        ///
        /// Sometimes these ids are suitable for our use, but other
        /// times it can recycle ids or do cute things like return
        /// the same id for double tap events.
        ///
        /// We instead provide consistent ids by generating them
        /// ourselves on the press and looking them up on move 
        /// and release events.
        /// </summary>
        private readonly Dictionary<int, int> _touchIdsMap = new Dictionary<int, int>();


        private TouchCollection LegacyGetState()
        {
            int currentFramestamp = this._currentFramestamp;

            //Clear out touches from previous frames that were released on the same frame they were touched that haven't been seen
            for (int i = _touchStates.Count - 1; i >= 0; i--)
            {
                TouchLocationData touch = _touchStates[i];

                //If a touch was pressed and released in a previous frame and the user didn't ask about it then trash it.
                if (touch.SameFrameReleased
                &&  touch.State == TouchLocationState.Pressed
                &&  touch.Framestamp < currentFramestamp)
                {
                    _touchStates.RemoveAt(i);
                }
            }

            TouchCollection result = (_touchStates.Count > 0) ? new TouchCollection(_touchStates) : TouchCollection.Empty;

            // Age all the touches, so any that were Pressed become Moved, and any that were Released are removed
            for (int i = _touchStates.Count - 1; i >= 0; i--)
            {
                TouchLocationData touch = _touchStates[i];
                switch (touch.State)
                {
                    case TouchLocationState.Pressed:
                        touch._previousState = touch.State;
                        touch._previousPosition = touch.Position;
                        if (touch.SameFrameReleased)
                            touch._state = TouchLocationState.Released;
                        else
                            touch._state = TouchLocationState.Moved;
                        _touchStates[i] = touch;
                        break;
                    case TouchLocationState.Moved:
                        touch._previousState = touch.State;
                        touch._previousPosition = touch.Position;
                        _touchStates[i] = touch;
                        break;
                    case TouchLocationState.Released:
                        _touchStates.RemoveAt(i);
                        break;
                }
            }

            return result;
        }

        protected void AddPressedEvent(int nativeTouchId, Vector2 position, Point winSize)
        {
            // Register touchId.
            int touchId;
            if (_touchIdsMap.TryGetValue(nativeTouchId, out touchId))
            {
                System.Diagnostics.Debug.Assert(false, "nativeTouchId already registered.");
            }
            else
            {
                touchId = _nextTouchId;
                _touchIdsMap[nativeTouchId] = touchId;

                if (_nextTouchId < int.MaxValue)
                    _nextTouchId++;
                else
                    _nextTouchId = StartingTouchId;
            }

            // scale position
            position.X = position.X * DisplayWidth / winSize.X;
            position.Y = position.Y * DisplayHeight / winSize.Y;

            TimeSpan currentTimestamp = this.CurrentTimestamp;
            int currentFramestamp = this._currentFramestamp;

            // Add event
            System.Diagnostics.Debug.Assert(_touchStates.TrueForAll(t => t.Id != touchId));
            TouchLocationData evt = new TouchLocationData(touchId, TouchLocationState.Pressed, position, currentTimestamp, currentFramestamp);
            _touchStates.Add(evt);

            // If we have gestures enabled then collect events for those too.
            // We also have to keep tracking any touches while we know about touches so we don't miss releases even if gesture recognition is disabled
            GesturesAddPressedEvent(touchId, position, currentTimestamp, currentFramestamp);
        }

        protected void AddMovedEvent(int nativeTouchId, Vector2 position, Point winSize)
        {
            // Find touchId.
            int touchId;
            if (!_touchIdsMap.TryGetValue(nativeTouchId, out touchId))
            {
                System.Diagnostics.Debug.Assert(false, "nativeTouchId not found.");
                return;
            }

            // scale position
            position.X = position.X * DisplayWidth / winSize.X;
            position.Y = position.Y * DisplayHeight / winSize.Y;

            TimeSpan currentTimestamp = this.CurrentTimestamp;
            int currentFramestamp = this._currentFramestamp;

            //Find the matching touch
            int tidx = FindTouchStateIndex(touchId);
            if (tidx != -1)
            {
                TouchLocationData existingTouch = _touchStates[tidx];
                {
                    // Update the touch based on the new one
                    System.Diagnostics.Debug.Assert(existingTouch.State != TouchLocationState.Released, "We shouldn't be changing state on a released location.");
                    System.Diagnostics.Debug.Assert(existingTouch.Timestamp <= currentTimestamp, "The currentTimestamp is older than our TouchLocationData.");

                    // Store the current state as the previous one.
                    existingTouch._previousState = existingTouch.State;
                    existingTouch._previousPosition = existingTouch.Position;

                    // Set the new state.
                    existingTouch._position = position;

                    // Set the new timestamp.
                    existingTouch._timestamp = currentTimestamp;
                    existingTouch._framestamp = currentFramestamp;

                    _touchStates[tidx] = existingTouch;
                }
            }

            // If we have gestures enabled then collect events for those too.
            // We also have to keep tracking any touches while we know about touches so we don't miss releases even if gesture recognition is disabled
            GesturesAddMovedEvent(touchId, position, currentTimestamp, currentFramestamp);
        }

        protected void AddReleasedEvent(int nativeTouchId, Vector2 position, Point winSize)
        {
            // Find and unregister touchId.
            int touchId;
            if (!_touchIdsMap.TryGetValue(nativeTouchId, out touchId))
            {
                System.Diagnostics.Debug.Assert(false, "nativeTouchId not found.");
                return;
            }
            _touchIdsMap.Remove(nativeTouchId);

            // scale position
            position.X = position.X * DisplayWidth / winSize.X;
            position.Y = position.Y * DisplayHeight / winSize.Y;

            TimeSpan currentTimestamp = this.CurrentTimestamp;
            int currentFramestamp = this._currentFramestamp;

            //Find the matching touch
            int tidx = FindTouchStateIndex(touchId);
            if (tidx != -1)
            {
                TouchLocationData existingTouch = _touchStates[tidx];
                {
                    //If we are moving straight from Pressed to Released and we've existed for multiple frames,
                    // that means we've never been seen, so just get rid of us
                    if (existingTouch.State == TouchLocationState.Pressed
                    && existingTouch.Framestamp != currentFramestamp)
                    {
                        _touchStates.RemoveAt(tidx);
                    }
                    else
                    {
                        //Otherwise update the touch based on the new one
                        System.Diagnostics.Debug.Assert(existingTouch.State != TouchLocationState.Released, "We shouldn't be changing state on a released location.");
                        System.Diagnostics.Debug.Assert(existingTouch.Timestamp <= currentTimestamp, "The currentTimestamp is older than our TouchLocationData.");

                        // Store the current state as the previous one.
                        existingTouch._previousState = existingTouch.State;
                        existingTouch._previousPosition = existingTouch.Position;

                        // Set the new state.
                        existingTouch._state = TouchLocationState.Released;
                        existingTouch._position = position;

                        //Going straight from pressed to released on the same frame
                        if (existingTouch._previousState == TouchLocationState.Pressed)
                        {
                            if (existingTouch.Framestamp == currentFramestamp)
                            {
                                //Lie that we are pressed for now
                                existingTouch.SameFrameReleased = true;
                                existingTouch._state = TouchLocationState.Pressed;
                            }
                        }

                        // Set the new timestamp.
                        existingTouch._timestamp = currentTimestamp;
                        existingTouch._framestamp = currentFramestamp;

                        _touchStates[tidx] = existingTouch;
                    }
                }
            }

            // If we have gestures enabled then collect events for those too.
            // We also have to keep tracking any touches while we know about touches so we don't miss releases even if gesture recognition is disabled
            GesturesAddReleasedEvent(touchId, position, currentTimestamp, currentFramestamp);
        }

        protected void AddCanceledEvent(int nativeTouchId, Vector2 position, Point winSize)
        {
            // Find and unregister touchId.
            int touchId;
            if (!_touchIdsMap.TryGetValue(nativeTouchId, out touchId))
            {
                System.Diagnostics.Debug.Assert(false, "nativeTouchId not found.");
                return;
            }
            _touchIdsMap.Remove(nativeTouchId);

            throw new NotImplementedException();
        }

        private int FindTouchStateIndex(int touchId)
        {
            for (int tidx = 0; tidx < _touchStates.Count; tidx++)
            {
                if (_touchStates[tidx].Id == touchId)
                    return tidx;
            }
            return -1;
        }

    }
}
