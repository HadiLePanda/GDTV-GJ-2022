using UnityEngine;

namespace GameJam
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleAttractorSkillEffect : SkillEffect
    {
        [Header("References")]
        [SerializeField] private particleAttractorLinear particleAttractor;

        [Header("Settings")]
        [SerializeField] private float speed = 3;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private bool attractTowardsTarget;

        private ParticleSystem particle;
        private Transform targetTransform;
        private Vector3 startSpawnPosition;
        private float spawnTime;

        private void Start()
        {
            particle = GetComponent<ParticleSystem>();
        }

        public void Setup(Entity caster, Transform targetTransform)
        {
            this.caster = caster;
            this.targetTransform = targetTransform;
            startSpawnPosition = targetTransform.position;
            particleAttractor.speed = speed;

            spawnTime = Time.time;
        }

        private void Update()
        {
            ProcessLifetime();

            // destroy self if caster or target disappeared
            if (caster == null)
            {
                Destroy(gameObject);

                return;
            }

            Vector3 targetPosition = Vector3.zero;
            if (targetTransform == null && startSpawnPosition != Vector3.zero)
            {
                targetPosition = startSpawnPosition;
            }
            else
            {
                targetPosition = targetTransform.position;
            }

            // follow source and set target object to the destination
            transform.position = attractTowardsTarget ? caster.Collider.bounds.center : targetPosition;
            particleAttractor.target.position = attractTowardsTarget ? targetPosition : caster.Collider.bounds.center;
        }

        private void ProcessLifetime()
        {
            if (Time.time - spawnTime >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}