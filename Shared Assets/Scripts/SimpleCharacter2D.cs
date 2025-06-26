#nullable enable

namespace Yarn.Unity.Samples
{
    using UnityEngine;
    using System.Threading;
    using Yarn.Unity.Attributes;
    using System.Collections.Generic;
    using UnityEngine.Events;
    using System.Threading.Tasks;

    public class SimpleCharacter2D : MonoBehaviour
    {
        public enum CharacterMode
        {
            PlayerControlledMovement,
            ExternallyControlledMovement,
            Interact,
        }

        public enum FacingDirection { Left, Right };

        public CharacterMode Mode { get; private set; }

        public bool CanInteract => Mode == CharacterMode.PlayerControlledMovement;

        [SerializeField] bool isPlayerControlled;

        [SerializeField] FacingDirection initialFacingDirection = FacingDirection.Left;

        #region Movement Variables

        [Group("Movement")]
        [SerializeField] float speed;

        [Group("Movement")]
        [SerializeField] bool lockY = false;
        [Group("Movement")]
        [ShowIf(nameof(lockY))]
        [Indent]
        [SerializeField] float yPosition = 0;
        [Group("Movement")]
        [HideIf(nameof(lockY))]
        [SerializeField] float gravity = 10;

        [Group("Movement")]
        [HideIf(nameof(lockY))]
        [SerializeField] float jumpHeight = 5;



        [Group("Movement")]
        [SerializeField] float walkAcceleration = 0.5f;
        [Group("Movement")]
        [HideIf(nameof(lockY))]
        [SerializeField] float airAcceleration = 0.5f;
        [Group("Movement")]
        [SerializeField] float groundDeceleration = 0.1f;
        [Group("Movement")]
        public Transform? lookTarget;
        [Group("Movement")]
        [ShowIf(nameof(isPlayerControlled))]
        [SerializeField] float outOfBoundsYPosition = -5;
        [Group("Movement")]
        [ShowIf(nameof(isPlayerControlled))]
        [HideIf(nameof(lockY))]
        [SerializeField] float groundStickiness = 0.05f;

        [Group("Movement")]
        [ShowIf(nameof(isPlayerControlled))]
        [SerializeField] InputAxisVector2 movementInput = new();

        [Group("Movement")]
        [ShowIf(nameof(isPlayerControlled))]
        [SerializeField] InputAxisButton interactInput = new();

        [Group("Movement")]
        [ShowIf(nameof(isPlayerControlled))]
        [HideIf(nameof(lockY))]
        [SerializeField] InputAxisButton jumpInput = new();


        struct MovementState
        {
            public Vector2 input;
            public Vector2 velocity;
            public bool isGrounded;

            public Vector2 lastGroundedPosition;

        }

        MovementState movementState;

        CapsuleCollider2D? capsuleCollider;

        readonly Collider2D[] overlaps = new Collider2D[10];
        readonly RaycastHit2D[] groundRaycasts = new RaycastHit2D[10];

        #endregion

        #region Animation Variables

        [Group("Animation")]
        [SerializeField] Animator? animator;
        [Group("Animation")]
        [AnimationParameter(nameof(animator), AnimatorControllerParameterType.Float)]
        [SerializeField] string? speedParameter;
        [Group("Animation")]
        [AnimationParameter(nameof(animator), AnimatorControllerParameterType.Int)]
        [SerializeField] string? directionParameter;
        [Group("Animation")]
        [AnimationState(nameof(animator))]
        [SerializeField] string? initialState;
        [Group("Animation")]
        [AnimationParameter(nameof(animator), AnimatorControllerParameterType.Float)]
        [SerializeField] string cycleOffsetParameter = "Cycle Offset";

        private int speedParameterHash;
        private int directionParameterHash;
        #endregion


        #region Interaction Variables
        [Group("Interaction")]
        [ShowIf(nameof(isPlayerControlled))]
        [SerializeField] float interactionRadius = 1f;
        [Group("Interaction")]
        [ShowIf(nameof(isPlayerControlled))]
        [SerializeField] Vector3 offset = Vector3.zero;

        [Group("Interaction")]
        [ShowIf(nameof(isPlayerControlled))]
        [SerializeField] UnityEvent<Interactable>? onInteracting;

        private List<Interactable> interactables = new();

