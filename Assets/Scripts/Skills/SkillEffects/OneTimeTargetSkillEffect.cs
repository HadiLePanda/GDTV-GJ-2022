using UnityEngine;

namespace GameJam
{
    [RequireComponent(typeof(ParticleSystem))]
    public class OneTimeTargetSkillEffect : SkillEffect
    {
        public bool followTarget = true;

        private ParticleSystem particle;

        //TODO Make a OneTimeAoeSkillEffect
        private void Start()
        {
            particle = GetComponent<ParticleSystem>();

            if (target != null)
            {
                transform.position = target.Collider.bounds.center;
            }
        }

        private void Update()
        {
            // follow the target's position
            if (target != null && followTarget)
            {
                transform.position = target.Collider.bounds.center;
            }

            // destroy self if target disappeared or particle ended
            if (!particle.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
}