using UnityEngine;
using UnityEngine.Events;

namespace GameJam
{
    public abstract class TriggerBase : MonoBehaviour
    {
        [Header("Event")]
        public UnityEvent OnTrigger;

        protected bool triggered;

        public virtual void Trigger()
        {
            triggered = true;
            OnTrigger?.Invoke();
        }
    }
}