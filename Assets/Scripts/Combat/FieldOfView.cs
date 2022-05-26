using UnityEngine;

namespace GameJam
{
    public class FieldOfView : MonoBehaviour
    {
        [Header("Settings")]
        [Range(0, 360)]
        public float angle = 140f;
        // radius of the FOV = the skill's cast range, or the monster aggroArea range etc..

        public bool IsInFOV(Transform target)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            return Vector3.Angle(transform.forward, directionToTarget) < angle / 2;
        }
    }
}