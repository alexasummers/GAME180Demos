using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowAnimationControl : MonoBehaviour
{
    Animator myAnim;

    // Start is called before the first frame update
    void Awake()
    {
        myAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        myAnim.SetBool("Fly", true);
    }
}
