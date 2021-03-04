using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.WebSocket;
using System;

public class NetSyncAttribute : Attribute { }

public class ArbitrarySyncPoc
{
    [NetSync]
    public string name = "tacos";
    [NetSync]
    private int counter = 17;
    [NetSync]
    protected Transform special;

    public ArbitrarySyncPoc()
    {

    }

    public ArbitrarySyncPoc(Transform inSpecial)
    {
        special = inSpecial;
        name = "flavor";
        counter = 24589;
    }
}

[Serializable]
public class SyncDataSet
{
    public string typeName;
    public string[] names, stringValues;
}


public abstract class NetworkBehaviour : MonoBehaviour
{
    /// <summary>
    /// The unique network object ID for this GameObject, prefixed with the owner ID if it's remotely-owned.
    /// </summary>
    public string networkObjectId { get; private set; }
    public string _networkObjectId = string.Empty;
    public void AssignNetworkObjectId(string nid)
    {
        if (!string.IsNullOrEmpty(networkObjectId))
        {
            throw new InvalidOperationException("Cannot assign a new network object ID to " + name + "; it already has network object ID " + networkObjectId);
        }
        networkObjectId = nid;
    }

    /// <summary>
    /// Reference to the singleton SocketController.
    /// </summary>
    private SocketController socketController;

    public bool isLocal { get; private set; }

    /// <summary>
    /// The path of the prefab in Assets/Resources/NetworkObjects for spawning local instances of remote objects.
    /// </summary>
    public string networkResourcePath;

    /// <summary>
    /// Called automatically when an object is added
    /// </summary>
    private void Spawn()
    {
        networkObjectId = socketController.CreateNetworkObject(this);

        ArbitrarySyncPoc arbos = new ArbitrarySyncPoc(this.transform);

        // Now, serialize by attribute.

        string reflectedTypeName = arbos.GetType().FullName;

        Dictionary<string, NetSerialize> serializers = new Dictionary<string, NetSerialize>();
        Dictionary<string, NetDeserialize> deserializers = new Dictionary<string, NetDeserialize>();

        serializers.Add(typeof(int).FullName, NetSerializeInt);
        serializers.Add(typeof(string).FullName, NetSerializeString);
        serializers.Add(typeof(Transform).FullName, NetSerializeMono);

        deserializers.Add(typeof(int).FullName, NetDeserializeInt);
        deserializers.Add(typeof(string).FullName, NetDeserializeString);
        deserializers.Add(typeof(Transform).FullName, NetDeserializeMono);

        List<string> serialNames = new List<string>();
        List<string> serialValues = new List<string>();

        System.Reflection.FieldInfo[] arboFields = Type.GetType(arbos.GetType().FullName).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        for (int i = 0; i < arboFields.Length; i++)
        {
            serialNames.Add(arboFields[i].Name);
            serialValues.Add(serializers[arboFields[i].FieldType.FullName](arboFields[i].GetValue(arbos)));
        }

        SyncDataSet nss = new SyncDataSet();
        nss.names = serialNames.ToArray();
        nss.stringValues = serialValues.ToArray();
        nss.typeName = reflectedTypeName;

        string portable = JsonUtility.ToJson(nss);





        // Now:
        // Can we turn it back into a new one.
        string received = portable;
        SyncDataSet s = JsonUtility.FromJson<SyncDataSet>(received);

        string flatTypeName = s.typeName;
        Type t = System.Reflection.TypeInfo.GetType(flatTypeName);
        object oc = Activator.CreateInstance(t);


        arboFields = t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        for (int i = 0; i < s.names.Length; i++)
        {
            System.Reflection.FieldInfo field = t.GetField(s.names[i], System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            string fieldTypeName = field.FieldType.FullName;

            field.SetValue(oc, deserializers[fieldTypeName](s.stringValues[i], fieldTypeName));
        }


        OnSpawn();
    }

    string NetSerializeInt(object inty)
    {
        return inty.ToString();
    }

    string NetSerializeString(object stringy)
    {
        return stringy.ToString();
    }

    string NetSerializeMono(object monoy)
    {
        // Find its network ID. We could have gotten anything.
        // We expect it'll either be a GameObject, or a Component,
        // so we can call GetComponent on either valid cast and get a network ID.
        if (monoy == null) return string.Empty;

        GameObject gm = monoy as GameObject;
        Component cm = monoy as Component;

        if (gm == null && cm == null)
        {
            Debug.LogWarning("Cannot sync a reference type that doesn't have components.");
        }

        NetworkBehaviour nm = null;
        if (gm != null) nm = gm.GetComponent<NetworkBehaviour>();
        if (cm != null) nm = cm.GetComponent<NetworkBehaviour>();

        if (nm == null)
        {
            Debug.LogWarning("Cannot sync a component/gameobject that doesn't have a NetworkBehaviour.");
            return string.Empty;
        }

        return nm.networkObjectId;
    }

    object NetDeserializeInt(string inty, string t)
    {
        return int.Parse(inty);
    }

    object NetDeserializeString(string stringy, string t)
    {
        return stringy;
    }

    object NetDeserializeMono(string monoy, string t)
    {
        NetworkBehaviour owner = socketController.GetNetworkBehaviour(monoy);

        if (t == typeof(GameObject).FullName)
        {
            return owner.gameObject;
        }

        string[] nameParts = t.Split('.');
        return owner.GetComponent(nameParts[nameParts.Length - 1]);

    }

    delegate string NetSerialize(object o);
    delegate object NetDeserialize(string s, string t);

    protected virtual void OnSpawn() { }

    private bool onStartCalled = false;
    protected void OnStart()
    {
        socketController = SocketController.Instance;
        onStartCalled = true;
        // If we don't have a networkObjectID, then we're local,
        // and we should self-create with Spawn.
        if (string.IsNullOrEmpty(networkObjectId))
        {
            isLocal = true;
            Spawn();
        }
        else
        {
            isLocal = false;
        }
    }

    // This will be used as the default Start behavior.
    // Anything overriding its own Start method will need to call OnStart
    // in a "just-have-to-know" way.
    // A warning will be called if this object is network-updated without OnStart having been called.
    private void Start()
    {
        OnStart();
    }

    protected void Update()
    {
        // for inspector
        _networkObjectId = networkObjectId;
        OnUpdate();
    }

    protected abstract void UpdateLocal();

    private void OnUpdate()
    {
        if (isLocal)
        {
            UpdateOriginalInstance();
        }
        UpdateLocal();
    }

    protected abstract void UpdateOriginalInstance();

    public abstract void OnReceiveMessage(string messageType, string message);

    public void BroadcastMessage(NetworkMessage message)
    {
        if (!isLocal) return;
        if (!onStartCalled) throw new InvalidOperationException("Cannot broadcast, object has not been correctly started. This is usually caused by defining a custom Start method that fails to call NetworkBehaviour.OnStart.");
        Envelope e = new Envelope(networkObjectId, message);
        socketController.SendEnvelope(e);
    }

}

public class SocketController : MonoBehaviour
{
    public string networkProvidedId = string.Empty;
    public string playerName { get; set; }

