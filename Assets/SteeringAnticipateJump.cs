using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringAnticipateJump : MonoBehaviour, ISteering
{
    // Start is called before the first frame update
    private SteeringCore agent;
    private SteeringVelocityMatch velocityMatch;
    public float lookAheadDistance = 10f;

    public LayerMask jumpDataLayers;

    public Vector3 debugJumpVelocity;
    void Awake()
    {
        agent = GetComponent<SteeringCore>();
        velocityMatch = GetComponent<SteeringVelocityMatch>();
    }

    // Update is called once per frame
    void Update()
    {
        // Look ahead
        RaycastHit info;
        if (Physics.Raycast(transform.position, agent.velocity, out info, lookAheadDistance, jumpDataLayers)) {
            print("I see a jump!");
            // extract jump data

            IHasJumpVelocity jumpData = info.transform.GetComponent<IHasJumpVelocity>();
            if (jumpData != null) {
                Vector3 targetVelocity = jumpData.jumpVelocity;
                debugJumpVelocity = targetVelocity;
                agent.SetLinearAcceleration(velocityMatch.GetSteering(targetVelocity).linearAcceleration);
                return;
            }

            JumpPointWithLandingPad jumpWithLand = info.transform.GetComponent<JumpPointWithLandingPad>();
            if (jumpWithLand != null) {
                Vector3 targetVelocity = jumpWithLand.ComputeRunUpVelocity(agent);
                debugJumpVelocity = targetVelocity;
                agent.SetLinearAcceleration(velocityMatch.GetSteering(targetVelocity).linearAcceleration);
            }
        }
    }

    public SteeringOutput GetSteering() {
        RaycastHit info;
        if (Physics.Raycast(transform.position, agent.velocity, out info, lookAheadDistance, jumpDataLayers)) {
            print("I see a jump!");
            // extract jump data

            IHasJumpVelocity jumpData = info.transform.GetComponent<IHasJumpVelocity>();
            if (jumpData != null) {
                Vector3 targetVelocity = jumpData.jumpVelocity;
                debugJumpVelocity = targetVelocity;
                return velocityMatch.GetSteering(targetVelocity);
            }

            JumpPointWithLandingPad jumpWithLand = info.transform.GetComponent<JumpPointWithLandingPad>();
            if (jumpWithLand != null) {
                Vector3 targetVelocity = jumpWithLand.ComputeRunUpVelocity(agent);
                debugJumpVelocity = targetVelocity;
                return velocityMatch.GetSteering(targetVelocity);
            }

            HoleFiller holeFiller = info.transform.GetComponent<HoleFiller>();
            if (holeFiller != null) {
                // maximum speed at that contact point
                Vector3 direction = info.point - transform.position;

                direction = -info.normal;

                direction.y = 0;
                direction = direction.normalized * agent.maxLinearAcceleration;

                debugJumpVelocity = direction;

                return SteeringOutput.Get(direction);
            }

        }
        return SteeringOutput.None();
    }
}