        private Interactable? _currentInteractable = null;
        private Interactable? CurrentInteractable
        {
            get => _currentInteractable;
            set
            {
                var prev = _currentInteractable;
                var next = value;
                _currentInteractable = value;

                if (prev != value)
                {
                    if (prev != null) { prev.IsCurrent = false; }
                    if (next != null) { next.IsCurrent = true; }
                }
            }
        }

        private TriggerArea? currentTriggerArea;

        #endregion

        #region Animation Commands

        public void SetFacingDirection(FacingDirection direction)
        {
            if (animator == null || directionParameterHash == -1)
            {
                Debug.LogWarning($"Can't set facing direction: no animator available, or direction parameter not set");
                return;
            }
            switch (direction)
            {
                case FacingDirection.Left:
                    animator.SetInteger(directionParameterHash, -1);
                    break;
                case FacingDirection.Right:
                    animator.SetInteger(directionParameterHash, 1);
                    break;
            }
        }

        #endregion

        #region Animation Logic

        protected void SetupAnimation()
        {
            speedParameterHash = string.IsNullOrEmpty(speedParameter) ? -1 : Animator.StringToHash(this.speedParameter);
            directionParameterHash = string.IsNullOrEmpty(directionParameter) ? -1 : Animator.StringToHash(this.directionParameter);

            if (animator != null)
            {
                if (!string.IsNullOrEmpty(initialState))
                {
                    animator.Play(initialState);
                }

                // Randomly offset the cycle for the idle states so that
                // characters don't sync up
                animator.SetFloat(cycleOffsetParameter, Random.value);
            }
        }

        public void UpdateAnimation()
        {
            var speedFactor = Mathf.Abs(this.movementState.velocity.x) / this.speed;

            if (this.movementState.isGrounded == false)
            {
                speedFactor = 0;
            }

            if (animator != null)
            {
                if (speedParameterHash != -1)
                {
                    animator.SetFloat(speedParameterHash, speedFactor);
                }

                if (directionParameterHash != -1)
                {
                    int direction;
                    if (lookTarget != null)
                    {
                        // We have a look target. Look towards it.
                        direction = (int)Mathf.Sign((lookTarget.transform.position - this.transform.position).x);
                    }
                    else if (!isPlayerControlled && lookTarget == null)
                    {
                        // We aren't player controlled, and we don't have a
                        // current look target. Return to our initial facing
                        // direction.
                        direction = initialFacingDirection switch
                        {
                            FacingDirection.Left => -1,
                            FacingDirection.Right => 1,
                            _ => 0,
                        };
                    }
                    else if (isPlayerControlled && Mathf.Abs(this.movementState.input.x) > 0)
                    {
                        // We're player controlled, and the player wants to
                        // move. Look in the direction we're moving.
                        direction = this.movementState.input.x switch
                        {
                            > 0 => 1,
                            < 0 => -1,
                            _ => 0,
                        };
                    }
                    else
                    {
                        // No direction to look at.
                        direction = 0;
                    }

                    animator.SetInteger(directionParameterHash, direction);
                }
            }


            // if (movementState.isGrounded)
            // {
            //     if (sprite == null)
            //     {
            //         return;
            //     }

            //     if (velocity.x == 0 && forwardSprite != null)
            //     {
            //         // change to forward-facing
            //         sprite.sprite = forwardSprite;
            //     }
            //     else if (Mathf.Abs(input.horizontal) > 0.01f)
            //     {
            //         // Update our facing direction
            //         if (velocity.x > 0 && rightSprite != null)
            //         {
            //             // change to right-facing
            //             sprite.sprite = rightSprite;
            //         }
            //         else if (leftSprite != null)
            //         {
            //             // change to left-facing
            //             sprite.sprite = leftSprite;
            //         }
            //     }
            // }
        }
        #endregion


        #region Movement Logic

        private void SetupMovement()
        {
            capsuleCollider = GetComponentInChildren<CapsuleCollider2D>();

            movementInput.Enable();
            interactInput.Enable();

        }

        protected void UpdateMovement()
        {
            if (Mode == CharacterMode.Interact)
            {
                // No movement at all; stay in place
                this.movementState.velocity = Vector2.zero;
                this.movementState.input = Vector2.zero;
                return;
            }

            if (isPlayerControlled && Mode == CharacterMode.PlayerControlledMovement)
            {
                Vector2 input = movementInput.Value;

                ApplyMovement(input);
            }
            else
            {
                ApplyMovement(Vector2.zero);
            }
        }

