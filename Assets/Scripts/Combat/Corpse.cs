using UnityEngine;

namespace GameJam
{
    [DisallowMultipleComponent]
    public abstract class Corpse : MonoBehaviour
    {
        //TODO maybe add a ragdoll and a setup method to be called from the entity dying
        [Header("Corpse Minion")]
        [SerializeField] protected Minion minionPrefab;

        [Header("Resurrection Effects")]
        [SerializeField] protected GameObject resurrectEffect;
        [SerializeField] protected AudioClip resurrectSound;

        public GameObject GetResurrectEffect() => resurrectEffect;
        public AudioClip GetResurrectSound() => resurrectSound;

        protected Entity entityInstance;

        public abstract Entity GetEntityPrefab();
        public abstract int GetEntityLevel();
        public Entity GetEntityInstance() => entityInstance;

        // resurrection ========================================
        public abstract bool CanBeResurrected();

        public virtual void ResurrectAsMinion(Entity owner)
        {
            if (!CanBeResurrected()) { return; }

            Minion minionInstance = SpawnMinion(owner, GetEntityLevel());
            PlayResurrectionEffects(minionInstance.transform.position);

            if (entityInstance != null)
            {
                entityInstance.RemoveEntity();
            }
        }

        protected Minion SpawnMinion(Entity owner, int level)
        {
            Minion minionInstance = Instantiate(minionPrefab, transform.position, transform.rotation);

            Entity corpseEntity = GetEntityInstance() != null ? GetEntityInstance() : GetEntityPrefab();
            minionInstance.Setup(owner, level, corpseEntity);

            return minionInstance;
        }

        protected void PlayResurrectionEffects(Vector3 position)
        {
            if (resurrectEffect != null)
                Game.Vfx.SpawnParticle(resurrectEffect, position);

            if (resurrectSound != null)
                Game.Audio.PlayWorldSfx(resurrectSound, position);
        }
    }
}