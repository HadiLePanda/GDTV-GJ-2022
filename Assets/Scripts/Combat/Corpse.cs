using UnityEngine;

namespace GameJam
{
    [DisallowMultipleComponent]
    public class Corpse : MonoBehaviour
    {
        //TODO maybe add a ragdoll and a setup method to be called from the entity dying

        [Header("References")]
        [SerializeField] private Entity entity;

        [Header("Corpse Minion")]
        [SerializeField] private Minion minionPrefab;

        [Header("Resurrection Effects")]
        [SerializeField] protected GameObject resurrectEffect;
        [SerializeField] protected AudioClip resurrectSound;

        public Entity Entity => entity;

        public GameObject GetResurrectEffect() => resurrectEffect;
        public AudioClip GetResurrectSound() => resurrectSound;

        // resurrection ========================================
        public bool CanBeResurrected() => !entity.IsAlive && minionPrefab != null;

        public void ResurrectAsMinion(Entity owner)
        {
            if (!CanBeResurrected()) { return; }

            Minion minionInstance = SpawnMinion(owner);
            PlayResurrectionEffects(minionInstance.transform.position);

            entity.RemoveCorpse();
        }

        private Minion SpawnMinion(Entity owner)
        {
            Minion minionInstance = Instantiate(minionPrefab, transform.position, transform.rotation);

            minionInstance.Setup(owner, entity.Level.Current, minionInstance.GetLifetime());

            return minionInstance;
        }

        private void PlayResurrectionEffects(Vector3 position)
        {
            if (resurrectEffect != null)
                Game.Vfx.SpawnParticle(resurrectEffect, position);

            if (resurrectSound != null)
                Game.Sfx.PlayWorldSfx(resurrectSound, position);
        }
    }
}