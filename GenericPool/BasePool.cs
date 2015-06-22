using System.Collections.Generic;

namespace Nordeus.DataStructures.Pool
{
	/// <summary>
	/// Base pool of reusable objects. You could borrow and return any type or object to pool (even ones that
	/// are not created by pool). For pool that keeps only element he created use Pool.
	/// </summary>
	public class BasePool<ObjectType> : IPool<ObjectType>, IPool
		where ObjectType : class
	{
		#region Fields

		// All object in pool who are created and not in use will be stored here.
		private Stack<ObjectType> cachedItems;

		// Method that is used for single element instantiation and setup.
		protected System.Func<ObjectType> factoryFunction;

		#endregion

		#region Constructor

		/// <summary>
		/// Create pool and fill it with 'initialCapacity' number of elements. If you fetch element from empty pool, 
		/// new element will be created and returned.
		/// </summary>
		/// <param name="factoryFunction">Function that will instantiate new elements.</param>
		/// <param name="initialCapacity">Initial capacity of Pool.</param>
		public BasePool(System.Func<ObjectType> factoryFunction, int initialCapacity)
		{
			if (factoryFunction == null) { throw new System.ArgumentNullException("Factory function cannot be null"); }
			Init();

			this.factoryFunction = factoryFunction;

			FillWithItems(initialCapacity);
		}

		protected BasePool()
		{
			Init();
		}

		private void Init()
		{
			cachedItems = new Stack<ObjectType>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the count of currently available items.
		/// </summary>
		public int Count
		{
			get { return cachedItems.Count; }
		}

		#endregion Properties

		#region Public API

		/// <summary>
		/// Borrow one element from pool. If pool is empty, create new elements and add it to cache before borrowing.
		/// </summary>
		public virtual ObjectType Fetch()
		{
			ObjectType itemFromPool = null;

			if (cachedItems.Count == 0)
			{
				// Create new item
				itemFromPool = factoryFunction();
			}
			else
			{
				// borrow from already created items
				itemFromPool = cachedItems.Pop();
			}

			return itemFromPool;
		}

		public void Release(object itemToStore)
		{
			Release((ObjectType)itemToStore);
		}

		/// <summary>
		/// Return element to pool.
		/// </summary>
		public virtual void Release(ObjectType itemToStore)
		{
			if (itemToStore == null)
			{
				throw new System.ArgumentNullException("Item to return cannot be null.");
			}

			cachedItems.Push(itemToStore);
		}

		/// <summary>
		/// Add 'count' items to the pool.
		/// </summary>
		public void FillWithItems(int count)
		{
			for (int i = 0; i < count; i++) { cachedItems.Push(factoryFunction()); }
		}

		/// <summary>
		/// Process 'count' cached items of the pool with the specified action.
		/// </summary>
		public void ProcessPoolItems(System.Action<ObjectType> action, int count = int.MaxValue)
		{
			if (cachedItems.Count == 0 || count <= 0) { return; }

			ObjectType item = cachedItems.Pop();
			ProcessPoolItems(action, count - 1);

			try
			{
				action(item);
			}
			catch (System.Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}

			cachedItems.Push(item);
		}


		public void Clear()
		{
			cachedItems.Clear();
		}

		#endregion Public API

		object IPool.Fetch()
		{
			return Fetch();
		}
	}
}