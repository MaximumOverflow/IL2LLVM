using IL2LLVM.Collections;
using IL2LLVM.Compilation;
using System.Numerics;

namespace IL2LLVM;

public static unsafe class Program
{
	public static void Main(string[] args)
	{
		using var module = new ILModule("Test");
		// Environment.SetEnvironmentVariable("IL2LLVM_OPTIMIZE", "0");
		var method = (delegate*<void>) module.GetMethodPtr(Function);
		module.Dump();
		using var arr = new NativeArray<int>(100);
		for (var i = 0; i < arr.Length; i++) arr[i] = i;

		method();
	}

	public static void Function()
	{
		var vec = new Vector3();
	}
}