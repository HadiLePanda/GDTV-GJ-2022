// based on Unity's FirstPersonController & ThirdPersonController scripts
using Controller2k;
using UnityEngine;

namespace GameJam
{
    // MoveState as byte for minimal bandwidth (otherwise it's int by default)
    public enum MoveState : byte { IDLE, WALKING, CROUCHING, CRAWLING, AIRBORNE, CLIMBING, SWIMMING }

    [RequireComponent(typeof(CharacterController2k))]
    public class PlayerControllerMovement : MovementBase
    {
        // components to be assigned in inspector
        [Header("Components")]
        public Player player;
        public Animator animator;
        public CharacterController2k controller;
        public PlayerLook look;
        // the collider for the character controller. NOT the hips collider. this
        // one is NOT affected by animations and generally a better choice for state
        // machine logic.
        public CapsuleCollider controllerCollider;
        private Camera cam;

        [Header("State")]
        public MoveState state = MoveState.IDLE;
        [HideInInspector] public Vector3 moveDir;

        [Header("Rotation")]
        public float rotationSpeed = 150;

        [Header("Walking")]
        public float walkSpeed = 5;
        public float walkAcceleration = 15; // set to maxint for instant speed
        public float walkDeceleration = 20; // feels best if higher than acceleration

        [Header("Crouching")]
        public float crouchSpeed = 3f;
        public float crouchAcceleration = 5; // set to maxint for instant speed
        public float crouchDeceleration = 10; // feels best if higher than acceleration
        public KeyCode crouchKey = KeyCode.C;
        bool crouchKeyPressed;

        [Header("Swimming")]
        public float swimSpeed = 4;
        public float swimAcceleration = 15; // set to maxint for instant speed
        public float swimDeceleration = 20; // feels best if higher than acceleration
        public float swimSurfaceOffset = 0.25f;
        Collider waterCollider;
        bool inWater => waterCollider != null; // standing in water / touching it?
        bool underWater; // deep enough in water so we need to swim?
        [Range(0, 1)] public float underwaterThreshold = 0.9f; // percent of body that need to be underwater to start swimming
        public LayerMask canStandInWaterCheckLayers = Physics.DefaultRaycastLayers; // set this to everything except water layer

        [Header("Jumping")]
        public float jumpSpeed = 7;
        bool jumpKeyPressed;

        [Header("Falling")]
        public float airborneAcceleration = 15; // set to maxint for instant speed
        public float airborneDeceleration = 20; // feels best if higher than acceleration
        public float fallMinimumMagnitude = 9; // walking down steps shouldn't count as falling and play no falling sound.
        public float fallDamageMinimumMagnitude = 13;
        public float fallDamageMultiplier = 2;
        [HideInInspector] public Vector3 lastFall;
        bool sprintingBeforeAirborne; // don't allow sprint key to accelerate while jumping. decision has to be made before that.

        [Header("Climbing")]
        public float climbSpeed = 3;
        Collider ladderCollider;

        [Header("Physics")]
        public float gravityMultiplier = 2;

        public Vector3 Velocity { get; private set; }
        public MoveState State => state;

        // we need to remember the last accelerated xz speed without gravity etc.
        // (using moveDir.xz.magnitude doesn't work well with mounted movement)
        float horizontalSpeed;

        // helper property to check grounded with some tolerance. technically we
        // aren't grounded when walking down steps, but this way we factor in a
        // minimum fall magnitude. useful for more tolerant jumping etc.
        // (= while grounded or while velocity not smaller than min fall yet)
        public bool isGroundedWithinTolerance =>
            controller.isGrounded || controller.velocity.y > -fallMinimumMagnitude;

        void Awake()
        {
            cam = Camera.main;
        }

        // input directions ////////////////////////////////////////////////////////
        Vector2 GetInputDirection()
        {
            // get input direction while alive and while not typing in chat
            // (otherwise 0 so we keep falling even if we die while jumping etc.)
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            return new Vector2(horizontal, vertical).normalized;
        }

