using System.Collections.Generic;

namespace Nordeus.DataStructures.Pool
{
	/// <summary>
	/// Simple generic <see cref="pool"/> of reusable objects. You could borrow and return any type or object to pool (even ones that
	/// are not created by pool). For pool that keeps only element he created use Pool.
	/// </summary>
	public class SimplePool<ObjectType> : BasePool<ObjectType> where ObjectType : class
	{
		#region Fields
		
		// Event that is called when some element is returned to Pool. Use it to prepare item for next use.
		public event System.Action<ObjectType> OnRelease;
		
		// Event that is called when some element is borrowed from to Pool.
		public event System.Action<ObjectType> OnFetch;
		
		#endregion
		
		#region Constructor
		
		/// <summary>
		/// Create pool and fill it with 'initialCapacity' number of elements. If you fetch element from empty pool, 
		/// new element will be created and returned.
		/// </summary>
		/// <param name="factoryFunction">Function that will instantiate new elements.</param>
		/// <param name="initialCapacity">Initial capacity of Pool.</param>
		/// <param name="releaseHandler">This method will be called every time item is returned to pool. You should use this method to reset element for next use.</param>
		/// <param name="fetchHandler">This method will be called every time item is borrowed from to pool.</param>
		public SimplePool(System.Func<ObjectType> factoryFunction, int initialCapacity, System.Action<ObjectType> releaseHandler = null, System.Action<ObjectType> fetchHandler = null)
			: base(factoryFunction, initialCapacity)
		{
			OnRelease = releaseHandler;
			OnFetch = fetchHandler;
		}

		protected SimplePool()
		{
		}

		#endregion Constructor
		
		#region Public API
		
		/// <summary>
		/// Borrow one element from pool. If pool is empty, create new elements and add it to cache before borrowing.
		/// </summary>
		public override ObjectType Fetch()
		{
			ObjectType itemFromPool = base.Fetch();
			if (OnFetch != null) { OnFetch(itemFromPool); }
			return itemFromPool;
		}
		
		/// <summary>
		/// Return element to pool.
		/// </summary>
		public override void Release(ObjectType itemToStore)
		{
			base.Release(itemToStore);
			if (OnRelease != null) { OnRelease(itemToStore); }
		}

		#endregion Public API
	}
}