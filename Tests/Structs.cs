using IL2LLVM.Compilation;
using System.Numerics;
using NUnit.Framework;
using IL2LLVM;

namespace Tests;

public class Structs
{
	private ILModule _ilModule;
	
	[SetUp]
	public void Setup()
	{
		_ilModule = new ILModule(nameof(Structs));
	}

	[Test]
	public void StructDeclarationAndAssignment()
	{
		static void Fn()
		{
			var a = new Vector3();
			var b = new Vector3(0);
			var c = new Vector3(0, 0, 0);
		}

		_ilModule.GetMethodPtr(Fn);
	}
	
	[Test]
	public unsafe void StructOutArg()
	{
		static void Fn(out Vector3 result) 
			=> result = new Vector3(42);

		var method = (delegate*<out Vector3, void>) _ilModule.GetMethodPtr(Fn);
		method(out var res); Assert.AreEqual(res, new Vector3(42));
	}
	
	[Test]
	public void StructOperators()
	{
		static void Fn()
		{
			var a = new Vector3(42);
			a += a; a -= a; a *= a; a /= a;
		}

		_ilModule.GetMethodPtr(Fn);
	}
}