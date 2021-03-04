using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingRoomController : MonoBehaviour
{

    bool isPlayerCreated = false;

    void Start()
    {
        SocketController.Instance.ConnectToSocketServer();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPlayerCreated)
        {
            if (!string.IsNullOrEmpty(SocketController.Instance.networkProvidedId))
            {
                // We're connected; create the player object
                GameObject localPlayer = Instantiate(Resources.Load<GameObject>("NetworkObjects/NetworkPlayer"), transform.position, Quaternion.identity);

                localPlayer.GetComponent<SocketedPlayer>().SetPlayerName(SocketController.Instance.playerName);

                isPlayerCreated = true;
            }
        }
    }
}
