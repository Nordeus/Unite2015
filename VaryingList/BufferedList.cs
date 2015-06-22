using System;
using System.Collections.Generic;
using UnityEngine;

public class BufferedList<T>
{
	private const int MinimumBufferSize = 32;

	public T[] buffer;
	public int size = 0;

	/// <summary>
	/// The array of buffers used for returning results by the <c>ToAppoximateArray</c> method.
	/// </summary>
	private T[][] approximateBuffers;

	/// <summary>
	/// How many levels of approximate buffers to use. Since each level holds a power-of-2-sized buffer, the biggest value approximate
	/// buffers can hold will be 2^(ApproximateBufferLevels - 1).
	/// </summary>
	private const int ApproximateBufferLevels = 17 /* 2^(17 - 1) = 65536 elements at the highest cache level */;

	public IEnumerator<T> GetEnumerator()
	{
		if (buffer != null)
		{
			for (int i = 0; i < size; ++i)
			{
				yield return buffer[i];
			}
		}
	}

	public T this[int i]
	{
		get { return buffer[i]; }
		set { buffer[i] = value; }
	}

	private void Allocate()
	{
		int newSize = MinimumBufferSize;
		if (buffer != null) { newSize = Mathf.Max(buffer.Length << 1, MinimumBufferSize); }
		Array.Resize<T>(ref buffer, newSize);
	}

	private void Trim()
	{
		if (size > 0)
		{
			Array.Resize(ref buffer, size);
		}
		else buffer = null;
	}

	public void Clear()
	{
		if (size > 0) { Array.Clear(buffer, 0, size); }
		size = 0;
	}

	public void Release() { size = 0; buffer = null; }

	public void Add(T item)
	{
		if (buffer == null || size == buffer.Length) Allocate();
		buffer[size++] = item;
	}

	public void Insert(int index, T item)
	{
		if (buffer == null || size == buffer.Length) Allocate();

		if (index < size)
		{
			for (int i = size; i > index; --i) buffer[i] = buffer[i - 1];
			buffer[index] = item;
			++size;
		}
		else Add(item);
	}

	public bool Contains(T item)
	{
		if (buffer == null) return false;
		return IndexOf(item) != -1;
	}
	
	public bool Remove(T item)
	{
		int loc = IndexOf(item);
		if (loc != -1) { RemoveAt(loc); }
		return loc != -1;
	}

	public void RemoveAt(int index)
	{
		if (buffer == null || index < 0 || index >= size) { return; }

		Shift(index, -1);
		Array.Clear(buffer, size, 1);
	}

	public int RemoveAll(Predicate<T> match)
	{
		if (buffer == null || match == null) { return 0; }

		int numberOfRemovedElements = 0;

		for (int i = 0; i < size; i++)
		{
			if (match(buffer[i]))
			{
				// Element should be removed.
				numberOfRemovedElements++;
				continue;
			}

			if (numberOfRemovedElements > 0)
			{
				// Element should not be removed. Reposition it instead.
				buffer[i - numberOfRemovedElements] = buffer[i];
			}
		}

		if (numberOfRemovedElements > 0)
		{
			// Clear last 'numberOfRemovedElements' elements
			Array.Clear(buffer, size - numberOfRemovedElements, numberOfRemovedElements);
			size -= numberOfRemovedElements;
		}

		return numberOfRemovedElements;
	}

	public int IndexOf(T item)
	{
		if (buffer == null) return -1;

		for (int i = 0; i < size; i++)
		{
			if (object.ReferenceEquals(item, buffer[i])) { return i; }
		}

		return -1;
	}

	/// <summary>
	/// Shift buffer data from start location by delta places left or right
	/// </summary>	
	private void Shift(int start, int delta)
	{
		if (delta < 0)
		{
			start -= delta;
		}

		if (start < size)
			Array.Copy(buffer, start, buffer, start + delta, size - start);

		size += delta;

		if (delta < 0)
			Array.Clear(buffer, size, -delta);
	}

	public void Sort(IComparer<T> comparer)
	{
		if (buffer != null)
		{
			Array.Sort<T>(buffer, 0, size, comparer);
		}
	}

	public int Count { get { return size; } }

	/// <summary>
	/// Make sure list could accept numberOfElements elements.
	/// </summary>
	/// <param name="numberOfElements">The number of elements to allocate.</param>
	public void MakeLargerThan(int numberOfElements)
	{
		int minSize = Mathf.Max(size, MinimumBufferSize);
		minSize = Mathf.Max(minSize, numberOfElements);
		if (minSize == 0 || (buffer != null && buffer.Length >= minSize)) { return; }

		int newSize = Mathf.NextPowerOfTwo(minSize);
		Array.Resize<T>(ref buffer, newSize);
	}

	public T Pop()
	{
		if (buffer != null && size != 0)
		{
			T val = buffer[--size];
			buffer[size] = default(T);
			return val;
		}
		return default(T);
	}

	public T[] ToArray()
	{
		return buffer;
	}

	/// <summary>
	/// Returns the list as an array that has approximately the same size as the list. The size of the array is a multiple of 2, so the
	/// exact size of the returned array will be the smallest multiple of 2 which can accomodate the entire list. The remaining elements (if
	/// any) are set to the default value of T.
	/// The purpose of this method is to minimize garbage collection, since the arrays on each "power of 2" level are cached and reused.
	/// The only time allocation occurs is the first time the given power of 2 size array has to be created for the current list contents.
	/// </summary>
	/// <returns>The array containing the list contents.</returns>
	public T[] ToApproximateArray()
	{
		if (size > 0)
		{
			// Determine which level we should put the result into - the result will be fitted into the smallest power-of-two-sized buffer
			// which can accomodate all list elements
			int shiftedSize = size - 1;
			int level = 0;
			do level++; while ((shiftedSize >>= 1) != 0);

			if (level >= ApproximateBufferLevels)
			{
				// Not enough space in the approximate buffers, fall back to the regular ToArray implementation
				return ToArray();
			}

			// Allocate buffers if needed
			if (approximateBuffers == null) approximateBuffers = new T[ApproximateBufferLevels][];
			if (approximateBuffers[level] == null) approximateBuffers[level] = new T[(int)Math.Pow(2, level)];

			// Fill the buffer on the target level
			T[] targetBuffer = approximateBuffers[level];
			Array.Copy(buffer, targetBuffer, size);

			// Clear the remaining elements
			Array.Clear(targetBuffer, size, targetBuffer.Length - size);

			return targetBuffer;
		}

		return null;
	}

	/// <summary>
	/// Warning: bubble sort
	/// </summary>
	public void Sort(System.Comparison<T> comparer)
	{
		bool changed = true;

		while (changed)
		{
			changed = false;

			for (int i = 1; i < size; ++i)
			{
				if (comparer.Invoke(buffer[i - 1], buffer[i]) > 0)
				{
					T temp = buffer[i];
					buffer[i] = buffer[i - 1];
					buffer[i - 1] = temp;
					changed = true;
				}
			}
		}
	}
}