    private static SocketController _instance;
    public static SocketController Instance { get { return _instance; } }
    private static int netIdCounter = 0;

    public string socketHost = "ec2-18-219-253-178.us-east-2.compute.amazonaws.com";
    public int socketPort = 3000;

    private Uri _socketUri;

    private WebSocket _webSocket;

    private Dictionary<string, NetworkBehaviour> networkObjects = new Dictionary<string, NetworkBehaviour>();

    public NetworkBehaviour GetNetworkBehaviour(string networkId)
    {
        return networkObjects[networkId];
    }

    void Awake()
    {
        if (_instance != null)
        {
            this.enabled = false;
            Debug.LogWarning("Multiple SocketController objects in scene. First is on " + _instance.gameObject.name + "; another is on " + gameObject.name + ", and has been disabled.");
            return;
        }

        DontDestroyOnLoad(this.gameObject);

        _instance = this;
    }

    public void ConnectToSocketServer()
    {
        _socketUri = new Uri(string.Format("ws://{0}:{1}", socketHost, socketPort));
        _webSocket = new WebSocket(_socketUri);

        _webSocket.Open();

        _webSocket.OnOpen += OnSocketOpen;
        _webSocket.OnMessage += OnSocketMessage;
        _webSocket.OnError += OnSocketError;
    }

    // Update is called once per frame
    void Update()
    {
        if (!string.IsNullOrEmpty(networkProvidedId) )
        {
            // how many can we push per frame? Make it 1.

            if (envelopeQueue.Count > 0)
            {
                Envelope e = envelopeQueue.Dequeue();

                string jsonMessage = JsonUtility.ToJson(e);

//                Debug.Log("Sending [" + jsonMessage + "]");
                _webSocket.Send(jsonMessage);
            }
        }
    }

    public string CreateNetworkObject(NetworkBehaviour networkObject)
    {
        if (!this.enabled) throw new InvalidOperationException("Can't create a network object on an invalid SocketController.");
        string networkObjectId = this.networkProvidedId + (netIdCounter++).ToString("0000");
        SendEnvelope(ObjectCreateMessage.GetEnvelope(networkObjectId, networkObject.networkResourcePath ));
        networkObjects.Add(networkObjectId, networkObject);

        return networkObjectId;
    }

    public void SendEnvelope(Envelope e)
    {
        if (!this.enabled) return;

        QueueEnvelope(e);
    }

    private Queue<Envelope> envelopeQueue = new Queue<Envelope>();
    private void QueueEnvelope(Envelope e)
    {
        envelopeQueue.Enqueue(e);
    }

