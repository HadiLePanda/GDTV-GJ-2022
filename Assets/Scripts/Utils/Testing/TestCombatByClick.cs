using UnityEngine;

namespace GameJam
{
    public class TestCombatByClick : MonoBehaviour
    {
        public LayerMask damageableLayers;
        public int damage = 10;

        private void Update()
        {
            Player player =  Player.localPlayer;

            // deal damage as raycast with left click
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1000, damageableLayers))
                {
                    if (hit.collider.TryGetComponent(out Corpse corpse))
                    {
                        player.Combat.DealDamage(corpse.GetEntityPrefab(), damage, hit.point, hit.normal);
                    }
                }
            }
            // resurrect with right click
            else if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1000, damageableLayers))
                {
                    if (hit.collider.TryGetComponent(out Corpse corpse) &&
                        corpse.CanBeResurrected())
                    {
                        corpse.ResurrectAsMinion(Player.localPlayer);
                    }
                }
            }

            // skills testing
            //if (Input.GetKeyDown(KeyCode.Alpha1))
            //{
            //    player.Skills.CmdUse(0);
            //}
            //if (Input.GetKeyDown(KeyCode.Alpha2))
            //{
            //    player.Skills.CmdUse(1);
            //}
            //if (Input.GetKeyDown(KeyCode.Alpha3))
            //{
            //    player.Skills.CmdUse(2);
            //}
            //if (Input.GetKeyDown(KeyCode.Alpha4))
            //{
            //    player.Skills.TryUse(3);
            //}
            //if (Input.GetKeyDown(KeyCode.Alpha5))
            //{
            //    player.Skills.TryUse(4);
            //}
        }
    }
}