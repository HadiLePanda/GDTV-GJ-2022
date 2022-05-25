using UnityEngine;
using UnityEngine.Pool;

namespace GameJam
{
    public class NumberPopupManager : SingletonMonoBehaviour<NumberPopupManager>
    {
        [Header("References")]
        [SerializeField] private Transform popupRoot;

        [Header("Pooling")]
        [SerializeField] private NumberPopup damagePopup;
        [SerializeField] private NumberPopup healPopup;
        [SerializeField] private NumberPopup manaPopup;
        [SerializeField] private NumberPopup experiencePopup;
        [SerializeField] private int defaultPoolSize = 20;
        [SerializeField] private int maxPoolSize = 30;

        private IObjectPool<NumberPopup> damagePopupPool;
        private IObjectPool<NumberPopup> healPopupPool;
        private IObjectPool<NumberPopup> manaPopupPool;

        protected override void Awake()
        {
            base.Awake();

            damagePopupPool = new ObjectPool<NumberPopup>(
                createFunc: () => CreateDamagePopup(),
                actionOnGet: (poolObject) => GetPopupFromPool(poolObject),
                actionOnRelease: (poolObject) => ReturnPopupToPool(poolObject),
                actionOnDestroy: (poolObject) => Destroy(poolObject.gameObject),
                collectionCheck: false,
                defaultCapacity: defaultPoolSize,
                maxSize: maxPoolSize);
            
            healPopupPool = new ObjectPool<NumberPopup>(
                createFunc: () => CreateHealPopup(),
                actionOnGet: (poolObject) => GetPopupFromPool(poolObject),
                actionOnRelease: (poolObject) => ReturnPopupToPool(poolObject),
                actionOnDestroy: (poolObject) => Destroy(poolObject.gameObject),
                collectionCheck: false,
                defaultCapacity: defaultPoolSize,
                maxSize: maxPoolSize);

            manaPopupPool = new ObjectPool<NumberPopup>(
                createFunc: () => CreateManaPopup(),
                actionOnGet: (poolObject) => GetPopupFromPool(poolObject),
                actionOnRelease: (poolObject) => ReturnPopupToPool(poolObject),
                actionOnDestroy: (poolObject) => Destroy(poolObject.gameObject),
                collectionCheck: false,
                defaultCapacity: defaultPoolSize,
                maxSize: maxPoolSize);

            //damagePopupPool = new ObjectPool<NumberPopup>(CreateDamagePopup, GetDamagePopupFromPool, ReturnDamagePopupToPool);
            //healPopupPool = new ObjectPool<NumberPopup>(CreateHealPopup, GetHealPopupFromPool, ReturnHealPopupToPool);
        }

        // pools =================================================================
        private NumberPopup CreateDamagePopup()
        {
            var popupInstance = Instantiate(damagePopup, popupRoot);
            popupInstance.gameObject.name = $"(Pool) {damagePopup.name} {popupInstance.transform.GetSiblingIndex()}";
            popupInstance.SetPool(damagePopupPool);
            popupInstance.gameObject.SetActive(false);

            return popupInstance;
        }
        private NumberPopup CreateHealPopup()
        {
            var popupInstance = Instantiate(healPopup, popupRoot);
            popupInstance.gameObject.name = $"(Pool) {healPopup.name} {popupInstance.transform.GetSiblingIndex()}";
            popupInstance.SetPool(healPopupPool);
            popupInstance.gameObject.SetActive(false);

            return popupInstance;
        }
        private NumberPopup CreateManaPopup()
        {
            var popupInstance = Instantiate(manaPopup, popupRoot);
            popupInstance.gameObject.name = $"(Pool) {manaPopup.name} {popupInstance.transform.GetSiblingIndex()}";
            popupInstance.SetPool(manaPopupPool);
            popupInstance.gameObject.SetActive(false);

            return popupInstance;
        }
        private void GetPopupFromPool(NumberPopup popup) => popup.gameObject.SetActive(true);
        private void ReturnPopupToPool(NumberPopup popup) => popup.gameObject.SetActive(false);

        // popups ================================================================
        public NumberPopup SpawnDamagePopup(Vector3 position)
        {
            NumberPopup popupInstance = damagePopupPool.Get();
            popupInstance.Setup(position);

            return popupInstance;
        }

        public NumberPopup SpawnHealPopup(Vector3 position)
        {
            NumberPopup popupInstance = healPopupPool.Get();
            popupInstance.Setup(position);

            return popupInstance;
        }

        public NumberPopup SpawnManaPopup(Vector3 position)
        {
            NumberPopup popupInstance = manaPopupPool.Get();
            popupInstance.Setup(position);

            return popupInstance;
        }

        public NumberPopup SpawnExperiencePopup(Vector3 position)
        {
            NumberPopup popupInstance = Instantiate(experiencePopup, popupRoot);
            popupInstance.Setup(position);

            return popupInstance;
        }
    }
}