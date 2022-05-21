using UnityEngine;

namespace GameJam
{
    [DisallowMultipleComponent]
    public class Level : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int current = 1;
        [SerializeField] private int max = 100;

        public int Current => current;
        public int Max => max;

        public void SetLevel(int newLevel) => current = Mathf.Clamp(newLevel, 1, max);
        public void SetMaxLevel(int newMaxLevel) => max = newMaxLevel;

        private void OnValidate()
        {
            current = Mathf.Clamp(current, 1, max);
        }
    }
}