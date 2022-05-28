namespace GameJam
{
    public class MobAliveCorpse : Corpse
    {
        private Entity entity;

        public override Entity GetEntityPrefab() => entity;
        public override int GetEntityLevel() => entity.Level.Current;

        private void Start()
        {
            entity = GetComponentInParent<Entity>();
            entityInstance = entity;
        }

        public override bool CanBeResurrected()
        {
            return GetEntityPrefab() != null && !GetEntityInstance().IsAlive && minionPrefab != null;
        }
    }
}