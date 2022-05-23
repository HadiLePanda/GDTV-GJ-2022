using UnityEngine;

namespace GameJam
{
    public class PlayerLook : MonoBehaviour
    {
        [Header("References")]
        public PlayerControllerMovement movement;
        public Player player;
        public GameObject lookTarget;

        [Header("Camera")]
        // the layer mask to use when trying to detect view blocking
        // (this way we dont zoom in all the way when standing in another entity)
        // (-> create a entity layer for them if needed)
        public LayerMask viewBlockingLayers;

        private Camera cam;

        [Header("Physical Interaction")]
        [Tooltip("Layers to use for raycasting. Check Default, Walls, Player, Zombie, Doors, Interactables, Item, etc. Uncheck IgnoreRaycast, AggroArea, Water, UI, etc.")]
        public LayerMask raycastLayers = Physics.DefaultRaycastLayers;

        public Vector3 lookDirectionFar
        {
            get
            {
                return cam.transform.forward;
            }
        }

        public Vector3 lookDirectionRaycasted
        {
            get
            {
                // same for local and other players
                // (positionRaycasted uses camera || syncedDirectionRaycasted anyway)
                return (lookPositionRaycasted - transform.position).normalized;
            }
        }

        // the far position, directionFar projected into nirvana
        public Vector3 lookPositionFar
        {
            get
            {
                Vector3 position = cam.transform.position;
                return position + lookDirectionFar * 9999f;
            }
        }

        // the raycasted position is needed for lookDirectionRaycasted calculation
        // and for firing, so we might as well reuse it here
        public Vector3 lookPositionRaycasted
        {
            get
            {
                // raycast based on position and direction, project into nirvana if nothing hit
                // (not * infinity because might overflow depending on position)
                RaycastHit hit;
                return Utils.RaycastWithout(cam.transform.position, cam.transform.forward, out hit, Mathf.Infinity, gameObject, raycastLayers)
                       ? hit.point
                       : lookPositionFar;
            }
        }

        void Awake()
        {
            cam = Camera.main;
        }

        void Update()
        {
            if (!player.IsAlive) return;

            // rotate model to look towards cursor position
            Vector3 mousePoint = Input.mousePosition;

            var mouseRay = cam.ScreenPointToRay(mousePoint);

            RaycastHit hit;
            if (Physics.Raycast(mouseRay, out hit, 500, viewBlockingLayers))
            {
                var mouseWorld = cam.ScreenToViewportPoint(mousePoint);
                mouseWorld.z = transform.position.z;
                mouseWorld.y = transform.position.y;

                var hitPos = hit.point;
                hitPos.y = transform.position.y;

                lookTarget.transform.position = hitPos;
            }
            else
            {
                lookTarget.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z + 5);
            }

            //player.modelRoot.transform.LookAt(lookTarget.transform.position);
            player.transform.LookAt(lookTarget.transform.position);
        }

        // debugging ///////////////////////////////////////////////////////////////
        void OnDrawGizmos()
        {
            if (cam == null) return;

            // draw camera forward
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, cam.transform.position + cam.transform.forward * 9999f);

            // draw all the different look positions
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, lookPositionFar);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, lookPositionRaycasted);
        }
    }
}