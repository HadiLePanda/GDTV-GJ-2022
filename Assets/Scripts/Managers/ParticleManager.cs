using UnityEngine;

namespace GameJam
{
    public class ParticleManager : SingletonMonoBehaviour<ParticleManager>
    {
        /// Note /!\ particle prefab should have an auto destroy component
        /// auto destruction timer is manually set in the inspector

        public GameObject SpawnParticle(GameObject particle, Vector3 position)
        {
            return SpawnParticle(particle, position, Quaternion.identity);
        }

        public GameObject SpawnParticle(GameObject particle, Vector3 position, Quaternion rotation)
        {
            GameObject particleInstance = Instantiate(particle, position, rotation);

            AutoDestroy autoDestroy = particleInstance.GetComponent<AutoDestroy>();
            if (autoDestroy == null)
            {
                Debug.LogError($"Particle [{particle}] does not have an AutoDestroy component. Make sure to assign one.");
            }

            return particleInstance;
        }

        public GameObject SpawnParticle(GameObject particle, Transform parent)
        {
            GameObject particleInstance = Instantiate(particle, parent);

            AutoDestroy autoDestroy = particleInstance.GetComponent<AutoDestroy>();
            if (autoDestroy == null)
            {
                Debug.LogError($"Particle [{particle}] does not have an AutoDestroy component. Make sure to assign one.");
            }

            return particleInstance;
        }
    }
}