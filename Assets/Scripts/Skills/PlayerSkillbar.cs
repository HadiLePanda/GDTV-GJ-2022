using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameJam
{
    [Serializable]
    public struct SkillbarEntry
    {
        public string reference;
        public KeyCode hotKey;
    }

    [RequireComponent(typeof(PlayerSkills))]
    public class PlayerSkillbar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Player player;

        //TODO New input system linking to skillbar keys
        [Header("Skillbar")]
        public SkillbarEntry[] slots =
        {
            new SkillbarEntry{reference="", hotKey=KeyCode.Alpha1},
            new SkillbarEntry{reference="", hotKey=KeyCode.Alpha2},
            new SkillbarEntry{reference="", hotKey=KeyCode.Alpha3},
            new SkillbarEntry{reference="", hotKey=KeyCode.Alpha4},
            new SkillbarEntry{reference="", hotKey=KeyCode.Alpha5},
            new SkillbarEntry{reference="", hotKey=KeyCode.Alpha6},
            new SkillbarEntry{reference="", hotKey=KeyCode.Alpha7},
            new SkillbarEntry{reference="", hotKey=KeyCode.Alpha8},
            new SkillbarEntry{reference="", hotKey=KeyCode.Alpha9},
            new SkillbarEntry{reference="", hotKey=KeyCode.Alpha0},
        };
        //public ScriptableSkill resurrectionSkillTemplate;
        //public Skill resurrectionSkill;

        [ReadOnlyInspector] public int selectedSlotIndex;

        private void Start()
        {
            //resurrectionSkill = new Skill(resurrectionSkillTemplate);

            // put the player skills templates on the skillbar (specificied in the inspector)
            // => we can choose to change which skills we have by changing the player prefab instance on each map
            Load();
        }

        //TODO Move input to new input system
        private void Update()
        {
            // change selected bar index with mouse scrollwheel
            ProcessSkillbarInput();

            //TODO move out of here
            ProcessCombatInput();
        }

        private void ProcessCombatInput()
        {
            // left click to use the selected skill
            if (Input.GetMouseButtonDown(0))
            {
                SkillbarEntry entry = slots[selectedSlotIndex];
                int skillIndex = player.Skills.GetSkillIndexByName(entry.reference);
                
                if (skillIndex != -1)
                {
                    ((PlayerSkills)player.Skills).TryUse(skillIndex);
                }
            }

            //TODO right click to cast the resurrect skill
            //if (Input.GetMouseButtonDown(1))
            //{
            //    ((PlayerSkills)player.Skills).TryUse(resurrectionSkill);
            //}
        }

        private void ProcessSkillbarInput()
        {
            for (int i = 0; i < slots.Length; ++i)
            {
                SkillbarEntry entry = slots[i];

                if (Input.GetKeyDown(entry.hotKey))
                {
                    selectedSlotIndex = i;
                }
            }
        }

        // skillbar ////////////////////////////////////////////////////////////////
        private void Load()
        {
            List<Skill> learned = player.Skills.skills.Where(skill => skill.level > 0).ToList();

            List<Skill> learnedCastableSkills = learned
                .Where(skill => skill.data is not PassiveSkill)
                .ToList();

            for (int i = 0; i < slots.Length; ++i)
            {
                // fill with default skills for a better first impression
                if (i < learnedCastableSkills.Count)
                {
                    slots[i].reference = learnedCastableSkills[i].name;
                }
            }
        }

        private void OnValidate()
        {
            // auto-reference entity
            if (player == null && TryGetComponent(out Player playerComponent))
            {
                player = playerComponent;
            }
        }
    }
}