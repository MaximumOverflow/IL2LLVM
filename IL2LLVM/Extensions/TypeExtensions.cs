using System.Collections.Concurrent;
using System.Reflection;

namespace IL2LLVM.Extensions;

public static class TypeExtensions
{
	private static readonly ConcurrentDictionary<(Type, Type), bool> _canCast = new();
	private static readonly ConcurrentDictionary<Type, IReadOnlyList<MemberInfo>> _valueMembers = new();

	public static IReadOnlyList<MemberInfo> GetValueMembers(this Type type)
	{
		if (_valueMembers.TryGetValue(type, out var mem)) return mem;
		var members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
		members.RemoveAll(m => m is not FieldInfo /*and not PropertyInfo || m.Name == "Item"*/);
		_valueMembers.TryAdd(type, members);
		return members;
	}

	public static bool IsUnsigned(this Type t)
		=> t == typeof(byte) || t == typeof(ushort) || t == typeof(uint) || t == typeof(ulong) || t == typeof(UIntPtr);
	
	public static bool IsInteger(this Type t)
		=> t == typeof(byte) || t == typeof(ushort) || t == typeof(uint) || t == typeof(ulong) || t == typeof(UIntPtr)
		|| t == typeof(sbyte) || t == typeof(short) || t == typeof(int) || t == typeof(long) || t == typeof(IntPtr) || t == typeof(bool);

	public static bool IsFloatingPoint(this Type t)
		=> t == typeof(float) || t == typeof(double) || t == typeof(Half);

	public static bool IsPointer(this Type t)
		=> t.IsPointer || t == typeof(IntPtr) || t == typeof(UIntPtr);

	public static bool CanBeCastTo(this Type t, Type to)
	{
		if (t.IsAssignableTo(to)) return true;
		if (_canCast.TryGetValue((t, to), out var cast)) return cast;

		if (t.IsByRef)
			cast = t.GetElementType()!.CanBeCastTo(to);

		if (!cast)
		{
			var tInt = t.IsInteger();
			var toInt = to.IsInteger();

			var tPtr = t.IsPointer();
			var toPtr = to.IsPointer();
		
			var tFlo = t.IsFloatingPoint();
			var toFlo = to.IsFloatingPoint();

			cast =  tInt && toInt || tFlo && toFlo || tPtr && toPtr || tInt && toPtr || tPtr && toInt || tFlo && toInt || tInt && toFlo;
		}

		if(!cast) cast = t.GetMethods().Any(m => 
			m.Name is "op_Implicit" or "op_Explicit"
			&& m.GetParameters()[0].ParameterType == t
			&& m.ReturnType == to);
		
		if(!cast) cast = to.GetMethods().Any(m => 
			m.Name is "op_Implicit" or "op_Explicit"
			&& m.GetParameters()[0].ParameterType == to
			&& m.ReturnType == t);
		
		_canCast.TryAdd((t, to), cast);
		return cast;
	}
}