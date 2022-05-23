// NavMesh + NavMeshAgent movement for monsters, pets, etc.
// => abstract because Warp needs to call RpcWarp in NetworkNavMeshAgent or
//    NetworkNavMeshAgentRubberbanding depending on the implementation
// => players can use it to but need to inherit and implement their own WASD
//    movement for it
using UnityEngine;
using UnityEngine.AI;

namespace GameJam
{
    [RequireComponent(typeof(NavMeshAgent))]
    [DisallowMultipleComponent]
    public abstract class NavMeshMovement : MovementBase
    {
        [Header("Components")]
        public NavMeshAgent agent;

        public override Vector3 GetVelocity() =>
            agent.velocity;

        // IsMoving:
        // -> agent.hasPath will be true if stopping distance > 0, so we can't
        //    really rely on that.
        // -> pathPending is true while calculating the path, which is good
        // -> remainingDistance is the distance to the last path point, so it
        //    also works when clicking somewhere onto a obstacle that isn't
        //    directly reachable.
        // -> velocity is the best way to detect WASD movement
        public override bool IsMoving()
        {
            //if (!agent.isOnNavMesh) return false;

            return agent.pathPending ||
                   agent.remainingDistance > agent.stoppingDistance ||
                   agent.velocity != Vector3.zero;
        }

        public override bool IsGrounded()
        {
            return IsValidSpawnPoint(agent.transform.position);
        }

        public override void SetSpeed(float speed)
        {
            agent.speed = speed;
        }

        // look at a transform while only rotating on the Y axis (to avoid weird
        // tilts)
        public override void LookAtY(Vector3 position)
        {
            transform.LookAt(new Vector3(position.x, transform.position.y, position.z));
        }

        public override bool CanNavigate()
        {
            return true;
        }

        public override void Navigate(Vector3 destination, float stoppingDistance)
        {
            agent.stoppingDistance = stoppingDistance;
            agent.destination = destination;
        }

        // when spawning we need to know if the last saved position is still valid
        // for this type of movement.
        public override bool IsValidSpawnPoint(Vector3 position)
        {
            return NavMesh.SamplePosition(position, out NavMeshHit _, 0.1f, NavMesh.AllAreas);
        }

        public override Vector3 NearestValidDestination(Vector3 destination)
        {
            return agent.NearestValidDestination(destination);
        }

        public override bool DoCombatLookAt()
        {
            return true;
        }

        public void OnDeath()
        {
            // reset movement. don't slide to a destination if we die while moving.
            Reset();
        }

        private void OnValidate()
        {
            if (TryGetComponent(out NavMeshAgent agent))
            {
                this.agent = agent;
            }
        }
    }
}