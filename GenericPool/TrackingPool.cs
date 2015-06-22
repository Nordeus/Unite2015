using System.Collections.Generic;
using UnityEngine;

namespace Nordeus.DataStructures.Pool
{
	/// <summary>
	/// Simple generic pool of reusable objects that keep track of object it created.
	/// </summary>
	public class TrackingPool<ObjectType> : SimplePool<ObjectType> where ObjectType : class
	{
		#region Fields

		// All borrowed items.
		private HashSet<ObjectType> activeItems;

		#endregion

		#region Constructor

		/// <summary>
		/// Create pool and fill it with 'initialCapacity' number of elements. When pool is emptied, one by one element is added to pool.
		/// </summary>
		/// <param name="factoryFunction">Function that will instantiate new elements.</param>
		/// <param name="initialCapacity">Initial capacity of Pool.</param>
		/// <param name="releaseHandler">This method will be called every time item is returned to pool. You should use this method to reset element for next use.</param>
		/// <param name="fetchHandler">This method will be called every time item is borrowed from to pool.</param>
		public TrackingPool(System.Func<ObjectType> factoryFunction, int initialCapacity, System.Action<ObjectType> releaseHandler = null, System.Action<ObjectType> fetchHandler = null)
			: base(factoryFunction, initialCapacity, releaseHandler, fetchHandler)
		{
			Init();
		}

		protected TrackingPool()
		{
			Init();
		}

		private void Init()
		{
			activeItems = new HashSet<ObjectType>();
		}

		#endregion

		#region Public API

		/// <summary>
		/// Borrow one element from pool. If pool is empty, create new elements and add it to cache before borrowing.
		/// </summary>
		public override ObjectType Fetch()
		{
			var itemFromPool = base.Fetch();
			activeItems.Add(itemFromPool);
			return itemFromPool;
		}

		/// <summary>
		/// Return element to pool.
		/// </summary>
		public override void Release(ObjectType itemToStore)
		{
			if (!activeItems.Contains(itemToStore))
			{
				if(itemToStore == null && !ReferenceEquals(itemToStore, null))Debug.Log("Skoro null!");
				throw new System.ArgumentException("Returned object not created by pool");
			}

			activeItems.Remove(itemToStore);
			base.Release(itemToStore);
		}

		#endregion
	}
}