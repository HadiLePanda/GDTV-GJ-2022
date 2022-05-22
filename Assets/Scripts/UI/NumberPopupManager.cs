using UnityEngine;
using UnityEngine.Pool;

namespace GameJam
{
    public class NumberPopupManager : SingletonMonoBehaviour<NumberPopupManager>
    {
        [Header("References")]
        [SerializeField] private Transform damagePopupRoot;
        [SerializeField] private Transform healPopupRoot;

        [Header("Settings")]
        [SerializeField] private float popupOffsetRadius = 1f;
        [SerializeField] private float critFontSizeIncrease = 0.2f;

        [Header("Pooling")]
        [SerializeField] private NumberPopup damagePopup;
        [SerializeField] private NumberPopup healPopup;
        [SerializeField] private int defaultPoolSize = 20;
        [SerializeField] private int maxPoolSize = 30;

        private IObjectPool<NumberPopup> damagePopupPool;
        private IObjectPool<NumberPopup> healPopupPool;

        protected override void Awake()
        {
            base.Awake();

            //damagePopupPool = new ObjectPool<NumberPopup>(
            //    createFunc: () => CreateDamagePopup(),
            //    actionOnGet: (poolObject) => GetDamagePopupFromPool(poolObject),
            //    actionOnRelease: (poolObject) => ReturnDamagePopupToPool(poolObject),
            //    actionOnDestroy: (poolObject) => Destroy(poolObject.gameObject),
            //    collectionCheck: false,
            //    defaultCapacity: defaultPoolSize,
            //    maxSize: maxPoolSize);
            //
            //damagePopupPool = new ObjectPool<NumberPopup>(
            //    createFunc: () => CreateDamagePopup(),
            //    actionOnGet: (poolObject) => GetDamagePopupFromPool(poolObject),
            //    actionOnRelease: (poolObject) => ReturnDamagePopupToPool(poolObject),
            //    actionOnDestroy: (poolObject) => Destroy(poolObject.gameObject),
            //    collectionCheck: false,
            //    defaultCapacity: defaultPoolSize,
            //    maxSize: maxPoolSize);

            damagePopupPool = new ObjectPool<NumberPopup>(CreateDamagePopup, GetDamagePopupFromPool, ReturnDamagePopupToPool);
            healPopupPool = new ObjectPool<NumberPopup>(CreateHealPopup, GetHealPopupFromPool, ReturnHealPopupToPool);
        }

        // pools =================================================================
        private NumberPopup CreateDamagePopup()
        {
            var popupInstance = Instantiate(damagePopup, damagePopupRoot);
            popupInstance.gameObject.name = $"(Pool) {damagePopup.name} {popupInstance.transform.GetSiblingIndex()}";
            popupInstance.SetPool(damagePopupPool);
            popupInstance.gameObject.SetActive(false);

            return popupInstance;
        }
        private void GetDamagePopupFromPool(NumberPopup popup) => popup.gameObject.SetActive(true);
        private void ReturnDamagePopupToPool(NumberPopup popup) => popup.gameObject.SetActive(false);

        private NumberPopup CreateHealPopup()
        {
            var popupInstance = Instantiate(healPopup, healPopupRoot);
            popupInstance.gameObject.name = $"(Pool) {healPopup.name} {popupInstance.transform.GetSiblingIndex()}";
            popupInstance.SetPool(healPopupPool);
            popupInstance.gameObject.SetActive(false);

            return popupInstance;
        }
        private void GetHealPopupFromPool(NumberPopup popup) => popup.gameObject.SetActive(true);
        private void ReturnHealPopupToPool(NumberPopup popup) => popup.gameObject.SetActive(false);

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

        private Vector3 GetRandomPopupPosition(Vector3 pos)
        {
            Vector2 randomOffset = Random.insideUnitCircle * popupOffsetRadius;
            Vector3 randomPosAroundSpawn = new Vector3(
                pos.x,
                pos.y + randomOffset.y,
                0);

            return randomPosAroundSpawn;
        }
    }
}