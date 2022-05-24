using UnityEngine;

namespace GameJam
{
    [RequireComponent(typeof(Collider))]
    public class TestHealingArea : MonoBehaviour
    {
        public int heal = 1;

        private void OnTriggerEnter(Collider col)
        {
            if (col.TryGetComponent(out Entity entity))
            {
                entity.Combat.Heal(entity, heal);
            }
        }
    }
}