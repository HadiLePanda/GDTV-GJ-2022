using DG.Tweening;
using UnityEngine;

namespace GameJam
{
    public class FadingManager : SingletonMonoBehaviour<FadingManager>
    {
        [Header("Fade References")]
        [SerializeField] private CanvasGroup fadingGroup;

        [Header("Fade Settings")]
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float fadeOutDuration = 2f;

        private Tween fadeTween;

        private void Start()
        {
            FadeIn(true);
        }

        public void FadeIn(bool instant = false)
        {
            if (!instant)
            {
                if (fadeTween != null)
                    fadeTween.Kill();

                fadeTween = fadingGroup.DOFade(0f, fadeInDuration);
            }
            else
            {
                fadingGroup.alpha = 0f;
            }
        }

        public void FadeOut(bool instant = false)
        {
            if (!instant)
            {
                if (fadeTween != null)
                    fadeTween.Kill();

                fadeTween = fadingGroup.DOFade(1f, fadeOutDuration);
            }
            else
            {
                fadingGroup.alpha = 1f;
            }
        }
    }
}