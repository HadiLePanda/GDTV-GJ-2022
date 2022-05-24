using UnityEngine;

namespace GameJam
{
    public class Mob : Entity
    {
        [Header("Mob Sounds")]
        [SerializeField] private AudioClip[] ambientSounds;
        public float ambientSoundProbability = 0.01f;

        public AudioClip[] GetAmbientSounds() => ambientSounds;
    }
}