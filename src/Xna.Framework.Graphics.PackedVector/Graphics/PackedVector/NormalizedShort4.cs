// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    public struct NormalizedShort4 : IPackedVector<ulong>, IEquatable<NormalizedShort4>
	{
		private ulong _packedValue;

        public NormalizedShort4(Vector4 vector)
		{
            _packedValue = PackInFour(vector.X, vector.Y, vector.Z, vector.W);
		}

        public NormalizedShort4(float x, float y, float z, float w)
		{
            _packedValue = PackInFour(x, y, z, w);
		}

        public static bool operator !=(NormalizedShort4 left, NormalizedShort4 right)
		{
			return !left.Equals(right);
		}

        public static bool operator ==(NormalizedShort4 left, NormalizedShort4 right)
		{
			return left.Equals(right);
		}

        [CLSCompliant(false)]
        public ulong PackedValue
        {
            get { return _packedValue; }
            set { _packedValue = value; }
		}

        public override bool Equals(object obj)
        {
            return (obj is NormalizedShort4) && Equals((NormalizedShort4)obj);
        }

        public bool Equals(NormalizedShort4 other)
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

        private static ulong PackInFour(float vectorX, float vectorY, float vectorZ, float vectorW)
		{
			const long mask = 0xFFFF;
            const long maxPos = 0x7FFF;
            const long minNeg = -maxPos;

			// clamp the value between min and max values
            var word4 = (ulong)((int)Math.Round(MathHelper.Clamp(vectorX * maxPos, minNeg, maxPos)) & mask);
            var word3 = (ulong)((int)Math.Round(MathHelper.Clamp(vectorY * maxPos, minNeg, maxPos)) & mask) << 0x10;
            var word2 = (ulong)((int)Math.Round(MathHelper.Clamp(vectorZ * maxPos, minNeg, maxPos)) & mask) << 0x20;
            var word1 = (ulong)((int)Math.Round(MathHelper.Clamp(vectorW * maxPos, minNeg, maxPos)) & mask) << 0x30;

			return (word4 | word3 | word2 | word1);
		}

		void IPackedVector.PackFromVector4(Vector4 vector)
		{
            _packedValue = PackInFour(vector.X, vector.Y, vector.Z, vector.W);
		}

		public Vector4 ToVector4()
		{
            const float maxVal = 0x7FFF;

			var v4 = new Vector4();
            v4.X = ((short)((_packedValue >> 0x00) & 0xFFFF)) / maxVal;
            v4.Y = ((short)((_packedValue >> 0x10) & 0xFFFF)) / maxVal;
            v4.Z = ((short)((_packedValue >> 0x20) & 0xFFFF)) / maxVal;
            v4.W = ((short)((_packedValue >> 0x30) & 0xFFFF)) / maxVal;
			return v4;
		}
	}
}
