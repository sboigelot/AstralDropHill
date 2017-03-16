namespace Assets.Scripts.Controller
{
    public struct CharacterState
    {
        public int StateIndex;
        
        public bool Idle;

        public bool Jump;

        public bool Attack;

        public bool Walk;

        public float DestinationX;

        public float DestinationY;
    }
}