using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISteering
{
    SteeringOutput GetSteering();
}

public class SteeringOutput
{
    public Vector3 linearAcceleration = Vector3.zero;
    public float angularAcceleration = 0f;

    public static SteeringOutput Get(Vector3 v)
    {
        return new SteeringOutput() { linearAcceleration = v };
    }

    public static SteeringOutput Get(float f)
    {
        return new SteeringOutput() { angularAcceleration = f };
    }

    public static SteeringOutput Get(Vector3 v, float f)
    {
        return new SteeringOutput() { linearAcceleration = v, angularAcceleration = f };
    }

    public static SteeringOutput None()
    {
        return new SteeringOutput();
    }

    public static SteeringOutput operator +(SteeringOutput left, SteeringOutput right)
    {
        return new SteeringOutput() { linearAcceleration = left.linearAcceleration + right.linearAcceleration, angularAcceleration = left.angularAcceleration + right.angularAcceleration };
    }

    public static SteeringOutput operator /(SteeringOutput s, float f)
    {
        return new SteeringOutput() { linearAcceleration = s.linearAcceleration / f, angularAcceleration = s.angularAcceleration / f };
    }

    public static SteeringOutput operator *(SteeringOutput s, float f)
    {
        return new SteeringOutput() { linearAcceleration = s.linearAcceleration * f, angularAcceleration = s.angularAcceleration * f };
    }

}

[RequireComponent(typeof(CharacterController))]
public class SteeringCore : MonoBehaviour
{
    // The steering core is what behaviors modify.
    // Everywhere knows position,
    // but we store velocity and rotation here.


    // Kinematics have a maximum speed and rotation.
    public float maxSpeed = 10.0f;
    public float maxRotation = 720f;    // 720 degrees / s = 1 full rotation in 0.5s. Maybe a little fast, we'll see.

    // Steering includes acceleration, linear and angular.
    public float maxLinearAcceleration = 16f;      // m/s/s
    public float maxAngularAcceleration = 360f;    // degrees/s/s

    public bool yVelocityEnabled = false;   // Most things walk


    // A quick-and-dirty pattern to make secure fields readable in the inspector.
    [Tooltip("Last designated velocity, updated each frame. Changing this will not change the agent's velocity.")]
    public Vector3 debugVelocity = Vector3.zero;    // m/s
    [Tooltip("Last designated rotation, updated each frame. Changing this will not change the agent's rotation.")]
    public float debugRotation = 0; // degrees/s

    [Tooltip("Last designated linear acceleration, updated each frame. Changing this will not change the agent's linear acceleration.")]
    public Vector3 debugLinearAccel = Vector3.zero; // Vector3 of m/s/s
    [Tooltip("Last designated angular acceleration, updated each frame. Changing this will not change the agent's angular acceleration.")]
    public float debugAngularAccel = 0; // deg/s/s

    public float debugSpeed = 0f;

    public bool showDebugGizmos = false;
    public float gizmosScale = 4.0f;

    public Vector3 velocity { get; private set; }   // m/s
    public float rotation { get; private set; } // degrees/s

    public Vector3 linearAcceleration { get; private set; }
    public float angularAcceleration { get; private set; }

    public float collisionRadius = 1f;
    public float selfRadius = 0.5f;

    private CharacterController myCharacterController;

    public float groundCheckDistance = 1.01f;
    public float lateralGroundCheckDistance = 0.6f;

    public float jumpPower = 8;

    public LayerMask walkableLayers;

    #region Unity Events

    void Start()
    {
        myCharacterController = GetComponent<CharacterController>();
    }

    private bool debugStopRequested = false;
    public void DebugStop() {
        debugStopRequested = true;
    }

