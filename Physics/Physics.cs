using System;
using UnityEngine;

namespace Ugly
{
    public static class Physics
    {
        #region Plane intersection
        public static bool RayPlaneIntersection(Vector3 planeNormal, Vector3 planePoint, Ray ray, out Vector3 point)
        {
            var ndotu = Vector3.Dot(planeNormal, ray.direction);
            if (Math.Abs(ndotu) < 0.000001f)
            {
                point = ray.origin;
                return false;
            }
            var w = ray.origin - planePoint;
            point = w + (-Vector3.Dot(planeNormal, w) / ndotu) * ray.direction + planePoint;
            return true;
        }

        public static bool RayPlaneIntersection(Vector3 planeNormal, Vector3 planePoint, Vector3 rayDirection, Vector3 rayOrigin, out Vector3 point)
        {
            var ndotu = planeNormal.x * rayDirection.x + planeNormal.y * rayDirection.y + planeNormal.z * rayDirection.z;

            if (Math.Abs(ndotu) < 0.000001f)
            {
                point = rayOrigin;
                return false;
            }
            var w = rayOrigin - planePoint;
            point = w + (-(planeNormal.x * w.x + planeNormal.y * w.y + planeNormal.z * w.z) / ndotu) * rayDirection + planePoint;
            return true;
        }
        #endregion

        #region Shpere intersection
        public static bool RaySphereIntersection(ShpereColliderData sphereData, Vector3 rayOrigin, Vector3 rayDirection, out Vector3 point)
        {
            float t0, t1;

            var L = sphereData.position - rayOrigin;
            float tca = L.x * rayDirection.x + L.y * rayDirection.y + L.z * rayDirection.z;
            if (tca < 0)
            {
                point = rayOrigin;
                return false;
            }
            float d2 = (L.x * L.x + L.y * L.y + L.z * L.z) - tca * tca;
            if (d2 > sphereData.radius)
            {
                point = rayOrigin;
                return false;
            }
            var thc = (float)Math.Sqrt(sphereData.radius - d2);
            t0 = tca - thc;
            t1 = tca + thc;

            if (t0 > t1)
            {
                var temp = t0;
                t0 = t1;
                t1 = temp;
            }

            if (t0 < 0)
            {
                t0 = t1;
                if (t0 < 0)
                {
                    point = rayOrigin;
                    return false;
                }
            }

            point = rayOrigin + rayDirection.normalized * t0;
            return true;
        }

        public static bool RaySphereIntersection(Vector3 sphereCenter, float sphereRadius, Vector3 rayOrigin, Vector3 rayDirection, out Vector3 point)
        {
            float t0, t1;

            var L = sphereCenter - rayOrigin;
            float tca = L.x * rayDirection.x + L.y * rayDirection.y + L.z * rayDirection.z;
            if (tca < 0)
            {
                point = rayOrigin;
                return false;
            }
            float d2 = (L.x * L.x + L.y * L.y + L.z * L.z) - tca * tca;
            if (d2 > sphereRadius)
            {
                point = rayOrigin;
                return false;
            }
            var thc = (float)Math.Sqrt(sphereRadius - d2);
            t0 = tca - thc;
            t1 = tca + thc;

            if (t0 > t1)
            {
                var temp = t0;
                t0 = t1;
                t1 = temp;
            }

            if (t0 < 0)
            {
                t0 = t1;
                if (t0 < 0)
                {
                    point = rayOrigin;
                    return false;
                }
            }

            point = rayOrigin + rayDirection.normalized * t0;
            return true;
        }

