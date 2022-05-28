using TMPro;
using UnityEngine;

namespace GameJam
{
    public class GroundCorpse : Corpse
    {
        [Header("Ground Corpse References")]
        [SerializeField] private Entity corpseEntity;
        [SerializeField] private TextMeshPro entityNameTextMesh;

        [Header("Settings")]
        [SerializeField] private bool useCustomLevel;
        [SerializeField] private int minionLevel = 1;
        [SerializeField] private int numberOfCorpses = 1;

        protected int remainingCorpses;

        public override Entity GetEntityPrefab() => corpseEntity;
        public override int GetEntityLevel() => GetEntityPrefab() == null || useCustomLevel
                                                ? minionLevel
                                                : GetEntityPrefab().Level.Current;

        private void Start()
        {
            remainingCorpses = numberOfCorpses;
            UpdateTextDisplay();
        }

        public override bool CanBeResurrected()
        {
            return GetEntityPrefab() != null && minionPrefab != null;
        }

        public override void ResurrectAsMinion(Entity owner)
        {
            base.ResurrectAsMinion(owner);

            remainingCorpses -= 1;
            UpdateTextDisplay();

            // destroy ground corpse if no more corpses can be spawned
            if (remainingCorpses <= 0)
            {
                Destroy(gameObject);
            }
        }

        private void UpdateTextDisplay()
        {
            entityNameTextMesh.text = $"{corpseEntity.gameObject.name} ({remainingCorpses})";
        }
    }
}