        Vector3 GetDesiredDirection(Vector2 inputDir)
        {
            // always move along the camera forward as it is the direction that is being aimed at
            Vector3 relativeForward = cam.transform.forward;
            Vector3 relativeRight = cam.transform.right;
            return relativeForward * inputDir.y + relativeRight * inputDir.x;
        }

        // movement state machine //////////////////////////////////////////////////
        bool EventJumpRequested()
        {
            // only while grounded, so jump key while jumping doesn't start a new
            // jump immediately after landing
            // => and not while sliding, otherwise we could climb slides by jumping
            // => not even while SlidingState.Starting, so we aren't able to avoid
            //    sliding by bunny hopping.
            // => grounded check uses min fall tolerance so we can actually still
            //    jump when walking down steps.
            return isGroundedWithinTolerance &&
                   controller.slidingState == SlidingState.NONE &&
                   jumpKeyPressed;
        }

        bool EventCrouchToggle()
        {
            return crouchKeyPressed;
        }

        bool EventFalling()
        {
            // use minimum fall magnitude so walking down steps isn't detected as
            // falling! otherwise walking down steps would show the fall animation
            // and play the landing sound.
            return !isGroundedWithinTolerance;
        }

        bool EventLanded()
        {
            return controller.isGrounded;
        }

        bool EventUnderWater()
        {
            // we can't really make it player position dependent, because he might
            // swim to the surface at which point it might be detected as standing
            // in water but not being under water, etc.
            if (inWater) // in water and valid water collider?
            {
                // raycasting from water to the bottom at the position of the player
                // seems like a very precise solution
                Vector3 origin = new Vector3(transform.position.x,
                                             waterCollider.bounds.max.y,
                                             transform.position.z);
                float distance = controllerCollider.height * underwaterThreshold;
                Debug.DrawLine(origin, origin + Vector3.down * distance, Color.cyan);

                // we are underwater if the raycast doesn't hit anything
                return !Utils.RaycastWithout(origin, Vector3.down, out RaycastHit hit, distance, gameObject, canStandInWaterCheckLayers);
            }
            return false;
        }

        bool EventLadderEnter()
        {
            return ladderCollider != null;
        }

        bool EventLadderExit()
        {
            // OnTriggerExit isn't good enough to detect ladder exits because we
            // shouldn't exit as soon as our head sticks out of the ladder collider.
            // only if we fully left it. so check this manually here:
            return ladderCollider != null &&
                   !ladderCollider.bounds.Intersects(controllerCollider.bounds);
        }

        // helper function to apply gravity based on previous Y direction
        float ApplyGravity(float moveDirY)
        {
            // apply full gravity while falling
            if (!controller.isGrounded)
                // gravity needs to be * Time.fixedDeltaTime even though we multiply
                // the final controller.Move * Time.fixedDeltaTime too, because the
                // unit is 9.81m/s²
                return moveDirY + Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
            // if grounded then apply no force. the new OpenCharacterController
            // doesn't need a ground stick force. it would only make the character
            // slide on all uneven surfaces.
            return 0;
        }

        // helper function to get move or walk speed depending on key press & endurance
        float GetWalkOrRunSpeed()
        {
            //bool runRequested = Input.GetKey(runKey);
            //return runRequested ? runSpeed : walkSpeed;
            return walkSpeed;
        }

        void ApplyFallDamage()
        {
            // measure only the Y direction. we don't want to take fall damage
            // if we jump forward into a wall because xz is high.
            float fallMagnitude = Mathf.Abs(lastFall.y);
            if (fallMagnitude >= fallDamageMinimumMagnitude)
            {
                int damage = Mathf.RoundToInt(fallMagnitude * fallDamageMultiplier);
                Debug.LogWarning("Fall Damage: " + damage);
            }
        }

