using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleThirdPerson : MonoBehaviour
{
    // Start is called before the first frame update

    public float speed;
    private CharacterController character;
    
    void Awake() {
        character = GetComponent<CharacterController>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move += Vector3.forward;
        if (Input.GetKey(KeyCode.A)) move += Vector3.left;
        if (Input.GetKey(KeyCode.S)) move += Vector3.back;
        if (Input.GetKey(KeyCode.D)) move += Vector3.right;

        move.Normalize();

        character.Move(move * (Time.deltaTime * speed));

    }
}
