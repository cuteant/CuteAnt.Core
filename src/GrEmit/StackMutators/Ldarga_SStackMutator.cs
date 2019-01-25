﻿using GrEmit.InstructionParameters;

namespace GrEmit.StackMutators
{
    internal class Ldarga_SStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            var index = (byte)((PrimitiveILInstructionParameter)parameter).Value;
            stack.Push(il.methodParameterTypes[index].MakeByRefType());
        }
    }
}