using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactTiming : MonoBehaviour
{
    int contactCount = 0;
    int contactFrameCount = 0;
    int totalFrameCount = 0;

    public bool Done {get; set;}

    public float ContactRate {
        get {
            if (totalFrameCount == 0) return 0;
            return (float)contactFrameCount / (float)totalFrameCount;
        }
    }

    public int ContactFrames {
        get { return contactFrameCount; }
    }

    public int TotalFrames {
        get {return totalFrameCount; }
    }

    public void Reset() {
        contactCount = 0;
        contactFrameCount = 0;
        totalFrameCount = 0;
        Done = false;
    }

    void Start()
    {
        Reset();
    }

    void FixedUpdate()
    {
        if (contactCount < 0) 
        {
            Debug.LogWarning("Negative contacts");
            contactCount = 0;
        }

        if (contactCount > 0) {
            // addddd it up
            contactFrameCount++;
        }
        totalFrameCount++;

    }

    void OnTriggerEnter(Collider other) {

        if (other.gameObject.layer == LayerMask.NameToLayer("Target")) {
            Done = true;
        } else {
            contactCount++;
        }

    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.layer != LayerMask.NameToLayer("Target")) {
            contactCount--;
        }
    }
}
