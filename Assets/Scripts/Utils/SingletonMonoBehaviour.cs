using UnityEngine;

namespace GameJam
{
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _instance;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                        "' already destroyed on application quit." +
                        " Won't create again - returning null.");
                    return null;
                }

                //--- An instance exists ---
                if (_instance != null) { return _instance; }

                //--- At least 1 instance exists ---
                var instances = FindObjectsOfType<T>();
                int instancesCount = instances.Length;

                if (instancesCount > 0)
                {
                    // Single instance
                    if (instancesCount == 1)
                    {
                        return _instance = instances[0];
                    }
                    // Multiple instances
                    for (var i = 1; i < instances.Length; i++)
                    {
                        Destroy(instances[i]);
                    }
                    return _instance = instances[0];
                }

                //--- No current instances ---
                SpawnNewSingletonInstanceGameObject();
                return null;
            }
        }

        private static bool applicationIsQuitting = false;

        protected virtual void Awake()
        {
            _instance = this as T;
        }

        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed, 
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        protected virtual void OnDestroy()
        {
            applicationIsQuitting = true;
        }

        private static void SpawnNewSingletonInstanceGameObject()
        {
            var newSingletonObject = new GameObject(typeof(T).Name.ToString());

            newSingletonObject.AddComponent<T>();

            Debug.LogError($"No instance of '{typeof(T)}' was found. Automatically spawned one. This should normally be added manually.");
        }
    }
}