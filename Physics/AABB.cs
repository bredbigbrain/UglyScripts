using UnityEngine;

namespace Ugly
{
    [ExecuteInEditMode]
    public class AABB : MonoBehaviour
    {

        public bool wired = false;

        public Vector3 Min()
        {
            return transform.position + Vector3.down * transform.lossyScale.y / 2 + Vector3.back * transform.lossyScale.z / 2 + Vector3.left * transform.lossyScale.x / 2;
        }

        public Vector3 Max()
        {
            return transform.position + Vector3.up * transform.lossyScale.y / 2 + Vector3.forward * transform.lossyScale.z / 2 + Vector3.right * transform.lossyScale.x / 2;
        }

        private void OnDrawGizmos()
        {
            var min = Min();
            var max = Max();

            if (wired)
            {
                Gizmos.DrawWireCube(transform.position, transform.lossyScale);
                Gizmos.DrawWireSphere(min, 0.025f * transform.lossyScale.x);
                Gizmos.DrawWireSphere(max, 0.025f * transform.lossyScale.x);
            }
            else
            {
                Gizmos.DrawCube(transform.position, transform.lossyScale);
                Gizmos.DrawSphere(min, 0.025f * transform.lossyScale.x);
                Gizmos.DrawSphere(max, 0.025f * transform.lossyScale.x);
            }

        }
    }
}