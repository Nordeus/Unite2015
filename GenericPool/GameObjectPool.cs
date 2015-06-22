using System;
using UnityEngine;

namespace Nordeus.DataStructures.Pool
{
	public class GameObjectPool : TrackingPool<GameObject>
	{
		private GameObject prefab;

		public GameObjectPool(GameObject prefab, int initialCapacity, Func<GameObject> factoryFunction)
			: base()
		{
			this.prefab = prefab;

			if (factoryFunction != null) { this.factoryFunction = factoryFunction; }
			else { this.factoryFunction = InstantiatePrefab; }

			OnRelease += ReleaseHandler;
			OnFetch += FetchHandler;

			FillWithItems(initialCapacity);
		}

		private GameObject InstantiatePrefab()
		{
			GameObject newGO = GameObject.Instantiate(prefab) as GameObject;
			newGO.SetActive(false);

			return newGO;
		}

		private static void FetchHandler(GameObject gameObject)
		{
			gameObject.SetActive(true);
		}

		private static void ReleaseHandler(GameObject gameObject)
		{
			gameObject.SetActive(false);
		}
	}
}
