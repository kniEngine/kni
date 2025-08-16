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
        internal static void BoundingBoxIntersectsBoundingBox(ref BoundingBox box, ref BoundingBox other, out bool result)
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

        internal static void BoundingBoxIntersectsBoundingFrustum(ref BoundingBox box, BoundingFrustum frustum, out bool result)
        {
            result = true;
            for (int i = 0; i < BoundingFrustum.PlaneCount; i++)
            {
                frustum._planes[i].Intersects(ref box, out PlaneIntersectionType planeIntersectionType);
                switch (planeIntersectionType)
                {
                    case PlaneIntersectionType.Front:
                        result = false;
                        return;
                }
            }

            BoundingBox fbox = BoundingBox.CreateFromPoints(frustum._corners);
            box.Intersects(ref fbox, out result);
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

        internal static void BoundingFrustumIntersectsBoundingFrustum(BoundingFrustum frustum, BoundingFrustum other, out bool result)
        {
            result = true;
            for (int i = 0; i < BoundingFrustum.PlaneCount; i++)
            {
                other.Intersects(ref frustum._planes[i], out PlaneIntersectionType planeIntersectionType);
                switch (planeIntersectionType)
                {
                    case PlaneIntersectionType.Front:
                        result = false;
                        return;
                }
            }

            for (int i = 0; i < BoundingFrustum.PlaneCount; i++)
            {
                frustum.Intersects(ref other._planes[i], out PlaneIntersectionType planeIntersectionType);
                switch (planeIntersectionType)
                {
                    case PlaneIntersectionType.Front:
                        result = false;
                        return;
                }
            }
        }

        internal static void BoundingFrustumIntersectsPlane(BoundingFrustum frustum, ref Plane plane, out PlaneIntersectionType result)
        {
            result = plane.Intersects(ref frustum._corners[0]);
            for (int i = 1; i < frustum._corners.Length; i++)
            {
                PlaneIntersectionType planeIntersectionType = plane.Intersects(ref frustum._corners[i]);
                if (planeIntersectionType != result)
                    result = PlaneIntersectionType.Intersecting;
            }
        }

        internal static void BoundingFrustumIntersectsBoundingSphere(BoundingFrustum frustum, ref BoundingSphere sphere, out bool result)
        {
            result = true;
            int sphereBack = 0;
            int sphereCenterBack = 0;
            for (int i = 0; i < BoundingFrustum.PlaneCount; i++)
            {
                float distance = frustum._planes[i].DotCoordinate(sphere.Center);
                if (distance > sphere.Radius) // PlaneIntersectionType.Front
                {
                    result = false;
                    return;
                }
                else if (distance < -sphere.Radius) // PlaneIntersectionType.Back
                {
                    sphereBack++;
                }
                else // PlaneIntersectionType.Intersecting
                {
                }

                if (distance < 0)
                    sphereCenterBack++;
            }

            // The sphere is inside the frustum (sphereBack==6),
            // or intersects with 1 face and is contained by the other 5 (sphereBack==5).
            if (sphereBack >= (BoundingFrustum.PlaneCount - 1))
                return;

            // The sphere center is inside the frustum.
            if (sphereCenterBack == BoundingFrustum.PlaneCount)
                return;

            // The sphere intersects with an edge.
            if (SegmentIntersectsBoundingSphere(ref frustum._corners[0], ref frustum._corners[1], ref sphere))
                return;
            if (SegmentIntersectsBoundingSphere(ref frustum._corners[1], ref frustum._corners[2], ref sphere))
                return;
            if (SegmentIntersectsBoundingSphere(ref frustum._corners[2], ref frustum._corners[3], ref sphere))
                return;
            if (SegmentIntersectsBoundingSphere(ref frustum._corners[3], ref frustum._corners[0], ref sphere))
                return;
            if (SegmentIntersectsBoundingSphere(ref frustum._corners[4], ref frustum._corners[5], ref sphere))
                return;
            if (SegmentIntersectsBoundingSphere(ref frustum._corners[5], ref frustum._corners[6], ref sphere))
                return;
            if (SegmentIntersectsBoundingSphere(ref frustum._corners[6], ref frustum._corners[7], ref sphere))
                return;
            if (SegmentIntersectsBoundingSphere(ref frustum._corners[7], ref frustum._corners[4], ref sphere))
                return;
            if (SegmentIntersectsBoundingSphere(ref frustum._corners[0], ref frustum._corners[4], ref sphere))
                return;
            if (SegmentIntersectsBoundingSphere(ref frustum._corners[1], ref frustum._corners[5], ref sphere))
                return;
            if (SegmentIntersectsBoundingSphere(ref frustum._corners[2], ref frustum._corners[6], ref sphere))
                return;
            if (SegmentIntersectsBoundingSphere(ref frustum._corners[3], ref frustum._corners[7], ref sphere))
                return;

            float dotCoord0 = frustum._planes[0].DotCoordinate(sphere.Center);
            if (dotCoord0 >= 0 && dotCoord0 <= sphere.Radius)
            {
                Vector3 pt0 = sphere.Center - dotCoord0 * frustum._planes[0].Normal;
                FixedPolygon4 points = new FixedPolygon4(frustum._corners, 1, 0, 3, 2);
                points.OrderQuad(ref frustum._planes[0].Normal);
                if (PointInPolygon(ref pt0, points))
                    return;
            }
            float dotCoord1 = frustum._planes[1].DotCoordinate(sphere.Center);
            if (dotCoord1 >= 0 && dotCoord1 <= sphere.Radius)
            {
                Vector3 pt1 = sphere.Center - dotCoord1 * frustum._planes[1].Normal;
                FixedPolygon4 points = new FixedPolygon4(frustum._corners, 4, 5, 6, 7);
                points.OrderQuad(ref frustum._planes[1].Normal);
                if (PointInPolygon(ref pt1, points))
                    return;
            }
            float dotCoord2 = frustum._planes[2].DotCoordinate(sphere.Center);
            if (dotCoord2 >= 0 && dotCoord2 <= sphere.Radius)
            {
                Vector3 pt2 = sphere.Center - dotCoord2 * frustum._planes[2].Normal;
                FixedPolygon4 points =new FixedPolygon4(frustum._corners, 3, 0, 7, 4);
                points.OrderQuad(ref frustum._planes[2].Normal);
                if (PointInPolygon(ref pt2, points))
                    return;
            }
            float dotCoord3 = frustum._planes[3].DotCoordinate(sphere.Center);
            if (dotCoord3 >= 0 && dotCoord3 <= sphere.Radius)
            {
                Vector3 pt3 = sphere.Center - dotCoord3 * frustum._planes[3].Normal;
                FixedPolygon4 points =new FixedPolygon4(frustum._corners, 1, 2, 5, 6);
                points.OrderQuad(ref frustum._planes[3].Normal);
                if (PointInPolygon(ref pt3, points))
                    return;
            }
            float dotCoord4 = frustum._planes[4].DotCoordinate(sphere.Center);
            if (dotCoord4 >= 0 && dotCoord4 <= sphere.Radius)
            {
                Vector3 pt4 = sphere.Center - dotCoord4 * frustum._planes[4].Normal;
                FixedPolygon4 points =new FixedPolygon4(frustum._corners, 0, 1, 4, 5);
                points.OrderQuad(ref frustum._planes[4].Normal);
                if (PointInPolygon(ref pt4, points))
                    return;
            }
            float dotCoord5 = frustum._planes[5].DotCoordinate(sphere.Center);
            if (dotCoord5 >= 0 && dotCoord5 <= sphere.Radius)
            {
                Vector3 pt5 = sphere.Center - dotCoord5 * frustum._planes[5].Normal;
                FixedPolygon4 points =new FixedPolygon4(frustum._corners, 2, 3, 6, 7);
                points.OrderQuad(ref frustum._planes[5].Normal);
                if (PointInPolygon(ref pt5, points))
                    return;
            }

            result = false;
        }

        private struct FixedPolygon4
        {
            private Vector3[] _corners;
            private int _v0, _v1,_v2,_v3;

            internal FixedPolygon4(Vector3[] corners, int v0, int v1, int v2, int v3)
            {
                this._corners = corners;
                this._v0 = v0;
                this._v1 = v1;
                this._v2 = v2;
                this._v3 = v3;
            }

            public Vector3 this[int idx]
            {
                get
                {
                    switch (idx)
                    {
                        case 0: return _corners[_v0];
                        case 1: return _corners[_v1];
                        case 2: return _corners[_v2];
                        case 3: return _corners[_v3];

                        default:
                            throw new InvalidOperationException("idx");
                    }
                }
            }

            private int GetInternalKey(int idx)
            {
                switch (idx)
                {
                    case 0: return _v0;
                    case 1: return _v1;
                    case 2: return _v2;
                    case 3: return _v3;

                    default:
                        throw new InvalidOperationException("idx");
                }
            }

            private void SetInternalKey(int idx, int keyInternal)
            {
                switch (idx)
                {
                    case 0: _v0 = keyInternal; return;
                    case 1: _v1 = keyInternal; return;
                    case 2: _v2 = keyInternal; return;
                    case 3: _v3 = keyInternal; return;

                    default:
                        throw new InvalidOperationException("idx");
                }
            }

            internal unsafe void OrderQuad(ref Vector3 normal)
            {
                Vector3 center = (this[0] + this[1] + this[2] + this[3]) * 0.25f;

                normal = Vector3.Normalize(normal);
                Vector3 refAxis = Math.Abs(normal.X) > 0.9f ? Vector3.UnitY : Vector3.UnitX;
                Vector3 u = Vector3.Normalize(Vector3.Cross(normal, refAxis));
                Vector3 v = Vector3.Cross(normal, u);

                // precompute angles
                float* angles = stackalloc float[4];
                angles[0] = (float)Math.Atan2(Vector3.Dot(this[0] - center, v), Vector3.Dot(this[0] - center, u));
                angles[1] = (float)Math.Atan2(Vector3.Dot(this[1] - center, v), Vector3.Dot(this[1] - center, u));
                angles[2] = (float)Math.Atan2(Vector3.Dot(this[2] - center, v), Vector3.Dot(this[2] - center, u));
                angles[3] = (float)Math.Atan2(Vector3.Dot(this[3] - center, v), Vector3.Dot(this[3] - center, u));

                // simple insertion sort for 4 items
                for (int i = 1; i < 4; i++)
                {
                    int keyInternal = GetInternalKey(i);
                    float keyAngle = angles[i];
                    int j = i-1;
                    while (j >= 0 && angles[j] > keyAngle)
                    {
                        SetInternalKey(j+1, GetInternalKey(j));
                        angles[j+1] = angles[j];
                        j--;
                    }
                    SetInternalKey(j+1, keyInternal);
                    angles[j+1] = keyAngle;
                }
            }
        }

        private static bool PointInPolygon(ref Vector3 pt, FixedPolygon4 points)
        {
            return (IsPointInTriangle(ref pt, points[0], points[1], points[2]) 
                ||  IsPointInTriangle(ref pt, points[0], points[2], points[3]));
        }

        static bool IsPointInTriangle(ref Vector3 pt, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 v0 = c - a;
            Vector3 v1 = b - a;
            Vector3 v2 = pt - a;

            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            float denom = dot00 * dot11 - dot01 * dot01;
            if (denom == 0) return false;

            float u = (dot11 * dot02 - dot01 * dot12) / denom;
            float v = (dot00 * dot12 - dot01 * dot02) / denom;

            return (u >= 0) && (v >= 0) && (u + v <= 1);
        }

        private static bool SegmentIntersectsBoundingSphere(ref Vector3 pos0, ref Vector3 pos1, ref BoundingSphere sphere)
        {
            Vector3 direction = pos1 - pos0;
            float sqSegmentLength = direction.LengthSquared();

            float segmentLength = (float)Math.Sqrt(sqSegmentLength);
            Vector3 segmentNormal = direction;
            segmentNormal.X /= segmentLength;
            segmentNormal.Y /= segmentLength;
            segmentNormal.Z /= segmentLength;

            Ray ray = new Ray(pos0, segmentNormal);

            ray.Intersects(ref sphere, out float? result);
            if (result != null && result < segmentLength)
                return true;

            return false;
        }

        internal static void BoundingSphereIntersectsBoundingSphere(ref BoundingSphere sphere, ref BoundingSphere other, out bool result)
        {
            Vector3.DistanceSquared(ref other.Center, ref sphere.Center, out float sqDistance);

            result = (sqDistance <= (other.Radius + sphere.Radius) * (other.Radius + sphere.Radius));
        }

        internal static void BoundingSphereIntersectsPlane(ref BoundingSphere sphere, ref Plane plane, out PlaneIntersectionType result)
        {
            Vector3.Dot(ref plane.Normal, ref sphere.Center, out float distance);
            distance += plane.D;

            if (distance > sphere.Radius)
                result = PlaneIntersectionType.Front;
            else if (distance < -sphere.Radius)
                result = PlaneIntersectionType.Back;
            else
                result = PlaneIntersectionType.Intersecting;
        }

        internal static void BoundingSphereIntersectsRay(ref BoundingSphere sphere, ref Ray ray, out float? result)
        {
            // Find the vector between where the ray starts the the sphere's centre
            Vector3 difference = sphere.Center - ray.Position;

            float differenceLengthSquared = difference.LengthSquared();
            float sphereRadiusSquared = sphere.Radius * sphere.Radius;

            float distanceAlongRay;

            // If the distance between the ray start and the sphere's centre is less than
            // the radius of the sphere, it means we've intersected. N.B. checking the LengthSquared is faster.
            if (differenceLengthSquared < sphereRadiusSquared)
            {
                result = 0.0f;
                return;
            }

            Vector3.Dot(ref ray.Direction, ref difference, out distanceAlongRay);
            // If the ray is pointing away from the sphere then we don't ever intersect
            if (distanceAlongRay < 0)
            {
                result = null;
                return;
            }

            // Next we kinda use Pythagoras to check if we are within the bounds of the sphere
            // if x = radius of sphere
            // if y = distance between ray position and sphere centre
            // if z = the distance we've travelled along the ray
            // if x^2 + z^2 - y^2 < 0, we do not intersect
            float dist = sphereRadiusSquared + distanceAlongRay * distanceAlongRay - differenceLengthSquared;

            result = (dist < 0) ? null : distanceAlongRay - (float?)Math.Sqrt(dist);
        }

        internal static void PlaneIntersectsRay(ref Plane plane, ref Ray ray, out float? result)
        {
            float den = Vector3.Dot(ray.Direction, plane.Normal);
            if (Math.Abs(den) < 0.00001f)
            {
                result = null;
                return;
            }

            result = (-plane.D - Vector3.Dot(plane.Normal, ray.Position)) / den;

            if (result < 0.0f)
            {
                if (result < -0.00001f)
                {
                    result = null;
                    return;
                }

                result = 0.0f;
            }
        }

        internal static void BoundingFrustumIntersectsRay(BoundingFrustum frustum, ref Ray ray, out float? result)
        {
            // From "Real-Time Collision Detection" (Page 198)

            float tfirst = 0;
            float tlast = float.MaxValue;
            for (int i = 0; i < BoundingFrustum.PlaneCount; i++)
            {
                float dist  = -(Vector3.Dot(frustum._planes[i].Normal, ray.Position) + frustum._planes[i].D);
                float denom = Vector3.Dot(frustum._planes[i].Normal, ray.Direction);

                const float epsilon = 1e-6f;

                if (Math.Abs(denom) < epsilon)
                {
                    if (dist > 0f) // ray runs parallel to the plane.
                    {
                        result = null;
                        return;
                    }
                }
                else
                {
                    float t = dist / denom;

                    if (denom < 0f)
                    {
                        if (t > tfirst)
                            tfirst = t;
                    }
                    else
                    {
                        if (t < tlast)
                            tlast = t;
                    }

                    if (tfirst > tlast)
                    {
                        result = null;
                        return;
                    }
                }
            }

            result = tfirst;
            return;
        }
    }
}
