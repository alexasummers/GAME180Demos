using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpAtTrigger : MonoBehaviour
{
    SteeringCore agent;
    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<SteeringCore>();
    }

    void OnTriggerEnter(Collider other) {

        if (other.tag != "JumpPoint") return;

        print("Trying to jump");
        agent.RequestJump();
    }
}
