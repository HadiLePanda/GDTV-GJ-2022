using UnityEngine;

namespace GameJam
{
    public class UISkillbar : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject panel;
        public Transform content;

        [Header("References")]
        public UISkillbarSlot slotPrefab;

        private void Update()
        {
            Player player = Player.localPlayer;
            if (player != null)
            {
                panel.SetActive(true);

                // instantiate/destroy enough slots
                Utils.BalancePrefabs(slotPrefab.gameObject, player.Skillbar.slots.Length, content);

                // refresh all
                for (int i = 0; i < player.Skillbar.slots.Length; ++i)
                {
                    SkillbarEntry entry = player.Skillbar.slots[i];

                    UISkillbarSlot slot = content.GetChild(i).GetComponent<UISkillbarSlot>();
                    //slot.dragAndDropable.name = i.ToString(); // drag and drop index

                    // hotkey overlay (without 'Alpha' etc.)
                    string pretty = entry.hotKey.ToString().Replace("Alpha", "");
                    slot.hotkeyText.text = pretty;

                    // current slot selected overlay
                    int selectedSlotIndex = player.Skillbar.selectedSlotIndex;

                    // skill inside hotbar?
                    int skillIndex = player.Skills.GetSkillIndexByName(entry.reference);
                    if (skillIndex != -1)
                    {
                        Skill skill = player.Skills.skills[skillIndex];
                        bool canCast = player.Skills.CastCheckSelf(skill);

                        // if movement does NOT support navigation then we need to
                        // check distance too. otherwise distance doesn't matter
                        // because we can navigate anywhere.
                        if (!player.Movement.CanNavigate())
                        {
                            canCast &= player.Skills.CastCheckDistance(skill, out Vector3 _);
                            canCast &= player.Skills.CastCheckFOV(skill);
                        }

                        // refresh skill slot
                        slot.button.interactable = canCast; // check mana, cooldowns, etc.
                        slot.button.onClick.SetListener(() =>
                        {
                            // try use the skill or walk closer if needed
                            ((PlayerSkills)player.Skills).TryUse(skillIndex);
                        });
                        // only build tooltip while it's actually shown. this
                        // avoids MASSIVE amounts of StringBuilder allocations.
                        //slot.tooltip.enabled = true;
                        //if (slot.tooltip.IsVisible())
                        //{
                        //    slot.tooltip.text = skill.ToolTip();
                        //}
                        //slot.dragAndDropable.dragable = true;
                        slot.image.color = Color.white;
                        slot.image.sprite = skill.icon;
                        float cooldown = skill.CooldownRemaining();
                        slot.cooldownOverlay.SetActive(cooldown > 0);
                        slot.cooldownText.text = cooldown.ToString("F0");
                        slot.cooldownCircle.fillAmount = skill.cooldown > 0 ? cooldown / skill.cooldown : 0;
                        slot.selectedOverlay.SetActive(i == selectedSlotIndex);
                    }
                    else
                    {
                        // clear the outdated reference
                        // (need to assign directly because it's a struct)
                        player.Skillbar.slots[i].reference = "";

                        // refresh empty slot
                        slot.button.onClick.RemoveAllListeners();
                        //slot.tooltip.enabled = false;
                        //slot.dragAndDropable.dragable = false;
                        slot.image.color = Color.clear;
                        slot.image.sprite = null;
                        slot.cooldownOverlay.SetActive(false);
                        slot.cooldownCircle.fillAmount = 0;
                        slot.selectedOverlay.SetActive(false);
                    }
                }
            }
            else panel.SetActive(false);
        }
    }
}