using UnityEngine;

namespace GameJam
{
    public class DeathTrigger : TriggerBase
    {
        [Header("Death Settings")]
        public Health health;

        private void OnEnable()
        {
            health.OnEmpty += HandleDied;
        }
        private void OnDisable()
        {
            health.OnEmpty -= HandleDied;
        }

        private void HandleDied()
        {
            if (triggered) { return; }

            Trigger();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 1.5f);
        }
    }
}