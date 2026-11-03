using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private LayerMask raycastMask = ~0;

    private InputSystem_Actions inputActions;
    private Camera cam;
    private Dragable holdableObject;

    private void Awake()
    {
        cam = Camera.main;
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable() => inputActions.MainCharacter.Enable();
    private void OnDisable()
    {
        inputActions.MainCharacter.Disable();
        DropHeld();
    }

    // Per-frame input: the joint target follows the cursor smoothly.
    // The old 10Hz tick made the target move in steps, which is a big part of the "dry" feel.
    private void Update()
    {
        bool pressed = inputActions.MainCharacter.LMB.IsPressed();

        if (holdableObject == null)
        {
            if (pressed) TryGrab();
        }
        else if (pressed)
        {
            holdableObject.Drag(PointerWorld());
        }
        else
        {
            DropHeld();
        }
    }

    private void TryGrab()
    {
        Vector2 worldPoint = PointerWorld();

        Collider2D hit = Physics2D.OverlapPoint(worldPoint, raycastMask);
        if (hit == null) return;

        // attachedRigidbody lets you grab by child colliders too
        Dragable dragable = hit.attachedRigidbody != null
            ? hit.attachedRigidbody.GetComponent<Dragable>()
            : hit.GetComponent<Dragable>();
        if (dragable == null) return;

        holdableObject = dragable;
        holdableObject.Grab(worldPoint);
    }

    private void DropHeld()
    {
        if (holdableObject == null) return;
        holdableObject.Release();
        holdableObject = null;
    }

    private Vector2 PointerWorld()
        => cam.ScreenToWorldPoint(inputActions.MainCharacter.MousePosition.ReadValue<Vector2>());
}