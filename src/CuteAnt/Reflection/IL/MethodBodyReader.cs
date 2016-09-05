/*
 * 作者：新生命开发团队（http://www.newlifex.com/）
 * 
 * 版权：版权所有 (C) 新生命开发团队 2002-2014
 * 
 * 修改：海洋饼干（cuteant@outlook.com）
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace CuteAnt.Reflection
{
	/// <summary>方法体读取器</summary>
	public class MethodBodyReader
	{
		#region -- 属性 --

		private MethodInfo _Method;

		/// <summary>方法</summary>
		public MethodInfo Method
		{
			get { return _Method; }

			//set { _Method = value; }
		}

		private List<ILInstruction> _Instructions;

		/// <summary>指令集合</summary>
		public List<ILInstruction> Instructions
		{
			get
			{
				if (_Instructions == null)
				{
					_Instructions = ConstructInstructions(Method);
				}
				return _Instructions;
			}

			//private set { _Instructions = value; }
		}

		//public Byte[] il = null;

		#endregion

		#region -- 构造函数 --

		/// <summary>为方法信息创建方法体读取器</summary>
		/// <param name="method"></param>
		public MethodBodyReader(MethodInfo method)
		{
			ValidationHelper.ArgumentNull(method, "method");
			_Method = method;

			//if (mi.GetMethodBody() != null)
			//{
			//    il = mi.GetMethodBody().GetILAsByteArray();
			//    ConstructInstructions(mi.Module);
			//}
		}

		#endregion

		#region -- IL读取方法 --

		private static UInt16 ReadUInt16(Byte[] il, ref Int32 p)
		{
			return (UInt16)(il[p++] | (il[p++] << 8));
		}

		private static Int32 ReadInt32(Byte[] il, ref Int32 p)
		{
			return ((il[p++] | (il[p++] << 8)) | (il[p++] << 0x10)) | (il[p++] << 0x18);
		}

		private static UInt64 ReadInt64(Byte[] il, ref Int32 p)
		{
			return (UInt64)(((il[p++] | (il[p++] << 8)) | (il[p++] << 0x10)) | (il[p++] << 0x18) | (il[p++] << 0x20) | (il[p++] << 0x28) | (il[p++] << 0x30) | (il[p++] << 0x38));
		}

		private static Double ReadDouble(Byte[] il, ref Int32 p)
		{
			return (((il[p++] | (il[p++] << 8)) | (il[p++] << 0x10)) | (il[p++] << 0x18) | (il[p++] << 0x20) | (il[p++] << 0x28) | (il[p++] << 0x30) | (il[p++] << 0x38));
		}

		private static SByte ReadSByte(Byte[] il, ref Int32 p)
		{
			return (SByte)il[p++];
		}

		private static Byte ReadByte(Byte[] il, ref Int32 p)
		{
			return (Byte)il[p++];
		}

		private static Single ReadSingle(Byte[] il, ref Int32 p)
		{
			return (Single)(((il[p++] | (il[p++] << 8)) | (il[p++] << 0x10)) | (il[p++] << 0x18));
		}

		#endregion

		#region -- 方法 --

		/// <summary>通过IL字节码构建指令集合</summary>
		/// <param name="mi"></param>
		private static List<ILInstruction> ConstructInstructions(MethodInfo mi)
		{
			List<ILInstruction> list = new List<ILInstruction>();
			MethodBody body = mi.GetMethodBody();
			if (body == null) { return list; }
			Byte[] il = body.GetILAsByteArray();
			Module module = mi.Module;
			LoadOpCodes();
			Int32 p = 0;

			while (p < il.Length)
			{
				ILInstruction instruction = new ILInstruction();

				// 当前指令的操作码
				OpCode code = OpCodes.Nop;
				UInt16 value = il[p++];
				if (value != 0xfe)
				{
					code = singleByteOpCodes[(Int32)value];
				}
				else
				{
					value = il[p++];
					code = multiByteOpCodes[(Int32)value];
					value = (UInt16)(value | 0xfe00);
				}
				instruction.Code = code;
				instruction.Offset = p - 1;
				Int32 metadataToken = 0;

				#region 操作数

				switch (code.OperandType)
				{
					case OperandType.InlineBrTarget:
						metadataToken = ReadInt32(il, ref p);
						metadataToken += p;
						instruction.Operand = metadataToken;
						break;

					case OperandType.InlineField:
						metadataToken = ReadInt32(il, ref p);
						instruction.Operand = module.ResolveField(metadataToken);
						break;

					case OperandType.InlineMethod:
						metadataToken = ReadInt32(il, ref p);

						try
						{
							instruction.Operand = module.ResolveMethod(metadataToken);
						}
						catch
						{
							instruction.Operand = module.ResolveMember(metadataToken);
						}
						break;

					case OperandType.InlineSig:
						metadataToken = ReadInt32(il, ref p);
						instruction.Operand = module.ResolveSignature(metadataToken);
						break;

					case OperandType.InlineTok:
						metadataToken = ReadInt32(il, ref p);

						// SSS : see what to do here
						break;

					case OperandType.InlineType:
						metadataToken = ReadInt32(il, ref p);
						instruction.Operand = module.ResolveType(metadataToken);
						break;

					case OperandType.InlineI:
						instruction.Operand = ReadInt32(il, ref p);
						break;

					case OperandType.InlineI8:
						instruction.Operand = ReadInt64(il, ref p);
						break;

					case OperandType.InlineNone:
						instruction.Operand = null;
						break;

					case OperandType.InlineR:
						instruction.Operand = ReadDouble(il, ref p);
						break;

					case OperandType.InlineString:
						metadataToken = ReadInt32(il, ref p);
						instruction.Operand = module.ResolveString(metadataToken);
						break;

					case OperandType.InlineSwitch:
						Int32 count = ReadInt32(il, ref p);
						Int32[] casesAddresses = new Int32[count];

						for (Int32 i = 0; i < count; i++)
						{
							casesAddresses[i] = ReadInt32(il, ref p);
						}
						Int32[] cases = new Int32[count];

						for (Int32 i = 0; i < count; i++)
						{
							cases[i] = p + casesAddresses[i];
						}
						break;

					case OperandType.InlineVar:
						instruction.Operand = ReadUInt16(il, ref p);
						break;

					case OperandType.ShortInlineBrTarget:
						instruction.Operand = ReadSByte(il, ref p) + p;
						break;

					case OperandType.ShortInlineI:
						instruction.Operand = ReadSByte(il, ref p);
						break;

					case OperandType.ShortInlineR:
						instruction.Operand = ReadSingle(il, ref p);
						break;

					case OperandType.ShortInlineVar:
						instruction.Operand = ReadByte(il, ref p);
						break;

					default:
						throw new InvalidOperationException("未知的操作类型" + code.OperandType);
				}

				#endregion

				list.Add(instruction);
			}
			return list;
		}

		/// <summary>获取方法体IL代码</summary>
		/// <returns></returns>
		public String GetBodyCode()
		{
			if (Instructions == null || Instructions.Count < 1) { return null; }
			StringBuilder sb = new StringBuilder();

			for (Int32 i = 0; i < Instructions.Count; i++)
			{
				sb.AppendLine(Instructions[i].ToString());
			}
			return sb.ToString();
		}

		/// <summary>获取方法IL代码，包括签名</summary>
		/// <returns></returns>
		public String GetCode()
		{
			//TODO: 获取方法IL代码，包括签名
			throw new NotImplementedException("未实现！");
		}

		#endregion

		#region -- 辅助 --

		private static OpCode[] multiByteOpCodes;
		private static OpCode[] singleByteOpCodes;

		/// <summary>加载操作码</summary>
		private static void LoadOpCodes()
		{
			if (singleByteOpCodes != null) { return; }
			singleByteOpCodes = new OpCode[0x100];
			multiByteOpCodes = new OpCode[0x100];

			foreach (var fi in typeof(OpCodes).GetFields())
			{
				if (fi.FieldType == typeof(OpCode))
				{
					//OpCode code = (OpCode)FieldInfoX.Create(fi).GetValue(null);
					var code = (OpCode)"".GetFieldInfoValue(fi);
					UInt16 index = (UInt16)code.Value;
					if (index < 0x100)
					{
						singleByteOpCodes[(Int32)index] = code;
					}
					else
					{
						ValidationHelper.InvalidOperationCondition((index & 0xff00) != 0xfe00, "无效操作码");
						multiByteOpCodes[index & 0xff] = code;
					}
				}
			}
		}

		#endregion
	}
}