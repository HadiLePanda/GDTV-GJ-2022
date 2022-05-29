// Area heal that heals all entities of same type in cast range
// => player heals players in cast range
// => monster heals monsters in cast range
//
// Based on BuffSkill so it can be added to Buffs list.
using System.Collections.Generic;
using UnityEngine;

namespace GameJam
{
    [CreateAssetMenu(menuName = "Game/Skills/Buff AoE", order = 1)]
    public class BuffAoeSkill : BuffSkill
    {
        [Header("Aoe References")]
        [SerializeField] private AoeSkillEffect aoeCookie;

        // OverlapSphereNonAlloc array to avoid allocations.
        // -> static so we don't create one per skill
        // -> this is worth it because skills are casted a lot!
        // -> should be big enough to work in just about all cases
        static Collider[] hitsBuffer = new Collider[10000];

        public override bool CheckTarget(Entity caster)
        {
            // no target necessary, but still set to self so that LookAt(target)
            // doesn't cause the player to look at a target that doesn't even matter
            //caster.SetTarget(caster);
            return true;
        }

        public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
        {
            // can cast anywhere
            destination = caster.transform.position;
            return true;
        }

        public override bool CheckFOV(Entity caster, int skillLevel)
        {
            // can cast without visibility
            return true;
        }

        public override void Apply(Entity caster, int skillLevel)
        {
            // candidates hashset to be 100% sure that we don't apply an area skill
            // to a candidate twice. this could happen if the candidate has more
            // than one collider (which it often has).
            HashSet<Entity> candidates = new HashSet<Entity>();

            // for the generic case, cast the skill from the entity location
            // for the player, cast the skill around the look target
            Vector3 castPosition = caster.transform.position;
            if (caster is Player player)
            {
                castPosition = player.Movement.look.lookTarget.transform.position
                                .ChangeY(player.transform.position.y);
            }

            // find all entities of same type in castRange around the caster
            int hits = Physics.OverlapSphereNonAlloc(caster.transform.position, castRange.Get(skillLevel), hitsBuffer);
            for (int i = 0; i < hits; ++i)
            {
                Collider co = hitsBuffer[i];
                Entity candidate = co.GetComponentInParent<Entity>();
                if (candidate != null &&
                    candidate.IsAlive) // can't buff dead people
                {
                    // ourself
                    if (canBuffSelf &&
                        candidate == caster)
                    {
                        candidates.Add(candidate);
                    }

                    // allies of the caster
                    if (canBuffAllies &&
                        caster.CanHeal(candidate)) // based on if we can heal them
                    {
                        // only heal same type check
                        if (canBuffOnlySameType &&
                            caster.GetType() != candidate.GetType())
                        {
                            continue;
                        }

                        candidates.Add(candidate);
                    }

                    // enemies
                    if (canBuffEnemies &&
                        caster.CanAttack(candidate))
                    {
                        candidates.Add(candidate);
                    }
                }
            }

            // spawn ground aoe range indicator
            SpawnAoeCookie(caster, castPosition, skillLevel);

            // apply to all candidates
            foreach (Entity candidate in candidates)
            {
                // add buff or replace if already in there
                candidate.Skills.AddOrRefreshBuff(new Buff(this, skillLevel));

                // show effect on target
                SpawnEffect(caster, candidate);
            }
        }

        private void SpawnAoeCookie(Entity caster, Vector3 spawnPosition, int skillLevel)
        {
            if (aoeCookie)
            {
                AoeSkillEffect aoeCookieInstance = Instantiate(aoeCookie, spawnPosition.ChangeY(spawnPosition.y + 0.01f), Quaternion.identity);
                aoeCookieInstance.caster = caster;
                aoeCookieInstance.Setup(castRange.Get(skillLevel));
            }
        }
    }
}