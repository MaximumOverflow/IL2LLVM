# IL2LLVM
IL2LLVM is an **experimental** transpiler aimed at translating Microsoft's CIL into optimised LLVM IR code.  
Currently, this project aims to reach feature parity with Unity's Burst compiler, meaning it should be able to compile function exclusively relying on unmanaged types.

### Please note
**The transpiler is still in its very early stages, only a very limited subset of CIL's opcodes are implemented and some don't behave as one would expect.**
**The project's APIs and general design are far from final, this is only a proof of concept for the sole purpose of experimentation.**
