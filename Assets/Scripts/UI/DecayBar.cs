using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam
{
    public class DecayBar : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] protected Minion minion;

        [Header("UI References")]
        [SerializeField] protected Image fill;
        [SerializeField] protected TMP_Text decayTimeText;

        private void Update()
        {
            if (minion == null) { return; }

            //TODO change to decay or remove all
            fill.fillAmount = minion.GetDecayTime();//minion.GetLifetime() != 0 ? minion.RemainingLifetime / minion.GetLifetime() : 0;

            //if (minion.RemainingLifetime > 0)
            //{
            //    decayTimeText.gameObject.SetActive(true);
            //    decayTimeText.text = Utils.PrettySeconds(minion.RemainingLifetime).ToString();
            //}
            //else
            //{
            //    decayTimeText.gameObject.SetActive(false);
            //}
        }
    }
}