        /// <summary>
        /// In cycles, if sphere is not moving, you better use a <see cref="LineSphereIntersection"/> with cashed <see cref="ShpereColliderData"/> from <see cref="GetSphereData"/>
        /// </summary>
        /// <param name="sphere"></param>
        /// <param name="linePoint"></param>
        /// <param name="lineDirection"></param>
        /// <param name="point"></param>
        /// <seealso cref="LineSphereIntersection"/>
        /// <seealso cref="GetSphereData"/>
        /// <returns></returns>
        public static bool LineSphereColliderIntersection(SphereCollider sphere, Vector3 linePoint, Vector3 lineDirection, out Vector3 point)
        {
            float maxSide = Mathf.Max(sphere.transform.localScale.x, sphere.transform.localScale.y, sphere.transform.localScale.z);
            float sphereRadius = sphere.radius * sphere.radius * maxSide * maxSide;

            float t0, t1;

            var L = sphere.transform.position + sphere.center * maxSide - linePoint;
            float tca = L.x * lineDirection.x + L.y * lineDirection.y + L.z * lineDirection.z;
            if (tca < 0)
            {
                point = linePoint;
                return false;
            }
            float d2 = (L.x * L.x + L.y * L.y + L.z * L.z) - tca * tca;
            if (d2 > sphereRadius)
            {
                point = linePoint;
                return false;
            }
            var thc = (float)Math.Sqrt(sphereRadius - d2);
            t0 = tca - thc;
            t1 = tca + thc;

            if (t0 > t1)
            {
                var temp = t0;
                t0 = t1;
                t1 = temp;
            }

            if (t0 < 0)
            {
                t0 = t1;
                if (t0 < 0)
                {
                    point = linePoint;
                    return false;
                }
            }

            point = linePoint + lineDirection.normalized * t0;
            return true;
        }

        public static ShpereColliderData GetSphereData(SphereCollider sphereCollider)
        {
            float maxSide = Mathf.Max(sphereCollider.transform.localScale.x, sphereCollider.transform.localScale.y, sphereCollider.transform.localScale.z);

            return new ShpereColliderData()
            {
                radius = sphereCollider.radius * sphereCollider.radius * maxSide * maxSide,
                position = sphereCollider.transform.position + sphereCollider.center * maxSide
            };
        }

        public struct ShpereColliderData
        {
            public float radius;
            public Vector3 position;
        }
        #endregion

        #region Box intersection

        public static bool RayBoxColliderIntersection(BoxCollider boxCollider, Vector3 rayOrigin, Vector3 rayDirection, out Vector3 hitPosittion)
        {
            var direction = rayDirection.normalized * float.MaxValue;
            var boxTransform = boxCollider.transform;

            var center = boxTransform.position + boxCollider.center;
            Vector3 size = new Vector3(boxTransform.lossyScale.x * boxCollider.size.x, boxTransform.lossyScale.y * boxCollider.size.y, boxTransform.lossyScale.z * boxCollider.size.z);

            var max = center + Vector3.up * size.y / 2 + Vector3.forward * size.z / 2 + Vector3.right * size.x / 2;
            var min = center + Vector3.down * size.y / 2 + Vector3.back * size.z / 2 + Vector3.left * size.x / 2;

            Quaternion invertedRotation = Quaternion.Inverse(boxTransform.rotation);
            var rayPositionInverted = invertedRotation * (rayOrigin - center) + center;
            rayDirection = invertedRotation * (direction - center) + center;

            float tfirst = 0.0f, tlast = 1.0f;

            hitPosittion = Vector3.zero;
            if (!RaySlabIntersect(rayPositionInverted.x, rayDirection.x, min.x, max.x, ref tfirst, ref tlast))
            {
                return false;
            }
            if (!RaySlabIntersect(rayPositionInverted.y, rayDirection.y, min.y, max.y, ref tfirst, ref tlast))
            {
                return false;
            }
            if (!RaySlabIntersect(rayPositionInverted.z, rayDirection.z, min.z, max.z, ref tfirst, ref tlast))
            {
                return false;
            }

            if (tfirst < tlast)
            {
                hitPosittion = rayOrigin + direction * tfirst;
            }
            else
            {
                hitPosittion = rayOrigin + direction * tlast;
            }

            return true;
        }
        
