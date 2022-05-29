using DG.Tweening;
using TMPro;
using UnityEngine;

namespace GameJam
{
    public class FadeTextOverTime : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text text;

        [Header("Settings")]
        [SerializeField] private float duration = 2f;
        [SerializeField] private bool fadeIn = true;

        private void Start()
        {
            if (fadeIn)
            {
                text.alpha = 0f;
                text.DOFade(1f, duration);
            }
            else
            {
                text.alpha = 1f;
                text.DOFade(0f, duration);
            }
        }
    }
}