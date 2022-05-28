using UnityEngine;

namespace GameJam
{
    [RequireComponent(typeof(BoxCollider))]
    public class CollisionTrigger : TriggerBase
    {
        [Header("Collision References")]
        public BoxCollider Collider;

        [Header("Collision Settings")]
        public LayerMask collisionLayers;

        private void OnTriggerEnter(Collider col)
        {
            if (triggered) { return; }

            if (collisionLayers.IsInLayerMask(col.gameObject))
            {
                Trigger();
            }
        }

        private void OnValidate()
        {
            if (Collider == null)
            {
                Collider = GetComponentInParent<BoxCollider>();
            }
        }

        private void OnDrawGizmos()
        {
            if (Collider == null) { return; }

            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(transform.position + Collider.center, Collider.bounds.size);
        }
    }
}