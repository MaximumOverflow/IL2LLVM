using System.Reflection;
using IL2LLVM.Compilation;
using LLVMSharp.Interop;

namespace IL2LLVM;

public class ILModule : IDisposable
{
	internal LLVMModuleRef LlvmModule;
	internal LLVMPassManagerRef PassManager;
	internal LLVMExecutionEngineRef LlvmJit;
	internal Dictionary<Type, LLVMTypeRef> Types;
	internal Dictionary<MethodBase, LLVMValueRef> Methods;

	public ILModule(string name)
	{
		LlvmModule = ILJit.LlvmContext.CreateModuleWithName(name);
		LlvmJit = LlvmModule.CreateExecutionEngine();
		Types = new Dictionary<Type, LLVMTypeRef>();
		Methods = new Dictionary<MethodBase, LLVMValueRef>();
		
		PassManager = LlvmModule.CreateFunctionPassManager();
		PassManager.AddBasicAliasAnalysisPass();
		PassManager.AddPromoteMemoryToRegisterPass();
		PassManager.AddInstructionCombiningPass();
		PassManager.AddReassociatePass();
		PassManager.AddNewGVNPass();
		PassManager.AddLoopUnrollPass();
		PassManager.AddLoopVectorizePass();
		PassManager.InitializeFunctionPassManager();
	}

	~ILModule() => Dispose();

	public void Dump() => LlvmModule.Dump();
	public void Verify() => LlvmModule.Verify(LLVMVerifierFailureAction.LLVMPrintMessageAction);

	public void Dispose()
	{
		LlvmJit.Dispose();
		PassManager.Dispose(); 
		// LlvmModule.Dispose(); //BUG in the bindigs' implementation, makes the program crash.
		GC.SuppressFinalize(this);
	}
}

public record struct ILType(Type Type, LLVMTypeRef LLVMType)
{
	public static implicit operator Type(ILType t) => t.Type;
	public static implicit operator LLVMTypeRef(ILType t) => t.LLVMType;
}

public record struct ILMethod(MethodBase Method, LLVMValueRef LLVMValue)
{
	public static implicit operator MethodBase(ILMethod t) => t.Method;
	public static implicit operator LLVMValueRef(ILMethod t) => t.LLVMValue;
}

internal record struct ILValue(Type Type, LLVMValueRef LLVMValue)
{
	public static implicit operator LLVMValueRef(ILValue t) => t.LLVMValue;
}

internal struct ILStack
{
	public readonly int Size;
	public readonly LLVMValueRef Base;
	public readonly LLVMValueRef Pointer;

	public ILStack(int size, LLVMBuilderRef builder)
	{
		Size = size;
		var stackSize = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)size);
		Base = builder.BuildArrayAlloca(LLVMTypeRef.Int1, stackSize, ".il_stack");
		Pointer = builder.BuildAlloca(LLVMTypeRef.CreatePointer(LLVMTypeRef.Int1, 0), ".il_stackPtr");
		builder.BuildStore(Base, Pointer);
	}

	public void Push(ILValue value, LLVMBuilderRef builder)
	{
		//Push value
		var ptr = builder.BuildLoad(Pointer);
		var vPtr = builder.BuildPointerCast(ptr, LLVMTypeRef.CreatePointer(value.LLVMValue.TypeOf, 0));
		builder.BuildStore(value, vPtr);

		//Advance pointer
		var iPtr = builder.BuildPtrToInt(ptr, LLVMTypeRef.Int64);
		iPtr = builder.BuildAdd(iPtr, value.LLVMValue.TypeOf.SizeOf);
		iPtr = builder.BuildIntToPtr(iPtr, Pointer.TypeOf.ElementType);
		builder.BuildStore(iPtr, Pointer);
	}

	public ILValue Pop(ILModule module, LLVMBuilderRef builder, Type type)
	{
		var ilType = module.GetType(type);
		
		//Regress pointer
		var ptr = builder.BuildLoad(Pointer);
		var iPtr = builder.BuildPtrToInt(ptr, LLVMTypeRef.Int64);
		iPtr = builder.BuildSub(iPtr, ilType.LLVMType.SizeOf);
		iPtr = builder.BuildIntToPtr(iPtr, Pointer.TypeOf.ElementType);
		builder.BuildStore(iPtr, Pointer);
		
		//Pop value
		var vPtr = builder.BuildPointerCast(iPtr, LLVMTypeRef.CreatePointer(ilType, 0));
		var llvmValue = builder.BuildLoad(vPtr);
		return new ILValue(type, llvmValue);
	}
}

internal readonly struct ILBlock
{
	public readonly int Start;
	public readonly Memory<byte> ILBytes;
	public int End => Start + ILBytes.Length;
	public readonly LLVMBasicBlockRef LLVMBlock;

	public ILBlock(int start, int length, byte[] ilBytes, ILMethod method)
	{
		Start = start;
		ILBytes = new Memory<byte>(ilBytes, start, length + 1);
		LLVMBlock = method.LLVMValue.AppendBasicBlock($"IL_{start:X4} - IL_{start+length-1:X4}");
	}

	public override string ToString() => $"IL_{Start:X4} - IL_{End-1:X4}";
	public static implicit operator LLVMBasicBlockRef(ILBlock t) => t.LLVMBlock;
}