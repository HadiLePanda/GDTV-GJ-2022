using System;
using UnityEngine;

namespace GameJam
{
    [RequireComponent(typeof(Level))]
    public abstract class Energy : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] protected Level level;

        protected int current = 0;

        // (may depend on buffs, items, etc.)
        public abstract int Max { get; }

        public int Current
        {
            get => Mathf.Min(current, Max);
            set => SetCurrent(value);
        }

        public float Percent() => Current != 0 && Max != 0
            ? Current / (float)Max
            : 0;

        // old value, new value
        public delegate void EnergyChangedDelegate(int oldValue, int newValue);
        public event EnergyChangedDelegate OnChanged;
        public event Action OnEmpty;

        private void Start()
        {
            Current = Max;
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
    }
}