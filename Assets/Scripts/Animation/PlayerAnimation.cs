using UnityEngine;

namespace GameJam
{
    public class PlayerAnimation : MonoBehaviour
    {
        // fields for all player components to avoid costly GetComponent calls
        [Header("Components")]
        public Player player;

        private Skills Skills => player.Skills;
        private PlayerControllerMovement Movement => player.Movement;

        [Header("Animation")]
        public float animationDirectionDampening = 0.05f;
        public float animationTurnDampening = 0.1f;
        Vector3 lastForward;

        private void Start()
        {
            lastForward = transform.forward;
        }

        // animation ///////////////////////////////////////////////////////////////

        // Vector.Angle and Quaternion.FromToRotation and Quaternion.Angle all end
        // up clamping the .eulerAngles.y between 0 and 360, so the first overflow
        // angle from 360->0 would result in a negative value (even though we added
        // something to it), causing a rapid twitch between left and right turn
        // animations.
        //
        // the solution is to use the delta quaternion rotation.
        // when turning by 0.5, it is:
        //   0.5 when turning right (0 + angle)
        //   364.6 when turning left (360 - angle)
        // so if we assume that anything >180 is negative then that works great.
        static float AnimationDeltaUnclamped(Vector3 lastForward, Vector3 currentForward)
        {
            Quaternion rotationDelta = Quaternion.FromToRotation(lastForward, currentForward);
            float turnAngle = rotationDelta.eulerAngles.y;
            return turnAngle >= 180 ? turnAngle - 360 : turnAngle;
        }

        private void LateUpdate()
        {
            // local velocity (based on rotation) for animations
            Vector3 localVelocity = transform.InverseTransformDirection(Movement.GetVelocity());

            // Turn value so that mouse-rotating the character plays some animation
            // instead of only raw rotating the model.
            float turnAngle = AnimationDeltaUnclamped(lastForward, transform.forward);
            lastForward = transform.forward;

            // apply animation parameters to all animators.
            // there might be multiple if we use skinned mesh equipment.
            foreach (Animator anim in GetComponentsInChildren<Animator>())
            {
                anim.SetFloat("DirX", localVelocity.x, animationDirectionDampening, Time.deltaTime); // smooth idle<->run transitions
                anim.SetFloat("DirY", localVelocity.y, animationDirectionDampening, Time.deltaTime); // smooth idle<->run transitions
                anim.SetFloat("DirZ", localVelocity.z, animationDirectionDampening, Time.deltaTime); // smooth idle<->run transitions
                anim.SetFloat("LastFallY", Movement.lastFall.y);
                anim.SetFloat("Turn", turnAngle, animationTurnDampening, Time.deltaTime); // smooth turn
                anim.SetBool("CROUCHING", Movement.State == MoveState.CROUCHING);
                anim.SetBool("CLIMBING", Movement.State == MoveState.CLIMBING);
                anim.SetBool("SWIMMING", Movement.State == MoveState.SWIMMING);
                anim.SetBool("DEAD", player.State == "DEAD");

                foreach (Skill skill in Skills.skills)
                {
                    if (skill.level > 0 && !(skill.data is PassiveSkill))
                    {
                        if (!Utils.AnimatorParameterExists(skill.name, anim))
                        {
                            Debug.LogError($"(Player: {name}) Animator parameter not set: {skill.name}. Remember to assign it!");
                            continue;
                        }

                        anim.SetBool(skill.name, skill.CastTimeRemaining() > 0);
                    }
                }

                // smoothest way to do climbing-idle is to stop right where we were
                //if (movement.state == MoveState.CLIMBING)
                //    animator.speed = localVelocity.y == 0 ? 0 : 1;
                //else
                anim.speed = 1;

                // grounded detection works best via .state
                // -> check AIRBORNE state instead of controller.isGrounded to have some
                //    minimum fall tolerance so we don't play the AIRBORNE animation
                //    while walking down steps etc.
                anim.SetBool("OnGround", Movement.State != MoveState.AIRBORNE);

                // upper body layer
                //animator.SetBool("UPPERBODY_HANDS", true);
            }
        }

        private void OnValidate()
        {
            if (player == null && TryGetComponent(out Player playerComponent))
            {
                player = playerComponent;
            }
        }
    }
}