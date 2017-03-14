namespace Assets.Scripts.Controller
{
    public struct CharacterState
    {
        public int StateIndex;

        public float X;

        public float Y;

        public CardinalDirection Direction;

        public bool Idle;

        public float JumpTime;

        public float AttackTime;

        public bool Walk;
    }
}