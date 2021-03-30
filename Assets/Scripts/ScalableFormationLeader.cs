using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalableFormationLeader : MonoBehaviour
{

    public enum ScalableFormation {
        DefensiveCircle,
        Phalanx,
        BattleSquare
    }

    public SteeringCore[] memberPool;
    public int sizeOfFormation = 9;
    public ScalableFormation formationType;
    public SlotBehavior slotBehavior;

    private int lastSizeOfFormation;
    private ScalableFormation lastFormationType;

    private Vector3[] formationPositions;
    private Vector3[] formationOrientations;

    public bool showDebugGizmos = false;

    // Start is called before the first frame update
    void Awake()
    {
        lastSizeOfFormation = 0;
        lastFormationType = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // efficiency pattern: Recompute only as necessary
        sizeOfFormation = Mathf.Clamp(sizeOfFormation, 1, memberPool.Length + 1);
        if (sizeOfFormation != lastSizeOfFormation) {
            lastSizeOfFormation = sizeOfFormation;
            formationPositions = new Vector3[sizeOfFormation - 1];
            formationOrientations = new Vector3[sizeOfFormation - 1];
        }

        switch (formationType) {
            case ScalableFormation.DefensiveCircle:
                RecomputeDefensiveCircle();
                break;
            case ScalableFormation.Phalanx:
                RecomputePhalanx();
                break;
            case ScalableFormation.BattleSquare:
                RecomputeBattleSquare();
                break;
        }

        if (slotBehavior == SlotBehavior.Direct) {
            for (int i = 0; i < sizeOfFormation - 1; i++) {
                memberPool[i].GetComponent<SteeringArrive>().enabled = false;
                memberPool[i].GetComponent<SteeringAlign>().enabled = false;
                memberPool[i].transform.position = formationPositions[i];
                memberPool[i].transform.forward = formationOrientations[i];
            }
        }

        if (slotBehavior == SlotBehavior.Steering) {
            for (int i = 0; i < sizeOfFormation - 1; i++) {
                memberPool[i].GetComponent<SteeringArrive>().enabled = true;
                memberPool[i].GetComponent<SteeringArrive>().targetTransform = null;
                memberPool[i].GetComponent<SteeringAlign>().enabled = true;
                memberPool[i].GetComponent<SteeringAlign>().alignTarget = null;
                memberPool[i].GetComponent<SteeringArrive>().targetPosition = formationPositions[i];
                memberPool[i].GetComponent<SteeringAlign>().alignDirection = formationOrientations[i];
            }
        }
    }

    void RecomputeDefensiveCircle() {
        // radius is based on size of circle. How far apart do we want
        // people to be? Call it 2.5 units.
        float circumference = 2.5f * sizeOfFormation;
        float radius = circumference / 2 / Mathf.PI;

        float degreeGap = 360f / sizeOfFormation;
        Quaternion rotator = Quaternion.AngleAxis(degreeGap, Vector3.up);
        Vector3 circleCenter = transform.position - transform.forward * radius;
        Vector3 centerToEdge = transform.position - circleCenter;

        for (int i = 1; i < sizeOfFormation; i++) {
            centerToEdge = rotator * centerToEdge;
            formationPositions[i-1] = circleCenter + centerToEdge;
            formationOrientations[i-1] = centerToEdge;
        }
    }

    void RecomputePhalanx() {
        throw new System.NotImplementedException("Phalanx not yet implemented");
    }

    void RecomputeBattleSquare() {
        throw new System.NotImplementedException("Battle Square not yet implemented");
    }

    void OnDrawGizmos() {
        if (!showDebugGizmos || formationPositions == null) return;

        Color c = Gizmos.color;
        Gizmos.color = new Color(1f, 0, 0, 0.5f);

        for (int i = 0; i < formationPositions.Length; i++) {
            Gizmos.DrawSphere(formationPositions[i], 0.5f);
            Gizmos.DrawLine(formationPositions[i], formationPositions[i] + formationOrientations[i].normalized);
        }



        Gizmos.color = c;
    }
}
