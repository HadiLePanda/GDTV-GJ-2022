using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameJam
{
    public class UICastBar : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject panel;
        public Image castFill;
        public TMP_Text skillNameText;
        public TMP_Text progressText;

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null &&
                player.State == "CASTING" && player.Skills.currentSkill != -1 &&
                player.Skills.skills[player.Skills.currentSkill].showCastBar)
            {
                panel.SetActive(true);

                Skill skill = player.Skills.skills[player.Skills.currentSkill];
                float ratio = skill.castTime > 0
                              ? (skill.castTime - skill.CastTimeRemaining()) / skill.castTime
                              : 0;

                castFill.fillAmount = ratio;
                skillNameText.text = skill.name.Replace("(Player) ", "");
                progressText.text = skill.CastTimeRemaining().ToString("F1") + "s";
            }
            else panel.SetActive(false);
        }
    }
}