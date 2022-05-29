using UnityEngine;

namespace GameJam
{
    public class SpinOverTime : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float degreesPerSecond = 20f;

        private void Update()
        {
            transform.RotateAround(transform.position, Vector3.up, degreesPerSecond * Time.deltaTime);
        }

    }
}