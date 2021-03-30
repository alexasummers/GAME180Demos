using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkMenuUIControl : MonoBehaviour
{

    public InputField txtServerUrl, txtPlayerName;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void btnConnect_OnClick()
    {
        SocketController.Instance.socketHost = txtServerUrl.text;
        SocketController.Instance.playerName = txtPlayerName.text;

        UnityEngine.SceneManagement.SceneManager.LoadScene("waitingRoom");

    }
}
