// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;


namespace Microsoft.Xna.Framework.Input.Touch
{
    public struct TouchLocation : IEquatable<TouchLocation>
    {
        /// <summary>
        ///Attributes 
        /// </summary>
        private int _id;
        private TouchLocationState _state;
        private Vector2 _position;
        private Vector2 _previousPosition;
        private TouchLocationState _previousState;

        /// <summary>
        /// Helper for assigning an invalid touch location.
        /// </summary>
        internal static readonly TouchLocation Invalid = new TouchLocation();

        #region Properties

        public int Id 
        { 
            get
            {
                return _id;
            }
        }

        public Vector2 Position 
        { 
            get
            {
                return _position;
            }
        }
        
        public float Pressure 
        { 
            get
            {
                return 0f;
            }
        }
                                
        public TouchLocationState State 
        { 
            get
            {
                return _state;
            } 
        }
        
        #endregion
        
        #region Constructors

        public TouchLocation(int id, TouchLocationState state, Vector2 position)
            : this(id, state, position, TouchLocationState.Invalid, Vector2.Zero)
        {
        }

        public TouchLocation(   int id, TouchLocationState state, Vector2 position, 
                                TouchLocationState previousState, Vector2 previousPosition)
        {
            _id = id;
            _state = state;
            _position = position;

            _previousState = previousState;
            _previousPosition = previousPosition;
        }

        #endregion

        public override bool Equals(object obj)
        {
            if (obj is TouchLocation)
                return Equals((TouchLocation)obj);

            return false;
        }

        public bool Equals(TouchLocation other)
        {
            return  _id.Equals(other._id) &&
                    _position.Equals(other._position) &&
                    _previousPosition.Equals(other._previousPosition);
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public override string ToString()
        {
            return "Touch id:"+_id+" state:"+_state + " position:" + _position + " pressure:" + 0f +" prevState:"+_previousState+" prevPosition:"+ _previousPosition + " previousPressure:" + 0f;
        }

        public bool TryGetPreviousLocation(out TouchLocation aPreviousLocation)
        {
            if (_previousState == TouchLocationState.Invalid)
            {
                aPreviousLocation._id = -1;
                aPreviousLocation._state = TouchLocationState.Invalid;
                aPreviousLocation._position = Vector2.Zero;
                aPreviousLocation._previousState = TouchLocationState.Invalid;
                aPreviousLocation._previousPosition = Vector2.Zero; 
                return false;
            }

            aPreviousLocation._id = _id;
            aPreviousLocation._state = _previousState;
            aPreviousLocation._position = _previousPosition;
            aPreviousLocation._previousState = TouchLocationState.Invalid;
            aPreviousLocation._previousPosition = Vector2.Zero;
            return true;
        }

        public static bool operator !=(TouchLocation left, TouchLocation right)
        {
            return  left._id != right._id || 
                    left._state != right._state ||
                    left._position != right._position ||
                    left._previousState != right._previousState ||
                    left._previousPosition != right._previousPosition;
        }

        public static bool operator ==(TouchLocation left, TouchLocation right)
        {
            return  left._id == right._id && 
                    left._state == right._state &&
                    left._position == right._position &&
                    left._previousState == right._previousState &&
                    left._previousPosition == right._previousPosition;
        }

    }
}
