using UnityEngine;

namespace GameJam
{
    public class ButtonSoundsTrigger : MonoBehaviour
    {
        [Header("Button Sounds")]
        [SerializeField] private AudioClip buttonHoverSound;
        [SerializeField] private AudioClip buttonClickSound;

        public void PlayButtonHoverSound()
        {
            if (buttonHoverSound != null)
                Game.Sfx.PlaySfx(buttonHoverSound);
        }
        public void PlayButtonClickSound()
        {
            if (buttonClickSound != null)
                Game.Sfx.PlaySfx(buttonClickSound);
        }
    }
}