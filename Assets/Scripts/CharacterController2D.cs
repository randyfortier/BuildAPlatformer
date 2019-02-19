using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour {

    [Header("General")]
    [SerializeField] private float movementSpeed = 10f;
    [Range(0, .3f)] [SerializeField] private float movementSmoothing = .05f;
    [SerializeField] private LayerMask groundLayers;
    public float speed = 0.0f;

    [Header("Jumping")]
    [SerializeField] private bool canAirControl = true;
    [SerializeField] private float jumpForce = 500f;
    [SerializeField] private Transform groundPosition;
    public bool isGrounded;

    [Header("Crouching")]
    [SerializeField] private Collider2D colliderToDisableOnCrouch;
    [SerializeField] private Transform ceilingPosition;
    [Range(0, 1)] [SerializeField] private float crouchSpeedMultiplier = .4f;

    private bool isFacingRight = true;

    const float groundedRadius = .2f;
    const float ceilingRadius = .2f;

    private Rigidbody2D rigidBody;
    private Vector3 velocity = Vector3.zero;

    [Header("Events")]
    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    private bool wasCrouching = false;

    private void Awake() {
        rigidBody = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null) {
            OnLandEvent = new UnityEvent();
        }

        if (OnCrouchEvent == null) {
            OnCrouchEvent = new BoolEvent();
        }
    }

    private void FixedUpdate() {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        // find any ground layer colliders closer than the ground position
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundPosition.position, groundedRadius, groundLayers);
        Debug.Log("Overlapping colliders: " + colliders.Length);
        for (int i = 0; i < colliders.Length; i++) {
            // if any of the colliders are not the object iself, it must be the ground
            if (colliders[i].gameObject != gameObject) {
                // we are now grounded
                isGrounded = true;

                // if we were not grounded before, but now are, generate the landed event
                if (!wasGrounded) {
                    OnLandEvent.Invoke();
                }
            }
        }
    }


    public void Move(float move, bool crouch, bool jump) {
        if (!crouch) {
            if (Physics2D.OverlapCircle(ceilingPosition.position, ceilingRadius, groundLayers)) {
                // the player cannot currently stand up
                crouch = true;
            }
        }

        // only control the player if grounded or canAirControl is turned on
        if (isGrounded || canAirControl) {
            if (crouch) {
                if (!wasCrouching) {
                    // if we weren't crouching before, but now are, generate the crouch event
                    wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // move more slowly when crouched
                move *= crouchSpeedMultiplier;

                // when crouching, disable the upper collider
                if (colliderToDisableOnCrouch != null) {
                    colliderToDisableOnCrouch.enabled = false;
                }
            } else {
                // enable the collider when not crouching
                if (colliderToDisableOnCrouch != null) {
                    colliderToDisableOnCrouch.enabled = true;
                }

                if (wasCrouching) {
                    // if previously crouching, but are no longer, generate the (un)crouch event
                    wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            // what speed do we want to travel?
            Vector3 targetVelocity = new Vector2(move * movementSpeed, GetComponent<Rigidbody2D>().velocity.y);

            // apply smoothing to the speed
            rigidBody.velocity = Vector3.SmoothDamp(rigidBody.velocity, targetVelocity, ref velocity, movementSmoothing);

            // export the speed
            speed = rigidBody.velocity.magnitude;

            if (move > 0 && !isFacingRight) {
                // flip the sprite horizontally when travelling left
                Flip();
            } else if (move < 0 && isFacingRight) {
                // flip the sprite horizontally when travelling left
                Flip();
            }
        }
        if (isGrounded && jump) {
            // add a vertical force to the player
            isGrounded = false;
            rigidBody.AddForce(new Vector2(0f, jumpForce));
        }
    }

    private void Flip() {
        // remember which way the sprite is facing
        isFacingRight = !isFacingRight;

        // multiply the player's x local scale by -1
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}
