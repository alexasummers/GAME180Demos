using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class KinematicCore : MonoBehaviour
{
    // The kinematic core is what behaviors modify.
    // Everywhere knows position,
    // but we store velocity and rotation here.
    // We'll also put up a utility for orientation.


    // Kinematics have a maximum speed and rotation.
    public float maxSpeed = 10.0f;
    public float maxRotation = 720f;    // 720 degrees / s = 1 full rotation in 0.5s. Maybe a little fast, we'll see.
    public bool yVelocityEnabled = false;   // Most things walk


    // A quick-and-dirty pattern to make secure fields readable in the inspector.
    [Tooltip("Last designated velocity, updated each frame. Changing this will not change the agent's velocity.")]
    public Vector3 debugVelocity = Vector3.zero;    // m/s
    [Tooltip("Last designated rotation, updated each frame. Changing this will not change the agent's rotation.")]
    public float debugRotation = 0; // degrees/s

    public bool showDebugGizmos = false;
    public float gizmosScale = 4.0f;

    public Vector3 velocity { get; private set; }   // m/s
    public float rotation { get; private set; } // degrees/s

    private CharacterController myCharacterController;

    #region Unity Events

    void Start()
    {
        myCharacterController = GetComponent<CharacterController>();
    }

    void FixedUpdate()
    {
        myCharacterController.Move(velocity * Time.fixedDeltaTime);
        transform.Rotate(Vector3.up, rotation * Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        debugVelocity = velocity;
        debugRotation = rotation;
    }

    #endregion

    /// <summary>
    /// Sets the kinematic velocity based on a direction and a scale of this kinematic's maximum speed.
    /// </summary>
    /// <param name="newVelocity">The new direction of movement. If yVelocityEnabled is false, the y component will be zeroed out before normalization.</param>
    /// <param name="scale">The magnitude of the new velocity vector as a scale of maximum speed. scale = 1 will produce maximum speed. Clamped to [0,1].</param>
    public void SetVelocity(Vector3 newVelocity, float scale)
    {
        // Clamp scale to a value between 0.0 and 1.0. This prevents erroneous input from creating confusing output.
        scale = Mathf.Clamp(scale, 0f, 1f);

        // Automatically cancel any y component in the velocity if the agent cannot move in y (i.e. if they can't fly).
        // This allows us to seek to a location at a different height without having to do extra calculations.
        if (!yVelocityEnabled) newVelocity.y = 0;

        // Normalize the new velocity so that its magnitude is 1.0.
        newVelocity.Normalize();

        // Now scale it by our clamped scale factor.
        velocity = newVelocity * scale * maxSpeed;

        // We don't need to move the agent right now; that will happen during the Update event, based on this computed velocity value.
    }

    /// <summary>
    /// Set rotation based on -100% to +100% of capacity.
    /// </summary>
    /// <param name="parameterizedRotation">Clamped to [-1, 1]. At -1, agent turns full angular speed left; at +1, agent turns full angular speed right.</param>
    public void SetRotation(float parameterizedRotation)
    {
        float scale = Mathf.Clamp(parameterizedRotation, -1f, 1f);
        rotation = maxRotation * scale;
    }

    private void OnDrawGizmos()
    {
        if (showDebugGizmos)
        {
            // show the current velocity and scale;
            // show orientation also

            Color oc = Gizmos.color;

            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, transform.position + velocity * gizmosScale / maxSpeed);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * gizmosScale);


            Gizmos.color = oc;

        }
    }

}