        // acceleration can be different when accelerating/decelerating
        float AccelerateSpeed(Vector2 inputDir, float currentSpeed, float targetSpeed, float acceleration)
        {
            // desired speed is between 'speed' and '0'
            float desiredSpeed = inputDir.magnitude * targetSpeed;

            // accelerate speed
            return Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * Time.fixedDeltaTime);
        }

        // rotate with QE keys
        //void RotateWithKeys()
        //{
        //    float horizontal2 = Input.GetAxis("Horizontal2");
        //    transform.Rotate(Vector3.up * horizontal2 * rotationSpeed * Time.fixedDeltaTime);
        //}

        void EnterLadder()
        {
            // make player look directly at ladder forward. but we also initialize
            // freelook manually already to overwrite the initial rotation, so
            // that in the end, the camera keeps looking at the same angle even
            // though we did modify transform.forward.
            // note: even though we set the rotation perfectly here, there's
            //       still one frame where it seems to interpolate between the
            //       new and the old rotation, which causes 1 odd camera frame.
            //       this could be avoided by overwriting transform.forward once
            //       more in LateUpdate.
            //look.InitializeFreeLook();
            transform.forward = ladderCollider.transform.forward;
        }

        MoveState UpdateIDLE(Vector2 inputDir, Vector3 desiredDir)
        {
            // QE key rotation
            //RotateWithKeys();

            // decelerate from last move (e.g. from jump)
            // (moveDir.xz can be set to 0 to have an interruption when landing)
            horizontalSpeed = AccelerateSpeed(inputDir, horizontalSpeed, 0, walkDeceleration);
            moveDir.x = desiredDir.x * horizontalSpeed;
            moveDir.y = ApplyGravity(moveDir.y);
            moveDir.z = desiredDir.z * horizontalSpeed;

            if (EventFalling())
            {
                //sprintingBeforeAirborne = false;
                return MoveState.AIRBORNE;
            }
            else if (EventJumpRequested())
            {
                // start the jump movement into Y dir, go to jumping
                // note: no endurance>0 check because it feels odd if we can't jump
                moveDir.y = jumpSpeed;
                //sprintingBeforeAirborne = false;
                PlayJumpSound();
                return MoveState.AIRBORNE;
            }
            else if (EventCrouchToggle())
            {
                // rescale capsule
                if (controller.TrySetHeight(controller.defaultHeight * 0.5f, true, true, false))
                    return MoveState.CROUCHING;
            }
            else if (EventLadderEnter())
            {
                EnterLadder();
                return MoveState.CLIMBING;
            }
            else if (EventUnderWater())
            {
                // rescale capsule
                if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
                    return MoveState.SWIMMING;
            }
            else if (inputDir != Vector2.zero)
            {
                return MoveState.WALKING;
            }

            return MoveState.IDLE;
        }

        MoveState UpdateWALKINGandRUNNING(Vector2 inputDir, Vector3 desiredDir)
        {
            // QE key rotation
            //RotateWithKeys();

            // walk or run?
            float speed = GetWalkOrRunSpeed();

            // move with acceleration (feels better)
            horizontalSpeed = AccelerateSpeed(inputDir, horizontalSpeed, speed, inputDir != Vector2.zero ? walkAcceleration : walkDeceleration);
            moveDir.x = desiredDir.x * horizontalSpeed;
            moveDir.y = ApplyGravity(moveDir.y);
            moveDir.z = desiredDir.z * horizontalSpeed;

            if (EventFalling())
            {
                //sprintingBeforeAirborne = speed == runSpeed;
                return MoveState.AIRBORNE;
            }
            else if (EventJumpRequested())
            {
                // start the jump movement into Y dir, go to jumping
                // note: no endurance>0 check because it feels odd if we can't jump
                moveDir.y = jumpSpeed;
                //sprintingBeforeAirborne = speed == runSpeed;
                PlayJumpSound();
                return MoveState.AIRBORNE;
            }
            else if (EventCrouchToggle())
            {
                // rescale capsule
                if (controller.TrySetHeight(controller.defaultHeight * 0.5f, true, true, false))
                {
                    // limit speed to crouch speed so we don't decelerate from run speed
                    // to crouch speed (hence crouching too fast for a short time)
                    horizontalSpeed = Mathf.Min(horizontalSpeed, crouchSpeed);
                    return MoveState.CROUCHING;
                }
            }
            else if (EventLadderEnter())
            {
                EnterLadder();
                return MoveState.CLIMBING;
            }
            else if (EventUnderWater())
            {
                // rescale capsule
                if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
                    return MoveState.SWIMMING;
            }
            // go to idle after fully decelerating (y doesn't matter)
            else if (moveDir.x == 0 && moveDir.z == 0)
            {
                return MoveState.IDLE;
            }

            ProgressStepCycle(inputDir, speed);
            return MoveState.WALKING;
        }

