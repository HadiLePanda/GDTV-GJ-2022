using UnityEngine;

namespace GameJam
{
    public interface IManaBonus
    {
        int GetManaBonus(int baseMana);
        int GetManaRecoveryBonus();
    }

    public class Mana : Energy
    {
        [Header("Mana")]
        [SerializeField] private LinearInt baseMana = new LinearInt { baseValue = 1 };

        // cache components that give a bonus (attributes, inventory, etc.)
        // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
        private IManaBonus[] _bonusComponents;
        private IManaBonus[] bonusComponents => _bonusComponents ??= GetComponents<IManaBonus>();

        public override int Max
        {
            get
            {
                int bonus = 0;
                int baseThisLevel = baseMana.Get(level.Current);
                foreach (IHealthBonus bonusComponent in bonusComponents)
                {
                    bonus += bonusComponent.GetHealthBonus(baseThisLevel);
                }
                return baseThisLevel + bonus;
            }
        }

        public override int RecoveryPerTick
        {
            get
            {
                int bonus = 0;
                foreach (IManaBonus bonusComponent in bonusComponents)
                {
                    bonus += bonusComponent.GetManaRecoveryBonus();
                }
                return recoveryPerTick + bonus;
            }
        }
    }
}