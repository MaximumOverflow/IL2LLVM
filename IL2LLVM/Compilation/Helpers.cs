using System.Reflection;
using IL2LLVM.Extensions;

namespace IL2LLVM.Compilation;

public static class CompilationHelper
{
	private static readonly object Lock = new();
	private static readonly Dictionary<int, Type> TypeTokens = new();
	private static readonly Dictionary<int, FieldInfo> FieldTokens = new();
	private static readonly Dictionary<int, MethodBase> MethodTokens = new();
	private static readonly Dictionary<FieldInfo, uint> FieldIndices = new();
	private static readonly HashSet<Module> Modules = new(Assembly.GetExecutingAssembly().GetModules());

	public static Type ResolveType(MethodBase caller, int token)
	{
		lock (Lock)
		{
			if (TypeTokens.TryGetValue(token, out var type))
				return type;

			foreach (var module in Modules)
			{
				try { type = module.ResolveType(token); break; }
				catch (ArgumentException) {}
			}

			if (type is null)
			{
				var declType = caller.DeclaringType!;
				if (declType.IsGenericType)
				{
					try { return caller.Module.ResolveType(token, declType.GetGenericArguments(), null); }
					catch (ArgumentException) {}
				}
				throw new MissingMethodException($"Cannot type method 0x{token:X} from {caller.DeclaringType}::{caller}.");
			}
		
			TypeTokens.Add(token, type);
			return type;
		}
	}
	
	public static MethodBase ResolveMethod(MethodBase caller, int token)
	{
		lock (Lock)
		{
			if (MethodTokens.TryGetValue(token, out var method))
				return method;

			try { method = caller.Module.ResolveMethod(token); }
			catch (ArgumentException) {}

			if (method is null)
			{
				foreach (var module in Modules)
				{
					try { method = module.ResolveMethod(token); break; }
					catch (ArgumentException) {}
				}
			}
			
			if (method is null)
			{
				var type = caller.DeclaringType!;
				if (type.IsGenericType)
				{
					try { return caller.Module.ResolveMethod(token, type.GetGenericArguments(), null)!; }
					catch (ArgumentException) {}
				}
				throw new MissingMethodException($"Cannot load method 0x{token:X} from {caller.DeclaringType}::{caller}.");
			}

			Modules.Add(caller.Module);
			MethodTokens.Add(token, method);
			return method;
		}
	}

	public static FieldInfo ResolveField(MethodBase caller, int token)
	{
		lock (Lock)
		{
			if (FieldTokens.TryGetValue(token, out var field))
				return field;
			
			try { field = caller.Module.ResolveField(token); }
			catch (ArgumentException) {}

			if (field is null)
			{
				foreach (var module in Modules)
				{
					try { field = module.ResolveField(token); break; }
					catch (ArgumentException) {}
				}
			}

			if (field is null)
			{
				var type = caller.DeclaringType!;
				if (type.IsGenericType)
				{
					try { return caller.Module.ResolveField(token, type.GetGenericArguments(), null)!; }
					catch (ArgumentException) {}
				}
				throw new MissingMethodException($"Cannot load field 0x{token:X} from {caller.DeclaringType}::{caller}.");
			}

			Modules.Add(caller.Module);
			FieldTokens.Add(token, field);
			return field;
		}
	}

	public static uint ResolveFieldIndex(MethodBase caller, int token)
		=> ResolveFieldIndex(ResolveField(caller, token));
	
	public static uint ResolveFieldIndex(FieldInfo field)
	{
		lock (Lock)
		{
			if (FieldIndices.TryGetValue(field, out var index))
				return index;

			var type = field.DeclaringType!;
			var members = type.GetValueMembers();
			index = (uint) members.IndexOf(field);
			FieldIndices.Add(field, index);
			return index;
		}
	}
}