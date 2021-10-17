using System.Runtime.InteropServices;

namespace IL2LLVM.Collections;

public readonly unsafe struct NativeArray<T> : IDisposable where T : unmanaged
{
	public readonly T* Pointer;
	public readonly int Length;

	public NativeArray(int length)
	{
		Length = length;
		Pointer = (T*) NativeMemory.Alloc((nuint) (sizeof(T) * length));
	}

	public ref T this[int i] 
		=> ref Pointer[i];

	public void Dispose() 
		=> NativeMemory.Free(Pointer);
}