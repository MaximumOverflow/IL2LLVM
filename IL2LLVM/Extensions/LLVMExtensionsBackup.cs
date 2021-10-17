// using IL2LLVM.Compilation;
// using LLVMSharp.Interop;
//
// namespace IL2LLVM.Extensions;
//
// public static class LLVMExtensions
// {
// 	internal static ILValue BuildCondLoad(this LLVMBuilderRef builder, ILValue value)
// 		=> value.Type.IsByRef ? new ILValue(value.Type.GetElementType()!, builder.BuildLoad(value)) : value;
//
// 	internal static LLVMValueRef BuildAdvancedStore(this LLVMBuilderRef builder, ILModule module, ILValue value, ILValue ptr)
// 	{
// 		value = BuildAutoCast(builder, module, value, ptr.Type.GetElementType()!);
// 		return builder.BuildStore(value, ptr);
// 	}
//
// 	internal static ILValue BuildAutoCast(this LLVMBuilderRef builder, ILModule module, ILValue value, Type toType)
// 	{
// 		var destT = module.GetType(toType).LLVMType;
// 		var destK = destT.Kind;
// 		
// 		if (value.Type == toType) return value;
//
// 		if (value.Type.IsByRef)
// 		{
// 			if(value.Type.GetElementType() == toType)
// 				return builder.BuildCondLoad(value);
//
// 			var k = value.LLVMValue.TypeOf.ElementType.Kind;
// 			if (k == LLVMTypeKind.LLVMIntegerTypeKind && (toType.IsPointer() || toType == typeof(IntPtr) || toType == typeof(UIntPtr)))
// 				return new ILValue(toType, builder.BuildIntToPtr(builder.BuildCondLoad(value), destT));
// 		}
// 		
// 		if (value.Type.IsPointer())
// 		{
// 			if(toType.IsPointer()) 
// 				return new ILValue(toType, builder.BuildPointerCast(value, destT));
// 			
// 			if (destK == LLVMTypeKind.LLVMIntegerTypeKind) 
// 				return new ILValue(toType, builder.BuildPtrToInt(value, destT));
// 		}
//
// 		if (toType.IsByRef && toType.GetElementType()! == value.Type)
// 		{
// 			var tmp = builder.BuildAlloca(value.LLVMValue.TypeOf);
// 			builder.BuildStore(value, tmp);
// 			return new ILValue(toType, tmp);
// 		}
//
//
// 		return value.LLVMValue.TypeOf.Kind switch
// 		{
// 			LLVMTypeKind.LLVMIntegerTypeKind when destK == LLVMTypeKind.LLVMIntegerTypeKind
// 				=> new ILValue(toType, builder.BuildIntCast(value, destT)),
// 			
// 			LLVMTypeKind.LLVMIntegerTypeKind when destK == LLVMTypeKind.LLVMFloatTypeKind && !value.Type.IsUnsigned()
// 				=> new ILValue(toType, builder.BuildSIToFP(value, destT)),
// 			
// 			LLVMTypeKind.LLVMIntegerTypeKind when destK == LLVMTypeKind.LLVMFloatTypeKind && value.Type.IsUnsigned()
// 				=> new ILValue(toType, builder.BuildUIToFP(value, destT)),
// 			
// 			LLVMTypeKind.LLVMIntegerTypeKind when destK == LLVMTypeKind.LLVMPointerTypeKind
// 				=> new ILValue(toType, builder.BuildIntToPtr(value, destT)),
// 			
// 			LLVMTypeKind.LLVMFloatTypeKind when destK == LLVMTypeKind.LLVMIntegerTypeKind && !toType.IsUnsigned()
// 				=> new ILValue(toType, builder.BuildFPToSI(value, destT)),
// 			
// 			LLVMTypeKind.LLVMFloatTypeKind when destK == LLVMTypeKind.LLVMIntegerTypeKind && toType.IsUnsigned()
// 				=> new ILValue(toType, builder.BuildFPToUI(value, destT)),
//
// 			_ => throw new NotImplementedException($"Casting from {value.Type} to {toType} not implemented yet.")
// 		};
// 	}
// }