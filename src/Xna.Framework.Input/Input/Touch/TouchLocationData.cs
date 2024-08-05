// Copyright (C)2024 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Input.Touch
{

    internal struct TouchLocationData : IEquatable<TouchLocationData>
    {
        private int _id;

        internal TouchLocationState _state;
        internal Vector2 _position;

        internal TouchLocationState _previousState;
        internal Vector2 _previousPosition;


        // Used for gesture recognition.
        internal Vector2 _velocity;
        private Vector2 _pressPosition;
        private TimeSpan _pressTimestamp;
        internal TimeSpan _timestamp;

        internal int _framestamp;

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

        internal int Id { get { return _id; } }
        internal TouchLocationState State { get { return _state; } }
        internal Vector2 Position { get { return _position; } }

        internal Vector2 Velocity { get { return _velocity; } }
        internal Vector2 PressPosition { get { return _pressPosition; } }
        internal TimeSpan PressTimestamp { get { return _pressTimestamp; } }
        internal TimeSpan Timestamp { get { return _timestamp; } }
        internal int Framestamp { get { return _framestamp; } }



        internal TouchLocation TouchLocation
        {
            get
            {
                return new TouchLocation(this._id,
                                         this._state, this._position,
                                         this._previousState, this._previousPosition);
            }
        }


        internal TouchLocationData(int id, TouchLocationState state, Vector2 position, TimeSpan timestamp, int framestamp)
        {
            _id = id;
            _state = state;
            _position = position;

            _previousState = TouchLocationState.Invalid;
            _previousPosition = Vector2.Zero;

            _timestamp = timestamp;
            _framestamp = framestamp;
            _velocity = Vector2.Zero;

            // If this is a pressed location then store the 
            // current position and timestamp as pressed.
            if (state == TouchLocationState.Pressed)
            {
                _pressPosition = position;
                _pressTimestamp = timestamp;
            }
            else
            {
                _pressPosition = Vector2.Zero;
                _pressTimestamp = TimeSpan.Zero;
            }

            SameFrameReleased = false;
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
                aPreviousLocation._framestamp = 0;
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
            aPreviousLocation._framestamp = _framestamp;
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
