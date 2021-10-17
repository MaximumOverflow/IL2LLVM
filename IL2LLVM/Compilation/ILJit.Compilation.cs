using System.Diagnostics;
using System.Runtime.CompilerServices;
using IL2LLVM.Extensions;
using LLVMSharp.Interop;
using System.Reflection;

namespace IL2LLVM.Compilation;

public static partial class ILJit
{
	internal static ILType CompileType(this ILModule module, Type type)
	{
		if(type == typeof(void)) return new ILType(type, LLVMTypeRef.Void);

		if (type.IsByRef || type.IsPointer)
		{
			var baseT = GetType(module, type.GetElementType()!);
			return new ILType(type, LLVMTypeRef.CreatePointer(baseT, 0));
		}
		
		if(type.IsClass) throw new InvalidOperationException($"Type '{type}' is not supported.");

		if(type.IsPrimitive)
		{
			if(type == typeof(bool)) return new ILType(type, LLVMTypeRef.Int1);
			if(type == typeof(Byte)) return new ILType(type, LLVMTypeRef.Int8);
			if(type == typeof(SByte)) return new ILType(type, LLVMTypeRef.Int8);
			if(type == typeof(Int16)) return new ILType(type, LLVMTypeRef.Int16);
			if(type == typeof(Int32)) return new ILType(type, LLVMTypeRef.Int32);
			if(type == typeof(Int64)) return new ILType(type, LLVMTypeRef.Int64);
			if(type == typeof(UInt16)) return new ILType(type, LLVMTypeRef.Int16);
			if(type == typeof(UInt32)) return new ILType(type, LLVMTypeRef.Int32);
			if(type == typeof(UInt64)) return new ILType(type, LLVMTypeRef.Int64);
			if(type == typeof(Single)) return new ILType(type, LLVMTypeRef.Float);
			if(type == typeof(Double)) return new ILType(type, LLVMTypeRef.Double);
			if(type == typeof(IntPtr)) return new ILType(type, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0));
			if(type == typeof(UIntPtr)) return new ILType(type, LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0));
			throw new InvalidOperationException($"Type '{type}' is not supported.");
		}
		
		var llvmType = module.LlvmModule.Context.CreateNamedStruct(CreateTypeName(type));
		module.Types.Add(type, llvmType);

		var fields = type.GetValueMembers();
		Span<LLVMTypeRef> members = stackalloc LLVMTypeRef[fields.Count];
		for (var i = 0; i < fields.Count; i++)
		{
			var field = fields[i];
			if(field is FieldInfo fInfo)
				members[i] = CompileType(module, fInfo.FieldType);
			else if(field is PropertyInfo pInfo)
				members[i] = CompileType(module, pInfo.PropertyType);
		}

		llvmType.StructSetBody(members, false);
		return new ILType(type, llvmType);
	}

	private static ILMethod CompileMethod(this ILModule module, MethodBase method)
	{
		CreateMethodSignature(module, method, out var retT, out var parT, out var fnSignature);
		var llvmValue = module.LlvmModule.AddFunction(CreateMethodName(method), fnSignature);
		var builder = module.LlvmModule.Context.CreateBuilder();
		var ilMethod = new ILMethod(method, llvmValue);
		module.Methods.Add(method, llvmValue);

		InitializeMethodEnvironment(module, ilMethod, parT, builder, out var locals, out var args, out var stack, out var ilBytes);
		
		var ilBlocks = DefineBlocks(ilMethod, ilBytes); builder.BuildBr(ilBlocks[0]);
		foreach (var block in ilBlocks.Values) CompileBlock(module, ilMethod, block, locals, args, stack, ilBlocks, builder);
		
		if (llvmValue.VerifyFunction(LLVMVerifierFailureAction.LLVMPrintMessageAction)) 
			OptimizeFunction(module, ilMethod);
		else
		{
			Console.WriteLine();
			Console.WriteLine(llvmValue);
			Console.WriteLine($"Could not compile method '{method.DeclaringType}.{method}'.\nThe process will now exit.");
			Environment.Exit(-1);
		}

		return ilMethod;
	}
	
	private static Dictionary<int, ILBlock> DefineBlocks(ILMethod ilMethod, byte[] ilBytes)
	{
		var blockStarts = new SortedSet<int> { 0 };
		for (var i = 0; i < ilBytes.Length; i++)
		{
			var opcode = (ILOpCode)ilBytes[i];
			switch (opcode)
			{
				//#### SKIP INSTRUCTIONS ####
				case ILOpCode.Stloc_u8:
				case ILOpCode.Ldloc_u8:
				case ILOpCode.Ldc_i4_i8:
				case ILOpCode.Ldarga_u8: i++; break;
				
				case ILOpCode.Call_mt:
				case ILOpCode.Stobj_tt:
				case ILOpCode.Ldfld_ft:
				case ILOpCode.Ldobj_tt:
				case ILOpCode.Ldflda_ft:
				case ILOpCode.Newobj_mt:
				case ILOpCode.Ldc_r4_f32: i += 4; break;
				case ILOpCode.Ldc_r8_f64: i += 8; break;

				case ILOpCode.CompoundCode:
				{
					var opcode2 = (ILCompoundOpCode) ilBytes[++i];
					switch (opcode2)
					{
						case ILCompoundOpCode.Sizeof_tt: i += 4; break;
					}
					break;
				}

				//#### BREAK INSTRUCTIONS ####
				case ILOpCode.Br_i8:
				{
					var target = unchecked((sbyte) ilBytes[++i]) + i + 1;
					blockStarts.Add(target); 
					break;
				}

				case ILOpCode.Br_i32:
				{
					var target = Unsafe.As<byte, int>(ref ilBytes[++i]);
					blockStarts.Add(target);
					i += 4;
					break;
				}

				case ILOpCode.Blt_i8:
				case ILOpCode.Bgt_i8:
				case ILOpCode.Blt_un_i8:
				case ILOpCode.Bgt_un_i8:
				case ILOpCode.Brtrue_i8:
				case ILOpCode.Brfalse_i8:
				{
					var target = unchecked((sbyte) ilBytes[++i]) + i + 1;
					blockStarts.Add(target);
					blockStarts.Add(i + 1);
					break;
				}
				
				case ILOpCode.Brtrue_i32:
				case ILOpCode.Brfalse_i32:
				{
					var target = Unsafe.As<byte, int>(ref ilBytes[++i]);
					blockStarts.Add(target);
					blockStarts.Add(i + 1);
					i += 4;
					break;
				}

				case ILOpCode.Ret when i < ilBytes.Length - 1:
				{
					blockStarts.Add(i + 1);
					break;
				}
			}
		}

		var blocks = new Dictionary<int, ILBlock>();
		foreach (var start in blockStarts)
		{
			for (var i = start; i < ilBytes.Length; i++)
			{
				var opcode = (ILOpCode)ilBytes[i];
				switch (opcode)
				{
					//#### SKIP INSTRUCTIONS ####
					case ILOpCode.Stloc_u8:
					case ILOpCode.Ldloc_u8:
					case ILOpCode.Ldc_i4_i8:
					case ILOpCode.Ldarga_u8: i++; break;
				
					case ILOpCode.Call_mt:
					case ILOpCode.Stobj_tt:
					case ILOpCode.Ldfld_ft:
					case ILOpCode.Ldobj_tt:
					case ILOpCode.Ldflda_ft:
					case ILOpCode.Newobj_mt:
					case ILOpCode.Ldc_r4_f32: i += 4; break;
					case ILOpCode.Ldc_r8_f64: i += 8; break;

					case ILOpCode.CompoundCode:
					{
						var opcode2 = (ILCompoundOpCode) ilBytes[++i];
						switch (opcode2)
						{
							case ILCompoundOpCode.Sizeof_tt: i += 4; break;
						}
						break;
					}
					
					//#### BREAK INSTRUCTIONS ####
					case ILOpCode.Br_i8:
					case ILOpCode.Blt_i8:
					case ILOpCode.Bgt_i8:
					case ILOpCode.Blt_un_i8:
					case ILOpCode.Bgt_un_i8:
					case ILOpCode.Brtrue_i8:
					case ILOpCode.Brfalse_i8:
					{
						var length = i - start + 1;
						blocks.Add(start, new ILBlock(start, length, ilBytes, ilMethod));
						goto BreakFor;
					}

					case ILOpCode.Br_i32:
					case ILOpCode.Brtrue_i32:
					case ILOpCode.Brfalse_i32:
					{
						throw new NotImplementedException();
					}

					case ILOpCode.Throw:
					{
						var name = $"{ilMethod.Method.DeclaringType}.{ilMethod.Method.Name}";
						throw new NotImplementedException($"Method '{name}' could throw an exception. Exceptions are not supported.");
					}
					
					case ILOpCode.Ret:
					{
						var length = i - start;
						blocks.Add(start, new ILBlock(start, length, ilBytes, ilMethod));
						goto BreakFor;
					}
				}
			}
			BreakFor: ;
		}

		return blocks;
	}

	private static void CompileBlock(ILModule module, ILMethod method, ILBlock block, 
		ILValue[] locals, ILValue[] args, ILStack ilStack, Dictionary<int, ILBlock> blocks, LLVMBuilderRef builder)
	{
		builder.PositionAtEnd(block);
		var ilBytes = block.ILBytes.Span;
		var stack = new Stack<ILValue>();

		void StackPush(ILValue value)
			=> stack.Push(value);

		ILValue StackPop(Type? type)
		{
			ILValue value;
			if (stack.Count != 0) value = stack.Pop();
			else if (type is null) throw new NotImplementedException("I have no idea what to do in this case.");
			else value = ilStack.Pop(module, builder, type);
			
			return type != null ? builder.BuildAutoCast(module, value, type) : value;
		}

		void FlushStack()
		{
			foreach (var ilValue in stack)
				ilStack.Push(ilValue, builder);
		}

		for (var i = 0; i < ilBytes.Length; i++)
		{
			var opcode = (ILOpCode) ilBytes[i];
			switch (opcode)
			{
				case ILOpCode.Nop: break;

				//#### TEST #### Might overflow the stack, might need to calculate a new max stack size. ####
				case ILOpCode.Ldarg_0: StackPush(args[0]); break;
				case ILOpCode.Ldarg_1: StackPush(args[1]); break;
				case ILOpCode.Ldarg_2: StackPush(args[2]); break;
				case ILOpCode.Ldarg_3: StackPush(args[3]); break;
				case ILOpCode.Ldarga_u8: StackPush(args[ilBytes[++i]]); break;
				
				case ILOpCode.Ldloc_0: StackPush(locals[0]); break;
				case ILOpCode.Ldloc_1: StackPush(locals[1]); break;
				case ILOpCode.Ldloc_2: StackPush(locals[2]); break;
				case ILOpCode.Ldloc_3: StackPush(locals[3]); break;
				case ILOpCode.Ldloc_u8: StackPush(locals[ilBytes[++i]]); break;
				case ILOpCode.Ldloca_u8: StackPush(locals[ilBytes[++i]]); break;
				//#### ENDTEST ####

				
				case ILOpCode.Ldc_i4_0: StackPush(new ILValue(typeof(int), LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0))); break;
				case ILOpCode.Ldc_i4_1: StackPush(new ILValue(typeof(int), LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 1))); break;
				case ILOpCode.Ldc_i4_2: StackPush(new ILValue(typeof(int), LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 2))); break;
				case ILOpCode.Ldc_i4_3: StackPush(new ILValue(typeof(int), LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 3))); break;
				case ILOpCode.Ldc_i4_4: StackPush(new ILValue(typeof(int), LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 4))); break;
				case ILOpCode.Ldc_i4_5: StackPush(new ILValue(typeof(int), LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 5))); break;
				case ILOpCode.Ldc_i4_6: StackPush(new ILValue(typeof(int), LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 6))); break;
				case ILOpCode.Ldc_i4_7: StackPush(new ILValue(typeof(int), LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 7))); break;
				case ILOpCode.Ldc_i4_8: StackPush(new ILValue(typeof(int), LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 8))); break;
				case ILOpCode.Ldc_i4_i8: StackPush(new ILValue(typeof(int), LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, ilBytes[++i]))); break;

				case ILOpCode.Ldc_r4_f32:
				{
					var value = Unsafe.As<byte, float>(ref ilBytes[++i]); i += 3;
					StackPush(new ILValue(typeof(float), LLVMValueRef.CreateConstReal(LLVMTypeRef.Float, value)));
					break;
				}
				
				case ILOpCode.Ldc_r8_f64:
				{
					var value = Unsafe.As<byte, double>(ref ilBytes[++i]); i += 7;
					StackPush(new ILValue(typeof(double), LLVMValueRef.CreateConstReal(LLVMTypeRef.Double, value)));
					break;
				}

				case ILOpCode.Ldind_i4:
				{
					var addr = StackPop(typeof(int).MakeByRefType());
					StackPush(builder.BuildCondLoad(addr));
					break;
				}

				case ILOpCode.Stloc_0: builder.BuildAdvancedStore(module, StackPop(locals[0].Type.GetElementType()), locals[0]); break;
				case ILOpCode.Stloc_1: builder.BuildAdvancedStore(module, StackPop(locals[1].Type.GetElementType()), locals[1]); break;
				case ILOpCode.Stloc_2: builder.BuildAdvancedStore(module, StackPop(locals[2].Type.GetElementType()), locals[2]); break;
				case ILOpCode.Stloc_3: builder.BuildAdvancedStore(module, StackPop(locals[3].Type.GetElementType()), locals[3]); break;
				case ILOpCode.Stloc_u8: { var p = ilBytes[++i]; builder.BuildAdvancedStore(module, StackPop(locals[p].Type.GetElementType()), locals[p]); break; }

				case ILOpCode.Stobj_tt:
				{
					var token = Unsafe.As<byte, int>(ref ilBytes[++i]); i += 3;
					var type = CompilationHelper.ResolveType(method, token);
					var value = StackPop(type);
					var ptr = StackPop(type.MakeByRefType());
					builder.BuildStore(value, ptr);
					break;
				}
				
				case ILOpCode.Ldfld_ft:
				{
					var target = Unsafe.As<byte, int>(ref ilBytes[++i]); i += 3;
					var field = CompilationHelper.ResolveField(method, target);
					
					var value = StackPop(field.DeclaringType!.MakeByRefType());
					var fields = field.DeclaringType!.GetValueMembers();
					var index = fields.IndexOf(field);
					if (index == -1) throw new NotImplementedException();

					var llvmValue = builder.BuildStructGEP(value, (uint)index);
					var fieldValue = new ILValue(field.FieldType, builder.BuildLoad(llvmValue));
					StackPush(fieldValue);
					break;
				}

				case ILOpCode.Stfld_ft:
				{
					var target = Unsafe.As<byte, int>(ref ilBytes[++i]); i += 3;
					var field = CompilationHelper.ResolveField(method, target);
					var index = CompilationHelper.ResolveFieldIndex(field);

					var value = StackPop(field.FieldType);
					var obj = StackPop(field.DeclaringType!.MakeByRefType());
					var ptr = builder.BuildStructGEP(obj, index);
					builder.BuildStore(value, ptr);
					break;
				}

				case ILOpCode.Br_i8:
				{
					var target = block.Start + unchecked((sbyte) ilBytes[++i]) + i + 1;
					var targetBlock = blocks[target];

					FlushStack();
					builder.BuildBr(targetBlock);
					break;
				}
				
				case ILOpCode.Brtrue_i8:
				{
					var target = block.Start + unchecked((sbyte) ilBytes[++i]) + i + 1;
					var targetBlock = blocks[target];
					var elseBlock = blocks[block.Start + i + 1];
					
					var value = StackPop(typeof(bool));
					FlushStack();
					builder.BuildCondBr(value, targetBlock, elseBlock);
					break;
				}

				case ILOpCode.Blt_i8:
				{
					var target = block.Start + unchecked((sbyte) ilBytes[++i]) + i + 1;
					var targetBlock = blocks[target];
					var elseBlock = blocks[block.Start + i + 1];

					var cmp = StackPop(typeof(int));
					var value = StackPop(typeof(int));
					var cond = builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, value, cmp);
					
					FlushStack();
					builder.BuildCondBr(cond, targetBlock, elseBlock);
					break;
				}

				case ILOpCode.Blt_un_i8:
				{
					var target = block.Start + unchecked((sbyte) ilBytes[++i]) + i + 1;
					var targetBlock = blocks[target];
					var elseBlock = blocks[block.Start + i + 1];

					var cmp = StackPop(typeof(uint));
					var value = StackPop(typeof(uint));
					var cond = builder.BuildICmp(LLVMIntPredicate.LLVMIntULT, value, cmp);
					
					FlushStack();
					builder.BuildCondBr(cond, targetBlock, elseBlock);
					break;
				}

				case ILOpCode.Newobj_mt:
				{
					var token = Unsafe.As<byte, int>(ref ilBytes[++i]); i += 3;
					var ctor = CompilationHelper.ResolveMethod(method, token);
					var ilType = GetType(module, ctor.DeclaringType!);

					var fn = GetMethod(module, ctor);
					var llvmValue = builder.BuildAlloca(ilType);
					
					var p = ctor.GetParameters();
					var param = new LLVMValueRef[fn.LLVMValue.ParamsCount];
					param.RevPopulate(1, par => StackPop(p[par - 1].ParameterType));
					param[0] = llvmValue;
					builder.BuildCall(fn, param);

					ilType = new ILType(ilType.Type.MakeByRefType(), LLVMTypeRef.CreatePointer(ilType.LLVMType, 0));
					StackPush(new ILValue(ilType, llvmValue));
					break;
				}

				case ILOpCode.Ldobj_tt:
				{
					var token = Unsafe.As<byte, int>(ref ilBytes[++i]); i += 3;
					var type = CompilationHelper.ResolveType(method, token);

					var value = builder.BuildCondLoad(StackPop(type));
					stack.Push(value);
					break;
				}

				case ILOpCode.Call_mt:
				{
					var token = Unsafe.As<byte, int>(ref ilBytes[++i]); i += 3;
					var mBase = CompilationHelper.ResolveMethod(method, token);
					var fn = GetMethod(module, mBase);
					
					if (mBase is MethodInfo mInfo)
					{
						if (mInfo.IsStatic)
						{
							var p = fn.Method.GetParameters();
							var param = new LLVMValueRef[fn.LLVMValue.ParamsCount];
							param.RevPopulate(par => StackPop(p[par].ParameterType));
							var ret = builder.BuildCall(fn, param);
							StackPush(new ILValue(mInfo.ReturnType, ret));
						}
						else
						{
							var p = fn.Method.GetParameters();
							var param = new LLVMValueRef[fn.LLVMValue.ParamsCount];
							param.RevPopulate(1, par => StackPop(p[par - 1].ParameterType));
							param[0] = StackPop(mInfo.DeclaringType!.MakeByRefType());
							var ret = builder.BuildCall(fn, param);
							StackPush(new ILValue(mInfo.ReturnType, ret));
						}

						break;
					}
					
					if (mBase is ConstructorInfo cInfo)
					{
						var p = fn.Method.GetParameters();
						var param = new LLVMValueRef[fn.LLVMValue.ParamsCount];
						param.RevPopulate(1, par => StackPop(p[par - 1].ParameterType));
						param[0] = StackPop(cInfo.DeclaringType!.MakeByRefType());
						builder.BuildCall(fn, param);
						break;
					}
					
					throw new NotImplementedException();
				}
				
				case ILOpCode.Conv_i : StackPush(builder.BuildAutoCast(module, StackPop(null), typeof(nint)));		break;
				case ILOpCode.Conv_i1: StackPush(builder.BuildAutoCast(module, StackPop(null), typeof(sbyte)));		break;
				case ILOpCode.Conv_i2: StackPush(builder.BuildAutoCast(module, StackPop(null), typeof(short)));		break;
				case ILOpCode.Conv_i4: StackPush(builder.BuildAutoCast(module, StackPop(null), typeof(int)));		break;
				case ILOpCode.Conv_i8: StackPush(builder.BuildAutoCast(module, StackPop(null), typeof(long)));		break;
				case ILOpCode.Conv_u : StackPush(builder.BuildAutoCast(module, StackPop(null), typeof(nuint)));		break;
				case ILOpCode.Conv_u1: StackPush(builder.BuildAutoCast(module, StackPop(null), typeof(byte)));		break;
				case ILOpCode.Conv_u2: StackPush(builder.BuildAutoCast(module, StackPop(null), typeof(ushort)));	break;
				case ILOpCode.Conv_u4: StackPush(builder.BuildAutoCast(module, StackPop(null), typeof(uint)));		break;
				case ILOpCode.Conv_u8: StackPush(builder.BuildAutoCast(module, StackPop(null), typeof(ulong)));		break;

				case ILOpCode.Conv_r4:
				{
					var value = StackPop(null);
					StackPush(builder.BuildAutoCast(module, value, typeof(float)));
					break;
				}
				
				case ILOpCode.Add:
				{
					var a = builder.BuildCondLoad(StackPop(null));
					var b = builder.BuildCondLoad(StackPop(a.Type));
					var kind = a.LLVMValue.TypeOf.Kind;
					
					switch (kind)
					{
						case LLVMTypeKind.LLVMIntegerTypeKind: StackPush(new ILValue(a.Type, builder.BuildAdd(a, b))); break;
						case LLVMTypeKind.LLVMFloatTypeKind: StackPush(new ILValue(a.Type, builder.BuildFAdd(a, b))); break;
						case LLVMTypeKind.LLVMDoubleTypeKind: StackPush(new ILValue(a.Type, builder.BuildFAdd(a, b))); break;
						default: throw new NotImplementedException($"Addition between values of type '{a.Type}' is not yet implemented.");
					}
					break;
				}
				
				case ILOpCode.Mul:
				{
					var a = builder.BuildCondLoad(StackPop(null));
					var b = builder.BuildCondLoad(StackPop(a.Type));
					var kind = a.LLVMValue.TypeOf.Kind;
					
					switch (kind)
					{
						case LLVMTypeKind.LLVMIntegerTypeKind: StackPush(new ILValue(a.Type, builder.BuildMul(a, b))); break;
						case LLVMTypeKind.LLVMFloatTypeKind: StackPush(new ILValue(a.Type, builder.BuildFMul(a, b))); break;
						case LLVMTypeKind.LLVMDoubleTypeKind: StackPush(new ILValue(a.Type, builder.BuildFMul(a, b))); break;
						default: throw new NotImplementedException($"Multiplication between values of type '{a.Type}' is not yet implemented.");
					}
					break;
				}

				case ILOpCode.Ret:
				{
					if (method.Method is ConstructorInfo)
					{ builder.BuildRetVoid(); break; }

					var mInfo = (MethodInfo)method.Method;
					var retT = mInfo.ReturnType;

					if (retT == typeof(void))
					{ builder.BuildRetVoid(); break; }

					var value = StackPop(retT);
					builder.BuildRet(value);
					break;
				}

				case ILOpCode.CompoundCode:
				{
					var opcode2 = (ILCompoundOpCode) ilBytes[++i];
					switch (opcode2)
					{
						case ILCompoundOpCode.Sizeof_tt:
						{
							var token = Unsafe.As<byte, int>(ref ilBytes[++i]); i += 3;
							var type = CompilationHelper.ResolveType(method, token);

							var ilType = GetType(module, type);
							StackPush(new ILValue(typeof(long), ilType.LLVMType.SizeOf));
							break;
						}
						
						case ILCompoundOpCode.Clt:
						{
							var b = StackPop(typeof(int));
							var a = StackPop(typeof(int));
							var cond = builder.BuildICmp(LLVMIntPredicate.LLVMIntSLT, a, b);
							var t = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 1);
							var f = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0);
							var push = builder.BuildSelect(cond, t, f);
							StackPush(new ILValue(typeof(int), push));
							break;
						}
						
						case ILCompoundOpCode.Initobj_tt:
						{
							i += 4;
							var ptr = StackPop(null);
							builder.BuildMemSet(module, ptr, 0);
							break;
						}

						default:
							Console.WriteLine(method.LLVMValue);
							throw new NotImplementedException($"Opcode {opcode2} at IL_{i + block.Start:X4} in method {method.Method} is not yet implemented.");
					}
					break;
				}

				default:
					Console.WriteLine(method.LLVMValue);
					throw new NotImplementedException($"Opcode {opcode} at IL_{i + block.Start:X4} in method {method.Method} is not yet implemented.");
			}
		}
	}
}