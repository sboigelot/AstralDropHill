using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public float WalkSpeed;
    public float RunSpeed;

    [SyncVar]
    public Position2 Position;

    void Awake()
    {
        InitState();
    } 

    [Server]
    private void InitState()
    {
        Position = new Position2
        {
            x = transform.position.x,
            y = transform.position.y
        };
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            HandleInputs();
        }
        SyncState();
    }

    void HandleInputs()
    {
        KeyCode[] arrowKeys = { KeyCode.Z, KeyCode.Q, KeyCode.S, KeyCode.D };
        bool walk = Input.GetKey(KeyCode.LeftShift);
        foreach (KeyCode arrowKey in arrowKeys)
        {
            if (!Input.GetKey(arrowKey)) continue;
            
            // ask real move on server.
            CmdMoveOnServer(arrowKey, walk);
        }
    }
    
    [Command]
    void CmdMoveOnServer(KeyCode arrowKey, bool walk)
    {
        Position = Move(Position, arrowKey, walk);
    }

    Position2 Move(Position2 previous, KeyCode arrowKey, bool walk)
    {
        float horizontalDisplacement = 0f;
        float verticalDisplacement = 0f;
        switch (arrowKey)
        {
            case KeyCode.Z:
                verticalDisplacement = 1;
                break;
            case KeyCode.S:
                verticalDisplacement = -1;
                break;
            case KeyCode.Q:
                horizontalDisplacement = -1;
                break;
            case KeyCode.D:
                horizontalDisplacement = 1;
                break;
        }

        var speed = walk ? WalkSpeed : RunSpeed;

        return new Position2
        {
            x = previous.x + horizontalDisplacement * speed * Time.deltaTime,
            y = previous.y + verticalDisplacement * speed * Time.deltaTime
        };
    }

    void SyncState()
    {
        transform.position = new Vector2(Position.x, Position.y);
    }
}

public struct Position2
{
    public float x;
    public float y;
}

