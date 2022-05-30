using System;
using UnityEngine;

namespace GameJam
{
    [RequireComponent(typeof(Level))]
    public abstract class Energy : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected Level level;
        [SerializeField] protected Health health;

        [Header("Energy Settings")]
        [SerializeField] protected bool spawnFull = true;
        [SerializeField] protected int recoveryPerTick = 0;
        [Tooltip("The amount of seconds between each recovery interval tick")]
        [SerializeField] protected float recoveryTickRate = 1f;
        
        private int current = 0;

        // (may depend on buffs, items, etc.)
        public abstract int Max { get; }

        // (may depend on buffs, items etc.)
        public abstract int RecoveryPerTick { get; }

        public int Current
        {
            get => Mathf.Min(current, Max);
            set => SetCurrent(value);
        }

        public void SetCurrent(int value)
        {
            bool emptyBefore = Current == 0;

            current = Mathf.Clamp(value, 0, Max);

            OnChanged?.Invoke(current, Max);

            if (Current == 0 && !emptyBefore)
            {
                OnEmpty?.Invoke();
            }
        }

        public float Percent() => Current != 0 && Max != 0
            ? Current / (float)Max
            : 0;

        // old value, new value
        public delegate void EnergyChangedDelegate(int oldValue, int newValue);
        public event EnergyChangedDelegate OnChanged;
        public delegate void EnergyRecoveredDelegate(int amount);
        public event EnergyRecoveredDelegate OnRecovered;
        public event Action OnEmpty;

        public void Add(int value)
        {
            Current += value;
        }

        public void Remove(int value)
        {
            Current -= value;
        }

        public void Deplete()
        {
            Current = 0;
        }

        private void Awake()
        {
            Current = Max;
        }

        private void Start()
        {
            // set full energy on start if needed
            if (spawnFull)
            {
                Current = Max;
            }

            // recovery energy periodically
            InvokeRepeating(nameof(Recover), recoveryTickRate, recoveryTickRate);
        }

        public virtual void Recover()
        {
            // don't recover while dead
            if (!enabled || health.Current <= 0) { return; }

            int valueBefore = current;
            int recoveryAmount = RecoveryPerTick;
            Add(recoveryAmount);

            // if we gained something, notify
            if (Current != valueBefore)
                OnRecovered?.Invoke(recoveryAmount);
        }

        private void OnValidate()
        {
            // auto-references
            if (health == null && TryGetComponent(out Health healthComponent))
            {
                health = healthComponent;
            }
            if (level == null && TryGetComponent(out Level levelComponent))
            {
                level = levelComponent;
            }
        }
    }
}