using System;
using UnityEngine;

namespace GameJam
{
    [RequireComponent(typeof(Level))]
    [DisallowMultipleComponent]
    public class Experience : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected Level level = null;

        [Header("Settingns")]
        [Tooltip("The max level difference where you start gaining no experience. Useful to hard cap not getting exp from lower level mobs.")]
        [SerializeField] protected int maxLevelDifference;

        [Header("Stats")]
        [SerializeField] [HideInInspector] protected long _current = 0;
        public long Current { get => _current; set => SetValue(value); }

        // required experience grows by 10% each level (like Runescape)
        [SerializeField] protected ExponentialLong _max = new ExponentialLong { multiplier = 100, baseValue = 1.1f };
        public long Max => _max.Get(level.Current);

        public void SetValue(long value)
        {
            if (value <= _current)
            {
                // decrease experience
                _current = Math.Max(value, 0);
            }
            else
            {
                // increase experience and handle level ups
                // set the new value (which might be more than expMax)
                _current = value;

                // now see if we leveled up (possibly more than once too)
                // (can't level up if already max level)
                while (_current >= Max && level.Current < level.Max)
                {
                    // subtract current level's required exp, then level up
                    _current -= Max;
                    level.SetLevel(level.Current + 1);

                    // call event
                    OnLevelUp?.Invoke();
                }

                // set to expMax if there is still too much exp remaining
                if (_current > Max) { _current = Max; }
            }
        }

        public event Action OnLevelUp;

        public float Percent()
            => Current != 0 && Max != 0 ? (float)Current / (float)Max : 0;

        // players gain exp depending on their level. if a player has a lower level
        // than the monster, then he gains more exp (up to 100% more) and if he has
        // a higher level, then he gains less exp (up to 100% less)
        // -> see tests for several commented examples!
        public static long BalanceExperienceReward(long reward, int attackerLevel, int victimLevel, int maxLevelDifference = 20)
        {
            // level difference 10 means 10% extra/less per level.
            // level difference 20 means 5% extra/less per level.
            // so the percentage step depends on the level difference:
            float percentagePerLevel = 1f / maxLevelDifference;

            // calculate level difference. it should cap out at +- maxDifference to
            // avoid power level exploits where a level 1 player kills a level 100
            // monster and immediately gets millions of experience points and levels
            // up to level 50 instantly. this would be bad for MMOs.
            // instead, we only consider +- maxDifference.
            int levelDiff = Mathf.Clamp(victimLevel - attackerLevel, -maxLevelDifference, maxLevelDifference);

            // calculate the multiplier. it will be +10%, +20% etc. when killing
            // higher level monsters. it will be -10%, -20% etc. when killing lower
            // level monsters.
            float multiplier = 1 + levelDiff * percentagePerLevel;

            // calculate reward
            return Convert.ToInt64(reward * multiplier);
        }

        private void OnValidate()
        {
            // auto-reference entity
            if (level == null && TryGetComponent(out Level levelComponent))
            {
                level = levelComponent;
            }
        }
    }
}
