﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Controller
{
    public class PlayerController : NetworkBehaviour
    {
        private Animator animator;

        [SyncVar] Color color;

        Queue<KeyCode> pendingMoves;
        CharacterState predictedState;

        private Rigidbody2D rigidbody2d;
        public float RunSpeed = 0.8f;

        [SyncVar(hook = "OnServerStateChanged")] CharacterState serverState;
        public float WalkSpeed = 0.3f;

        public void Start()
        {
            rigidbody2d = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();

            if (isLocalPlayer)
            {
                pendingMoves = new Queue<KeyCode>();
                UpdatePredictedState();

                var cameraController = Camera.main.GetComponent<CameraController>();
                cameraController.Target = gameObject;
            }
            SyncState();
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
                StateIndex = 0
            };
        }

        public void Update()
        {
            if (isLocalPlayer)
            {
                HandleInputs();
            }
            SyncState();
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
            KeyCode[] arrowKeys = {KeyCode.Z, KeyCode.Q, KeyCode.S, KeyCode.D, KeyCode.Space};
            var walk = Input.GetKey(KeyCode.LeftShift);
            pendingMoves.Enqueue(KeyCode.F1);
            CmdMoveOnServer(KeyCode.F1, walk);
            foreach (var arrowKey in arrowKeys)
            {
                if (!Input.GetKey(arrowKey)) continue;

                pendingMoves.Enqueue(arrowKey);
                CmdMoveOnServer(arrowKey, walk);
            }
            UpdatePredictedState();
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
            var idle = true;
            switch (arrowKey)
            {
                case KeyCode.Z:
                    verticalDisplacement = 1;
                    idle = false;
                    break;
                case KeyCode.S:
                    verticalDisplacement = -1;
                    idle = false;
                    break;
                case KeyCode.Q:
                    horizontalDisplacement = -1;
                    idle = false;
                    break;
                case KeyCode.D:
                    horizontalDisplacement = 1;
                    idle = false;
                    break;
                case KeyCode.F1:
                    idle = true;
                    break;
            }

            var speed = walk ? WalkSpeed : RunSpeed;

            return new CharacterState
            {
                //StateIndex = arrowKey == KeyCode.F1 ? previous.StateIndex : 1 + previous.StateIndex,
                StateIndex = 1 + previous.StateIndex,
                //DestinationX = previous.DestinationX + horizontalDisplacement * speed * Time.deltaTime,
                //DestinationY = previous.DestinationY + verticalDisplacement * speed * Time.deltaTime,
                DestinationX = transform.position.x + horizontalDisplacement * speed * Time.deltaTime,
                DestinationY = transform.position.y + verticalDisplacement * speed * Time.deltaTime,
                Idle = idle,
                Walk = walk,
                Attack = arrowKey == KeyCode.Space
            };
        }

        public void OnServerStateChanged(CharacterState newState)
        {
            serverState = newState;
            if (pendingMoves != null)
            {
                while (pendingMoves.Count > 0 &&
                       pendingMoves.Count > predictedState.StateIndex - serverState.StateIndex)
                {
                    pendingMoves.Dequeue();
                }
                UpdatePredictedState();
            }
        }

        private void SyncColor()
        {
            //GetComponent<Renderer>().material.color = (isLocalPlayer ? Color.white : Color.grey) * color;
        }

        void SyncState()
        {
            var stateToRender = isLocalPlayer
                //||
                //                serverState.StateIndex == predictedState.StateIndex 
                ? predictedState
                : serverState;

            animator.SetBool("IsMoving", !stateToRender.Idle && !stateToRender.Attack);
            animator.SetBool("IsAttacking", stateToRender.Attack);

            if (stateToRender.Idle)
            {
                //rigidbody2d.velocity = Vector2.zero;
            }
            else
            {
                animator.SetFloat("LastMoveX", animator.GetFloat("MoveX"));
                animator.SetFloat("LastMoveY", animator.GetFloat("MoveY"));
            }

            //Vector2 direction = new Vector2(
            //    stateToRender.DestinationX - transform.position.x,
            //    stateToRender.DestinationY - transform.position.y).normalized;
            //animator.SetFloat("MoveX", direction.x);
            //animator.SetFloat("MoveY", direction.y);
            //rigidbody2d.MovePosition(new Vector2(stateToRender.DestinationX, stateToRender.DestinationY));

            var targetPos = new Vector2(stateToRender.DestinationX, stateToRender.DestinationY);
            var distance = Vector2.Distance(targetPos, transform.position);
            //Debug.Log(distance);
            //if (distance > 1)
            //{
            //    Debug.Log("Should teleport: distance: " + distance);
            //    //transform.position = targetPos;
            //}

            var velocity = new Vector2(
                stateToRender.DestinationX - transform.position.x,
                stateToRender.DestinationY - transform.position.y);
            animator.SetFloat("MoveX", velocity.x);
            animator.SetFloat("MoveY", velocity.y);
            rigidbody2d.velocity = velocity;


            //Vector2 distance = new Vector2(
            //    stateToRender.DestinationX - transform.position.x,
            //    stateToRender.DestinationY - transform.position.y);
            //animator.SetFloat("MoveX", distance.normalized.x);
            //animator.SetFloat("MoveY", distance.normalized.y);
            //var destination = new Vector2(stateToRender.DestinationX, stateToRender.DestinationY);

            //if (distance.magnitude > RunSpeed * 10) //reconcile with server version
            //{
            //    transform.position = destination;
            //}
            //else
            //{
            //rigidbody2d.MovePosition(velocity);
            //}
        }
    }
}