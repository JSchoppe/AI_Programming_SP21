using System;
using UnityEngine;
using EngineInterop;
using Services;
using Input;
using Math;
using Extensions.Unity;
using Extensions.CSharp;

namespace AI.Actors.StateQueueActors
{
    /// <summary>
    /// An actor that follows the mouse click to a position on the scene geometry.
    /// </summary>
    public sealed class MouseClickFollowActor
        : StateQueueActor<MouseClickFollowActor.BehaviourState>
    {
        #region State Definition
        /// <summary>
        /// Defines the states available to this actor.
        /// </summary>
        public enum BehaviourState : byte
        {
            Moving, Jumping
        }
        #endregion
        #region Constant Fields
        // Creates an inverted parabola from 0 to 1
        // with a peak at 1.
        private readonly ContinuousFunction PROJECTILE_01
            = (float x) => { return 2f * (4f * x) * (1f - x); };
        #endregion
        #region Behaviour State Fields
        private Vector2 target;
        private float jumpTimeElapsed;
        private float jumpStartAngleY;
        private Vector2 jumpGapStart;
        private Vector2 jumpGapDirection;
        #endregion
        #region Reference Fields
        private UnityUpdateContext updateContext;
        #endregion
        #region Inspector Fields
        [Header("References")]
        [Tooltip("Notifies this script when a collider has been clicked.")]
        [SerializeField] private ColliderClickBroadcaster colliderClickBroadcaster = default;
        [Header("Movement Parameters")]
        [Tooltip("The turn speed in radians per second.")]
        [SerializeField] private float turnSpeed = 1f;
        [Tooltip("The walk speed in meters per second.")]
        [SerializeField] private float walkSpeed = 1f;
        [Header("Jump Parameters")]
        [Tooltip("The jump height in meters.")]
        [SerializeField] private float jumpHeight = 1f;
        [Tooltip("The time spent to jump.")]
        [SerializeField] private float jumpDuration = 1f;
        [Tooltip("The distance this actor will jump over a gap.")]
        [SerializeField] private float jumpGapDistance = 3f;
        private void OnValidate()
        {
            // Prevent parameters that cause inescapable
            // behaviour or errors.
            turnSpeed.LowerLimit(0.005f);
            walkSpeed.LowerLimit(0.005f);
            jumpDuration.LowerLimit(0.005f);
            jumpGapDistance.LowerLimit(0f);
        }
        #endregion
        #region Initialization
        private void Start()
        {
            // Grab a copy of the update service and
            // subscribe to the collider click broadcaster.
            updateContext = ServiceManager.RetrieveService<UnityUpdateContext>();
            colliderClickBroadcaster.ColliderClicked += OnColliderClicked;
        }
        #endregion
        #region On Destroy
        private void OnDestroy()
        {
            updateContext.Draw -= MoveUpdate;
            updateContext.Draw -= JumpUpdate;
            updateContext.Draw -= JumpGapUpdate;
        }
        #endregion
        // TODO this functionality should be abstracted.
        #region Transform Wrapping
        // These properties are used to simplify
        // the logic into a local movement space.
        private Vector2 Position
        {
            get => new Vector2(transform.position.x, transform.position.z);
            set
            {
                transform.position = new Vector3
                {
                    x = value.x,
                    y = transform.position.y,
                    z = value.y
                };
            }
        }
        public Vector2 Forwards
        {
            get => new Vector2(transform.forward.x, transform.forward.z);
            set
            {
                transform.forward = new Vector3(value.x, 0f, value.y);
            }
        }
        private float Elevation
        {
            set
            {
                transform.SetPositionY(value);
            }
        }
        #endregion
        #region Collider Clicked Reciever
        private void OnColliderClicked(RaycastHit hitInfo)
        {
            // Set the target and push state that will invoke
            // moving, then jumping behaviour.
            target = new Vector2(hitInfo.point.x, hitInfo.point.z);
            EnqueueStateInterrupt(BehaviourState.Moving, BehaviourState.Jumping);
        }
        #endregion
        #region State Initialization
        protected override void EnterState(BehaviourState state)
        {
            switch (state)
            {
                case BehaviourState.Moving: EnterMove(); break;
                case BehaviourState.Jumping: EnterJump(); break;
                default: throw new NotImplementedException();
            }
        }
        #endregion
        // Behaviour code starts here.
        // TODO these behaviour sets can be abstracted to pluggable classes.
        #region Move Behaviour
        private void EnterMove()
        {
            updateContext.Draw += MoveUpdate;
        }
        private void MoveUpdate(float deltaTime)
        {
            // TODO allocate on class scope for efficiency.
            Vector2 desiredDirection = target - Position;
            // Turn the actor towards their target.
            Forwards = Vector3.RotateTowards(Forwards, desiredDirection, turnSpeed * deltaTime, 0f);
            // Make sure we are not stepping over the target.
            if (walkSpeed * walkSpeed * deltaTime * deltaTime < desiredDirection.sqrMagnitude)
            {
                float walkDistance = walkSpeed * deltaTime;
                // Is there ground in front of this actor?
                if (Physics.Raycast(
                    transform.position + transform.forward * walkDistance + Vector3.up,
                    Vector3.down,
                    2f))
                {
                    // Then move forwards.
                    Position += Forwards * walkDistance;
                }
                else
                {
                    // Otherwise is there ground further ahead?
                    if (Physics.Raycast(
                        transform.position + transform.forward * jumpGapDistance + Vector3.up,
                        Vector3.down,
                        2f))
                    {
                        // Jump over to the other side.
                        updateContext.Draw -= MoveUpdate;
                        EnterJumpGap();
                    }
                }
            }
            else
            {
                // Exit this state once we reach the destination.
                Position = target;
                ExitMove();
            }
        }
        private void ExitMove()
        {
            updateContext.Draw -= MoveUpdate;
            OnStateExited();
        }
        #endregion
        #region Jump Gap Behaviour
        private void EnterJumpGap()
        {
            updateContext.Draw += JumpGapUpdate;
            jumpTimeElapsed = 0f;
            jumpGapStart = Position;
            jumpGapDirection = Forwards;
        }
        private void JumpGapUpdate(float deltaTime)
        {
            // Animate jumping over the gap.
            jumpTimeElapsed += deltaTime;
            float interpolant = jumpTimeElapsed / (jumpGapDistance / walkSpeed);
            if (interpolant < 1f)
            {
                Position = jumpGapStart + jumpGapDirection * interpolant * jumpGapDistance;
                Elevation = jumpHeight * PROJECTILE_01(interpolant);
            }
            else
            {
                Position = jumpGapStart + jumpGapDirection * jumpGapDistance;
                Elevation = 0f;
                ExitJumpGap();
            }
        }
        private void ExitJumpGap()
        {
            updateContext.Draw -= JumpGapUpdate;
            EnterMove();
        }
        #endregion
        #region Jump Behaviour
        private void EnterJump()
        {
            updateContext.Draw += JumpUpdate;
            jumpTimeElapsed = 0f;
            jumpStartAngleY = transform.eulerAngles.y;
        }
        private void JumpUpdate(float deltaTime)
        {
            // Advance animation timing.
            // TODO should be abstracted or use Animator.
            jumpTimeElapsed += deltaTime;
            float interpolant = jumpTimeElapsed / jumpDuration;
            // Make sure the animation has not elapsed.
            if (interpolant < 1f)
            {
                // Apply animation.
                Elevation = jumpHeight * PROJECTILE_01(interpolant);
                transform.localScale = Vector3.one - Vector3.one * 0.25f * PROJECTILE_01(interpolant);
                transform.SetEulerAngleY(jumpStartAngleY + 360f * interpolant);
            }
            else
            {
                // Finalize animation and exit.
                Elevation = 0f;
                transform.localScale = Vector3.one;
                transform.SetEulerAngleY(jumpStartAngleY);
                ExitJump();
            }
        }
        private void ExitJump()
        {
            updateContext.Draw -= JumpUpdate;
            OnStateExited();
        }
        #endregion
    }
}
