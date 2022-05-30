using UnityEngine;

namespace GameJam
{
    public class AoeSkillEffect : SkillEffect
    {
        public ParticleSystem cookieParticle;
        public bool followTarget = true;

        private ParticleSystem[] particles;

        private void Awake()
        {
            particles = GetComponentsInChildren<ParticleSystem>();
        }

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
            var size = radius * 2;
            var mainParticle = cookieParticle.main;
            mainParticle.startSize = size;

            foreach (ParticleSystem particle in particles)
            {
                var particleMain = particle.main;
                particleMain.startSize = size;
            }

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