        MoveState UpdateCROUCHING(Vector2 inputDir, Vector3 desiredDir)
        {
            // QE key rotation
            //RotateWithKeys();

            // move with acceleration (feels better)
            horizontalSpeed = AccelerateSpeed(inputDir, horizontalSpeed, crouchSpeed, inputDir != Vector2.zero ? crouchAcceleration : crouchDeceleration);
            moveDir.x = desiredDir.x * horizontalSpeed;
            moveDir.y = ApplyGravity(moveDir.y);
            moveDir.z = desiredDir.z * horizontalSpeed;

            if (EventFalling())
            {
                // rescale capsule if possible
                if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
                {
                    //sprintingBeforeAirborne = false;
                    return MoveState.AIRBORNE;
                }
            }
            else if (EventJumpRequested())
            {
                // stop crouching when pressing jump key. this feels better than
                // jumping from the crouching state.

                // rescale capsule if possible
                if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
                {
                    return MoveState.IDLE;
                }
            }
            else if (EventCrouchToggle())
            {
                // rescale capsule if possible
                if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
                {
                    return MoveState.IDLE;
                }
            }
            else if (EventLadderEnter())
            {
                // rescale capsule if possible
                if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
                {
                    EnterLadder();
                    return MoveState.CLIMBING;
                }
            }
            else if (EventUnderWater())
            {
                // rescale capsule
                if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
                {
                    return MoveState.SWIMMING;
                }
            }

            ProgressStepCycle(inputDir, crouchSpeed);
            return MoveState.CROUCHING;
        }

        MoveState UpdateAIRBORNE(Vector2 inputDir, Vector3 desiredDir)
        {
            // QE key rotation
            //RotateWithKeys();

            // max speed depends on what we did before jumping/falling
            //float speed = sprintingBeforeAirborne ? runSpeed : walkSpeed;
            float speed = walkSpeed;

            // move with acceleration (feels better)
            horizontalSpeed = AccelerateSpeed(inputDir, horizontalSpeed, speed, inputDir != Vector2.zero ? airborneAcceleration : airborneDeceleration);
            moveDir.x = desiredDir.x * horizontalSpeed;
            moveDir.y = ApplyGravity(moveDir.y);
            moveDir.z = desiredDir.z * horizontalSpeed;

            if (EventLanded())
            {
                // apply fall damage only in AIRBORNE->Landed.
                // (e.g. not if we run face forward into a wall with high velocity)
                ApplyFallDamage();
                PlayLandingSound();
                return MoveState.IDLE;
            }
            else if (EventLadderEnter())
            {
                EnterLadder();
                return MoveState.CLIMBING;
            }
            else if (EventUnderWater())
            {
                // rescale capsule
                if (controller.TrySetHeight(controller.defaultHeight * 0.25f, true, true, false))
                {
                    return MoveState.SWIMMING;
                }
            }

            return MoveState.AIRBORNE;
        }

