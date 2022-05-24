using UnityEngine;

namespace GameJam
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class ProjectileSkillEffect : SkillEffect
    {
        [Header("References")]
        [SerializeField] private GameObject explosionEffect;
        [SerializeField] private AudioClip explosionSound;

        [Header("Settings")]
        [SerializeField] private ForceMode rbForceMode = ForceMode.VelocityChange;
        [SerializeField] private bool followCasterLook;

        private Rigidbody rb;
        private float spawnTime;

        ProjectileData data;

        Vector3 castForwardDirection;

        public GameObject GetExplosionEffect() => explosionEffect;
        public AudioClip GetExplosionSound() => explosionSound;
        public bool HasTarget() => target != null;

        public void Setup(Entity caster, ProjectileData projectileData)
        {
            this.caster = caster;
            this.data = projectileData;

            castForwardDirection = caster.transform.forward;

            spawnTime = Time.time;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            spawnTime = Time.time;
        }

        private void Update()
        {
            if (caster == null && target == null)
            {
                // something went wrong because no fight created this projectile, destroy
                Destroy(gameObject);
            }

            ProcessLifetime();
        }

        private void FixedUpdate()
        {
            ProcessMovement();
        }

        // lifetime =================================================================
        private void ProcessLifetime()
        {
            if (Time.time - spawnTime >= data.lifetime)
            {
                Destroy(gameObject);
            }
        }

        // movement =================================================================
        private void ProcessMovement()
        {
            Vector3 forceDirection = Vector3.zero;
            if (followCasterLook)
            {
                forceDirection = caster.transform.forward;
            }
            else if (HasTarget())
            {
                Vector3 directionToTarget = target.transform.position - transform.position;
                forceDirection = directionToTarget;
            }
            else
            {
                forceDirection = castForwardDirection;
            }
            rb.AddForce(forceDirection * data.speed, rbForceMode);
        }
        // damage ====================================================================
        private void ApplyAttack(Entity target)
        {
            // apply damage
            caster.Combat.DealDamage(target, data.damage, Vector3.zero, -Vector3.forward, data.stunChance, data.stunTime);

            // apply drain
            if (data.manaDrain > 0 || data.healthDrain > 0)
            {
                caster.Combat.Drain(target, data.manaDrain, data.healthDrain);
            }
        }

        // destruction ==============================================================
        private void DestroyProjectile()
        {
            PlayHitEffects();

            Destroy(gameObject);
        }

        // effects ==================================================================
        private void PlayHitEffects()
        {
            if (GetExplosionEffect() != null)
                Game.Vfx.SpawnParticle(explosionEffect, transform.position, Quaternion.identity);

            if (GetExplosionSound() != null)
                Game.Sfx.PlayWorldSfx(explosionSound, transform.position);
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.TryGetComponent(out Entity entity))
            {
                // if this projectile is homing to a target, it can only hit target
                if (target != null)
                {
                    if (entity != target) return;

                    ApplyAttack(entity);
                    DestroyProjectile();
                    return;
                }
                // otherwise check if can be attacked by the caster
                else if (caster.CanAttack(entity))
                {
                    ApplyAttack(entity);
                    DestroyProjectile();
                }
            }
        }
    }
}