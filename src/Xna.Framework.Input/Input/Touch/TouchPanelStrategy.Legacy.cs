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
        /// The current gesture state.
        /// </summary>
        private readonly List<TouchLocationData> _gestureStates = new List<TouchLocationData>();

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
        /// </summary>
        private readonly Dictionary<int, int> _touchIdsMap = new Dictionary<int, int>();

        internal readonly Queue<GestureSample> GestureList = new Queue<GestureSample>();


        /// <summary>
        /// Returns true if a touch gesture is available.
        /// </summary>
        private bool LegacyIsGestureAvailable
        {
            get
            {
                // Process the pending gesture events. (May cause hold events)
                TimeSpan currentTimestamp = this.CurrentTimestamp;
                UpdateGestures(currentTimestamp, false);

                return GestureList.Count > 0;
            }
        }

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

        protected void LegacyAddEvent(int nativeTouchId, TouchLocationState state, Vector2 position, Point winSize)
        {
            switch (state)
            {
                case TouchLocationState.Pressed:
                    AddPressedEvent(nativeTouchId, position, winSize);
                    break;

                case TouchLocationState.Moved:
                    AddMovedEvent(nativeTouchId, position, winSize);
                    break;

                case TouchLocationState.Released:
                    AddReleasedEvent(nativeTouchId, position, winSize);
                    break;

                case TouchLocationState.Invalid:
                default:
                    throw new InvalidOperationException();
            }

        }

        private void AddPressedEvent(int nativeTouchId, Vector2 position, Point winSize)
        {
            // Different platforms return different touch identifiers
            // based on the specifics of their implementation and the
            // system drivers.
            //
            // Sometimes these ids are suitable for our use, but other
            // times it can recycle ids or do cute things like return
            // the same id for double tap events.
            //
            // We instead provide consistent ids by generating them
            // ourselves on the press and looking them up on move 
            // and release events.
            // 
            int touchId;
            // Try to find the touch id.
            if (!_touchIdsMap.TryGetValue(nativeTouchId, out touchId))
            {
                touchId = _nextTouchId;
                _touchIdsMap[nativeTouchId] = touchId;

                if (_nextTouchId < int.MaxValue)
                    _nextTouchId++;
                else
                    _nextTouchId = StartingTouchId;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
                // did we miss some event to clear old events?
            }

            // scale position
            position.X = position.X * ((float)DisplayWidth / winSize.X);
            position.Y = position.Y * ((float)DisplayHeight / winSize.Y);

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

        private void AddMovedEvent(int nativeTouchId, Vector2 position, Point winSize)
        {
            // Different platforms return different touch identifiers
            // based on the specifics of their implementation and the
            // system drivers.
            //
            // Sometimes these ids are suitable for our use, but other
            // times it can recycle ids or do cute things like return
            // the same id for double tap events.
            //
            // We instead provide consistent ids by generating them
            // ourselves on the press and looking them up on move 
            // and release events.
            // 
            int touchId;
            // Try to find the touch id.
            if (!_touchIdsMap.TryGetValue(nativeTouchId, out touchId))
            {
                System.Diagnostics.Debug.Assert(false);
                // If we got here that means either the device is sending
                // us bad, out of order, or old touch events.
                // In any case just ignore them.
                return;
            }

            // scale position
            position.X = position.X * (DisplayWidth / winSize.X);
            position.Y = position.Y * (DisplayHeight / winSize.Y);

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

                    // Update the velocity.
                    UpdateVelocity(currentTimestamp, ref existingTouch);

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

        private void AddReleasedEvent(int nativeTouchId, Vector2 position, Point winSize)
        {
            // Different platforms return different touch identifiers
            // based on the specifics of their implementation and the
            // system drivers.
            //
            // Sometimes these ids are suitable for our use, but other
            // times it can recycle ids or do cute things like return
            // the same id for double tap events.
            //
            // We instead provide consistent ids by generating them
            // ourselves on the press and looking them up on move 
            // and release events.
            // 
            int touchId;
            // Try to find the touch id.
            if (!_touchIdsMap.TryGetValue(nativeTouchId, out touchId))
            {
                System.Diagnostics.Debug.Assert(false);
                // If we got here that means either the device is sending
                // us bad, out of order, or old touch events.
                // In any case just ignore them.
                return;
            }

            // scale position
            position.X = position.X * (DisplayWidth / winSize.X);
            position.Y = position.Y * (DisplayHeight / winSize.Y);

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

                        // Update the velocity.
                        UpdateVelocity(currentTimestamp, ref existingTouch);

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

            // unmap the hardware id.
            _touchIdsMap.Remove(nativeTouchId);
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

        private int FindGestureStateIndex(int touchId)
        {
            for (int gidx = 0; gidx < _gestureStates.Count; gidx++)
            {
                if (_gestureStates[gidx].Id == touchId)
                    return gidx;
            }
            return -1;
        }        

        private static void UpdateVelocity(TimeSpan currentTimestamp, ref TouchLocationData existingTouch)
        {
            TimeSpan elapsed = currentTimestamp - existingTouch.Timestamp;
            // If time has elapsed then update the velocity.
            if (elapsed > TimeSpan.Zero)
            {
                // Use a simple low pass filter to accumulate velocity.
                Vector2 delta = existingTouch.Position - existingTouch._previousPosition;
                Vector2 velocity = delta / (float)elapsed.TotalSeconds;
                existingTouch._velocity += (velocity - existingTouch.Velocity) * 0.45f;
            }
        }

        /// <summary>
        /// Returns the next available gesture on touch panel device.
        /// </summary>
        /// <returns><see cref="GestureSample"/></returns>
        private GestureSample LegacyReadGesture()
        {
            // Return the next gesture.
            return GestureList.Dequeue();
        }

        #region Gesture Recognition

        private void GesturesAddPressedEvent(int touchId, Vector2 position, TimeSpan currentTimestamp, int currentFramestamp)
        {
            if (EnabledGestures != GestureType.None || _gestureStates.Count > 0)
            {
                TouchLocationData evt = new TouchLocationData(touchId, TouchLocationState.Pressed, position, currentTimestamp, currentFramestamp);
                _gestureStates.Add(evt);

                if (EnabledGestures != GestureType.None)
                    UpdateGestures(currentTimestamp, true);

                // Age all the touches, so any that were Pressed become Moved, and any that were Released are removed
                for (int i = _gestureStates.Count - 1; i >= 0; i--)
                {
                    TouchLocationData touch = _gestureStates[i];
                    switch (touch.State)
                    {
                        case TouchLocationState.Pressed:
                            touch._previousState = touch.State;
                            touch._previousPosition = touch.Position;
                            if (touch.SameFrameReleased)
                                touch._state = TouchLocationState.Released;
                            else
                                touch._state = TouchLocationState.Moved;
                            _gestureStates[i] = touch;
                            break;
                        case TouchLocationState.Moved:
                            touch._previousState = touch.State;
                            touch._previousPosition = touch.Position;
                            _gestureStates[i] = touch;
                            break;
                        case TouchLocationState.Released:
                            _gestureStates.RemoveAt(i);
                            break;
                    }
                }
            }
        }

        private void GesturesAddMovedEvent(int touchId, Vector2 position, TimeSpan currentTimestamp, int currentFramestamp)
        {            
            //Find the matching gesture
            int gidx = FindGestureStateIndex(touchId);
            if (gidx != -1)
            {
                TouchLocationData existingTouch = _gestureStates[gidx];
                {
                    // Update the touch based on the new one
                    System.Diagnostics.Debug.Assert(existingTouch.State != TouchLocationState.Released, "We shouldn't be changing state on a released location.");
                    System.Diagnostics.Debug.Assert(existingTouch.Timestamp <= currentTimestamp, "The currentTimestamp is older than our TouchLocationData.");

                    // Store the current state as the previous one.
                    existingTouch._previousPosition = existingTouch.Position;
                    existingTouch._previousState = existingTouch.State;

                    // Set the new state.
                    existingTouch._position = position;

                    // Update the velocity.
                    UpdateVelocity(currentTimestamp, ref existingTouch);

                    // Set the new timestamp.
                    existingTouch._timestamp = currentTimestamp;
                    existingTouch._framestamp = currentFramestamp;

                    _gestureStates[gidx] = existingTouch;
                }
            }

            if (EnabledGestures != GestureType.None || _gestureStates.Count > 0)
            {
                if (EnabledGestures != GestureType.None)
                    UpdateGestures(currentTimestamp, true);

                // Age all the touches, so any that were Pressed become Moved, and any that were Released are removed
                for (int i = _gestureStates.Count - 1; i >= 0; i--)
                {
                    TouchLocationData touch = _gestureStates[i];
                    switch (touch.State)
                    {
                        case TouchLocationState.Pressed:
                            touch._previousState = touch.State;
                            touch._previousPosition = touch.Position;
                            if (touch.SameFrameReleased)
                                touch._state = TouchLocationState.Released;
                            else
                                touch._state = TouchLocationState.Moved;
                            _gestureStates[i] = touch;
                            break;
                        case TouchLocationState.Moved:
                            touch._previousState = touch.State;
                            touch._previousPosition = touch.Position;
                            _gestureStates[i] = touch;
                            break;
                        case TouchLocationState.Released:
                            _gestureStates.RemoveAt(i);
                            break;
                    }
                }
            }
        }

        private void GesturesAddReleasedEvent(int touchId, Vector2 position, TimeSpan currentTimestamp, int currentFramestamp)
        {
            //Find the matching gesture
            int gidx = FindGestureStateIndex(touchId);
            if (gidx != -1)
            {
                TouchLocationData existingTouch = _gestureStates[gidx];
                {
                    //If we are moving straight from Pressed to Released and we've existed for multiple frames,
                    // that means we've never been seen, so just get rid of us
                    if (existingTouch.State == TouchLocationState.Pressed
                    &&  existingTouch.Framestamp != currentFramestamp)
                    {
                        _gestureStates.RemoveAt(gidx);
                    }
                    else
                    {
                        //Otherwise update the touch based on the new one
                        System.Diagnostics.Debug.Assert(existingTouch.State != TouchLocationState.Released, "We shouldn't be changing state on a released location.");
                        System.Diagnostics.Debug.Assert(existingTouch.Timestamp <= currentTimestamp, "The currentTimestamp is older than our TouchLocationData.");

                        // Store the current state as the previous one.
                        existingTouch._previousPosition = existingTouch.Position;
                        existingTouch._previousState = existingTouch.State;

                        // Set the new state.
                        existingTouch._position = position;
                        existingTouch._state = TouchLocationState.Released;

                        // Update the velocity.
                        UpdateVelocity(currentTimestamp, ref existingTouch);

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

                        _gestureStates[gidx] = existingTouch;
                    }
                }
            }

            if (EnabledGestures != GestureType.None || _gestureStates.Count > 0)
            {
                if (EnabledGestures != GestureType.None)
                    UpdateGestures(currentTimestamp, true);

                // Age all the touches, so any that were Pressed become Moved, and any that were Released are removed
                for (int i = _gestureStates.Count - 1; i >= 0; i--)
                {
                    TouchLocationData touch = _gestureStates[i];
                    switch (touch.State)
                    {
                        case TouchLocationState.Pressed:
                            touch._previousState = touch.State;
                            touch._previousPosition = touch.Position;
                            if (touch.SameFrameReleased)
                                touch._state = TouchLocationState.Released;
                            else
                                touch._state = TouchLocationState.Moved;
                            _gestureStates[i] = touch;
                            break;
                        case TouchLocationState.Moved:
                            touch._previousState = touch.State;
                            touch._previousPosition = touch.Position;
                            _gestureStates[i] = touch;
                            break;
                        case TouchLocationState.Released:
                            _gestureStates.RemoveAt(i);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Maximum distance a touch location can wiggle and 
        /// not be considered to have moved.
        /// </summary>
        internal const float TapJitterTolerance = 35.0f;

        internal static readonly TimeSpan TimeRequiredForHold = TimeSpan.FromMilliseconds(1024);

        /// <summary>
        /// The pinch touch locations.
        /// </summary>
        private readonly TouchLocationData[] _pinchTouch = new TouchLocationData[2];

        /// <summary>
        /// If true the pinch touch locations are valid and
        /// a pinch gesture has begun.
        /// </summary>
        private bool _pinchGestureStarted;

        private bool IsGestureEnabled(GestureType gestureType)
        {
            return (EnabledGestures & gestureType) != 0;
        }

        /// <summary>
        /// Used to disable emitting of tap gestures.
        /// </summary>
        bool _tapDisabled;

        /// <summary>
        /// Used to disable emitting of hold gestures.
        /// </summary>
        bool _holdDisabled;


        private void UpdateGestures(TimeSpan currentTimestamp, bool stateChanged)
        {
            // These are observed XNA gesture rules which we follow below.  Please
            // add to them if a new case is found.
            //
            //  - Tap occurs on release.
            //  - DoubleTap occurs on the first press after a Tap.
            //  - Tap, Double Tap, and Hold are disabled if a drag begins or more than one finger is pressed.
            //  - Drag occurs when one finger is down and actively moving.
            //  - Pinch occurs if 2 or more fingers are down and at least one is moving.
            //  - If you enter a Pinch during a drag a DragComplete is fired.
            //  - Drags are classified as horizontal, vertical, free, or none and stay that way.
            //

            // First get a count of touch locations which 
            // are not in the released state.
            int heldLocations = 0;
            foreach (TouchLocationData touch in _gestureStates)
                heldLocations += touch.State != TouchLocationState.Released ? 1 : 0;

            // As soon as we have more than one held point then 
            // tap and hold gestures are disabled until all the 
            // points are released.
            if (heldLocations > 1)
            {
                _tapDisabled = true;
                _holdDisabled = true;
            }

            // Process the touch locations for gestures.
            for (int i = 0; i < _gestureStates.Count; i++)
            {
                TouchLocationData touch = _gestureStates[i];
                switch (touch.State)
                {
                    case TouchLocationState.Pressed:
                    case TouchLocationState.Moved:
                        {
                            // The DoubleTap event is emitted on first press as
                            // opposed to Tap which happens on release.
                            if (touch.State == TouchLocationState.Pressed
                            &&  ProcessDoubleTap(ref touch))
                                break;

                            // Any time more than one finger is down and pinch is
                            // enabled then we exclusively do pinch processing.
                            if (IsGestureEnabled(GestureType.Pinch) && heldLocations > 1)
                            {
                                // Save or update the first pinch point.
                                if (_pinchTouch[0].State == TouchLocationState.Invalid
                                ||  _pinchTouch[0].Id == touch.Id)
                                    _pinchTouch[0] = touch;

                                // Save or update the second pinch point.
                                else if (_pinchTouch[1].State == TouchLocationState.Invalid
                                     ||  _pinchTouch[1].Id == touch.Id)
                                    _pinchTouch[1] = touch;

                                // NOTE: Actual pinch processing happens outside and
                                // below this loop to ensure both points are updated
                                // before gestures are emitted.
                                break;
                            }

                            // If we're not dragging try to process a hold event.
                            float sqDist = Vector2.DistanceSquared(touch.Position, touch.PressPosition);
                            if (_dragGestureStarted == GestureType.None && sqDist < TapJitterTolerance * TapJitterTolerance)
                            {
                                ProcessHold(currentTimestamp, ref touch);
                                break;
                            }

                            // If the touch state has changed then do a drag gesture.
                            if (stateChanged)
                                ProcessDrag(ref touch);
                            break;
                        }

                    case TouchLocationState.Released:
                        {
                            // If the touch state hasn't changed then this
                            // is an old release event... skip it.
                            if (!stateChanged)
                                break;

                            // If this is one of the pinch locations then we
                            // need to fire off the complete event and stop
                            // the pinch gesture operation.
                            if (_pinchGestureStarted
                            &&  (touch.Id == _pinchTouch[0].Id || touch.Id == _pinchTouch[1].Id))
                            {
                                if (IsGestureEnabled(GestureType.PinchComplete))
                                    GestureList.Enqueue(new GestureSample(
                                                            GestureType.PinchComplete, touch.Timestamp,
                                                            Vector2.Zero, Vector2.Zero,
                                                            Vector2.Zero, Vector2.Zero));

                                _pinchGestureStarted = false;
                                _pinchTouch[0] = TouchLocationData.Invalid;
                                _pinchTouch[1] = TouchLocationData.Invalid;
                                break;
                            }

                            // If there are still other pressed locations then there
                            // is nothing more we can do with this release.
                            if (heldLocations != 0)
                                break;

                            // From testing XNA it seems we need a velocity 
                            // of about 100 to classify this as a flick.
                            if (IsGestureEnabled(GestureType.Flick))
                            {
                                float sqDist = Vector2.DistanceSquared(touch.Position, touch.PressPosition);
                                if (sqDist > TapJitterTolerance * TapJitterTolerance
                                &&  touch.Velocity.LengthSquared() > 100.0f * 100.0f)
                                {
                                    GestureList.Enqueue(new GestureSample(
                                                            GestureType.Flick, touch.Timestamp,
                                                            Vector2.Zero, Vector2.Zero,
                                                            touch.Velocity, Vector2.Zero));

                                    //fall through, a drag should still happen even if a flick does
                                }
                            }

                            // If a drag is active then we need to finalize it.
                            if (_dragGestureStarted != GestureType.None)
                            {
                                if (IsGestureEnabled(GestureType.DragComplete))
                                    GestureList.Enqueue(new GestureSample(
                                                            GestureType.DragComplete, touch.Timestamp,
                                                            Vector2.Zero, Vector2.Zero,
                                                            Vector2.Zero, Vector2.Zero));

                                _dragGestureStarted = GestureType.None;
                                break;
                            }

                            // If all else fails try to process it as a tap.
                            ProcessTap(currentTimestamp, ref touch);
                            break;
                        }
                }
            }

            // If the touch state hasn't changed then there is no 
            // cleanup to do and no pinch to process.
            if (!stateChanged)
                return;

            // If we have two pinch points then update the pinch state.
            if (IsGestureEnabled(GestureType.Pinch)
            &&  _pinchTouch[0].State != TouchLocationState.Invalid
            &&  _pinchTouch[1].State != TouchLocationState.Invalid)
                ProcessPinch(_pinchTouch);
            else
            {
                // Make sure a partial pinch state 
                // is not left hanging around.
                _pinchGestureStarted = false;
                _pinchTouch[0] = TouchLocationData.Invalid;
                _pinchTouch[1] = TouchLocationData.Invalid;
            }

            // If all points are released then clear some states.
            if (heldLocations == 0)
            {
                _tapDisabled = false;
                _holdDisabled = false;
                _dragGestureStarted = GestureType.None;
            }
        }

        private void ProcessHold(TimeSpan currentTimestamp, ref TouchLocationData touch)
        {
            if (!IsGestureEnabled(GestureType.Hold) || _holdDisabled)
                return;

            TimeSpan elapsed = currentTimestamp - touch.PressTimestamp;
            if (elapsed < TimeRequiredForHold)
                return;

            _holdDisabled = true;

            GestureList.Enqueue(
                new GestureSample(GestureType.Hold,
                                    touch.Timestamp,
                                    touch.Position, Vector2.Zero,
                                    Vector2.Zero, Vector2.Zero));
        }

        private bool ProcessDoubleTap(ref TouchLocationData touch)
        {
            if (!IsGestureEnabled(GestureType.DoubleTap)
            ||  _tapDisabled
            ||  _lastTap.State == TouchLocationState.Invalid)
                return false;

            // If the new tap is too far away from the last then
            // this cannot be a double tap event.
            float sqDist = Vector2.DistanceSquared(touch.Position, _lastTap.Position);
            if (sqDist > TapJitterTolerance * TapJitterTolerance)
                return false;

            // Check that this tap happened within the standard 
            // double tap time threshold of 300 milliseconds.
            TimeSpan elapsed = touch.Timestamp - _lastTap.Timestamp;
            if (elapsed.TotalMilliseconds > 300)
                return false;

            GestureList.Enqueue(new GestureSample(
                           GestureType.DoubleTap, touch.Timestamp,
                           touch.Position, Vector2.Zero,
                           Vector2.Zero, Vector2.Zero));

            // Disable taps until after the next release.
            _tapDisabled = true;

            return true;
        }

        private TouchLocationData _lastTap;

        private void ProcessTap(TimeSpan currentTimestamp, ref TouchLocationData touch)
        {
            if (_tapDisabled)
                return;

            // If the release is too far away from the press 
            // position then this cannot be a tap event.
            float sqDist = Vector2.DistanceSquared(touch.PressPosition, touch.Position);
            if (sqDist > TapJitterTolerance * TapJitterTolerance)
                return;

            // If we pressed and held too long then don't 
            // generate a tap event for it.
            TimeSpan elapsed = currentTimestamp - touch.PressTimestamp;
            if (elapsed > TimeRequiredForHold)
                return;

            // Store the last tap for 
            // double tap processing.
            _lastTap = touch;

            // Fire off the tap event immediately.
            if (IsGestureEnabled(GestureType.Tap))
            {
                GestureSample tap = new GestureSample(
                    GestureType.Tap, touch.Timestamp,
                    touch.Position, Vector2.Zero,
                    Vector2.Zero, Vector2.Zero);
                GestureList.Enqueue(tap);
            }
        }

        private GestureType _dragGestureStarted = GestureType.None;

        private void ProcessDrag(ref TouchLocationData touch)
        {
            bool dragH = IsGestureEnabled(GestureType.HorizontalDrag);
            bool dragV = IsGestureEnabled(GestureType.VerticalDrag);
            bool dragF = IsGestureEnabled(GestureType.FreeDrag);

            if (!dragH && !dragV && !dragF)
                return;

            // Make sure this is a move event and that we have
            // a previous touch location.
            TouchLocationData prevTouch;
            if (touch.State != TouchLocationState.Moved
            ||  !touch.TryGetPreviousLocationData(out prevTouch))
                return;

            Vector2 delta = touch.Position - prevTouch.Position;

            // If we're free dragging then stick to it.
            if (_dragGestureStarted != GestureType.FreeDrag)
            {
                bool isHorizontalDelta = Math.Abs(delta.X) > Math.Abs(delta.Y * 2.0f);
                bool isVerticalDelta = Math.Abs(delta.Y) > Math.Abs(delta.X * 2.0f);
                bool classify = _dragGestureStarted == GestureType.None;

                // Once we enter either vertical or horizontal drags
                // we stick to it... regardless of the delta.
                if (dragH
                &&  ((classify && isHorizontalDelta) || _dragGestureStarted == GestureType.HorizontalDrag))
                {
                    delta.Y = 0;
                    _dragGestureStarted = GestureType.HorizontalDrag;
                }
                else if (dragV
                     && ((classify && isVerticalDelta) || _dragGestureStarted == GestureType.VerticalDrag))
                {
                    delta.X = 0;
                    _dragGestureStarted = GestureType.VerticalDrag;
                }

                // If the delta isn't either horizontal or vertical
                //then it could be a free drag if not classified.
                else if (dragF && classify)
                {
                    _dragGestureStarted = GestureType.FreeDrag;
                }
                else
                {
                    // If we couldn't classify the drag then
                    // it is nothing... set it to complete.
                    _dragGestureStarted = GestureType.DragComplete;
                }
            }

            // If the drag could not be classified then no gesture.
            if (_dragGestureStarted == GestureType.None
            ||  _dragGestureStarted == GestureType.DragComplete)
                return;

            _tapDisabled = true;
            _holdDisabled = true;

            GestureList.Enqueue(new GestureSample(
                                    _dragGestureStarted, touch.Timestamp,
                                    touch.Position, Vector2.Zero,
                                    delta, Vector2.Zero));
        }

        private void ProcessPinch(TouchLocationData[] touches)
        {
            TouchLocationData prevPos0;
            TouchLocationData prevPos1;

            if (!touches[0].TryGetPreviousLocationData(out prevPos0))
                prevPos0 = touches[0];

            if (!touches[1].TryGetPreviousLocationData(out prevPos1))
                prevPos1 = touches[1];

            Vector2 delta0 = touches[0].Position - prevPos0.Position;
            Vector2 delta1 = touches[1].Position - prevPos1.Position;

            // Get the newest timestamp.
            TimeSpan timestamp = touches[0].Timestamp > touches[1].Timestamp ? touches[0].Timestamp : touches[1].Timestamp;

            // If we were already in a drag state then fire
            // off the drag completion event.
            if (_dragGestureStarted != GestureType.None)
            {
                if (IsGestureEnabled(GestureType.DragComplete))
                    GestureList.Enqueue(new GestureSample(
                                            GestureType.DragComplete, timestamp,
                                            Vector2.Zero, Vector2.Zero,
                                            Vector2.Zero, Vector2.Zero));

                _dragGestureStarted = GestureType.None;
            }

            GestureList.Enqueue(new GestureSample(
                GestureType.Pinch,
                timestamp,
                touches[0].Position, touches[1].Position,
                delta0, delta1));

            _pinchGestureStarted = true;
            _tapDisabled = true;
            _holdDisabled = true;
        }

        #endregion

    }
}
