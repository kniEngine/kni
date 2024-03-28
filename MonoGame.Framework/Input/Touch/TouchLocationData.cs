// Copyright (C)2024 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Input.Touch
{

    internal struct TouchLocationData : IEquatable<TouchLocationData>
    {
        private int _id;
        internal TouchLocationState _state;
        private Vector2 _position;
        internal Vector2 _previousPosition;
        internal TouchLocationState _previousState;

        // Used for gesture recognition.
        private Vector2 _velocity;
        private Vector2 _pressPosition;
        private TimeSpan _pressTimestamp;
        private TimeSpan _timestamp;

        /// <summary>
        /// True if this touch was pressed and released on the same frame.
        /// In this case we will keep it around for the user to get by GetState that frame.
        /// However if they do not call GetState that frame, this touch will be forgotten.
        /// </summary>
        internal bool SameFrameReleased;

        /// <summary>
        /// Helper for assigning an invalid touch location.
        /// </summary>
        internal static readonly TouchLocationData Invalid = new TouchLocationData();


        internal Vector2 PressPosition { get { return _pressPosition; } }

        internal TimeSpan PressTimestamp { get { return _pressTimestamp; } }

        internal TimeSpan Timestamp { get { return _timestamp; } }

        internal Vector2 Velocity { get { return _velocity; } }

        internal int Id { get { return _id; } }

        internal Vector2 Position { get { return _position; } }

        internal TouchLocationState State { get { return _state; } }

        internal TouchLocation TouchLocation
        {
            get
            {
                return new TouchLocation(this._id,
                                         this._state, this._position,
                                         this._previousState, this._previousPosition);
            }
        }


        internal TouchLocationData(int id, TouchLocationState state, Vector2 position, TimeSpan timestamp)
        {
            _id = id;
            _state = state;
            _position = position;

            _previousState = TouchLocationState.Invalid;
            _previousPosition = Vector2.Zero;

            _timestamp = timestamp;
            _velocity = Vector2.Zero;

            // If this is a pressed location then store the 
            // current position and timestamp as pressed.
            if (state == TouchLocationState.Pressed)
            {
                _pressPosition = _position;
                _pressTimestamp = _timestamp;
            }
            else
            {
                _pressPosition = Vector2.Zero;
                _pressTimestamp = TimeSpan.Zero;
            }

            SameFrameReleased = false;
        }


        /// <summary>
        /// Returns a copy of the touch with the state changed to moved.
        /// </summary>
        /// <returns>The new touch location.</returns>
        internal TouchLocationData AsMovedState()
        {
            TouchLocationData touch = this;

            // Store the current state as the previous.
            touch._previousState = touch._state;
            touch._previousPosition = touch._position;

            // Set the new state.
            touch._state = TouchLocationState.Moved;

            return touch;
        }

        /// <summary>
        /// Updates the touch location using the new event.
        /// </summary>
        /// <param name="touchEvent">The next event for this touch location.</param>
        internal bool UpdateState(TouchLocationData touchEvent)
        {
            System.Diagnostics.Debug.Assert(Id == touchEvent.Id, "The touch event must have the same Id!");
            System.Diagnostics.Debug.Assert(State != TouchLocationState.Released, "We shouldn't be changing state on a released location!");
            System.Diagnostics.Debug.Assert(touchEvent.State == TouchLocationState.Moved
                                         || touchEvent.State == TouchLocationState.Released, "The new touch event should be a move or a release!");
            System.Diagnostics.Debug.Assert(touchEvent.Timestamp >= _timestamp, "The touch event is older than our timestamp!");

            // Store the current state as the previous one.
            _previousPosition = _position;
            _previousState = _state;

            // Set the new state.
            _position = touchEvent._position;
            if (touchEvent.State == TouchLocationState.Released)
                _state = touchEvent._state;

            // If time has elapsed then update the velocity.
            Vector2 delta = _position - _previousPosition;
            TimeSpan elapsed = touchEvent.Timestamp - _timestamp;
            if (elapsed > TimeSpan.Zero)
            {
                // Use a simple low pass filter to accumulate velocity.
                Vector2 velocity = delta / (float)elapsed.TotalSeconds;
                _velocity += (velocity - _velocity) * 0.45f;
            }

            //Going straight from pressed to released on the same frame
            if (_previousState == TouchLocationState.Pressed
            &&  _state == TouchLocationState.Released
            &&  elapsed == TimeSpan.Zero)
            {
                //Lie that we are pressed for now
                SameFrameReleased = true;
                _state = TouchLocationState.Pressed;
            }

            // Set the new timestamp.
            _timestamp = touchEvent.Timestamp;

            // Return true if the state actually changed.
            return _state != _previousState || delta.LengthSquared() > 0.001f;
        }

        public bool TryGetPreviousLocationData(out TouchLocationData aPreviousLocation)
        {
            if (_previousState == TouchLocationState.Invalid)
            {
                aPreviousLocation._id = -1;
                aPreviousLocation._state = TouchLocationState.Invalid;
                aPreviousLocation._position = Vector2.Zero;
                aPreviousLocation._previousState = TouchLocationState.Invalid;
                aPreviousLocation._previousPosition = Vector2.Zero;
                aPreviousLocation._timestamp = TimeSpan.Zero;
                aPreviousLocation._pressPosition = Vector2.Zero;
                aPreviousLocation._pressTimestamp = TimeSpan.Zero;
                aPreviousLocation._velocity = Vector2.Zero;
                aPreviousLocation.SameFrameReleased = false;
                return false;
            }

            aPreviousLocation._id = _id;
            aPreviousLocation._state = _previousState;
            aPreviousLocation._position = _previousPosition;
            aPreviousLocation._previousState = TouchLocationState.Invalid;
            aPreviousLocation._previousPosition = Vector2.Zero;
            aPreviousLocation._timestamp = _timestamp;
            aPreviousLocation._pressPosition = _pressPosition;
            aPreviousLocation._pressTimestamp = _pressTimestamp;
            aPreviousLocation._velocity = _velocity;
            aPreviousLocation.SameFrameReleased = SameFrameReleased;

            return true;
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public override bool Equals(object obj)
        {
            if (obj is TouchLocationData)
                return Equals((TouchLocationData)obj);

            return false;
        }

        public bool Equals(TouchLocationData other)
        {
            return _id.Equals(other._id)
                && _position.Equals(other._position)
                && _previousPosition.Equals(other._previousPosition);
        }

        public static bool operator !=(TouchLocationData left, TouchLocationData right)
        {
            return left._id != right._id
                || left._state != right._state
                || left._position != right._position
                || left._previousState != right._previousState
                || left._previousPosition != right._previousPosition;
        }

        public static bool operator ==(TouchLocationData left, TouchLocationData right)
        {
            return left._id == right._id
                && left._state == right._state
                && left._position == right._position
                && left._previousState == right._previousState
                && left._previousPosition == right._previousPosition;
        }

        public override string ToString()
        {
            return String.Format("{{ Touch id: {0}, state: {1}, position: {2}, pressure: {3}, prevState: {4}, prevPosition: {5}, previousPressure: {6} }}",
                _id, _state, _position, 0f, _previousState, _previousPosition, 0f);
        }

    }
}
