#if !NO_RUNTIME
using System;
using CuteAnt;
#if FEAT_IKVM
using Type = IKVM.Reflection.Type;
#endif


namespace ProtoBuf.Serializers
{
	sealed class CombGuidSerializer : IProtoSerializer
	{
#if FEAT_IKVM
		readonly Type expectedType;
#else
		static readonly Type expectedType = typeof(CombGuid);
#endif
		public CombGuidSerializer(ProtoBuf.Meta.TypeModel model)
		{
#if FEAT_IKVM
			expectedType = model.MapType(typeof(CombGuid));
#endif
		}

		public Type ExpectedType { get { return expectedType; } }

		bool IProtoSerializer.RequiresOldValue { get { return false; } }
		bool IProtoSerializer.ReturnsValue { get { return true; } }

#if !FEAT_IKVM
		public void Write(object value, ProtoWriter dest)
		{
			BclHelpers.WriteCombGuid((CombGuid)value, dest);
		}
		public object Read(object value, ProtoReader source)
		{
			Helpers.DebugAssert(value == null); // since replaces
			return BclHelpers.ReadCombGuid(source);
		}
#endif

#if FEAT_COMPILER
		void IProtoSerializer.EmitWrite(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
		{
			ctx.EmitWrite(ctx.MapType(typeof(BclHelpers)), "WriteCombGuid", valueFrom);
		}
		void IProtoSerializer.EmitRead(Compiler.CompilerContext ctx, Compiler.Local valueFrom)
		{
			ctx.EmitBasicRead(ctx.MapType(typeof(BclHelpers)), "ReadCombGuid", ExpectedType);
		}
#endif

	}
}
#endif