        MoveState UpdateCLIMBING(Vector2 inputDir, Vector3 desiredDir)
        {
            // finished climbing?
            if (EventLadderExit())
            {
                // player rotation was adjusted to ladder rotation before.
                // let's reset it, but also keep look forward
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                ladderCollider = null;
                return MoveState.IDLE;
            }

            // interpret forward/backward movement as upward/downward
            // note: NO ACCELERATION, otherwise we would climb really fast when
            //       sprinting towards a ladder. and the actual climb feels way too
            //       unresponsive when accelerating.
            moveDir.x = inputDir.x * climbSpeed;
            moveDir.y = inputDir.y * climbSpeed;
            moveDir.z = 0;

            // make the direction relative to ladder rotation. so when pressing right
            // we always climb to the right of the ladder, no matter how it's rotated
            moveDir = ladderCollider.transform.rotation * moveDir;
            Debug.DrawLine(transform.position, transform.position + moveDir, Color.yellow, 0.1f, false);

            return MoveState.CLIMBING;
        }

        MoveState UpdateSWIMMING(Vector2 inputDir, Vector3 desiredDir)
        {
            // ladder under / above water?
            if (EventLadderEnter())
            {
                // rescale capsule if possible
                if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
                {
                    EnterLadder();
                    return MoveState.CLIMBING;
                }
            }
            // not under water anymore?
            else if (!EventUnderWater())
            {
                // rescale capsule if possible
                if (controller.TrySetHeight(controller.defaultHeight * 1f, true, true, false))
                {
                    return MoveState.IDLE;
                }
            }

            // QE key rotation
            //RotateWithKeys();

            // horizontal input axis rotates the character instead of strafing
            if (player.IsMovementAllowed())
                transform.Rotate(Vector3.up * inputDir.x * rotationSpeed * Time.fixedDeltaTime);

            // move with acceleration (feels better)
            horizontalSpeed = AccelerateSpeed(inputDir, horizontalSpeed, swimSpeed, inputDir != Vector2.zero ? swimAcceleration : swimDeceleration);
            moveDir.x = desiredDir.x * horizontalSpeed;
            moveDir.z = desiredDir.z * horizontalSpeed;

            // gravitate toward surface
            if (waterCollider != null)
            {
                float surface = waterCollider.bounds.max.y;
                float surfaceDirection = surface - controller.bounds.min.y - swimSurfaceOffset;
                moveDir.y = surfaceDirection * swimSpeed;
            }
            else moveDir.y = 0;

            return MoveState.SWIMMING;
        }

        //TODO move to player input
        // use Update to check Input
        void Update()
        {
            // only if movement allowed (not typing, dead, menu etc.)
            if (player.IsMovementAllowed())
            {
                if (!jumpKeyPressed) jumpKeyPressed = Input.GetButtonDown("Jump");
                if (!crouchKeyPressed) crouchKeyPressed = Input.GetKeyDown(crouchKey);
            }
        }

        // CharacterController movement is physics based and requires FixedUpdate.
        // (using Update causes strange movement speeds in builds otherwise)
        void FixedUpdate()
        {
            // get input and desired direction based on camera and ground
            Vector2 inputDir = player.IsMovementAllowed() ? GetInputDirection() : Vector2.zero;
            Vector3 desiredDir = GetDesiredDirection(inputDir);

            Debug.DrawLine(transform.position, transform.position + desiredDir, Color.blue);
            Debug.DrawLine(transform.position, transform.position + desiredDir, Color.cyan);

            // update state machine
            if (State == MoveState.IDLE) state = UpdateIDLE(inputDir, desiredDir);
            else if (State == MoveState.WALKING) state = UpdateWALKINGandRUNNING(inputDir, desiredDir);
            else if (State == MoveState.CROUCHING) state = UpdateCROUCHING(inputDir, desiredDir);
            else if (State == MoveState.AIRBORNE) state = UpdateAIRBORNE(inputDir, desiredDir);
            else if (State == MoveState.CLIMBING) state = UpdateCLIMBING(inputDir, desiredDir);
            else if (State == MoveState.SWIMMING) state = UpdateSWIMMING(inputDir, desiredDir);
            else Debug.LogError("Unhandled Movement State: " + State);

            // cache this move's state to detect landing etc. next time
            if (!controller.isGrounded) lastFall = controller.velocity;

            // move depending on latest moveDir changes
            Debug.DrawLine(transform.position, transform.position + moveDir * Time.fixedDeltaTime, Color.magenta);
            controller.Move(moveDir * Time.fixedDeltaTime); // note: returns CollisionFlags if needed
            Velocity = controller.velocity; // for animations and fall damage

            // reset keys no matter what
            jumpKeyPressed = false;
            crouchKeyPressed = false;
        }

