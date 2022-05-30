using System.Collections.Generic;
using UnityEngine;

namespace GameJam
{
    [CreateAssetMenu(menuName = "Game/Skills/Resurrect", order = 1)]
    public class ResurrectSkill : ScriptableSkill
    {
        // OverlapSphereNonAlloc array to avoid allocations.
        // -> static so we don't create one per skill
        // -> this is worth it because skills are casted a lot!
        // -> should be big enough to work in just about all cases
        static Collider[] hitsBuffer = new Collider[10000];

        [Header("Resurrection")]
        public ParticleAttractorSkillEffect effect;

        [Header("Aoe References")]
        [SerializeField] private AoeSkillEffect aoeCookie;

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

        public override bool CheckTarget(Entity caster)
        {
            return true;
        }

        public override void Apply(Entity caster, int skillLevel)
        {
            // candidates hashset to be 100% sure that we don't apply an area skill
            // to a candidate twice. this could happen if the candidate has more
            // than one collider (which it often has).
            HashSet<Corpse> candidates = new HashSet<Corpse>();

            // for the generic case, cast the skill from the entity location
            // for the player, cast the skill around the cursor (look target)
            Vector3 castPosition = caster.transform.position;
            if (caster is Player player)
            {
                castPosition = player.Movement.look.lookTarget.transform.position
                                .ChangeY(player.transform.position.y);
            }

            // find all entities of same type in castRange around the caster
            int hits = Physics.OverlapSphereNonAlloc(castPosition, castRange.Get(skillLevel), hitsBuffer);
            for (int i = 0; i < hits; ++i)
            {
                Collider co = hitsBuffer[i];
                Corpse candidate = co.GetComponentInParent<Corpse>();
                if (candidate != null &&
                    candidate.CanBeResurrected()) // can't heal dead people
                {
                    if (caster.CanResurrectCorpse(candidate))
                    {
                        candidates.Add(candidate);
                    }
                }
            }

            // spawn ground aoe range indicator
            SpawnAoeCookie(caster, castPosition, skillLevel);

            // apply to all candidates
            foreach (Corpse candidate in candidates)
            {
                // resurrect
                caster.Combat.ResurrectCorpse(candidate);

                // show effect on candidate
                Entity spawnTarget = candidate.GetEntityInstance() != null ? candidate.GetEntityInstance() : caster;
                Transform spawnTransform = candidate.transform;

                SpawnEffect(caster, spawnTarget, spawnTransform);
            }
        }

        // helper function to spawn the skill effect on someone
        // (used by all the buff implementations and to load them after saving)
        public void SpawnEffect(Entity caster, Entity target, Transform spawnTransform)
        {
            if (effect != null)
            {
                GameObject go = Instantiate(effect.gameObject, target.transform.position, Quaternion.identity);
                ParticleAttractorSkillEffect effectComponent = go.GetComponent<ParticleAttractorSkillEffect>();
                effectComponent.caster = caster;
                effectComponent.target = target;
                effectComponent.Setup(caster, spawnTransform);
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