namespace RogueliteGame.Components
{
    public struct Projectile
    {
        public float Lifetime;      // Time before bullet despawns (seconds)
        public int Damage;          // How much damage it deals
        public uint OwnerID;        // Who shot it (to avoid self-damage)
    }
}