using UnityEngine;

namespace GameJam
{
    public static class Game
    {
        public static ScenesManager Scenes;
        public static AudioManager Sfx;
        public static ParticleManager Vfx;

        static Game()
        {
            GameObject go = Object.FindObjectOfType<DDOL>().gameObject;

            Scenes = go.GetComponent<ScenesManager>();
            Sfx = go.GetComponent<AudioManager>();
            Vfx = go.GetComponent<ParticleManager>();
        }
    }
}