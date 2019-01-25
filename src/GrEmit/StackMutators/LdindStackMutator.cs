﻿using GrEmit.InstructionParameters;
using GrEmit.Utils;

namespace GrEmit.StackMutators
{
    internal class LdindStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            var type = ((TypeILInstructionParameter)parameter).Type;
            CheckNotEmpty(il, stack, () => "In order to perform the 'ldind' instruction load an address onto the evaluation stack");
            var esType = stack.Pop();
            CheckIsAPointer(il, esType);
            var pointer = esType.ToType();
            if (pointer.IsByRef)
            {
                var elementType = pointer.GetElementType();
                if (elementType.IsValueType)
                    CheckCanBeAssigned(il, type.MakeByRefType(), pointer);
                else
                    CheckCanBeAssigned(il, type, elementType);
            }
            else if (pointer.IsPointer)
            {
                var elementType = pointer.GetElementType();
                if (elementType.IsValueType)
                    CheckCanBeAssigned(il, type.MakePointerType(), pointer);
                else
                    CheckCanBeAssigned(il, type, elementType);
            }
            else if (!type.IsPrimitive && type != typeof(object))
                ThrowError(il, $"Unable to load an instance of type '{Formatter.Format(type)}' from a pointer of type '{Formatter.Format(pointer)}' indirectly");
            stack.Push(type);
        }
    }
}