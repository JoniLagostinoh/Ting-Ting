using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Dragable : MonoBehaviour
{
    [Header("Grab spring (TargetJoint2D)")]
    [Tooltip("Spring stiffness in Hz. Higher = snappier, lower = floaty.")]
    [SerializeField] private float frequency = 4f;
    [Tooltip("0 = bouncy wobble, 1 = no overshoot. 0.3-0.5 feels lively.")]
    [SerializeField, Range(0f, 1f)] private float dampingRatio = 0.35f;
    [Tooltip("Max pull force per unit of mass, so heavy things feel heavy.")]
    [SerializeField] private float maxForcePerMass = 400f;

    [Header("While held")]
    [Tooltip("Lighter gravity while held makes the object feel carried, not dangling.")]
    [SerializeField] private float heldGravityScale = 0.3f;
    [Tooltip("Low = swings and spins freely around the grab point.")]
    [SerializeField] private float heldAngularDamping = 1.5f;

    [Header("Release / throw")]
    [SerializeField] private float throwMultiplier = 1.15f;
    [SerializeField] private float maxThrowSpeed = 25f;

    [Header("Juice")]
    [SerializeField] private float pickupScale = 1.1f;
    [SerializeField] private float scaleSpeed = 14f;

    protected Rigidbody2D rb;
    protected TargetJoint2D joint;
    public bool IsHeld => joint != null;

    private float baseGravity;
    private float baseAngularDamping;
    private Vector3 baseScale;
    private Vector3 targetScale;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // fast throws won't tunnel

        baseGravity = rb.gravityScale;
        baseAngularDamping = rb.angularDamping;
        baseScale = targetScale = transform.localScale;
    }

    protected virtual void Update()
    {
        // Smooth "pop" when picked up and settle when dropped
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, 1f - Mathf.Exp(-scaleSpeed * Time.deltaTime));
    }

    public virtual void Grab(Vector2 hitPoint)
    {
        if (IsHeld) return;

        // The joint pulls from the exact point you clicked, so grabbing a corner
        // makes the object swing and rotate around that corner. No fake rotation code needed.
        joint = gameObject.AddComponent<TargetJoint2D>();
        joint.autoConfigureTarget = false;
        joint.anchor = transform.InverseTransformPoint(hitPoint);
        joint.target = hitPoint;
        joint.frequency = frequency;
        joint.dampingRatio = dampingRatio;
        joint.maxForce = rb.mass * maxForcePerMass;

        rb.gravityScale = heldGravityScale;
        rb.angularDamping = heldAngularDamping;
        rb.WakeUp();

        targetScale = baseScale * pickupScale;
    }

    public virtual void Drag(Vector2 worldPoint)
    {
        if (!IsHeld) return;
        joint.target = worldPoint;
    }

    public virtual void Release()
    {
        if (!IsHeld) return;

        Destroy(joint);
        joint = null;

        rb.gravityScale = baseGravity;
        rb.angularDamping = baseAngularDamping;

        // Keep the momentum you built up (spring-driven velocity + spin) and exaggerate it a bit
        Vector2 v = rb.linearVelocity * throwMultiplier;
        rb.linearVelocity = Vector2.ClampMagnitude(v, maxThrowSpeed);

        targetScale = baseScale;
    }
}