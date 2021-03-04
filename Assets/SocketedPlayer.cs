using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SocketedPlayer : NetworkBehaviour
{
    public float moveSpeed = 5; // m/s

    public int networkWindowMs = 100;   // 10 updates/s to start.
    private float networkRefreshTimer = 0;

    private CharacterController characterController;

    private DateTime lastUpdate = DateTime.Now;
    private DateTime previousUpdate = DateTime.MinValue;

    private Vector3 fromPosition, toPosition;
    private Quaternion fromRotation, toRotation;
    private float interpolationSeconds = 0;

    public string playerName = string.Empty;

    public UnityEngine.UI.Text txtName;

    protected override void OnSpawn()
    {
        base.OnSpawn();
        BroadcastMessage(new PlayerNameMessage() { playerName = this.playerName });
    }

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
        BroadcastMessage(new PlayerNameMessage() { playerName = this.playerName });
    }

    public override void OnReceiveMessage(string messageType, string message)
    {
        switch (messageType)
        {
            case "PlayerStateMessage":
                PlayerStateMessage psm = JsonUtility.FromJson<PlayerStateMessage>(message);

                if (previousUpdate == DateTime.MinValue)
                {
                    previousUpdate = DateTime.Now;
                    lastUpdate = DateTime.Now;
                    // Really we want to make these the lerp targets
                    // for frame-behind sync
                    transform.position = psm.position;
                    transform.rotation = psm.rotation;
                }
                else
                {
                    fromPosition = toPosition;
                    fromRotation = toRotation;

                    toPosition = psm.position;
                    toRotation = psm.rotation;

                    previousUpdate = lastUpdate;
                    lastUpdate = DateTime.Now;
                    interpolationSeconds = 0;
                }

                break;
            case "PlayerNameMessage":
                this.playerName = JsonUtility.FromJson<PlayerNameMessage>(message).playerName;
                break;
        }
    }


    void Start()
    {
        OnStart();

        fromPosition = toPosition = transform.position;
        fromRotation = toRotation = transform.rotation;

        networkRefreshTimer = networkWindowMs;
        characterController = GetComponent<CharacterController>();
        if (!isLocal)
        {
            Destroy(transform.Find("Camera").gameObject);
        }
        else
        {
            transform.Find("Camera").gameObject.SetActive(true);
        }
    }


    protected override void UpdateOriginalInstance()
    {
        networkRefreshTimer -= Time.deltaTime * 1000;
        if (networkRefreshTimer < 0)
        {
            networkRefreshTimer = networkWindowMs;
            BroadcastMessage(new PlayerStateMessage() { position = transform.position, rotation = transform.rotation });
        }

        // hook up wasd and mouse
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.A)) moveDirection += -transform.right;
        if (Input.GetKey(KeyCode.S)) moveDirection += -transform.forward;
        if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;

        characterController.SimpleMove(moveDirection * moveSpeed);
    }

    protected override void UpdateLocal()
    {
        if (!isLocal)
        {
            // lerpy
            interpolationSeconds += Time.deltaTime;
            float t = (float)(interpolationSeconds / (lastUpdate - previousUpdate).TotalSeconds);

            transform.position = Vector3.Lerp(fromPosition, toPosition, t);
            transform.rotation = Quaternion.Lerp(fromRotation, toRotation, t);

        }

        if (txtName.text == null || txtName.text.Length == 0) 
        {
            if (playerName != null && playerName.Length > 0) txtName.text = playerName;
        }
    }
}

[Serializable]
public class PlayerStateMessage : NetworkMessage
{
    public Vector3 position;
    public Quaternion rotation;
}

[Serializable]
public class PlayerNameMessage : NetworkMessage
{
    public string playerName;
}
