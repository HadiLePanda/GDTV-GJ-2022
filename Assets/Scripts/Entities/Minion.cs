using UnityEngine;

namespace GameJam
{
    public class Minion : Entity
    {
        [Header("Minion Sounds")]
        [SerializeField] private AudioClip[] ambientSounds;
        public float ambientSoundProbability = 0.01f;

        [Header("Minion Settings")]
        [Tooltip("In Seconds.")]
        [SerializeField] private float lifetime = 10f;

        public AudioClip[] GetAmbientSounds() => ambientSounds;

        private float remainingLifetime;
        public float RemainingLifetime
        {
            get => remainingLifetime;
            set
            {
                remainingLifetime = Mathf.Clamp(value, 0f, GetLifetime());
            }
        }

        private Entity owner;
        public Entity Owner => owner;

        public float GetLifetime() => lifetime;
        public void AddLifetime(float time) => RemainingLifetime += time;

        private void Awake()
        {
            RemainingLifetime = GetLifetime();
        }

        private void Update()
        {
            ProcessLifetime();
        }

        private void ProcessLifetime()
        {
            RemainingLifetime -= Time.deltaTime;

            // lifetime ran out, minion dies
            if (RemainingLifetime <= 0)
            {
                Health.Deplete();
            }
        }

        public void Setup(Entity owner, int level, float lifetime)
        {
            this.owner = owner;

            // change faction to owner's
            Faction = owner.Faction;

            // setup runtime stats
            Level.SetLevel(level);
            RemainingLifetime = lifetime;
        }

        // combat =====================================
        public override bool CanAttack(Entity entity)
        {
            return base.CanAttack(entity) &&
                    entity != owner && (owner != null && owner.CanAttack(entity));
        }

        // minions can't resurrect other corpses
        public override bool CanResurrect(Entity entity) => false;

        // death ======================================
        protected override void OnDeath()
        {
            base.OnDeath();

            Mana.Deplete();
        }
    }
}