        private void ApplyMovement(Vector2 input)
        {
            if (movementState.isGrounded)
            {
                movementState.velocity.y = 0;

                if (jumpInput.WasPressedThisFrame)
                {
                    // Calculate the velocity required to achieve the target jump height.
                    movementState.velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(-gravity));
                }
            }

            if (lockY)
            {
                movementState.isGrounded = true;
            }

            movementState.input = input;

            float acceleration = movementState.isGrounded ? walkAcceleration : airAcceleration;
            float deceleration = movementState.isGrounded ? groundDeceleration : 0;

            if (input.x != 0)
            {
                movementState.velocity.x = Mathf.MoveTowards(movementState.velocity.x, speed * input.x, acceleration * Time.deltaTime);
            }
            else
            {
                movementState.velocity.x = Mathf.MoveTowards(movementState.velocity.x, 0, deceleration * Time.deltaTime);
            }

            if (capsuleCollider != null)
            {
                movementState.velocity.y += -gravity * Time.deltaTime;
            }

            transform.Translate(movementState.velocity * Time.deltaTime);

            if (lockY == false && groundStickiness > 0 && jumpInput.IsPressed == false && movementState.isGrounded == false)
            {
                // We were grounded last frame, but we're not anymore, and we're
                // not jumping. We're therefore free-floating in the air. Do a
                // quick short-range raycast to see if there's a collider right
                // below us. If there is, snap to it.

                var raycastHits = Physics2D.Raycast(transform.position, Vector2.down, new() { useTriggers = false }, groundRaycasts, groundStickiness);

                for (int hitID = 0; hitID < raycastHits; hitID++)
                {
                    RaycastHit2D hit = groundRaycasts[hitID];
                    if (hit.collider == capsuleCollider)
                    {
                        continue;
                    }
                    transform.position = hit.point;
                    movementState.isGrounded = true;
                    break;
                }
            }

            movementState.isGrounded = false;

            if (capsuleCollider != null)
            {
                Physics2D.SyncTransforms();

                // Retrieve all colliders we have intersected after velocity has been applied.
                int hits = Physics2D.OverlapCapsule(
                    point: this.capsuleCollider.offset + (Vector2)transform.position,
                    size: capsuleCollider.size,
                    direction: CapsuleDirection2D.Vertical,
                    angle: 0,
                    contactFilter: new() { useTriggers = false },
                    results: overlaps);

                for (int hitIndex = 0; hitIndex < hits; hitIndex++)
                {
                    Collider2D hit = overlaps[hitIndex];

                    // Ignore our own collider.
                    if (hit == capsuleCollider)
                        continue;

                    ColliderDistance2D colliderDistance = hit.Distance(capsuleCollider);

                    // Ensure that we are still overlapping this collider.
                    // The overlap may no longer exist due to another intersected collider
                    // pushing us out of this one.
                    if (colliderDistance.isOverlapped)
                    {
                        transform.Translate(colliderDistance.pointA - colliderDistance.pointB);

                        // If we intersect an object beneath us, set grounded to true. 
                        if (Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && movementState.velocity.y < 0)
                        {
                            movementState.isGrounded = true;
                        }
                    }
                }

                // If the character falls out of bounds, warp them to the last point
                // they were on the ground
                if (transform.position.y < outOfBoundsYPosition)
                {
                    transform.position = movementState.lastGroundedPosition;
                    movementState.velocity = Vector2.zero;
                }

                // If we're grounded, set last grounded position to here
                if (movementState.isGrounded)
                {
                    movementState.lastGroundedPosition = transform.position;
                }
            }

            if (lockY)
            {
                transform.position = new Vector3(transform.position.x, yPosition, transform.position.z);
                movementState.isGrounded = true;
            }

        }

        #endregion

        #region Interaction Logic

