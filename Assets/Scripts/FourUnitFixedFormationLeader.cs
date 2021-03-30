using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public enum SlotBehavior {
        Direct,
        Steering
    }

public class FourUnitFixedFormationLeader : MonoBehaviour
{
    public enum FormationType {
        BattleLine,
        DefensiveCircle,
        FingerFour,
        TwoAbreastInCover
    }


    public SteeringCore[] followers;    // should be three exactly
    public FormationType formationType;

    public SlotBehavior slotBehavior;

    public bool showDebugGizmos = false;
    Vector3[] followerPositions = new Vector3[3];
    Vector3[] followerOrientations = new Vector3[3];

    void Start()
    {
        
    }

    void Update()
    {
        // configure the target alignment and position
        // for each sub-position based on position.


        switch (formationType) 
        {
            case FormationType.BattleLine:
                for (int i = 0; i < 3; i++) followerOrientations[i] = transform.forward;
                followerPositions[0] = transform.position - transform.right * 1.65f;
                followerPositions[1] = transform.position + transform.right * 1.65f;
                followerPositions[2] = transform.position + transform.right * 3;
                break;  
            case FormationType.DefensiveCircle:
                followerPositions[0] = transform.position - transform.right * 2 - transform.forward * 1.5f;
                followerPositions[1] = transform.position + transform.right * 2 - transform.forward * 1.5f;
                followerPositions[2] = transform.position - transform.forward * 3.5f;

                followerOrientations[0] = -transform.right;
                followerOrientations[1] = transform.right;
                followerOrientations[2] = -transform.forward;

                break;  
            case FormationType.FingerFour:
                followerPositions[0] = transform.position - transform.right - transform.forward * 0.5f;
                followerOrientations[0] = -transform.right;

                followerPositions[1] = transform.position + transform.right - transform.forward * 0.5f;
                followerOrientations[1] = transform.forward + transform.right;

                followerPositions[2] = transform.position + transform.right * 2 - transform.forward * 1.5f;
                followerOrientations[2] = -transform.forward;
                break;  
            case FormationType.TwoAbreastInCover:
                followerPositions[0] = transform.position + transform.right * 1.5f;
                followerOrientations[0] = transform.forward;
                
                followerPositions[1] = transform.position - transform.forward * 1.5f;
                followerOrientations[1] = -transform.forward - transform.right;

                followerPositions[2] = transform.position - transform.forward * 1.5f + transform.right * 1.5f;
                followerOrientations[2] = -transform.forward + transform.right;
                break;  
        }

        switch (slotBehavior) {
            case SlotBehavior.Steering:
                for (int i = 0; i < 3; i++) {
                    if (followers.Length > i) {
                        followers[i].GetComponent<SteeringArrive>().enabled = true;
                        followers[i].GetComponent<SteeringAlign>().enabled = true;
                        followers[i].GetComponent<SteeringArrive>().targetPosition = followerPositions[i];
                        followers[i].GetComponent<SteeringAlign>().alignDirection = followerOrientations[i];
                    }
                }
                break;
            case SlotBehavior.Direct:
                for (int i = 0; i < 3; i++) {
                    if (followers.Length > i) {
                        followers[i].GetComponent<SteeringArrive>().enabled = false;
                        followers[i].GetComponent<SteeringAlign>().enabled = false;
                        followers[i].transform.position = followerPositions[i];
                        followers[i].transform.forward = followerOrientations[i];
                    }
                }
                break;

        }

    }

    void OnDrawGizmos() {
        if (!showDebugGizmos || followerPositions == null) return;

        Color c = Gizmos.color;

        Gizmos.color = new Color(1f, 0, 0, 0.5f);

        // show position and orientation of each slot
        for (int i = 0; i < followerPositions.Length; i++) {
            Gizmos.DrawSphere(followerPositions[i], 0.5f);
            Gizmos.DrawLine(followerPositions[i], followerPositions[i] + followerOrientations[i].normalized);
        }

        Gizmos.color = c;
    }
}
