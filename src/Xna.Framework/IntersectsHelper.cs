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
        }

        internal static void BoundingFrustumIntersectsPlane(BoundingFrustum frustum, Plane plane, out PlaneIntersectionType result)
        {
            result = plane.Intersects(ref frustum._corners[0]);
            for (int i = 1; i < frustum._corners.Length; i++)
            {
                PlaneIntersectionType planeIntersectionType = plane.Intersects(ref frustum._corners[i]);
                if (planeIntersectionType != result)
                    result = PlaneIntersectionType.Intersecting;
            }
        }

        internal static void BoundingFrustumIntersectsBoundingSphere(BoundingFrustum frustum, BoundingSphere sphere, out bool result)
        {
            result = true;
            for (int i = 0; i < BoundingFrustum.PlaneCount; i++)
            {
                frustum._planes[i].Intersects(ref sphere, out PlaneIntersectionType planeIntersectionType);
                switch (planeIntersectionType)
                {
                    case PlaneIntersectionType.Front:
                        result = false;
                        return;
                }
            }
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

        internal static void BoundingFrustumIntersectsRay(BoundingFrustum frustum, Ray ray, out float? result)
        {
            ContainmentType ctype = ContainmentType.Contains;
            for (int i = 0; i < BoundingFrustum.PlaneCount; i++)
            {
                frustum._planes[i].DotCoordinate(ref ray.Position, out float dot);
                if (dot > 0)
                {
                    ctype = ContainmentType.Disjoint;
                    break;
                }
            }

            switch (ctype)
            {
                case ContainmentType.Disjoint:
                    result = null;
                    return;
                case ContainmentType.Contains:
                    result = 0.0f;
                    return;
                case ContainmentType.Intersects:

                    // TODO: Needs additional test for not 0.0 and null results.

                    result = null;

                    float min = float.MinValue;
                    float max = float.MaxValue;
                    for (int p = 0; p < BoundingFrustum.PlaneCount; p++)
                    {
                        Vector3 normal = frustum._planes[p].Normal;

                        float result2;
                        Vector3.Dot(ref ray.Direction, ref normal, out result2);

                        float result3;
                        Vector3.Dot(ref ray.Position, ref normal, out result3);

                        result3 += frustum._planes[p].D;

                        if ((double)Math.Abs(result2) < 9.99999974737875E-06)
                        {
                            if ((double)result3 > 0.0)
                                return;
                        }
                        else
                        {
                            float result4 = -result3 / result2;
                            if ((double)result2 < 0.0)
                            {
                                if ((double)result4 > (double)max)
                                    return;
                                if ((double)result4 > (double)min)
                                    min = result4;
                            }
                            else
                            {
                                if ((double)result4 < (double)min)
                                    return;
                                if ((double)result4 < (double)max)
                                    max = result4;
                            }
                        }

                        float? distance;
                        frustum._planes[p].Intersects(ref ray, out distance);
                        if (distance.HasValue)
                        {
                            min = Math.Min(min, distance.Value);
                            max = Math.Max(max, distance.Value);
                        }
                    }

                    float temp = min >= 0.0 ? min : max;
                    if (temp < 0.0)
                    {
                        return;
                    }
                    result = temp;

                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
