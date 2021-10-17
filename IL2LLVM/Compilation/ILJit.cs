using System.Reflection;
using LLVMSharp.Interop;

namespace IL2LLVM.Compilation;

public static partial class ILJit
{
	internal static readonly LLVMContextRef LlvmContext;

	static ILJit()
	{
		if (Environment.GetEnvironmentVariable("IL2LLVM_OPTIMIZE") is null)
			Environment.SetEnvironmentVariable("IL2LLVM_OPTIMIZE", "1");
		
		LlvmContext = LLVMContextRef.Global;
		LLVM.LinkInMCJIT();
		LLVM.InitializeX86TargetInfo();
		LLVM.InitializeX86Target();
		LLVM.InitializeX86TargetMC();
		LLVM.InitializeX86AsmParser();
		LLVM.InitializeX86AsmPrinter();
	}
	
	internal static ILType GetType(this ILModule module, Type type) 
		=> module.Types.TryGetValue(type, out var t) ? new ILType(type, t) : CompileType(module, type);

	internal static ILMethod GetMethod(this ILModule module, MethodBase method)
		=> module.Methods.TryGetValue(method, out var v) ? new ILMethod(method, v) : CompileMethod(module, method);
	
	public static ILMethod GetMethod(this ILModule module, Delegate method)
		=> GetMethod(module, method.Method);

	public static IntPtr GetMethodPtr(this ILModule module, MethodInfo method)
	{
		var fn = GetMethod(module, method);
		return (IntPtr) module.LlvmJit.GetFunctionAddress(CreateMethodName(method));
	}
	
	public static IntPtr GetMethodPtr(this ILModule module, Delegate method)
	{
		var fn = GetMethod(module, method);
		return (IntPtr) module.LlvmJit.GetFunctionAddress(CreateMethodName(method.Method));
	}
}