    private void OnSocketOpen(WebSocket w)
    {
        if (!this.enabled) return;

        string envPath = ".env";

#if UNITY_EDITOR
        envPath = "Assets\\" + envPath;
#endif

        string[] envLines = System.IO.File.ReadAllLines(envPath); // maybe?
        Dictionary<string, string> envVars = new Dictionary<string, string>();
        for (int i = 0; i < envLines.Length; i++)
        {
            string k = envLines[i].Substring(0, envLines[i].IndexOf('=')),
                   v = envLines[i].Substring(envLines[i].IndexOf('=') + 1);
            envVars.Add(k, v);
        }

        string masterKey = envVars.ContainsKey("MASTER_KEY") ? envVars["MASTER_KEY"] : string.Empty;

        // Send the master command
        Envelope masterCommand = new Envelope("9999", "MasterKey", masterKey);
        w.Send(JsonUtility.ToJson(masterCommand));
    }

    private void OnSocketMessage(WebSocket w, string message)
    {
        if (!this.enabled) return;
        // messages will normally be encoded with an owning character.

        //Debug.Log("Received socket message:\n" + message);

        if (message[0] == '_')
        {
            // extract ID and eject
            networkProvidedId = message.Substring(1);
            return;
        }

        string senderId = message[0].ToString();    // needed?
        string envelope = message.Substring(1);

        Envelope e = JsonUtility.FromJson<Envelope>(envelope);

        string senderNid = e.nid;
        // senderNid and first character of e.nid should match.
        if (networkObjects.ContainsKey(senderNid))
        {
            if (e.mt == "ObjectDestroyMessage")
            {
                // Simple enough, yeah?
                Destroy(networkObjects[senderNid].gameObject);
                networkObjects.Remove(senderNid);
                print("Remote object " + senderNid + " destroyed");
            }
            else
            {
                print("Received message of type " + e.mt + " for " + senderNid);
                networkObjects[senderNid].OnReceiveMessage(e.mt, e.m);
            }
        }
        else
        {
            if (e.mt == "ObjectCreateMessage")
            {
                ProcessObjectCreateMessage(senderNid, e.m);
            }

            if (e.mt == "MultiMessage")
            {
                // haHA! Get 'em all!
                MultiMessage mm = JsonUtility.FromJson<MultiMessage>(e.m);

                Dictionary<string, string> messagesByType = mm.GetMessages();

                // Find the create message first, if it's there
                if (messagesByType.ContainsKey("ObjectCreateMessage"))
                {
                    ProcessObjectCreateMessage(senderNid, JsonUtility.FromJson<Envelope>(messagesByType["ObjectCreateMessage"]).m);
                    messagesByType.Remove("ObjectCreateMessage");
                }

                foreach (string mt in messagesByType.Keys)
                {
                    networkObjects[senderNid].OnReceiveMessage(mt, JsonUtility.FromJson<Envelope>(messagesByType[mt]).m );
                }
            }
        }

    }

    private void ProcessObjectCreateMessage(string senderNid, string message)
    {
        ObjectCreateMessage ocm = JsonUtility.FromJson<ObjectCreateMessage>(message);
        string typeToCreate = ocm.typeName;
        // This is now a resource path.

        GameObject newNetworkObject = Instantiate(Resources.Load<GameObject>("NetworkObjects/" + typeToCreate));

        NetworkBehaviour newSocketedObject = newNetworkObject.GetComponent<NetworkBehaviour>();
        newSocketedObject.AssignNetworkObjectId(senderNid);
        networkObjects.Add(senderNid, newSocketedObject);
    }

    private void OnSocketError(WebSocket w, string errorMessage)
    {
        if (!this.enabled) return;
        Debug.LogError(errorMessage);
    }

}

[Serializable]
public class Envelope
{
    public string nid;   // a network object ID, the owner of the message
    public string mt = string.Empty;
    public string m = string.Empty;

    public Envelope(string nid, NetworkMessage msg)
    {
        this.nid = nid;
        mt = msg.GetType().ToString();
        m = JsonUtility.ToJson(msg);
    }

    public Envelope(string nid, string messageType, NetworkMessage msg)
    {
        this.nid = nid;
        mt = messageType;
        m = JsonUtility.ToJson(msg);
    }

    public Envelope(string nid, string messageType, string message)
    {
        this.nid = nid;
        mt = messageType;
        m = message;
    }
}

public class NetworkMessage { }

[Serializable]
public class ObjectCreateMessage : NetworkMessage
{
    public string typeName;

    public ObjectCreateMessage(string tn)
    {
        typeName = tn;
    }

    public static Envelope GetEnvelope(string nid, string tn)
    {
        ObjectCreateMessage o = new ObjectCreateMessage(tn);
        return new Envelope(nid, o);
    }
}

[Serializable]
public class MultiMessage : NetworkMessage
{
    public string[] messageTypes;
    public string[] messageStrings;

    private Dictionary<string, string> messagesByType;

    public Dictionary<string, string> GetMessages()
    {
        messagesByType = new Dictionary<string, string>();
        for (int i = 0; i < messageTypes.Length; i++)
        {
            messagesByType.Add(messageTypes[i], messageStrings[i]);
        }
        return messagesByType;
    }

}
