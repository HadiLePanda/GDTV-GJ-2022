using UnityEngine;

namespace GameJam
{
    public class TimerTrigger : TriggerBase
    {
        [Header("Timer Settings")]
        public float timer = 5f;

        private float startTime;

        private void Start()
        {
            startTime = Time.time;
        }

        private void Update()
        {
            if (triggered) { return; }

            if (Time.time - startTime >= timer)
            {
                Trigger();
            }
        }
    }
}