// A simple skill effect that follows the target until it ends.
// -> Can be used for buffs.
//
// Note: Particle Systems need Simulation Space = Local for it to work.
using UnityEngine;

namespace GameJam
{
    public class BuffSkillEffect : SkillEffect
    {
        private float lastRemainingTime = Mathf.Infinity;
        [HideInInspector] public string buffName;

        private void Update()
        {
            // only while target still exists, buff still active and hasn't been recasted
            if (target != null)
            {
                int index = target.Skills.GetBuffIndexByName(buffName);
                if (index != -1)
                {
                    Buff buff = target.Skills.buffs[index];
                    if (lastRemainingTime >= buff.BuffTimeRemaining())
                    {
                        transform.position = target.Collider.bounds.center;
                        lastRemainingTime = buff.BuffTimeRemaining();
                        return;
                    }
                }
            }

            // if we got here then something wasn't good, let's destroy self
            Destroy(gameObject);
        }
    }
}