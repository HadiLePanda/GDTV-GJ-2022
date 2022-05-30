using UnityEngine;

namespace GameJam
{
    public static class Game
    {
        public static ScenesManager Scenes;
        public static AudioManager Audio;
        public static ParticleManager Vfx;
        public static FadingManager Fader;
        public static InputManager Input;

        static Game()
        {
            GameObject go = Object.FindObjectOfType<DDOL>().gameObject;

            Scenes = go.GetComponent<ScenesManager>();
            Audio = go.GetComponent<AudioManager>();
            Vfx = go.GetComponent<ParticleManager>();
            Fader = go.GetComponent<FadingManager>();
            Input = go.GetComponent<InputManager>();
        }
    }
}