using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Controller
{
    public class PlayerController : NetworkBehaviour
    {
        readonly float easing = 0.1f;

        readonly float spacing = 1.0f;

        private Animator animator;

        [SyncVar] Color color;

        Queue<KeyCode> pendingMoves;
        CharacterState predictedState;
        public float RunSpeed = 0.8f;

        [SyncVar(hook = "OnServerStateChanged")] CharacterState serverState;
        public float WalkSpeed = 0.3f;

        public void Start()
        {
            if (isLocalPlayer)
            {
                pendingMoves = new Queue<KeyCode>();
                UpdatePredictedState();
            }
            SyncState(true);
            SyncColor();
        }

        public void Awake()
        {
            InitState();
        }

        [Server]
        private void InitState()
        {
            Color[] colors = {Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.yellow};
            color = colors[Random.Range(0, colors.Length)];
            serverState = new CharacterState
            {
                StateIndex = 0,
                X = transform.position.x,
                Y = transform.position.y
            };
        }

        public void Update()
        {
            if (isLocalPlayer)
            {
                HandleInputs();
            }
            SyncState(false);
        }

        private void UpdatePredictedState()
        {
            predictedState = serverState;
            foreach (var arrowKey in pendingMoves)
            {
                predictedState = Move(predictedState, arrowKey, predictedState.Walk);
            }
        }

        private void HandleInputs()
        {
            KeyCode[] arrowKeys = {KeyCode.Z, KeyCode.Q, KeyCode.S, KeyCode.D};
            var walk = Input.GetKey(KeyCode.LeftShift);
            foreach (var arrowKey in arrowKeys)
            {
                if (!Input.GetKey(arrowKey)) continue;

                pendingMoves.Enqueue(arrowKey);
                UpdatePredictedState();
                CmdMoveOnServer(arrowKey, walk);
            }
        }

        [Command]
        public void CmdMoveOnServer(KeyCode arrowKey, bool walk)
        {
            serverState = Move(serverState, arrowKey, walk);
        }

        private CharacterState Move(CharacterState previous, KeyCode arrowKey, bool walk)
        {
            var horizontalDisplacement = 0f;
            var verticalDisplacement = 0f;
            var direction = CardinalDirection.South;
            var idle = true;
            switch (arrowKey)
            {
                case KeyCode.Z:
                    verticalDisplacement = 1;
                    direction = CardinalDirection.North;
                    idle = false;
                    break;
                case KeyCode.S:
                    verticalDisplacement = -1;
                    direction = CardinalDirection.South;
                    idle = false;
                    break;
                case KeyCode.Q:
                    horizontalDisplacement = -1;
                    direction = CardinalDirection.West;
                    idle = false;
                    break;
                case KeyCode.D:
                    horizontalDisplacement = 1;
                    direction = CardinalDirection.East;
                    idle = false;
                    break;
            }

            var speed = walk ? WalkSpeed : RunSpeed;

            return new CharacterState
            {
                StateIndex = 1 + previous.StateIndex,
                X = previous.X + horizontalDisplacement * speed * Time.deltaTime,
                Y = previous.Y + verticalDisplacement * speed * Time.deltaTime,
                Direction = direction,
                Idle = idle,
                Walk = walk
            };
        }

        public void OnServerStateChanged(CharacterState newState)
        {
            serverState = newState;
            if (pendingMoves != null)
            {
                while (pendingMoves.Count > predictedState.StateIndex - serverState.StateIndex)
                {
                    pendingMoves.Dequeue();
                }
                UpdatePredictedState();
            }
        }

        private void SyncColor()
        {
            GetComponent<Renderer>().material.color = (isLocalPlayer ? Color.white : Color.grey) * color;
        }

        void SyncState(bool init)
        {
            var stateToRender = isLocalPlayer ? predictedState : serverState;
            var target = spacing * (stateToRender.X * Vector3.right + stateToRender.Y * Vector3.up);
            transform.position = init ? target : Vector3.Lerp(transform.position, target, easing);

            if (animator == null)
                animator = GetComponent<Animator>();

            if (animator != null)
            {
                animator.SetInteger("Direction", (int) stateToRender.Direction);
                animator.SetBool("Idle", stateToRender.Idle);
            }
        }
    }
}