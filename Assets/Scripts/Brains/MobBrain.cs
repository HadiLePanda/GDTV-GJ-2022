// some common events shared amongst all mob brain implementations.
// -> it's not necessary to inherit from MobBrain for your custom mob brains.
//    this is just to save redundancies.
using UnityEngine;

namespace GameJam
{
    public abstract class MobBrain : CommonBrain
    {
        public bool EventRandomAmbientSound(Mob entity)
        {
            return entity.IsAlive &&
                   Random.value <= entity.ambientSoundProbability;
        }

        protected void PlayRandomLivingSound(Mob entity)
        {
            if (entity.IsAlive && entity.GetAmbientSounds().Length > 0)
            {
                int n = Random.Range(0, entity.GetAmbientSounds().Length);
                AudioClip randomClip = entity.GetAmbientSounds()[n];
                entity.VoiceAudio.PlayOneShot(randomClip);
            }
        }
    }
}