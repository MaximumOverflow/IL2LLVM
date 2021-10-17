using IL2LLVM.Compilation;
using System.Numerics;
using NUnit.Framework;
using IL2LLVM;

namespace Tests;

public class Primitives
{
	private ILModule _ilModule;
	
	[SetUp]
	public void Setup()
	{
		_ilModule = new ILModule(nameof(Structs));
	}
	
	[Test]
	public void DoNothing()
	{
		static void Fn() {}
		
		_ilModule.GetMethodPtr(Fn);
	}

	[Test]
	public void PrimitiveDeclarationAndAssignment()
	{
		static void Fn()
		{
			byte a = 0;
			sbyte b = 0;
			short c = 0;
			ushort d = 0;
			int e = 0;
			uint f = 0;
			long g = 0;
			ulong h = 0;
			float i = 0.0f;
			double j = 0.0;
		}

		_ilModule.GetMethodPtr(Fn);
	}
	
	[Test]
	public void PrimitiveSum()
	{
		static void Fn()
		{
			byte a = 0;			a += a;
			sbyte b = 0;		b += b;
			short c = 0;		c += c;
			ushort d = 0;		d += d;
			int e = 0;			e += e;
			uint f = 0;			f += f;
			long g = 0;			g += g;
			ulong h = 0;		h += h;
			float i = 0.0f;		i += i;
			double j = 0.0;		j += j;
		}
		
		_ilModule.GetMethodPtr(Fn);
	}
	
	[Test]
	public void PrimitiveSub()
	{
		static void Fn()
		{
			byte a = 0;			a -= a;
			sbyte b = 0;		b -= b;
			short c = 0;		c -= c;
			ushort d = 0;		d -= d;
			int e = 0;			e -= e;
			uint f = 0;			f -= f;
			long g = 0;			g -= g;
			ulong h = 0;		h -= h;
			float i = 0.0f;		i -= i;
			double j = 0.0;		j -= j;
		}
		
		_ilModule.GetMethodPtr(Fn);
	}
	
	[Test]
	public void PrimitiveMul()
	{
		static void Fn()
		{
			byte a = 0;			a *= a;
			sbyte b = 0;		b *= b;
			short c = 0;		c *= c;
			ushort d = 0;		d *= d;
			int e = 0;			e *= e;
			uint f = 0;			f *= f;
			long g = 0;			g *= g;
			ulong h = 0;		h *= h;
			float i = 0.0f;		i *= i;
			double j = 0.0;		j *= j;
		}
		
		_ilModule.GetMethodPtr(Fn);
	}
	
	[Test]
	public void PrimitiveDiv()
	{
		static void Fn()
		{
			byte a = 0;			a /= a;
			sbyte b = 0;		b /= b;
			short c = 0;		c /= c;
			ushort d = 0;		d /= d;
			int e = 0;			e /= e;
			uint f = 0;			f /= f;
			long g = 0;			g /= g;
			ulong h = 0;		h /= h;
			float i = 0.0f;		i /= i;
			double j = 0.0;		j /= j;
		}
		
		_ilModule.GetMethodPtr(Fn);
	}
}