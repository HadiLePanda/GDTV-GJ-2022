using UnityEngine;
using UnityEngine.UI;

namespace GameJam
{
    public class UISettings: Window
    {
        [Header("References")]
        [SerializeField] private Slider masterVolumeSlider;

        private void OnEnable()
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
        private void OnDisable()
        {
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
        }

        private void Start()
        {
            masterVolumeSlider.value = Game.Audio.GetSettingMasterVolume();
        }

        public void SetMasterVolume(float volume)
        {
            Game.Audio.SetMasterVolume(volume);
        }
    }
}