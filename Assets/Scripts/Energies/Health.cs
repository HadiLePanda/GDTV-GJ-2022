using UnityEngine;

namespace GameJam
{
    public interface IHealthBonus
    {
        int GetHealthBonus(int baseHealth);
    }

    public class Health : Energy
    {
        [Header("Health")]
        [SerializeField] private LinearInt baseHealth = new LinearInt { baseValue = 1 };

        // cache components that give a bonus (attributes, inventory, etc.)
        // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
        private IHealthBonus[] _bonusComponents;
        private IHealthBonus[] bonusComponents => _bonusComponents ??= GetComponents<IHealthBonus>();

        public override int Max
        {
            get
            {
                int bonus = 0;
                int baseThisLevel = baseHealth.Get(level.Current);
                foreach (IHealthBonus bonusComponent in bonusComponents)
                {
                    bonus += bonusComponent.GetHealthBonus(baseThisLevel);
                }
                return baseThisLevel + bonus;
            }
        }
    }
}