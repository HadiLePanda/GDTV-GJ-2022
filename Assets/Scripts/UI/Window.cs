using DG.Tweening;
using UnityEngine;

namespace GameJam
{
    public class Window : MonoBehaviour
    {
        [Header("Window References")]
        public GameObject panel;
        public GameObject windowPanel;
        public CanvasGroup canvasGroup;

        [Header("Window Settings")]
        public bool animateWindow = true;
        public float fadeTime = 0.2f;
        public bool blockRaycast = true;

        public bool IsOpen => panel.activeSelf;

        private void Start()
        {
            canvasGroup.blocksRaycasts = blockRaycast;
        }

        public void Show()
        {
            if (IsOpen) { return; }

            if (animateWindow && Time.timeScale == 1)
            {
                windowPanel.transform.localScale = Vector3.zero;
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = true;
                if (blockRaycast)
                    canvasGroup.blocksRaycasts = true;

                panel.SetActive(true);

                canvasGroup.DOFade(1f, fadeTime);
                windowPanel.transform.DOScale(1f, fadeTime);
            }
            else
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = blockRaycast;
                panel.SetActive(true);
            }
        }
        public void Hide()
        {
            if (!IsOpen) { return; }

            if (animateWindow && Time.timeScale == 1)
            {
                canvasGroup.interactable = false;

                canvasGroup.DOFade(0f, fadeTime);
                windowPanel.transform.DOScale(0f, fadeTime).OnComplete(() =>
                {
                    if (blockRaycast)
                        canvasGroup.blocksRaycasts = false;

                    panel.SetActive(false);
                });
            }
            else
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                panel.SetActive(false);
            }
        }
    }
}