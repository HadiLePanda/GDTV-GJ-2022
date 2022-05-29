using UnityEngine;

namespace GameJam
{
    public class AoeSkillEffect : SkillEffect
    {
        public ParticleSystem cookieParticle;
        public bool followTarget = true;

        private void OnEnable()
        {
            if (target != null)
            {
                transform.position = target.Collider.bounds.center;
            }

            cookieParticle.gameObject.SetActive(false);
        }

        public void Setup(float radius)
        {
            var particle = cookieParticle.main;
            particle.startSize = radius;
            cookieParticle.gameObject.SetActive(true);
        }

        private void Update()
        {
            // follow the target's position
            if (target != null && followTarget)
            {
                transform.position = target.Collider.bounds.center;
            }

            // destroy self if particle ended
            if (!cookieParticle.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
}