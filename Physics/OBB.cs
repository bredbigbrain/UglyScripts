using UnityEngine;

namespace Ugly
{
    [ExecuteInEditMode]
    public class OBB : MonoBehaviour
    {
        public Vector3 Min()
        {
            return RotateAroundCenter(MinLocal());
        }

        public Vector3 Max()
        {
            return RotateAroundCenter(MaxLocal());
        }

        public Vector3 MinLocal()
        {
            return transform.position + Vector3.down * transform.lossyScale.y / 2 + Vector3.back * transform.lossyScale.z / 2 + Vector3.left * transform.lossyScale.x / 2;
        }

        public Vector3 MaxLocal()
        {
            return transform.position + Vector3.up * transform.lossyScale.y / 2 + Vector3.forward * transform.lossyScale.z / 2 + Vector3.right * transform.lossyScale.x / 2;
        }

        public Vector3 RotateAroundCenter(Vector3 Point)
        {
            return transform.rotation * (Point - transform.position) + transform.position;
        }

        public Vector3 RotateAroundCenterInverted(Vector3 Point)
        {
            return Quaternion.Inverse(transform.rotation) * (Point - transform.position) + transform.position;
        }

        private void OnDrawGizmos()
        {
            var min = Min();
            var max = Max();

            var localMin = MinLocal();
            var localMax = MaxLocal();

            Gizmos.DrawLine(min, RotateAroundCenter(localMin + Vector3.up * transform.lossyScale.y));
            Gizmos.DrawLine(min, RotateAroundCenter(localMin + Vector3.forward * transform.lossyScale.z));
            Gizmos.DrawLine(min, RotateAroundCenter(localMin + Vector3.right * transform.lossyScale.x));

            Gizmos.DrawLine(max, RotateAroundCenter(localMax + Vector3.down * transform.lossyScale.y));
            Gizmos.DrawLine(max, RotateAroundCenter(localMax + Vector3.back * transform.lossyScale.z));
            Gizmos.DrawLine(max, RotateAroundCenter(localMax + Vector3.left * transform.lossyScale.x));

            Gizmos.DrawLine(RotateAroundCenter(localMin + Vector3.forward * transform.lossyScale.z), RotateAroundCenter(localMin + Vector3.right * transform.lossyScale.x + Vector3.forward * transform.lossyScale.z));
            Gizmos.DrawLine(RotateAroundCenter(localMin + Vector3.forward * transform.lossyScale.z), RotateAroundCenter(localMin + Vector3.up * transform.lossyScale.y + Vector3.forward * transform.lossyScale.z));
            Gizmos.DrawLine(RotateAroundCenter(localMin + Vector3.right * transform.lossyScale.x), RotateAroundCenter(localMin + Vector3.right * transform.lossyScale.x + Vector3.forward * transform.lossyScale.z));
            Gizmos.DrawLine(RotateAroundCenter(localMin + Vector3.right * transform.lossyScale.x), RotateAroundCenter(localMin + Vector3.up * transform.lossyScale.y + Vector3.right * transform.lossyScale.x));
            Gizmos.DrawLine(RotateAroundCenter(localMin + Vector3.up * transform.lossyScale.y), RotateAroundCenter(localMin + Vector3.up * transform.lossyScale.y + Vector3.forward * transform.lossyScale.z));
            Gizmos.DrawLine(RotateAroundCenter(localMin + Vector3.up * transform.lossyScale.y), RotateAroundCenter(localMin + Vector3.up * transform.lossyScale.y + Vector3.right * transform.lossyScale.x));

            Gizmos.DrawWireSphere(min, 0.025f);
            Gizmos.DrawWireSphere(max, 0.025f);
        }
    }
}