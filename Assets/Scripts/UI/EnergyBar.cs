using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam
{
    public abstract class EnergyBar : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] protected bool isPlayerBar;
        [SerializeField] protected Entity entity;

        [Header("UI References")]
        [SerializeField] protected Image fill;
        [SerializeField] protected TMP_Text amountText;

        protected virtual Entity GetTarget() => isPlayerBar ? Player.localPlayer : entity;
        protected abstract float GetFillPercentage();
        protected abstract int GetCurrentEnergy();

        private void Update()
        {
            if (GetTarget() == null) { return; }

            fill.fillAmount = GetFillPercentage();
            amountText.text = GetCurrentEnergy().ToString();
        }
    }
}