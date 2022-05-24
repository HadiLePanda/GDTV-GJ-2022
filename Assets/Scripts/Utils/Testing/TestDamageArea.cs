using UnityEngine;

namespace GameJam
{
    [RequireComponent(typeof(Collider))]
    public class TestDamageArea : MonoBehaviour
    {
        public int damage = 1;
        public float stunChance = 0.3f;
        public float stunTime = 1f;

        private void OnTriggerEnter(Collider col)
        {
            if (col.TryGetComponent(out Entity entity))
            {
                entity.Combat.DealDamage(entity, damage, Vector3.zero, -entity.transform.forward, stunChance, stunTime);
            }
        }
    }
}