/*
 * ���ߣ������������Ŷӣ�http://www.newlifex.com/��
 * 
 * ��Ȩ����Ȩ���� (C) �����������Ŷ� 2002-2014
 * 
 * �޸ģ�������ɣ�cuteant@outlook.com��
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace CuteAnt.Reflection
{
	/// <summary>�������ȡ��</summary>
	public class MethodBodyReader
	{
		#region -- ���� --

		private MethodInfo _Method;

		/// <summary>����</summary>
		public MethodInfo Method
		{
			get { return _Method; }

			//set { _Method = value; }
		}

		private List<ILInstruction> _Instructions;

		/// <summary>ָ���</summary>
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

		#region -- ���캯�� --

		/// <summary>Ϊ������Ϣ�����������ȡ��</summary>
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

		#region -- IL��ȡ���� --

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

		#region -- ���� --

		/// <summary>ͨ��IL�ֽ��빹��ָ���</summary>
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

				// ��ǰָ��Ĳ�����
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

				#region ������

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
						throw new InvalidOperationException("δ֪�Ĳ�������" + code.OperandType);
				}

				#endregion

				list.Add(instruction);
			}
			return list;
		}

		/// <summary>��ȡ������IL����</summary>
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

		/// <summary>��ȡ����IL���룬����ǩ��</summary>
		/// <returns></returns>
		public String GetCode()
		{
			//TODO: ��ȡ����IL���룬����ǩ��
			throw new NotImplementedException("δʵ�֣�");
		}

		#endregion

		#region -- ���� --

		private static OpCode[] multiByteOpCodes;
		private static OpCode[] singleByteOpCodes;

		/// <summary>���ز�����</summary>
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
					var code = (OpCode)"".GetValue(fi);
					UInt16 index = (UInt16)code.Value;
					if (index < 0x100)
					{
						singleByteOpCodes[(Int32)index] = code;
					}
					else
					{
						ValidationHelper.InvalidOperationCondition((index & 0xff00) != 0xfe00, "��Ч������");
						multiByteOpCodes[index & 0xff] = code;
					}
				}
			}
		}

		#endregion
	}
}