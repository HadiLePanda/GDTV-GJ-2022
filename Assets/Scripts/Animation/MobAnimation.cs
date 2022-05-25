using UnityEngine;

namespace GameJam
{
    public class MobAnimation : MonoBehaviour
    {
        [Header("Components")]
        public Mob mob;

        private AIMovement Movement => mob.Movement;

        [Header("Animation")]
        public float animationDirectionDampening = 0.05f;
        public float animationTurnDampening = 0.1f;
        Vector3 lastForward;

        private void Start()
        {
            lastForward = transform.forward;
        }

        private void LateUpdate()
        {
            // local velocity (based on rotation) for animations
            Vector3 localVelocity = transform.InverseTransformDirection(Movement.GetVelocity());
            lastForward = transform.forward;

            // pass parameters to animation state machine
            // => passing the states directly is the most reliable way to avoid all
            //    kinds of glitches like movement sliding, attack twitching, etc.
            // => make sure to import all looping animations like idle/run/attack
            //    with 'loop time' enabled, otherwise the client might only play it
            //    once
            // => only play moving animation while the actually moving (velocity).
            //    the MOVING state might be delayed to due latency or we might be in
            //    MOVING while a path is still pending, etc.
            // => skill names are assumed to be boolean parameters in animator
            //    so we don't need to worry about an animation number etc.
            mob.Animator.SetFloat("DirX", localVelocity.x, animationDirectionDampening, Time.deltaTime); // smooth idle<->run transitions
            mob.Animator.SetFloat("DirY", localVelocity.y, animationDirectionDampening, Time.deltaTime); // smooth idle<->run transitions
            mob.Animator.SetFloat("DirZ", localVelocity.z, animationDirectionDampening, Time.deltaTime); // smooth idle<->run transitions
            mob.Animator.SetBool("MOVING", mob.State == "MOVING" && Movement.IsMoving());
            mob.Animator.SetBool("CASTING", mob.State == "CASTING");
            mob.Animator.SetBool("STUNNED", mob.State == "STUNNED");
            mob.Animator.SetBool("DEAD", mob.State == "DEAD");
            foreach (Skill skill in mob.Skills.skills)
            {
                if (!Utils.AnimatorParameterExists(skill.name, mob.Animator))
                {
                    Debug.LogError($"(Mob: {name}) Animator parameter not set: {skill.name}. Remember to assign it!");
                    continue;
                }

                mob.Animator.SetBool(skill.name, skill.CastTimeRemaining() > 0);
            }
        }

        private void OnValidate()
        {
            if (mob == null && TryGetComponent(out Mob mobComponent))
            {
                mob = mobComponent;
            }
        }
    }
}