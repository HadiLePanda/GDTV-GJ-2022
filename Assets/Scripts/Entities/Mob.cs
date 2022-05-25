using UnityEngine;

namespace GameJam
{
    public class Mob : Entity
    {
        [Header("Mob Sounds")]
        [SerializeField] private AudioClip[] ambientSounds;
        public float ambientSoundProbability = 0.01f;

        [HideInInspector] public Vector3 startPosition;

        public new MobSkills Skills => (MobSkills)base.Skills;
        public new AIMovement Movement => (AIMovement)base.Movement;

        public AudioClip[] GetAmbientSounds() => ambientSounds;
    }
}