using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace GameJam
{
    public class NumberPopup : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshPro numberText;

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 0.7f;
        [SerializeField] private float movementDuration = 0.75f;
        [SerializeField] private float defaultFontSize = 5f;
        [SerializeField] private Color defaultColor = Color.white;

        private IObjectPool<NumberPopup> pool;

        public void SetPool(IObjectPool<NumberPopup> pool) => this.pool = pool;

        public void Setup(Vector3 pos)
        {
            // we setup positions when pulling this popup from its pool
            transform.SetPositionAndRotation(pos, Quaternion.identity);

            // reset values to default
            numberText.color = defaultColor;
            numberText.alpha = 1f;
            numberText.fontSize = defaultFontSize;
            numberText.text = "-";
            //numberText.text = textDisplay; //number >= 0 ? $"+{number}" : $"-{number}";

            // move with dotween
            transform.DOMove(transform.position + Vector3.up, movementDuration).OnComplete(() => {
                pool.Release(this);
            });
        }

        private void Update()
        {
            // fade out
            if (numberText.alpha > 0)
            {
                ProcessFadingOut();
            }
        }

        private void ProcessFadingOut()
        {
            numberText.alpha = Mathf.MoveTowards(numberText.alpha, 0f, fadeDuration * Time.deltaTime);
        }
    }
}