        void ProgressStepCycle(Vector3 inputDir, float speed)
        {
            if (Velocity.sqrMagnitude > 0 && (inputDir.x != 0 || inputDir.y != 0))
            {
                stepCycle += (Velocity.magnitude + (speed * (State == MoveState.WALKING ? 1 : runStepLength))) *
                             Time.fixedDeltaTime;
            }

            if (stepCycle > nextStep)
            {
                nextStep = stepCycle + runStepInterval;
                PlayFootStepAudio();
            }
        }

        public override Vector3 GetVelocity()
        {
            return Velocity;
        }

        public override bool IsMoving()
        {
            return Velocity != Vector3.zero;
        }

        public override bool IsGrounded()
        {
            return controller.isGrounded;
        }

        public override void SetSpeed(float speed)
        {
            throw new System.NotImplementedException();
        }

        public override void LookAtY(Vector3 position)
        {
            transform.LookAt(new Vector3(position.x, transform.position.y, position.z));
        }

        public override void Reset()
        {
            // we have no navigation, so we don't need to reset any paths
        }

        public override void Warp(Vector3 destination)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanNavigate()
        {
            return false;
        }

        public override void Navigate(Vector3 destination, float stoppingDistance)
        {
            // character controller movement doesn't allow navigation
        }

        public override bool IsValidSpawnPoint(Vector3 position)
        {
            // all positions allowed for now.
            // we should probably raycast later.
            return true;
        }

        public override Vector3 NearestValidDestination(Vector3 destination)
        {
            // character controller movement doesn't allow navigation
            return destination;
        }

        public override bool DoCombatLookAt()
        {
            // player should use keys/mouse to look at. don't overwrite it.
            return false;
        }

        void OnTriggerEnter(Collider co)
        {
            // touching ladder? then set ladder collider
            if (co.CompareTag("Ladder"))
                ladderCollider = co;
            // touching water? then set water collider
            else if (co.CompareTag("Water"))
                waterCollider = co;
        }

        void OnTriggerExit(Collider co)
        {
            // not touching water anymore? then clear water collider
            if (co.CompareTag("Water"))
                waterCollider = null;
        }

        private void OnValidate()
        {
            // auto-reference entity
            if (player == null && TryGetComponent(out Player playerComponent))
            {
                player = playerComponent;
            }
        }

        void OnGUI()
        {
            // show data next to player for easier debugging. this is very useful!
            if (Debug.isDebugBuild)
            {
                // project player position to screen
                Vector3 center = controllerCollider.bounds.center;
                Vector3 point = cam.WorldToScreenPoint(center);

                // in front of camera and in screen?
                if (point.z >= 0 && Utils.IsPointInScreen(point))
                {
                    GUI.color = new Color(0, 0, 0, 0.5f);
                    GUILayout.BeginArea(new Rect(point.x, Screen.height - point.y, 150, 200));

                    // some info for all players, including local
                    GUILayout.Label("grounded=" + controller.isGrounded);
                    GUILayout.Label("groundedTol=" + isGroundedWithinTolerance);
                    GUILayout.Label("lastFall=" + lastFall);
                    GUILayout.Label("sliding=" + controller.slidingState);
                    GUILayout.Label("state=" + State);

                    GUILayout.EndArea();
                    GUI.color = Color.white;
                }
            }
        }
    }
}