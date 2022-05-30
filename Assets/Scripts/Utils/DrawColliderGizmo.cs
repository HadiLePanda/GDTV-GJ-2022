using UnityEngine;

namespace GameJam
{
    [SelectionBase]
    public class DrawColliderGizmo : MonoBehaviour
    {
        [Header("References")]
        public Collider Collider;

        private void OnDrawGizmos()
        {
            if (Collider == null) { return; }

            Gizmos.DrawWireCube(Collider.bounds.center, Collider.bounds.size);
        }
    }
}