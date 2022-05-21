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

        private Entity owner;

        public Entity Owner => owner;

        public void Setup(Entity owner, int level)
        {
            this.owner = owner;

            // change faction to owner's
            Faction = owner.Faction;

            // setup stats
            Level.SetLevel(level);
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