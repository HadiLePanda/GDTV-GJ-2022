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
        [SerializeField] private LayerMask blockingLayers;

        private Rigidbody rb;
        private float spawnTime;
        private Vector3 castForwardDirection;

        ProjectileData data;

        public GameObject GetExplosionEffect() => explosionEffect;
        public AudioClip GetExplosionSound() => explosionSound;
        public bool HasTarget() => target != null;

        public void Setup(Entity caster, ProjectileData projectileData)
        {
            this.caster = caster;
            this.data = projectileData;

            if (caster is Player player)
            {
                // for the player we get the direction of cast towards the look pointer
                castForwardDirection = (player.Movement.look.lookTarget.transform.position.ChangeY(0) - player.Skills.effectMount.position.ChangeY(0)).normalized;
            }
            else
            {
                // for other entities, just shoot forward
                castForwardDirection = caster.transform.forward;
            }
            

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
                Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
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
            // get hit normal
            Quaternion hitRotation = Quaternion.FromToRotation(target.transform.position, transform.transform.position);
            Vector3 hitNormal = hitRotation.eulerAngles;

            // apply damage
            caster.Combat.DealDamage(target,
                                    data.damage,
                                    target.transform.position, hitNormal,
                                    data.stunChance,
                                    data.stunTime);

            // apply drain
            if (data.manaDrain > 0 || data.healthDrain > 0)
            {
                caster.Combat.Drain(target,
                                    data.manaDrain,
                                    data.healthDrain);
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
                Game.Audio.PlayWorldSfx(explosionSound, transform.position);
        }

        private void OnTriggerEnter(Collider col)
        {
            // collide with blocking obstacles
            if ((blockingLayers.value & (1 << col.transform.gameObject.layer)) > 0)
            {
                DestroyProjectile();
                return;
            }

            // hit entities
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