using UnityEngine;

namespace GameJam
{
    public static class TransformExtensions
    {
        public static bool IsFacingTarget(this Transform transform, Transform target, float dotTreshold = 0f)
        {
            var vectorToTarget = target.position - transform.position;
            vectorToTarget.Normalize();

            float dot = Vector3.Dot(transform.forward, vectorToTarget);

            return dot >= dotTreshold;
        }
    }
}