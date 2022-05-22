using System.Linq;
using UnityEngine;

namespace GameJam
{
    public class Player : Entity
    {
        [Header("Player Components")]
        [SerializeField] private MinionStorage minionStorage;

        public static Player localPlayer;

        private void Awake()
        {
            localPlayer = this;
        }

        // death ===============================================
        protected override void OnDeath()
        {
            base.OnDeath();

            Mana.Deplete();

            KillAllSummonedMinions();
        }

        public override void RemoveCorpse()
        {
            // hide player model
            modelRoot.gameObject.SetActive(false);
        }

        private void KillAllSummonedMinions()
        {
            // kill all released minions
            Minion[] releasedMinions = FindObjectsOfType<Minion>().Where(x => x.Owner == this).ToArray();

            foreach (Minion releasedMinion in releasedMinions)
            {
                releasedMinion.Health.Deplete();
            }

            // empty the minion storage just in case
            minionStorage.Empty();
        }
    }
}