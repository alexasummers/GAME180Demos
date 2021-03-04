using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.WebSocket;
using System;

public class DirectSocketTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Uri target = new Uri("ws://ec2-18-219-253-178.us-east-2.compute.amazonaws.com:3000");

        WebSocket w = new WebSocket(target);

        w.Open();

        w.OnOpen += OnWebSocketOpen;
        w.OnMessage += OnMessage;

        w.OnError += OnError;
    }

    private void OnError(WebSocket w, string e)
    {

    }

    private void OnMessage(WebSocket w, string s)
    {
        //Debug.Log("Message received: [" + s + "]");
        // For now, don't bother with decoding, just pick off the sender
        // and proc the rest of the message.
        string senderId = s[0].ToString();
        string envelope = s.Substring(1);

        Envelope e = JsonUtility.FromJson<Envelope>(envelope);
    }

    private void OnWebSocketOpen(WebSocket w)
    {
        Debug.Log("Socket open!");
        w.Send("Hello world");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


