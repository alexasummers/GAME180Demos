using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParrotController : MonoBehaviour
{
    public enum BehaviorType
    {
        StandStill,
        SingleIdleOnly,
        AnyIdleOnly,
        RandomizedIdle
    }
    public BehaviorType behavior;
    private BehaviorType lastBehavior;

    private float switchCounter = 0;

    public float animationSpeed = 0.5f;

    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        lastBehavior = behavior;
    }


    void Start()
    {

    }


    void Update()
    {
        bool startBehavior = lastBehavior != behavior;
        switch (behavior)
        {
            case BehaviorType.StandStill:
                anim.speed = 0;
                break;
            case BehaviorType.SingleIdleOnly:
                anim.speed = animationSpeed;
                if (startBehavior) anim.SetBool("Idle A", true);
                break;
            case BehaviorType.AnyIdleOnly:
                anim.speed = animationSpeed;
                if (startBehavior)
                {
                    anim.SetBool("Idle " + (new string[] { "A", "B", "C" }[Random.Range(0, 3)]).ToString(), true);
                }
                break;
            case BehaviorType.RandomizedIdle:
                anim.speed = animationSpeed;
                switchCounter -= Time.deltaTime;
                if (switchCounter <= 0)
                {
                    switchCounter = Random.Range(3f, 5f);
                    anim.SetBool("Idle " + (new string[] { "B", "C" }[Random.Range(0, 2)]).ToString(), true);
                }
                break;
        }

        lastBehavior = behavior;
    }
}
