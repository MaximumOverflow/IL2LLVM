using IL2LLVM.Compilation;
using LLVMSharp.Interop;

namespace IL2LLVM.Extensions;

public static class LLVMExtensions
{
	internal static ILValue BuildCondLoad(this LLVMBuilderRef builder, ILValue value)
		=> value.Type.IsByRef ? new ILValue(value.Type.GetElementType()!, builder.BuildLoad(value)) : value;

	internal static LLVMValueRef BuildAdvancedStore(this LLVMBuilderRef builder, ILModule module, ILValue value, ILValue ptr)
	{
		value = BuildAutoCast(builder, module, value, ptr.Type.GetElementType()!);
		return builder.BuildStore(value, ptr);
	}

	internal static ILValue BuildAutoCast(this LLVMBuilderRef builder, ILModule module, ILValue value, Type toType)
	{
		if (value.Type == toType) return value;
		var dt = module.GetType(toType);
		
		//#### REFERENCE CASTS ####		These are special cases to ensure CIL compatibility
		if (value.Type.IsByRef && value.Type.GetElementType() == toType)
			return new ILValue(toType, builder.BuildLoad(value));
		
		if (value.Type.IsByRef && value.Type.GetElementType()!.CanBeCastTo(toType))
			return BuildAutoCast(builder, module, builder.BuildCondLoad(value), toType);

		if (toType.IsByRef && (value.Type == typeof(long) || value.Type == typeof(int)))
			return new ILValue(toType, builder.BuildIntToPtr(value, dt));

		if (!value.Type.CanBeCastTo(toType))
			throw new InvalidCastException($"Cannot cast '{value.Type}' to '{toType}'.");
		
		var dk = dt.LLVMType.Kind;
		var tk = value.LLVMValue.TypeOf.Kind;

		
		//#### POINTER CASTS ####
		if (tk == LLVMTypeKind.LLVMIntegerTypeKind && toType.IsPointer())
			return new ILValue(toType, builder.BuildIntToPtr(value, dt));
		
		if (value.Type.IsPointer() && dk == LLVMTypeKind.LLVMIntegerTypeKind)
			return new ILValue(toType, builder.BuildPtrToInt(value, dt));
		
		
		//#### INTEGER CASTS ####
		if (tk == LLVMTypeKind.LLVMIntegerTypeKind)
		{
			if (dk == LLVMTypeKind.LLVMIntegerTypeKind)
				return new ILValue(toType, builder.BuildIntCast(value, dt));
		
			if (dk == LLVMTypeKind.LLVMFloatTypeKind) return value.Type.IsUnsigned() 
				? new ILValue(toType, builder.BuildUIToFP(value, dt))
				: new ILValue(toType, builder.BuildSIToFP(value, dt));
		}
		
		
		//#### FLOATING POINT CASTS ####
		if (tk == LLVMTypeKind.LLVMFloatTypeKind)
		{
			if (dk == LLVMTypeKind.LLVMFloatTypeKind)
				return new ILValue(toType, builder.BuildFPCast(value, dt));
			
			if (dk == LLVMTypeKind.LLVMIntegerTypeKind) return toType.IsUnsigned() 
				? new ILValue(toType, builder.BuildFPToUI(value, dt))
				: new ILValue(toType, builder.BuildFPToSI(value, dt));
		}

		throw new NotImplementedException($"Cast from '{value.Type}' to '{toType}' is not yet implemented.");
	}

	internal static LLVMValueRef BuildMemSet(this LLVMBuilderRef builder, ILModule module, ILValue ptr, byte value)
	{
		var memset = module.LlvmModule.GetNamedFunction("llvm.memset.p0i8.i64");
		// ReSharper disable once ConditionIsAlwaysTrueOrFalse
		if (memset == null)
		{
			var fnType = LLVMTypeRef.CreateFunction(LLVMTypeRef.Void, stackalloc LLVMTypeRef[]
			{
				LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0), LLVMTypeRef.Int8,
				LLVMTypeRef.Int64, LLVMTypeRef.Int1,
			}, false);
			memset = module.LlvmModule.AddFunction("llvm.memset.p0i8.i64", fnType);
		}

		Span<LLVMValueRef> param = stackalloc LLVMValueRef[]
		{
			builder.BuildPointerCast(ptr, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0)),
			LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, value),
			ptr.LLVMValue.TypeOf.ElementType.SizeOf,
			LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 0),

		};

		return builder.BuildCall(memset, param, default);
	}
}