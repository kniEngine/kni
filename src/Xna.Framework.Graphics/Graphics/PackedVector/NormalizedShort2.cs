// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	public struct NormalizedShort2 : IPackedVector<uint>, IEquatable<NormalizedShort2>
	{
		private uint _packedValue;

        public NormalizedShort2(Vector2 vector)
		{
            _packedValue = PackInTwo(vector.X, vector.Y);
		}

        public NormalizedShort2(float x, float y)
		{
            _packedValue = PackInTwo(x, y);
		}

        public static bool operator !=(NormalizedShort2 left, NormalizedShort2 right)
		{
			return !left.Equals(right);
		}

        public static bool operator ==(NormalizedShort2 left, NormalizedShort2 right)
		{
			return left.Equals(right);
		}

        [CLSCompliant(false)]
        public uint PackedValue
        {
            get { return _packedValue; }
            set { _packedValue = value; }
		}

		public override bool Equals(object obj)
		{
            return (obj is NormalizedShort2) && Equals((NormalizedShort2)obj);
		}

        public bool Equals(NormalizedShort2 other)
		{
            return _packedValue.Equals(other._packedValue);
		}

		public override int GetHashCode()
		{
			return _packedValue.GetHashCode();
		}

		public override string ToString()
		{
            return _packedValue.ToString("X");
		}

		public Vector2 ToVector2()
		{
            const float maxVal = 0x7FFF;

			var v2 = new Vector2();
            v2.X = ((short)(_packedValue & 0xFFFF)) / maxVal;
            v2.Y = (short)(_packedValue >> 0x10) / maxVal;
			return v2;
		}

		private static uint PackInTwo(float vectorX, float vectorY)
		{
			const float maxPos = 0x7FFF;
            const float minNeg = -maxPos;

			// clamp the value between min and max values
            // Round rather than truncate.
            var word2 = (uint)((int)MathHelper.Clamp((float)Math.Round(vectorX * maxPos), minNeg, maxPos) & 0xFFFF);
            var word1 = (uint)(((int)MathHelper.Clamp((float)Math.Round(vectorY * maxPos), minNeg, maxPos) & 0xFFFF) << 0x10);

			return (word2 | word1);
		}

		void IPackedVector.PackFromVector4(Vector4 vector)
		{
            _packedValue = PackInTwo(vector.X, vector.Y);
		}

        /// <summary>
        /// Gets the packed vector in Vector4 format.
        /// </summary>
        /// <returns>The packed vector in Vector4 format</returns>
		public Vector4 ToVector4()
		{
            const float maxVal = 0x7FFF;

			var v4 = new Vector4(0,0,0,1);
            v4.X = ((short)((_packedValue >> 0x00) & 0xFFFF)) / maxVal;
            v4.Y = ((short)((_packedValue >> 0x10) & 0xFFFF)) / maxVal;
			return v4;
		}
	}
}