        public static bool RayAABBIntersect(Vector3 rayOrigin, Vector3 rayDirection, AABB aabb, out Vector3 hitPosittion)
        {
            Vector3 min = aabb.Min(), max = aabb.Max(), direction = rayDirection.normalized * float.MaxValue;
            float tfirst = 0.0f, tlast = 1.0f;

            hitPosittion = Vector3.zero;
            if (!RaySlabIntersect(rayOrigin.x, direction.x, min.x, max.x, ref tfirst, ref tlast))
            {
                return false;
            }
            if (!RaySlabIntersect(rayOrigin.y, direction.y, min.y, max.y, ref tfirst, ref tlast))
            {
                return false;
            }
            if (!RaySlabIntersect(rayOrigin.z, direction.z, min.z, max.z, ref tfirst, ref tlast))
            {
                return false;
            }

            if (tfirst < tlast)
            {
                hitPosittion = rayOrigin + direction * tfirst;
            }
            else
            {
                hitPosittion = rayOrigin + direction * tlast;
            }

            return true;
        }

        public static bool RayOOBIntersect(Vector3 rayOrigin, Vector3 rayDirection, OBB obb, out Vector3 hitPosittion)
        {
            var boxTransform = obb.transform;
            var direction = rayDirection.normalized * float.MaxValue;
            var center = boxTransform.position;
            var size = boxTransform.lossyScale;

            var max = center + Vector3.up * size.y / 2 + Vector3.forward * size.z / 2 + Vector3.right * size.x / 2;
            var min = center + Vector3.down * size.y / 2 + Vector3.back * size.z / 2 + Vector3.left * size.x / 2;

            Quaternion invertedRotation = Quaternion.Inverse(boxTransform.rotation);
            var rayOriginInv = invertedRotation * (rayOrigin - center) + center;
            rayDirection = invertedRotation * (direction - center) + center;

            float tfirst = 0.0f, tlast = 1.0f;

            hitPosittion = Vector3.zero;
            if (!RaySlabIntersect(rayOriginInv.x, rayDirection.x, min.x, max.x, ref tfirst, ref tlast))
            {
                return false;
            }
            if (!RaySlabIntersect(rayOriginInv.y, rayDirection.y, min.y, max.y, ref tfirst, ref tlast))
            {
                return false;
            }
            if (!RaySlabIntersect(rayOriginInv.z, rayDirection.z, min.z, max.z, ref tfirst, ref tlast))
            {
                return false;
            }

            if (tfirst < tlast)
            {
                hitPosittion = rayOrigin + direction * tfirst;
            }
            else
            {
                hitPosittion = rayOrigin + direction * tlast;
            }

            return true;
        }

        public static bool RayAABBoxIntersect(Vector3 rayOrigin, Vector3 Dir, Vector3 Min, Vector3 Max, ref float tfirst, ref float tlast)
        {
            tfirst = 0.0f;
            tlast = 1.0f;

            if (!RaySlabIntersect(rayOrigin.x, Dir.x, Min.x, Max.x, ref tfirst, ref tlast))
            {
                return false;
            }
            if (!RaySlabIntersect(rayOrigin.y, Dir.y, Min.y, Max.y, ref tfirst, ref tlast))
            {
                return false;
            }
            if (!RaySlabIntersect(rayOrigin.z, Dir.z, Min.z, Max.z, ref tfirst, ref tlast))
            {
                return false;
            }

            return true;
        }

        static bool RaySlabIntersect(float start, float dir, float min, float max, ref float tfirst, ref float tlast)
        {
            if (Mathf.Abs(dir) < 1.0E-8)
            {
                return (start < max && start > min);
            }

            float tmin = (min - start) / dir;
            float tmax = (max - start) / dir;
            if (tmin > tmax)
            {
                float temp = tmin;
                tmin = tmax;
                tmax = temp;
            }

            if (tmax < tfirst || tmin > tlast)
            {
                return false;
            }

            if (tmin > tfirst)
            {
                tfirst = tmin;
            }
            if (tmax < tlast)
            {
                tlast = tmax;
            }
            return true;
        }

        #endregion
    }
}