        public void SetupInteraction()
        {
            interactables.Clear();

            interactables.AddRange(FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        }

        protected void UpdateInteraction()
        {
            if (isPlayerControlled == false)
            {
                // Only player-controlled characters can interact
                return;
            }

            if (!CanInteract)
            {
                // We can only interact if we're allowed to move around.
                return;
            }

            var previousInteractable = CurrentInteractable;

            (float Distance, Interactable? Interactable) nearest = (float.PositiveInfinity, null);

            for (int i = 0; i < interactables.Count; i++)
            {
                var interactable = interactables[i];

                if (!interactable.isActiveAndEnabled)
                {
                    // We can't interact if the component or its gameobject
                    // isn't enabled
                    continue;
                }

                if (interactable.gameObject == gameObject)
                {
                    // We can't interact with ourselves
                    continue;
                }

                if (interactable.gameObject.TryGetComponent<SimpleCharacter>(out var character) && !character.IsAlive)
                {
                    // We can't interact with characters that aren't alive
                    continue;
                }

                var distance = Vector3.Distance(transform.TransformPoint(offset), interactable.transform.position);
                if (distance > interactionRadius)
                {
                    continue;
                }
                if (distance < nearest.Distance)
                {
                    nearest = (distance, interactable);
                }
            }

            if (previousInteractable != nearest.Interactable)
            {
                CurrentInteractable = nearest.Interactable;
            }

            if (this.capsuleCollider != null)
            {
                // If we have a collider, check to see if we're in a trigger
                // area. If we are, and weren't before, then run code to handle
                // entering this area.

                // Overlap our capsule with colliders.
                TriggerArea? hitTriggerArea = null;
                int hits = Physics2D.OverlapCapsule(
                    point: this.capsuleCollider.offset + (Vector2)transform.position,
                    size: capsuleCollider.size,
                    direction: CapsuleDirection2D.Vertical,
                    angle: 0,
                    contactFilter: new() { useTriggers = true },
                    results: overlaps);

                // Are we overlapping a trigger area?
                for (int i = 0; i < hits; i++)
                {
                    var collider = overlaps[i];
                    if (collider.TryGetComponent<TriggerArea>(out hitTriggerArea))
                    {
                        break;
                    }
                }

                if (this.currentTriggerArea != hitTriggerArea)
                {
                    this.currentTriggerArea = hitTriggerArea;
                    if (this.currentTriggerArea != null)
                    {
                        // We entered a trigger area.
                        this.OnEnteredTriggerArea(this.currentTriggerArea);
                    }
                    else
                    {
                        // We exited a trigger area.
                    }
                }
            }

            if (interactInput.WasPressedThisFrame && CurrentInteractable != null)
            {
                async YarnTask RunInteraction(Interactable interactable, CancellationToken cancellationToken)
                {
                    var previousMode = Mode;
                    Mode = CharacterMode.Interact;

                    if (interactable.InteractorShouldTurnToFaceWhenInteracted)
                    {
                        lookTarget = interactable.transform;
                    }

                    interactable.IsCurrent = false;
                    CurrentInteractable = null;

                    onInteracting?.Invoke(interactable);
                    await interactable.Interact(gameObject);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (interactable.InteractorShouldTurnToFaceWhenInteracted)
                    {
                        lookTarget = null;
                    }

                    // Wait a frame so that if 'advance dialogue' is the same
                    // button as 'interact', we don't accidentally trigger a new
                    // dialogue with the same input as leaving the previous
                    // dialogue (i.e. we'd never leave dialogue)
                    await YarnTask.Yield();

                    Mode = previousMode;

                }
                RunInteraction(CurrentInteractable, this.destroyCancellationToken).Forget();
            }
        }

        private void OnEnteredTriggerArea(TriggerArea currentTriggerArea)
        {
            async YarnTask RunEnter()
            {
                this.CurrentInteractable = null;
                var previousMode = this.Mode;
                this.Mode = CharacterMode.Interact;
                await currentTriggerArea.OnPlayerEntered();
                this.Mode = previousMode;
            }
            RunEnter().Forget();
        }

        #endregion

        #region Core Logic

        protected void Awake()
        {
            Mode = CharacterMode.PlayerControlledMovement;

            SetupMovement();
            SetupAnimation();
            SetupInteraction();
        }

        protected void Update()
        {
            UpdateMovement();
            UpdateAnimation();
            UpdateInteraction();
        }

        protected void OnDrawGizmosSelected()
        {
            if (isPlayerControlled)
            {
                // Show interaction volume
                Gizmos.color = Color.yellow;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Gizmos.DrawWireSphere(offset, interactionRadius);
            }
        }
        #endregion
    }
}
