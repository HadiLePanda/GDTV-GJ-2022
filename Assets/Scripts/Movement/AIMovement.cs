// navmesh movement for monsters, pets, etc.
// -> uses NetworkNavMeshAgent instead of Rubberbanding version!
using UnityEngine;

namespace GameJam
{
    [DisallowMultipleComponent]
    public class AIMovement : NavMeshMovement
    {
        public override void Reset()
        {
            agent.ResetMovement();
        }

        public override void Warp(Vector3 destination)
        {
            agent.Warp(destination);
        }

        protected virtual void FixedUpdate()
        {
            // update footstep cycle when the navmesh is on the move
            if (IsMoving())
            {
                ProgressStepCycle(agent.steeringTarget, agent.speed);
            }
        }

        void ProgressStepCycle(Vector3 inputDir, float speed)
        {
            if (GetVelocity().sqrMagnitude > 0 && (inputDir.x != 0 || inputDir.y != 0))
            {
                stepCycle += (GetVelocity().magnitude + speed) *
                             Time.fixedDeltaTime;
            }

            if (stepCycle > nextStep)
            {
                nextStep = stepCycle + runStepInterval;
                PlayFootStepAudio();
            }
        }
    }
}