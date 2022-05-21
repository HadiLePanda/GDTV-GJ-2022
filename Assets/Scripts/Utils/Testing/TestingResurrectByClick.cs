using UnityEngine;

namespace GameJam
{
    public class TestingResurrectByClick : MonoBehaviour
    {
        public LayerMask layerCheck;

        private void Update()
        {
            // deal damage as raycast with left click
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1000, layerCheck))
                {
                    if (hit.collider.TryGetComponent(out Corpse corpse))
                    {
                        corpse.Entity.Health.Deplete();

                        Debug.Log("Killed " + hit.transform.name);
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