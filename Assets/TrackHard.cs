using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackHard : MonoBehaviour
{

    public Transform trackTarget;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = trackTarget.position;
        
    }
}
