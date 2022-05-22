using Controller2k;
using UnityEngine;

namespace GameJam
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Entity entity;
        [SerializeField] private CharacterController2k controller;

        [Header("Settings")]
        [SerializeField] private float speed = 6f;

        private void Update()
        {
            if (!entity.IsAlive) { return; }

            Move();
        }

        private void Move()
        {
            float xMovement = Input.GetAxis("Horizontal");
            float zMovement = Input.GetAxis("Vertical");

            float yMovement = -9.81f * Time.deltaTime;

            Vector3 direction = new Vector3(xMovement, yMovement, zMovement).normalized;

            Vector3 desiredMovement = direction * speed * Time.deltaTime;

            controller.Move(desiredMovement);
        }

    }
}