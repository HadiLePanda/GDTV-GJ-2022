using UnityEngine;

namespace GameJam
{
    public class TestCombatByClick : MonoBehaviour
    {
        public LayerMask layerCheck;
        public int damage = 10;

        private void Update()
        {
            Player player =  Player.localPlayer;

            // deal damage as raycast with left click
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1000, layerCheck))
                {
                    if (hit.collider.TryGetComponent(out Corpse corpse))
                    {
                        player.Combat.DealDamage(corpse.Entity, damage, Vector3.zero, Vector3.forward);
                    }
                }
            }
            // resurrect with right click
            else if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1000, layerCheck))
                {
                    if (hit.collider.TryGetComponent(out Corpse corpse) &&
                        corpse.CanBeResurrected())
                    {
                        corpse.ResurrectAsMinion(Player.localPlayer);

                        Debug.Log("Resurrecting " + hit.transform.name);
                    }
                }
            }
        }
    }
}