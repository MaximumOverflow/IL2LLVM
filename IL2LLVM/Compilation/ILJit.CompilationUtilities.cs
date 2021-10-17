using IL2LLVM.Extensions;
using System.Reflection;
using LLVMSharp.Interop;

namespace IL2LLVM.Compilation;

public static partial class ILJit
{
	private static string CreateTypeName(Type type)
	{
		#if DEBUG
		return $"{type}_0x{type.MetadataToken:X}";
		#else
		return $"ILType_0x{type.MetadataToken:X}";
		#endif
	}
	
	private static string CreateMethodName(MethodBase method)
	{
		#if DEBUG
		return $"{method.DeclaringType!}.{method.Name}_0x{method.MetadataToken:X}";
		#else
		return $"ILMethod_0x{method.MetadataToken:X}";
		#endif
	}
	
	private static void CreateMethodSignature(ILModule module, MethodBase method, 
		out ILType ret, out ILType[] par, out LLVMTypeRef signature)
	{
		if (method is MethodInfo mInfo)
		{
			ret = GetType(module, mInfo.ReturnType);
			
			if (method.IsStatic)
			{
				var p = mInfo.GetParameters();
				par = new ILType[p.Length];
				par.Populate(i => GetType(module, p[i].ParameterType));
			}
			else
			{
				var p = mInfo.GetParameters();
				par = new ILType[p.Length + 1];
				par[0] = GetType(module, method.DeclaringType!.MakeByRefType());
				par.Populate(1, i => GetType(module, p[i - 1].ParameterType));
			}
		}
		else if (method is ConstructorInfo cInfo)
		{
			ret = GetType(module, typeof(void));
			
			var p = cInfo.GetParameters();
			par = new ILType[p.Length + 1];
			par[0] = GetType(module, method.DeclaringType!.MakeByRefType());
			par.Populate(1, i => GetType(module, p[i - 1].ParameterType));
		}
		else throw new NotImplementedException($"Cannot compile method of type '{method.GetType()}'.");
		signature = LLVMTypeRef.CreateFunction(ret, par.ArraySelect(p => p.LLVMType));
	}

	private static void InitializeMethodEnvironment(ILModule module, ILMethod method, ILType[] paramT, LLVMBuilderRef builder,
		out ILValue[] locals, out ILValue[] args, out ILStack stack, out byte[] ilBytes)
	{
		var initBlock = method.LLVMValue.AppendBasicBlock(".init");
		builder.PositionAtEnd(initBlock);
		
		var body = method.Method.GetMethodBody()!;
		
		var loc = body.LocalVariables;
		locals = new ILValue[loc.Count];
		locals.Populate(i =>
		{
			var type = loc[i].LocalType;
			if (type.IsByRef) type = type.GetElementType()!.MakePointerType();
			return new ILValue(type.MakeByRefType(), builder.BuildAlloca(GetType(module, type), $".il_loc{i}"));
		});
		
		args = paramT.ArraySelect((p, i) =>
		{
			var type = p.Type;
			if (type.IsByRef) return new ILValue(type, method.LLVMValue.Params[i]);
			var arg = new ILValue(type.MakeByRefType(), builder.BuildAlloca(GetType(module, type), $".il_arg{i}"));
			builder.BuildStore(method.LLVMValue.Params[i], arg);
			return arg;
		});

		stack = new ILStack(body.MaxStackSize, builder);
		ilBytes = body.GetILAsByteArray()!;
	}

	private static void OptimizeFunction(ILModule module, ILMethod method)
	{
		if(Environment.GetEnvironmentVariable("IL2LLVM_OPTIMIZE") == "1")
			module.PassManager.RunFunctionPassManager(method);
	}
}