    void FixedUpdate()
    {
        if (debugStopRequested) {
            velocity = Vector3.zero;
            debugStopRequested = false;
            return;
        }

        bool isGrounded = IsGrounded();
        if (isGrounded) {
            velocity += linearAcceleration * Time.fixedDeltaTime;
        }

        rotation += angularAcceleration * Time.fixedDeltaTime;

//        if (!yVelocityEnabled) velocity = new Vector3(velocity.x, 0, velocity.z);
        // strip out the Y in case we're jumping or falling
        Vector3 verticalVelocity = new Vector3(0, velocity.y, 0);
        Vector3 lateralVelocity = new Vector3(velocity.x, 0, velocity.z);

        lateralVelocity = lateralVelocity.normalized * Mathf.Clamp(lateralVelocity.magnitude, -maxSpeed, maxSpeed);

        velocity = lateralVelocity + verticalVelocity;

        //velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude, -maxSpeed, maxSpeed);
        rotation = Mathf.Clamp(rotation, -maxRotation, maxRotation);

        if (!IsGrounded()) {
            print("aerial");
            velocity += new Vector3(0, -9.8f, 0) * Time.fixedDeltaTime;
        } else {
            if (velocity.y < 0) {
                velocity = new Vector3(velocity.x, 0, velocity.z);
            }
        }

        myCharacterController.Move(velocity * Time.fixedDeltaTime);
        transform.Rotate(Vector3.up, rotation * Time.fixedDeltaTime);
    }

    bool IsGrounded() {
        RaycastHit info;
        if (Physics.Raycast(transform.position, -Vector3.up, out info, groundCheckDistance, walkableLayers)) {
            return true;
        } else {

            // mm hang on let's double check?
            if (myCharacterController.isGrounded) return true;

            return false;
        }
    }

private bool isJumpRequested = false;
    public void RequestJump() {
        isJumpRequested = true;
    }

    void Update() {
        if (isJumpRequested && IsGrounded()) {
            velocity += Vector3.up * jumpPower;
        }
        isJumpRequested = false;
    }

    private void LateUpdate()
    {
        debugVelocity = velocity;
        debugRotation = rotation;
        debugLinearAccel = linearAcceleration;
        debugSpeed = velocity.magnitude;
    }

    #endregion

    public void SetPositionMatch(Vector3 targetPosition)
    {
        // set target velocity to "toward the position."
        SetVelocityMatch(targetPosition - transform.position, 1);
    }

    public void SetVelocityMatch(Vector3 direction, float scale)
    {
        // what's the target velocity, and what's my current velocity?
        if (!yVelocityEnabled) direction.y = 0;

        Vector3 normalizedVelocity = velocity / maxSpeed;

        Vector3 targetVelocity = direction.normalized * Mathf.Clamp(scale, 0, 1);

        Vector3 accelerationDirection = targetVelocity - normalizedVelocity;
        accelerationDirection.Normalize();

        SetLinearAcceleration(accelerationDirection * maxLinearAcceleration);
    }

    public void SetVelocityMatch(Vector3 targetVelocity)
    {
        if (!yVelocityEnabled) targetVelocity.y = 0;

//        Vector3 normalizedVelocity = velocity / maxSpeed;

        Vector3 accelerationDirection = targetVelocity - velocity;
        accelerationDirection.Normalize();

        SetLinearAcceleration(accelerationDirection * maxLinearAcceleration);
    }

    public void SetLinearAcceleration(Vector3 desiredAcceleration)
    {
        if (!yVelocityEnabled) desiredAcceleration.y = 0;

        // clamp magnitude to max.
        float accelMagnitude = desiredAcceleration.magnitude;
        linearAcceleration = desiredAcceleration.normalized * Mathf.Clamp(accelMagnitude, -maxLinearAcceleration, maxLinearAcceleration);
    }

    public void SetMaxLinearAcceleration(Vector3 direction)
    {
        if (!yVelocityEnabled) direction.y = 0;
        linearAcceleration = direction.normalized * maxLinearAcceleration;
    }

    public SteeringOutput GetMaxAcceleration(Vector3 direction) {
        if (direction == Vector3.zero) return SteeringOutput.None();
        return SteeringOutput.Get(direction.normalized * maxLinearAcceleration);
    }

    public void SetAngularAcceleration(float desiredAcceleration)
    {
        // should this be a quaternion?
        angularAcceleration = Mathf.Clamp(desiredAcceleration, -maxAngularAcceleration, maxAngularAcceleration);
    }

    public void SetMaxAngularAcceleration(float direction)
    {
        angularAcceleration = direction < 0 ? -maxAngularAcceleration : maxAngularAcceleration;
        if (direction == 0) angularAcceleration = 0;
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

            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + linearAcceleration * gizmosScale / maxLinearAcceleration);

            Gizmos.color = oc;

        }
    }

}
