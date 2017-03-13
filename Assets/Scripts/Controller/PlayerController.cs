using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    public float WalkSpeed;
    public float RunSpeed;

    [SyncVar] public Position2 Position;

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

    private Animator animator;
    
    public enum Direction
    {
        South = 0,
        West = 1,
        North = 2,
        East = 3
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
        KeyCode[] arrowKeys = {KeyCode.Z, KeyCode.Q, KeyCode.S, KeyCode.D};
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
        var direction = Direction.South;
        switch (arrowKey)
        {
            case KeyCode.Z:
                verticalDisplacement = 1;
                direction = Direction.North;
                break;
            case KeyCode.S:
                verticalDisplacement = -1;
                direction = Direction.South;
                break;
            case KeyCode.Q:
                horizontalDisplacement = -1;
                direction = Direction.West;
                break;
            case KeyCode.D:
                horizontalDisplacement = 1;
                direction = Direction.East;
                break;
        }

        var speed = walk ? WalkSpeed : RunSpeed;

        return new Position2
        {
            x = previous.x + horizontalDisplacement * speed * Time.deltaTime,
            y = previous.y + verticalDisplacement * speed * Time.deltaTime,
            direction = direction
        };
    }

    void SyncState()
    {
        transform.position = new Vector2(Position.x, Position.y);

        if (animator == null)
            animator = this.GetComponent<Animator>();

        if (animator != null)
            animator.SetInteger("Direction", (int)Position.direction);
    }
}

public struct Position2
{
    public float x;
    public float y;
    public PlayerController.Direction direction;
}

