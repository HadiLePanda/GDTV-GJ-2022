// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<TMP_Text> etc.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam
{
    public class UISkillbarSlot : MonoBehaviour
    {
        [Header("UI References")]
        //public UIShowToolTip tooltip;
        //public UIDragAndDropable dragAndDropable;
        public Image image;
        public Button button;
        public GameObject cooldownOverlay;
        public TMP_Text cooldownText;
        public Image cooldownCircle;
        public TMP_Text hotkeyText;
        public GameObject selectedOverlay;
    }
}