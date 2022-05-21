using System.Collections;
using UnityEngine;

namespace GameJam
{
    public class AutoDestroy : MonoBehaviour
	{
		[Header("Settings")]
		[SerializeField] private float timer = 5.0f;

		private Coroutine autoDestroyCoroutine;

		private void Start()
		{
			if (autoDestroyCoroutine == null)
				autoDestroyCoroutine = StartCoroutine(StartAutoDestroy());
		}

		public float GetTimer() => timer;
		public void SetTimer(float time)
		{
			if (autoDestroyCoroutine != null)
				StopCoroutine(autoDestroyCoroutine);

			timer = time;
			autoDestroyCoroutine = StartCoroutine(StartAutoDestroy());
		}

		private IEnumerator StartAutoDestroy()
		{
			yield return new WaitForSeconds(timer);

			Destroy(gameObject);
		}
	}
}