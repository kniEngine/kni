// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
    internal class IntersectsHelper
    {
        internal static void BoundingBoxIntersectsoundingBox(ref BoundingBox box, ref BoundingBox other, out bool result)
        {
            if ((box.Max.X >= other.Min.X) && (box.Min.X <= other.Max.X))
            {
                if ((box.Max.Y < other.Min.Y) || (box.Min.Y > other.Max.Y))
                {
                    result = false;
                    return;
                }

                result = (box.Max.Z >= other.Min.Z) && (box.Min.Z <= other.Max.Z);
                return;
            }

            result = false;
            return;
        }

        internal static void BoundingBoxIntersectsPlane(ref BoundingBox box, ref Plane plane, out PlaneIntersectionType result)
        {
            // See http://zach.in.tu-clausthal.de/teaching/cg_literatur/lighthouse3d_view_frustum_culling/index.html

            Vector3 positiveVertex;
            Vector3 negativeVertex;

            if (plane.Normal.X >= 0)
            {
                positiveVertex.X = box.Max.X;
                negativeVertex.X = box.Min.X;
            }
            else
            {
                positiveVertex.X = box.Min.X;
                negativeVertex.X = box.Max.X;
            }

            if (plane.Normal.Y >= 0)
            {
                positiveVertex.Y = box.Max.Y;
                negativeVertex.Y = box.Min.Y;
            }
            else
            {
                positiveVertex.Y = box.Min.Y;
                negativeVertex.Y = box.Max.Y;
            }

            if (plane.Normal.Z >= 0)
            {
                positiveVertex.Z = box.Max.Z;
                negativeVertex.Z = box.Min.Z;
            }
            else
            {
                positiveVertex.Z = box.Min.Z;
                negativeVertex.Z = box.Max.Z;
            }

            // Inline Vector3.Dot(plane.Normal, negativeVertex) + plane.D;
            var distance = plane.Normal.X * negativeVertex.X + plane.Normal.Y * negativeVertex.Y + plane.Normal.Z * negativeVertex.Z + plane.D;
            if (distance > 0)
            {
                result = PlaneIntersectionType.Front;
                return;
            }

            // Inline Vector3.Dot(plane.Normal, positiveVertex) + plane.D;
            distance = plane.Normal.X * positiveVertex.X + plane.Normal.Y * positiveVertex.Y + plane.Normal.Z * positiveVertex.Z + plane.D;
            if (distance < 0)
            {
                result = PlaneIntersectionType.Back;
                return;
            }

            result = PlaneIntersectionType.Intersecting;
        }

        // adapted from http://www.scratchapixel.com/lessons/3d-basic-lessons/lesson-7-intersecting-simple-shapes/ray-box-intersection/
        internal static void BoundingBoxIntersectsRay(ref BoundingBox box, ref Ray ray, out float? result)
        {
            const float Epsilon = 1e-6f;

            float? tMin = null, tMax = null;

            if (Math.Abs(ray.Direction.X) < Epsilon)
            {
                if (ray.Position.X < box.Min.X || ray.Position.X > box.Max.X)
                {
                    result = null;
                    return;
                }
            }
            else
            {
                tMin = (box.Min.X - ray.Position.X) / ray.Direction.X;
                tMax = (box.Max.X - ray.Position.X) / ray.Direction.X;

                if (tMin > tMax)
                {
                    var temp = tMin;
                    tMin = tMax;
                    tMax = temp;
                }
            }

            if (Math.Abs(ray.Direction.Y) < Epsilon)
            {
                if (ray.Position.Y < box.Min.Y || ray.Position.Y > box.Max.Y)
                {
                    result = null;
                    return;
                }
            }
            else
            {
                var tMinY = (box.Min.Y - ray.Position.Y) / ray.Direction.Y;
                var tMaxY = (box.Max.Y - ray.Position.Y) / ray.Direction.Y;

                if (tMinY > tMaxY)
                {
                    var temp = tMinY;
                    tMinY = tMaxY;
                    tMaxY = temp;
                }

                if ((tMin.HasValue && tMin > tMaxY) || (tMax.HasValue && tMinY > tMax))
                {
                    result = null;
                    return;
                }

                if (!tMin.HasValue || tMinY > tMin) tMin = tMinY;
                if (!tMax.HasValue || tMaxY < tMax) tMax = tMaxY;
            }

            if (Math.Abs(ray.Direction.Z) < Epsilon)
            {
                if (ray.Position.Z < box.Min.Z || ray.Position.Z > box.Max.Z)
                {
                    result = null;
                    return;
                }
            }
            else
            {
                var tMinZ = (box.Min.Z - ray.Position.Z) / ray.Direction.Z;
                var tMaxZ = (box.Max.Z - ray.Position.Z) / ray.Direction.Z;

                if (tMinZ > tMaxZ)
                {
                    var temp = tMinZ;
                    tMinZ = tMaxZ;
                    tMaxZ = temp;
                }

                if ((tMin.HasValue && tMin > tMaxZ) || (tMax.HasValue && tMinZ > tMax))
                {
                    result = null;
                    return;
                }

                if (!tMin.HasValue || tMinZ > tMin) tMin = tMinZ;
                if (!tMax.HasValue || tMaxZ < tMax) tMax = tMaxZ;
            }

            // having a positive tMax and a negative tMin means the ray is inside the box
            // we expect the intesection distance to be 0 in that case
            if ((tMin.HasValue && tMin < 0) && tMax > 0)
            {
                result = 0;
                return;
            }

            // a negative tMin means that the intersection point is behind the ray's origin
            // we discard these as not hitting the AABB
            if (tMin < 0)
            {
                result = null;
                return;
            }

            result = tMin;
            return;
        }

        internal static void BoundingBoxIntersectsBoundingSphere(ref BoundingBox box, ref BoundingSphere sphere, out bool result)
        {
            double squareDistance = 0.0;
            if (sphere.Center.X < box.Min.X) squareDistance += (sphere.Center.X - box.Min.X) * (sphere.Center.X - box.Min.X);
            else if (sphere.Center.X > box.Max.X) squareDistance += (sphere.Center.X - box.Max.X) * (sphere.Center.X - box.Max.X);
            if (sphere.Center.Y < box.Min.Y) squareDistance += (sphere.Center.Y - box.Min.Y) * (sphere.Center.Y - box.Min.Y);
            else if (sphere.Center.Y > box.Max.Y) squareDistance += (sphere.Center.Y - box.Max.Y) * (sphere.Center.Y - box.Max.Y);
            if (sphere.Center.Z < box.Min.Z) squareDistance += (sphere.Center.Z - box.Min.Z) * (sphere.Center.Z - box.Min.Z);
            else if (sphere.Center.Z > box.Max.Z) squareDistance += (sphere.Center.Z - box.Max.Z) * (sphere.Center.Z - box.Max.Z);
            result = squareDistance <= sphere.Radius * sphere.Radius;
        }

    }
}
