using System.Collections.Generic;

namespace Nordeus.DataStructures.Pool
{
	/// <summary>
	/// Wrapper around a simple generic pool of reusable objects that puts all fetched objects into a dictionary of active objects.
	/// Provides an indexed Fetch and Release calls which use the underlying dictionary. 
	/// If there is an active object with the requested index, returns object from dictionary instead of fetching a new one 
	/// from the pool.
	/// </summary>
	public class IndexedPool<ObjectType> : BasePool<ObjectType> where ObjectType : class 
	{
		#region Fields
		
		/// <summary>
		/// Dictionary of active objects, used for indexed fetched and release calls.
		/// </summary>
		private Dictionary<int, object> activeObjects = new Dictionary<int, object>();

		// Event that is called when some element is returned to Pool. Use it to prepare item for next use.
		public event System.Action<ObjectType, int> OnRelease;
		
		// Event that is called when some element is borrowed from to Pool.
		public event System.Action<ObjectType, int> OnFetch;

		#endregion

		#region Constructor

		public IndexedPool(System.Func<ObjectType> factoryFunction, int initialCapacity, 
		                  System.Action<ObjectType, int> releaseHandler = null, System.Action<ObjectType, int> fetchHandler = null) :
			base(factoryFunction, initialCapacity)
		{
			OnRelease = releaseHandler;
			OnFetch = fetchHandler;

			if (releaseHandler != null)
			{
				ProcessPoolItems((x) => releaseHandler(x, -1));
			}
		}

		#endregion Constructor

		#region Public API

		/// <summary>
		/// Return the active object with the requested index through the out variable, from the dictionary or by fetching from pool.
		/// Return a bool telling whether this index was active.
		/// </summary>
		public ObjectType Fetch(int index, bool shouldActivate = false)
		{
			object activeObject = null;
			if (!activeObjects.TryGetValue(index, out activeObject))
			{
				activeObject = Fetch();
				shouldActivate = true;
				activeObjects.Add(index, activeObject);
			}

            if (shouldActivate && OnFetch != null) { OnFetch(activeObject as ObjectType, index); }

            return activeObject as ObjectType;
		}

		/// <summary>
		/// Return element to pool.
		/// </summary>
		public void Release(int index)
		{
			object activeObject = null;
			if (activeObjects.TryGetValue(index, out activeObject))
			{
                Release(activeObject as ObjectType);
				if (OnRelease != null) { OnRelease(activeObject as ObjectType, index); }
				activeObjects.Remove(index);
			}
		}

		/// <summary>
		/// Determines whether an object with the specified index is fetched.
		/// </summary>
		public bool IsActive(int index)
		{
			return activeObjects.ContainsKey(index);
		}

		public ICollection<object> ActiveObjects
		{
			get { return activeObjects.Values; }
		}

		#endregion Public API
	}
}