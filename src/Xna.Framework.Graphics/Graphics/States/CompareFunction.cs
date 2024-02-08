// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// The comparison function used for depth, stencil, and alpha tests.
    /// </summary>
    public enum CompareFunction
    {
        /// <summary>
        /// Always passes the test.
        /// </summary>
        Always,
        /// <summary>
        /// Never passes the test.
        /// </summary>
        Never,
        /// <summary>
        /// Passes the test when the new value is less than current value.
        /// </summary>
        Less,
        /// <summary>
        /// Passes the test when the new value is less than or equal to current value.
        /// </summary>
        LessEqual,
        /// <summary>
        /// Passes the test when the new value is equal to current value.
        /// </summary>
        Equal,
        /// <summary>
        /// Passes the test when the new value is greater than or equal to current value.
        /// </summary>
        GreaterEqual,
        /// <summary>
        /// Passes the test when the new value is greater than current value.
        /// </summary>
        Greater,
        /// <summary>
        /// Passes the test when the new value does not equal to current value.
        /// </summary>
        NotEqual
    }
}