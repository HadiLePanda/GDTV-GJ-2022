using UnityEngine;

namespace GameJam
{
    [RequireComponent(typeof(Collider))]
    public class TestManaArea : MonoBehaviour
    {
        public int mana = 50;

        private void OnTriggerEnter(Collider col)
        {
            if (col.TryGetComponent(out Entity entity))
            {
                entity.Mana.Add(mana);
                entity.Combat.ShowManaPopup(mana);
            }
        }
    }
}