using System;

namespace Nordeus.DataStructures
{
#if !UNITY_WEBPLAYER
	using System.Runtime.InteropServices;

	/// <summary>
	/// This struct is used to access the hidden .Net fields of the Array type.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct ArrayHeader
	{
		/// <summary>
		/// Array type.
		/// </summary>
		public UIntPtr type;

		/// <summary>
		/// Array length. 
		/// </summary>
		public UIntPtr length;
	}
#endif

	/// <summary>
	/// Highly specialized, unsafe-code-calling class which can alter the perceived length of the underlying buffer in runtime without
	/// copying the array. See the <c>AsArrayOfLength</c> method docs for more details.
	/// </summary>
	/// <remarks>
	/// Since this class is abstract, you need to use a subclass which implements the functionality of your desired buffer type.
	/// Unfortunately, due to a limitation in C#'s unsafe code implementation, you cannot use arbitrary classes as type arguments, since
	/// the buffer elements have to be of a blittable type, and there's no way to enforce blittability through type parameter constraints.
	/// (And no, "where T: struct" doesn't cut it.)
	/// </remarks>
	/// <remarks>
	/// Note: This class is NOT thread-safe!
	/// example usage:
	///	verts.AsArrayOfLength(verts.size, (buffer) =>
	///	{
	///		mesh.vertices = buffer;
	///	});
	/// </remarks>
	/// <remarks>
	/// </remarks>
	public abstract class VaryingList<T> : BufferedList<T>
	{
		/// <summary>
		/// The action delegate which will be invoked with the list's buffer (with its size modified) as a parameter.
		/// </summary>
		/// <param name="array">The underlying buffer with its size changed for the duration of this delegate's invokation.</param>
		public delegate void ArrayAction(T[] array);

#if !UNITY_WEBPLAYER

		/// <summary>
		/// Changes the perceived length of the underlying integer buffer for the duration of the <paramref name="action"/> invokation. The
		/// perceived buffer size is changed to <paramref name="length"/> and then the <paramref name="action"/> callback is invoked. During
		/// that time, the code in the <paramref name="action"/> callback sees the buffer as having the given <paramref name="length"/>.
		/// When the callback returns, the buffer's perceived size is returned to its normal value.
		/// </summary>
		/// <remarks>Note: This method is NOT thread safe! Do not expect delayed actions to perceive the buffer as having the given length,
		/// since the length will be returned to normal as soon as the action delegate finishes.</remarks>
		/// <param name="length">The length with which the underlying list buffer will be perceived. If it is less than zero, the action is
		/// not performed and this method exits immediately. If it is larger than the actual buffer size, this method acts as if it's the
		/// same as the buffer size.</param>
		/// <param name="action">The action delegate during whose invokation the buffer will have a modified length. If it is null, this
		/// method exits immediately.</param>
		public unsafe void AsArrayOfLength(int length, ArrayAction action)
		{
			if (action == null || length <= 0) return;
			if (length > size) length = size;

			void* pBuffer = GetBufferPointer();

			// Get the header
			ArrayHeader* header = (ArrayHeader*)pBuffer - 1;

			// Change the length
			UIntPtr originalLength = header->length;
			header->length = new UIntPtr((ulong)length);

			// Do stuff with the changed array
			action(buffer);

			// Revert back to old length
			header->length = originalLength;
		}

		/// <summary>
		/// Gets the buffer pointer. This is the only piece of information a subclass needs to provide to this class. The buffer has to
		/// contain only blittable types, and the compiler prevents unsafe code from operating on generic type arguments, so this method is
		/// a workaround for that.
		/// </summary>
		/// <returns>The buffer pointer.</returns>
		public abstract unsafe void* GetBufferPointer();
#endif

		/// <summary>
		/// Set return array with trimed-out free space. For non-web build we use unsafe code to change buffer duration
		/// in array header.
		/// </summary>
		public void TrimBuffer(ArrayAction action)
		{
#if !UNITY_WEBPLAYER
			AsArrayOfLength(size, action);
#else
			T[] bufferCopy = new T[size];
			Array.Copy(buffer, bufferCopy, size);
			action(bufferCopy);
#endif
		}
	}

	/// <summary>
	/// The implementation of <see cref="VaryingList<T>"/> for the integer element type.
	/// </summary>
	public class VaryingIntList : VaryingList<int>
	{
#if !UNITY_WEBPLAYER
		public override unsafe void* GetBufferPointer()
		{
			fixed (void* pBuffer = buffer)
			{
				return pBuffer;
			}
		}
#endif
	}

	/// <summary>
	/// The implementation of <see cref="VaryingList<T>"/> for the <see cref="Vector2"/> element type.
	/// </summary>
	public class VaryingVector2List : VaryingList<UnityEngine.Vector2>
	{
#if !UNITY_WEBPLAYER
		public override unsafe void* GetBufferPointer()
		{
			fixed (void* pBuffer = buffer)
			{
				return pBuffer;
			}
		}
#endif
	}

	/// <summary>
	/// The implementation of <see cref="VaryingList<T>"/> for the <see cref="Vector3"/> element type.
	/// </summary>
	public class VaryingVector3List : VaryingList<UnityEngine.Vector3>
	{
#if !UNITY_WEBPLAYER
		public override unsafe void* GetBufferPointer()
		{
			fixed (void* pBuffer = buffer)
			{
				return pBuffer;
			}
		}
#endif
	}

	/// <summary>
	/// The implementation of <see cref="VaryingList<T>"/> for the <see cref="Vector4"/> element type.
	/// </summary>
	public class VaryingVector4List : VaryingList<UnityEngine.Vector4>
	{
#if !UNITY_WEBPLAYER
		public override unsafe void* GetBufferPointer()
		{
			fixed (void* pBuffer = buffer)
			{
				return pBuffer;
			}
		}
#endif
	}

	/// <summary>
	/// The implementation of <see cref="VaryingList<T>"/> for the <see cref="Color32"/> element type.
	/// </summary>
	public class VaryingColor32List : VaryingList<UnityEngine.Color32>
	{
#if !UNITY_WEBPLAYER
		public override unsafe void* GetBufferPointer()
		{
			fixed (void* pBuffer = buffer)
			{
				return pBuffer;
			}
		}
#endif
	}

	/// <summary>
	/// The implementation of <see cref="VaryingList<T>"/> for the <see cref="Color"/> element type.
	/// </summary>
	public class VaryingColorList : VaryingList<UnityEngine.Color>
	{
#if !UNITY_WEBPLAYER
		public override unsafe void* GetBufferPointer()
		{
			fixed (void* pBuffer = buffer)
			{
				return pBuffer;
			}
		}
#endif
	}
}
