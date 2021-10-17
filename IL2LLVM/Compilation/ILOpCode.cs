using System.Diagnostics.CodeAnalysis;

namespace IL2LLVM.Compilation;

[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum ILOpCode
{
	///<summary>Add two values, returning a new value.</summary>
	Add = 0x58,
	
	///<summary>Add signed integer values with overflow check.</summary>
	Add_ovf = 0xD6,
	
	///<summary>Add unsigned integer values with overflow check.</summary>
	Add_ovf_un = 0xD7,
	
	///<summary>Bitwise AND of two integral values, returns an integral value.</summary>
	And = 0x5F,

	///<summary>Branch to target if equal.</summary>
	Beq_i32 = 0x3B,
	
	///<summary>Branch to target if equal, short form.</summary>
	Beq_i8 = 0x2E,
	
	///<summary>Branch to target if greater than or equal to.</summary>
	Bge_i32 = 0x3C,
	
	///<summary>Branch to target if greater than or equal to, short form.</summary>
	Bge_i8 = 0x2F,
	
	///<summary>Branch to target if greater than or equal to (unsigned or unordered).</summary>
	Bge_u32 = 0x41,
	
	///<summary>Branch to target if greater than or equal to (unsigned or unordered), short form.</summary>
	Bge_u8 = 0x34,
	
	///<summary>Branch to target if greater than.</summary>
	Bgt_i32 = 0x3D,
	
	///<summary>Branch to target if greater than, short form.</summary>
	Bgt_i8 = 0x30,
	
	///<summary>Branch to target if greater than (unsigned or unordered).</summary>
	Bgt_u32 = 0x42,
	
	///<summary>Branch to target if greater than (unsigned or unordered), short form.</summary>
	Bgt_un_i8 = 0x35,
	
	///<summary>Branch to target if less than or equal to.</summary>
	Ble_i32 = 0x3E,
	
	///<summary>Branch to target if less than or equal to, short form.</summary>
	Ble_i8 = 0x31,
	
	///<summary>Branch to target if less than or equal to (unsigned or unordered).</summary>
	Ble_u32 = 0x43,
	
	///<summary>Branch to target if less than or equal to (unsigned or unordered), short form.</summary>
	Ble_un_i8 = 0x36,
	
	///<summary>Branch to target if less than.</summary>
	Blt_i32 = 0x3F,
	
	///<summary>Branch to target if less than, short form.</summary>
	Blt_i8 = 0x32,
	
	///<summary>Branch to target if less than (unsigned or unordered).</summary>
	Blt_u32 = 0x44,
	
	///<summary>Branch to target if less than (unsigned or unordered), short form.</summary>
	Blt_un_i8 = 0x37,
	
	///<summary>Branch to target if unequal or unordered.</summary>
	Bne_u32 = 0x40,
	
	///<summary>Branch to target if unequal or unordered, short form.</summary>
	Bne_un_i8 = 0x33,
	
	///<summary>Convert a boxable value to its boxed form.</summary>
	Box_tt= 0x8C,
	
	///<summary>Branch to target.</summary>
	Br_i32 = 0x38,
	
	///<summary>Branch to target, short form.</summary>
	Br_i8 = 0x2B,
	
	///<summary>Inform a debugger that a breakpoint has been reached.</summary>
	Break = 0x01,
	
	///<summary>Branch to target if value is zero (false).</summary>
	Brfalse_i32 = 0x39,
	
	///<summary>Branch to target if value is zero (false), short form.</summary>
	Brfalse_i8 = 0x2C,

	///<summary>Branch to target if value is non-zero (true).</summary>
	Brtrue_i32 = 0x3A,
	
	///<summary>Branch to target if value is non-zero (true), short form.</summary>
	Brtrue_i8 = 0x2D,

	///<summary>Call method described by method.</summary>
	Call_mt = 0x28,

	///<summary>Call a method associated with an object.</summary>
	Callvirt_mt = 0x6F,
	
	///<summary>Cast obj to class.</summary>
	Castclass_tt = 0x74,

	///<summary>Throw ArithmeticException if value is not a finite number.</summary>
	Ckfinite = 0xC3,
	
	///<summary>Convert to native int, pushing native int on stack.</summary>
	Conv_i = 0xD3,
	
	///<summary>Convert to int8, pushing int32 on stack.</summary>
	Conv_i1 = 0x67,
	
	///<summary>Convert to int16, pushing int32 on stack.</summary>
	Conv_i2 = 0x68,
	
	///<summary>Convert to int32, pushing int32 on stack.</summary>
	Conv_i4 = 0x69,
	
	///<summary>Convert to int64, pushing int64 on stack.</summary>
	Conv_i8 = 0x6A,
	
	///<summary>Convert to a native int (on the stack as native int) and throw an exception on overflow.</summary>
	Conv_ovf_i = 0xD4,
	
	///<summary>Convert unsigned to a native int (on the stack as native int) and throw an exception on overflow.</summary>
	Conv_ovf_i_un = 0x8A,
	
	///<summary>Convert to an int8 (on the stack as int32) and throw an exception on overflow.</summary>
	Conv_ovf_i1 = 0xB3,
	
	///<summary>Convert unsigned to an int8 (on the stack as int32) and throw an exception on overflow.</summary>
	Conv_ovf_i1_un = 0x82,
	
	///<summary>Convert to an int16 (on the stack as int32) and throw an exception on overflow.</summary>
	Conv_ovf_i2 = 0xB5,
	
	///<summary>Convert unsigned to an int16 (on the stack as int32) and throw an exception on overflow.</summary>
	Conv_ovf_i2_un = 0x83,
	
	///<summary>Convert to an int32 (on the stack as int32) and throw an exception on overflow.</summary>
	Conv_ovf_i4 = 0xB7,
	
	///<summary>Convert unsigned to an int32 (on the stack as int32) and throw an exception on overflow.</summary>
	Conv_ovf_i4_un = 0x84,
	
	///<summary>Convert to an int64 (on the stack as int64) and throw an exception on overflow.</summary>
	Conv_ovf_i8 = 0xB9,
	
	///<summary>Convert unsigned to an int64 (on the stack as int64) and throw an exception on overflow.</summary>
	Conv_ovf_i8_un = 0x85,
	
	///<summary>Convert to a native unsigned int (on the stack as native int) and throw an exception on overflow.</summary>
	Conv_ovf_u = 0xD5,
	
	///<summary>Convert unsigned to a native unsigned int (on the stack as native int) and throw an exception on overflow.</summary>
	Conv_ovf_u_un = 0x8B,
	
	///<summary>Convert to an unsigned int8 (on the stack as int32) and throw an exception on overflow.</summary>
	Conv_ovf_u1 = 0xB4,
	
	///<summary>Convert unsigned to an unsigned int8 (on the stack as int32) and throw an exception on overflow.</summary>
	Conv_ovf_u1_un = 0x86,
	
	///<summary>Convert to an unsigned int16 (on the stack as int32) and throw an exception on overflow.</summary>
	Conv_ovf_u2 = 0xB6,
	
	///<summary>Convert unsigned to an unsigned int16 (on the stack as int32) and throw an exception on overflow.</summary>
	Conv_ovf_u2_un = 0x87,
	
	///<summary>Convert to an unsigned int32 (on the stack as int32) and throw an exception on overflow.</summary>
	Conv_ovf_u4 = 0xB8,
	
	///<summary>Convert unsigned to an unsigned int32 (on the stack as int32) and throw an exception on overflow.</summary>
	Conv_ovf_u4_un = 0x88,
	
	///<summary>Convert to an unsigned int64 (on the stack as int64) and throw an exception on overflow.</summary>
	Conv_ovf_u8 = 0xBA,
	
	///<summary>Convert unsigned to an unsigned int64 (on the stack as int64) and throw an exception on overflow.</summary>
	Conv_ovf_u8_un = 0x89,
	
	///<summary>Convert unsigned integer to floating-point, pushing F on stack.</summary>
	Conv_r_un = 0x76,
	
	///<summary>Convert to float32, pushing F on stack.</summary>
	Conv_r4 = 0x6B,
	
	///<summary>Convert to float64, pushing F on stack.</summary>
	Conv_r8 = 0x6C,
	
	///<summary>Convert to native unsigned int, pushing native int on stack.</summary>
	Conv_u = 0xE0,
	
	///<summary>Convert to unsigned int8, pushing int32 on stack.</summary>
	Conv_u1 = 0xD2,
	
	///<summary>Convert to unsigned int16, pushing int32 on stack.</summary>
	Conv_u2 = 0xD1,
	
	///<summary>Convert to unsigned int32, pushing int32 on stack.</summary>
	Conv_u4 = 0x6D,
	
	///<summary>Convert to unsigned int64, pushing int64 on stack.</summary>
	Conv_u8 = 0x6E,
	
	///<summary>Copy data from memory to memory.</summary>
	Cpblk = 0x17,
	
	///<summary>Copy a value type from src to dest.</summary>
	Cpobj_tt= 0x70,

	///<summary>Divide two values to return a quotient or floating-point result.</summary>
	Div = 0x5B,
	
	///<summary>Divide two values, unsigned, returning a quotient.</summary>
	Div_un = 0x5C,
	
	///<summary>Duplicate the value on the top of the stack.</summary>
	Dup = 0x25,
	
	///<summary>End fault clause of an exception block.</summary>
	Endfault = 0xDC,

	///<summary>End finally clause of an exception block.</summary>
	Endfinally = 0xDC,

	///<summary>Test if obj is an instance of class, returning null or an instance of that class or interface.</summary>
	Isinst_tt = 0x75,
	
	///<summary>Exit current method and jump to the specified method.</summary>
	Jmp_mt = 0x27,

	///<summary>Load argument 0 onto the stack.</summary>
	Ldarg_0 = 0x02,
	
	///<summary>Load argument 1 onto the stack.</summary>
	Ldarg_1 = 0x03,
	
	///<summary>Load argument 2 onto the stack.</summary>
	Ldarg_2 = 0x04,
	
	///<summary>Load argument 3 onto the stack.</summary>
	Ldarg_3 = 0x05,
	
	///<summary>Load argument numbered num onto the stack, short form.</summary>
	Ldarg_u8 = 0x0E,

	///<summary>Fetch the address of argument argNum, short form.</summary>
	Ldarga_u8 = 0x0F,
	
	///<summary>Push num of type int32 onto the stack as int32.</summary>
	Ldc_i4_i32 = 0x20,
	
	///<summary>Push 0 onto the stack as int32.</summary>
	Ldc_i4_0 = 0x16,
	
	///<summary>Push 1 onto the stack as int32.</summary>
	Ldc_i4_1 = 0x17,
	
	///<summary>Push 2 onto the stack as int32.</summary>
	Ldc_i4_2 = 0x18,
	
	///<summary>Push 3 onto the stack as int32.</summary>
	Ldc_i4_3 = 0x19,
	
	///<summary>Push 4 onto the stack as int32.</summary>
	Ldc_i4_4 = 0x1A,
	
	///<summary>Push 5 onto the stack as int32.</summary>
	Ldc_i4_5 = 0x1B,
	
	///<summary>Push 6 onto the stack as int32.</summary>
	Ldc_i4_6 = 0x1C,
	
	///<summary>Push 7 onto the stack as int32.</summary>
	Ldc_i4_7 = 0x1D,
	
	///<summary>Push 8 onto the stack as int32.</summary>
	Ldc_i4_8 = 0x1E,
	
	///<summary>Push -1 onto the stack as int32.</summary>
	Ldc_i4_m1 = 0x15,

	///<summary>Push num onto the stack as int32, short form.</summary>
	Ldc_i4_i8 = 0x1F,
	
	///<summary>Push num of type int64 onto the stack as int64.</summary>
	Ldc_i8_i64 = 0x21,
	
	///<summary>Push num of type float32 onto the stack as F.</summary>
	Ldc_r4_f32 = 0x22,
	
	///<summary>Push num of type float64 onto the stack as F.</summary>
	Ldc_r8_f64 = 0x23,
	
	///<summary>Load the element at index onto the top of the stack.</summary>
	Ldelem_tt= 0xA3,
	
	///<summary>Load the element with type native int at index onto the top of the stack as a native int.</summary>
	Ldelem_i = 0x97,
	
	///<summary>Load the element with type int8 at index onto the top of the stack as an int32.</summary>
	Ldelem_i1 = 0x90,
	
	///<summary>Load the element with type int16 at index onto the top of the stack as an int32.</summary>
	Ldelem_i2 = 0x92,
	
	///<summary>Load the element with type int32 at index onto the top of the stack as an int32.</summary>
	Ldelem_i4 = 0x94,
	
	///<summary>Load the element with type int64 at index onto the top of the stack as an int64.</summary>
	Ldelem_i8 = 0x96,
	
	///<summary>Load the element with type float32 at index onto the top of the stack as an F.</summary>
	Ldelem_r4 = 0x98,
	
	///<summary>Load the element with type float64 at index onto the top of the stack as an F.</summary>
	Ldelem_r8 = 0x99,
	
	///<summary>Load the element at index onto the top of the stack as an O. The type of the O is the same as the element type of the array pushed on the CIL stack.</summary>
	Ldelem_ref = 0x9A,
	
	///<summary>Load the element with type unsigned int8 at index onto the top of the stack as an int32.</summary>
	Ldelem_u1 = 0x91,
	
	///<summary>Load the element with type unsigned int16 at index onto the top of the stack as an int32.</summary>
	Ldelem_u2 = 0x93,
	
	///<summary>Load the element with type unsigned int32 at index onto the top of the stack as an int32.</summary>
	Ldelem_u4 = 0x95,

	///<summary>Load the address of element at index onto the top of the stack.</summary>
	Ldelema_tt = 0x8F,
	
	///<summary>Push the value of field of object (or value type) obj, onto the stack.</summary>
	Ldfld_ft = 0x7B,
	
	///<summary>Push the address of field of object obj on the stack.</summary>
	Ldflda_ft = 0x7C,

	///<summary>Indirect load value of type native int as native int on the stack.</summary>
	Ldind_i = 0x4D,
	
	///<summary>Indirect load value of type int8 as int32 on the stack.</summary>
	Ldind_i1 = 0x46,
	
	///<summary>Indirect load value of type int16 as int32 on the stack.</summary>
	Ldind_i2 = 0x48,
	
	///<summary>Indirect load value of type int32 as int32 on the stack.</summary>
	Ldind_i4 = 0x4A,
	
	///<summary>Indirect load value of type int64 as int64 on the stack.</summary>
	Ldind_i8 = 0x4C,
	
	///<summary>Indirect load value of type float32 as F on the stack.</summary>
	Ldind_r4 = 0x4E,
	
	///<summary>Indirect load value of type float64 as F on the stack.</summary>
	Ldind_r8 = 0x4F,
	
	///<summary>Indirect load value of type object ref as O on the stack.</summary>
	Ldind_ref = 0x50,
	
	///<summary>Indirect load value of type unsigned int8 as int32 on the stack.</summary>
	Ldind_u1 = 0x47,
	
	///<summary>Indirect load value of type unsigned int16 as int32 on the stack.</summary>
	Ldind_u2 = 0x49,
	
	///<summary>Indirect load value of type unsigned int32 as int32 on the stack.</summary>
	Ldind_u4 = 0x4B,
	
	///<summary>Push the length (of type native unsigned int) of array on the stack.</summary>
	Ldlen = 0x8E,

	///<summary>Load local variable 0 onto stack.</summary>
	Ldloc_0 = 0x06,
	
	///<summary>Load local variable 1 onto stack.</summary>
	Ldloc_1 = 0x07,
	
	///<summary>Load local variable 2 onto stack.</summary>
	Ldloc_2 = 0x08,
	
	///<summary>Load local variable 3 onto stack.</summary>
	Ldloc_3 = 0x09,
	
	///<summary>Load local variable of index indx onto stack, short form.</summary>
	Ldloc_u8= 0x11,

	///<summary>Load address of local variable with index indx, short form.</summary>
	Ldloca_u8= 0x12,
	
	///<summary>Push a null reference on the stack.</summary>
	Ldnull = 0x14,
	
	///<summary>Copy the value stored at address src to the stack.</summary>
	Ldobj_tt= 0x71,
	
	///<summary>Push the value of the static field on the stack.</summary>
	Ldsfld_ft = 0x7E,
	
	///<summary>Push the address of the static field, field, on the stack.</summary>
	Ldsflda_ft = 0x7F,
	
	///<summary>Push a string object for the literal string.</summary>
	Ldstr_str = 0x72,
	
	///<summary>Convert metadata token to its runtime representation.</summary>
	Ldtoken_t = 0xD0,

	///<summary>Exit a protected region of code.</summary>
	Leave_i32 = 0xDD,
	
	///<summary>Exit a protected region of code, short form.</summary>
	Leave_i8 = 0xDE,

	///<summary>Push a typed reference to ptr of type class onto the stack.</summary>
	Mkrefany_tt = 0xC6,
	
	///<summary>Multiply values.</summary>
	Mul = 0x5A,
	
	///<summary>Multiply signed integer values. Signed result shall fit in same size.</summary>
	Mul_ovf = 0xD8,
	
	///<summary>Multiply unsigned integer values. Unsigned result shall fit in same size.</summary>
	Mul_ovf_un = 0xD9,
	
	///<summary>Negate value.</summary>
	Neg = 0x65,
	
	///<summary>Create a new array with elements of type etype.</summary>
	Newarr_tt = 0x8D,
	
	///<summary>Allocate an uninitialized object or value type and call ctor.</summary>
	Newobj_mt = 0x73,

	///<summary>Do nothing (No operation).</summary>
	Nop = 0x00,
	
	///<summary>Bitwise complement (logical not).</summary>
	Not = 0x66,
	
	///<summary>Bitwise OR of two integer values, returns an integer.</summary>
	Or = 0x60,
	
	///<summary>Pop value from the stack.</summary>
	Pop = 0x26,

	///<summary>Remainder when dividing one value by another.</summary>
	Rem = 0x5D,
	
	///<summary>Remainder when dividing one unsigned value by another.</summary>
	Rem_un = 0x5E,
	
	///<summary>Return from method, possibly with a value.</summary>
	Ret = 0x2A,

	///<summary>Shift an integer left (shifting in zeros), return an integer.</summary>
	Shl = 0x62,
	
	///<summary>Shift an integer right (shift in sign), return an integer.</summary>
	Shr = 0x63,
	
	///<summary>Shift an integer right (shift in zero), return an integer.</summary>
	Shr_un = 0x64,

	///<summary>Store value to the argument numbered num, short form.</summary>
	Starg_u8 = 0x10,
	
	///<summary>Replace array element at index with the value on the stack.</summary>
	Stelem_tt= 0xA4,
	
	///<summary>Replace array element at index with the i value on the stack.</summary>
	Stelem_i = 0x9B,
	
	///<summary>Replace array element at index with the int8 value on the stack.</summary>
	Stelem_i1 = 0x9C,
	
	///<summary>Replace array element at index with the int16 value on the stack.</summary>
	Stelem_i2 = 0x9D,
	
	///<summary>Replace array element at index with the int32 value on the stack.</summary>
	Stelem_i4 = 0x9E,
	
	///<summary>Replace array element at index with the int64 value on the stack.</summary>
	Stelem_i8 = 0x9F,
	
	///<summary>Replace array element at index with the float32 value on the stack.</summary>
	Stelem_r4 = 0xA0,
	
	///<summary>Replace array element at index with the float64 value on the stack.</summary>
	Stelem_r8 = 0xA1,
	
	///<summary>Replace array element at index with the ref value on the stack.</summary>
	Stelem_ref = 0xA2,
	
	///<summary>Replace the value of field of the object obj with value.</summary>
	Stfld_ft = 0x7D,
	
	///<summary>Store value of type native int into memory at address.</summary>
	Stind_i = 0xDF,
	
	///<summary>Store value of type int8 into memory at address.</summary>
	Stind_i1 = 0x52,
	
	///<summary>Store value of type int16 into memory at address.</summary>
	Stind_i2 = 0x53,
	
	///<summary>Store value of type int32 into memory at address.</summary>
	Stind_i4 = 0x54,
	
	///<summary>Store value of type int64 into memory at address.</summary>
	Stind_i8 = 0x55,
	
	///<summary>Store value of type float32 into memory at address.</summary>
	Stind_r4 = 0x56,
	
	///<summary>Store value of type float64 into memory at address.</summary>
	Stind_r8 = 0x57,
	
	///<summary>Store value of type object ref (type O) into memory at address.</summary>
	Stind_ref = 0x51,

	///<summary>Pop a value from stack into local variable 0.</summary>
	Stloc_0 = 0x0A,
	
	///<summary>Pop a value from stack into local variable 1.</summary>
	Stloc_1 = 0x0B,
	
	///<summary>Pop a value from stack into local variable 2.</summary>
	Stloc_2 = 0x0C,
	
	///<summary>Pop a value from stack into local variable 3.</summary>
	Stloc_3 = 0x0D,
	
	///<summary>Pop a value from stack into local variable indx, short form.</summary>
	Stloc_u8= 0x13,
	
	///<summary>Store a value of type typeTok at an address.</summary>
	Stobj_tt= 0x81,
	
	///<summary>Replace the value of the static field with val.</summary>
	Stsfld_ft = 0x80,
	
	///<summary>Subtract value2 from value1, returning a new value.</summary>
	Sub = 0x59,
	
	///<summary>Subtract native int from a native int. Signed result shall fit in same size.</summary>
	Sub_ovf = 0xDA,
	
	///<summary>Subtract native unsigned int from a native unsigned int. Unsigned result shall fit in same size.</summary>
	Sub_ovf_un = 0xDB,

	///<summary>Throw an exception.</summary>
	Throw = 0x7A,

	///<summary>Extract a value-type from obj, its boxed representation, and copy to the top of the stack.</summary>
	Unbox_any_tt= 0xA5,

	///<summary>Bitwise XOR of integer values, returns an integer.</summary>
	Xor = 0x61,
	
	CompoundCode = 0xFE,
}

[SuppressMessage("ReSharper", "IdentifierTypo")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum ILCompoundOpCode
{
	///<summary>Return argument list handle for the current method.</summary>
	Arglist = 0x00,
	
	///<summary>Push 1 (of type int32) if value1 lower than value2, else push 0.</summary>
	Clt = 0x04,
	
	///<summary>Push 1 (of type int32) if value1 lower than value2, unsigned or unordered, else push 0.</summary>
	Clt_un = 0x05,
	
	///<summary>Call a virtual method on a type constrained to be type T. </summary>
	Constrained_tt = 0x16,
	
	// ///<summary>Call method indicated on the stack with arguments described by callsitedescr.</summary>
	// Calli <callsitedescr> = 0x29,
	
	///<summary>Push 1 (of type int32) if value1 equals value2, else push 0.</summary>
	Ceq = 0x01,
	
	///<summary>Push 1 (of type int32) if value1 greater that value2, else push 0.</summary>
	Cgt = 0x02,
	
	///<summary>Push 1 (of type int32) if value1 greater that value2, unsigned or unordered, else push 0.</summary>
	Cgt_un = 0x03,
	
	///<summary>End an exception handling filter clause.</summary>
	Endfilter = 0x11,
	
	///<summary>Set all bytes in a block of memory to a given byte value.</summary>
	Initblk = 0x18,
	
	///<summary>Initialize the value at address dest.</summary>
	Initobj_tt= 0x15,
	
	///<summary>Fetch the address of argument argNum.</summary>
	Ldarga_u16 = 0x0A,
	
	///<summary>Load argument numbered num onto the stack.</summary>
	Ldarg_u16 = 0x09,
	
	///<summary>Push a pointer to a method referenced by method, on the stack.</summary>
	Ldftn_mt = 0x06,
	
	///<summary>Load local variable of index indx onto stack.</summary>
	Ldloc_u16 = 0x0C,
	
	///<summary>Load address of local variable with index indx.</summary>
	Ldloca_u16 = 0x0D,
	
	///<summary>Push address of virtual method on the stack.</summary>
	Ldvirtftn_mt = 0x07,
	
	// ///<summary>The specified fault check(s) normally performed as part of the execution of the subsequent instruction can/shall be skipped.</summary>	Prefix to instruction
	// No_ {typecheck, rangecheck,nullcheck} = 0x19,
	
	///<summary>Specify that the subsequent array address operation performs no type check at runtime, and that it returns a controlled-mutability managed pointer.</summary>	Prefix to instruction
	readonly_ = 0x1E,
	
	///<summary>Push the type token stored in a typed reference.</summary>
	Refanytype = 0x1D,
	
	///<summary>Push the address stored in a typed reference.</summary>
	Refanyval_tt = 0xC2,
	
	///<summary>Allocate space from the local memory pool.</summary>
	Localloc = 0x0F,
	
	///<summary>Rethrow the current exception.</summary>
	Rethrow = 0x1A,
	
	///<summary>Push the size, in bytes, of a type as an unsigned int32.</summary>
	Sizeof_tt= 0x1C,
	
	///<summary>Store value to the argument numbered num.</summary>
	Starg_u16 = 0x0B,
	
	///<summary>Pop a value from stack into local variable indx.</summary>
	Stloc_u16 = 0x0E,
	
	// ///<summary>Jump to one of n values.</summary>
	// Switch <uint32, int32, int32 (t1__tN)> = 0x45,
	
	///<summary>Subsequent call terminates current method.</summary>	Prefix to instruction
	Tail_ = 0x14,
	
	// ///<summary>Subsequent pointer instruction might be unaligned.</summary>	Prefix to instruction
	// Unaligned_ (alignment) = 0x12,
	
	///<summary>Extract a value-type from obj, its boxed representation, and push a controlled-mutability managed pointer to it to the top of the stack.</summary>
	Unbox_tt = 0x79,
	
	///<summary>Subsequent pointer reference is volatile.</summary>	Prefix to instruction
	volatile_ = 0x13,
}