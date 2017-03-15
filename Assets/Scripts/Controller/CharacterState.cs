namespace Assets.Scripts.Controller
{
    public struct CharacterState
    {
        public int StateIndex;
        
        public bool Idle;

        public float JumpTime;

        public float AttackTime;

        public bool Walk;
        public float DestinationX;
        public float DestinationY;
    }
}