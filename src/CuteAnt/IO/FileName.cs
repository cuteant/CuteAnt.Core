//using System;

//namespace CuteAnt.IO
//{
//	/// <summary>
//	/// Represents a directory path or filename.
//	/// The equality operator is overloaded to compare for path equality (case insensitive, normalizing paths with '..\')
//	/// </summary>
//	public sealed class FileName : IEquatable<FileName>
//	{
//		private readonly String normalizedFileName;

//		public FileName(String fileName)
//		{
//			if (fileName == null)
//			{
//				throw new ArgumentNullException("fileName");
//			}
//			if (fileName.Length == 0)
//			{
//				throw new ArgumentException("The empty String is not a valid FileName");
//			}
//			this.normalizedFileName = FileHelper.NormalizePath(fileName);
//		}

//		/// <summary>
//		/// Creates a FileName instance from the String.
//		/// It is valid to pass null or an empty String to this method (in that case, a null reference will be returned).
//		/// </summary>
//		public static FileName Create(String fileName)
//		{
//			if (fileName.IsNullOrWhiteSpace())
//			{
//				return null;
//			}
//			else
//			{
//				return new FileName(fileName);
//			}
//		}

//		public static implicit operator String(FileName fileName)
//		{
//			if (fileName != null)
//			{
//				return fileName.normalizedFileName;
//			}
//			else
//			{
//				return null;
//			}
//		}

//		public override String ToString()
//		{
//			return normalizedFileName;
//		}

//		#region -- Equals and GetHashCode implementation --

//		public override Boolean Equals(object obj)
//		{
//			return Equals(obj as FileName);
//		}

//		public Boolean Equals(FileName other)
//		{
//			if (other != null)
//			{
//				return String.Equals(normalizedFileName, other.normalizedFileName, StringComparison.OrdinalIgnoreCase);
//			}
//			else
//			{
//				return false;
//			}
//		}

//		public override Int32 GetHashCode()
//		{
//			return StringComparer.OrdinalIgnoreCase.GetHashCode(normalizedFileName);
//		}

//		public static Boolean operator ==(FileName left, FileName right)
//		{
//			if (ReferenceEquals(left, right))
//			{
//				return true;
//			}
//			if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
//			{
//				return false;
//			}
//			return left.Equals(right);
//		}

//		public static Boolean operator !=(FileName left, FileName right)
//		{
//			return !(left == right);
//		}

//		[ObsoleteAttribute("Warning: comparing FileName with String results in case-sensitive comparison")]
//		public static Boolean operator ==(FileName left, String right)
//		{
//			return (String)left == right;
//		}

//		[ObsoleteAttribute("Warning: comparing FileName with String results in case-sensitive comparison")]
//		public static Boolean operator !=(FileName left, String right)
//		{
//			return (String)left != right;
//		}

//		[ObsoleteAttribute("Warning: comparing FileName with String results in case-sensitive comparison")]
//		public static Boolean operator ==(String left, FileName right)
//		{
//			return left == (String)right;
//		}

//		[ObsoleteAttribute("Warning: comparing FileName with String results in case-sensitive comparison")]
//		public static Boolean operator !=(String left, FileName right)
//		{
//			return left != (String)right;
//		}

//		#endregion
//	}
//}