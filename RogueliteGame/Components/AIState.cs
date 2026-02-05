using Microsoft.Xna.Framework;

namespace RogueliteGame.Components
{
    public enum EnemyState
    {
        Wander,
        Chase,
        Attack
    }

    public struct AIState
    {
        public EnemyState State;
        public float AttackCooldown;
        public Vector2 WanderTarget;
        public float WanderTimer;
    }
}