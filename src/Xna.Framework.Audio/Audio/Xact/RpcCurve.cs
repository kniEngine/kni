// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Audio
{
    struct RpcCurve
    {
        public uint FileOffset;
        public int Variable;
        public bool IsGlobal;
        public RpcParameter Parameter;
        public RpcPoint[] Points;

        public float Evaluate(float position)
        {
            // TODO: We need to implement the different RpcPointTypes.

            RpcPoint first = Points[0];
            if (position <= first.Position)
                return first.Value;

            RpcPoint second = Points[Points.Length - 1];
            if (position >= second.Position)
                return second.Value;

            for (int i = 1; i < Points.Length; ++i)
            {
                second = Points[i];
                if (second.Position >= position)
                    break;

                first = second;
            }

            switch (first.Type)
            {
                default:
                case RpcPointType.Linear:
                {
                    float t = (position - first.Position) / (second.Position - first.Position);
                    return first.Value + ((second.Value - first.Value) * t);
                }
            }
        }
    }
}