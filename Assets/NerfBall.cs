using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NerfBall : MonoBehaviour
{
    public float speed = 20f;
    // Start is called before the first frame update
    void Start()
    {
    }

    void FixedUpdate() 
    {
        GetComponent<Rigidbody>().velocity = transform.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x < -200 || transform.position.x > 200 || transform.position.z < -200 || transform.position.z > 200) Destroy(this.gameObject);
    }

    void OnCollisionEnter(Collision collision) {
        Destroy(this.gameObject);
    }
}
