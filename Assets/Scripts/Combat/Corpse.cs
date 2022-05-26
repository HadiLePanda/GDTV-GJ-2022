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

        public abstract Entity GetEntity();
        public abstract int GetEntityLevel();

        // resurrection ========================================
        public abstract bool CanBeResurrected();

        public virtual void ResurrectAsMinion(Entity owner)
        {
            if (!CanBeResurrected()) { return; }

            Minion minionInstance = SpawnMinion(owner, GetEntityLevel());
            PlayResurrectionEffects(minionInstance.transform.position);

            if (entityInstance != null)
            {
                entityInstance.RemoveCorpse();
            }
        }

        protected Minion SpawnMinion(Entity owner, int level)
        {
            Minion minionInstance = Instantiate(minionPrefab, transform.position, transform.rotation);

            minionInstance.Setup(owner, level, minionInstance.GetLifetime(), GetEntity());

            return minionInstance;
        }

        protected void PlayResurrectionEffects(Vector3 position)
        {
            if (resurrectEffect != null)
                Game.Vfx.SpawnParticle(resurrectEffect, position);

            if (resurrectSound != null)
                Game.Sfx.PlayWorldSfx(resurrectSound, position);
        }
    }
}