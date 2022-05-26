using UnityEngine;

namespace GameJam
{
    public class Aura : MonoBehaviour
    {
        [Header("References")]
        public Entity entity;
        public GameObject NormalAuraRoot;
        public GameObject ResurrectableAuraRoot;

        private void OnEnable()
        {
            entity.Health.OnEmpty += OnDeath;
        }
        private void OnDisable()
        {
            entity.Health.OnEmpty -= OnDeath;
        }

        private void Start()
        {
            NormalAuraRoot.SetActive(true);
        }

        private void OnDeath()
        {
            NormalAuraRoot.SetActive(false);

            if (entity.TryGetComponent(out Corpse corpse) &&
                corpse.CanBeResurrected())
            {
                ResurrectableAuraRoot.SetActive(true);
            }
        }

        private void OnValidate()
        {
            if (entity == null && GetComponentInParent<Entity>() != null)
            {
                entity = GetComponentInParent<Entity>();
            }
        }
    }
}