using UnityEngine;

namespace GameJam
{
    public class ParticleManager : SingletonMonoBehaviour<ParticleManager>
    {
        /// Note /!\ particle prefab should have an auto destroy component
        /// auto destruction timer is manually set in the inspector <summary>
        /// Note /!\ particle prefab should have an auto destroy component

        public GameObject SpawnParticle(GameObject particle, Vector3 position)
        {
            return SpawnParticle(particle, position, Quaternion.identity);
        }

        public GameObject SpawnParticle(GameObject particle, Vector3 position, Quaternion rotation)
        {
            return Instantiate(particle, position, rotation);
        }

        public GameObject SpawnParticle(GameObject particle, Transform parent)
        {
            return Instantiate(particle, parent);
        }
    }
}