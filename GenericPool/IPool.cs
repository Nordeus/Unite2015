namespace Nordeus.DataStructures.Pool
{
	public interface IPool
	{
		object Fetch();
		void Release(object itemToStore);
	}

	public interface IPool<T>
	{
		T Fetch();
		void Release(T itemToStore);
	}
}