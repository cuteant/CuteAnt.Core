using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CuteAnt.IO
{
	/// <summary>文件资源</summary>
	public static class FileSource
	{
		#region -- method ReleaseFile --

		/// <summary>释放文件</summary>
		/// <param name="asm"></param>
		/// <param name="filenName"></param>
		/// <param name="dest"></param>
		/// <param name="overWrite"></param>
		public static void ReleaseFile(this Assembly asm, String filenName, String dest, Boolean overWrite)
		{
			if (filenName.IsNullOrWhiteSpace()) { return; }
			if (asm == null)
			{
				asm = Assembly.GetCallingAssembly();
			}
			var stream = GetFileResource(asm, filenName);
			if (stream == null)
			{
				throw new ArgumentException("filenName", String.Format("在程序集{0}中无法找到名为{1}的资源！", asm.GetName().Name, filenName));
			}
			if (dest.IsNullOrWhiteSpace())
			{
				dest = filenName;
			}
			if (!Path.IsPathRooted(dest))
			{
				var str = PathHelper.ApplicationBasePath;

				//dest = Path.Combine(str, dest);
				dest = Path.GetFullPath(Path.Combine(str, dest));
			}
			if (File.Exists(dest) && !overWrite) { return; }

			//String path = Path.GetDirectoryName(dest);
			//if (!path.IsNullOrWhiteSpace() && !Directory.Exists(path))
			//{
			//	Directory.CreateDirectory(path);
			//}
			dest.EnsureDirectory(true);

			try
			{
				if (File.Exists(dest))
				{
					File.Delete(dest);
				}

				using (var fs = File.Create(dest))
				{
					stream.CopyTo(fs, 4096);
				}
			}
			catch { }
			finally { stream.Dispose(); }
		}

		#endregion

		#region -- method ReleaseFolder --

		/// <summary>释放文件夹</summary>
		/// <param name="asm"></param>
		/// <param name="prefix"></param>
		/// <param name="dest"></param>
		/// <param name="overWrite"></param>
		public static void ReleaseFolder(this Assembly asm, String prefix, String dest, Boolean overWrite)
		{
			//if (asm == null)
			//{
			//  asm = Assembly.GetCallingAssembly();
			//}
			ReleaseFolder(asm, prefix, dest, overWrite, null);
		}

		/// <summary>释放文件夹</summary>
		/// <param name="asm"></param>
		/// <param name="prefix"></param>
		/// <param name="dest"></param>
		/// <param name="overWrite"></param>
		/// <param name="filenameResolver"></param>
		public static void ReleaseFolder(this Assembly asm, String prefix, String dest, Boolean overWrite, Func<String, String> filenameResolver)
		{
			if (asm == null)
			{
				asm = Assembly.GetCallingAssembly();
			}

			// 找到符合条件的资源
			String[] names = asm.GetManifestResourceNames();
			if (names == null || names.Length < 1) { return; }
			IEnumerable<String> ns = null;
			if (prefix.IsNullOrWhiteSpace())
			{
				ns = names.AsEnumerable();
			}
			else
			{
				ns = names.Where(e => e.StartsWithIgnoreCase(prefix));
			}
			if (dest.IsNullOrWhiteSpace())
			{
				dest = PathHelper.ApplicationBasePath;
			}

			// 开始处理
			foreach (var item in ns)
			{
				var stream = asm.GetManifestResourceStream(item);

				// 计算filenName
				String filenName = null;

				// 去掉前缀
				if (filenameResolver != null)
				{
					filenName = filenameResolver(item);
				}
				if (filenName.IsNullOrWhiteSpace())
				{
					filenName = item;
					if (!prefix.IsNullOrWhiteSpace())
					{
						filenName = filenName.Substring(prefix.Length);
					}
					if (filenName[0] == '.')
					{
						filenName = filenName.Substring(1);
					}
					var ext = Path.GetExtension(item);
					filenName = filenName.Substring(0, filenName.Length - ext.Length);
					filenName = filenName.Replace(".", @"\") + ext;

					//filenName = Path.Combine(dest, filenName);
					filenName = Path.Combine(dest, filenName);
				}
				if (File.Exists(filenName) && !overWrite) { return; }

				//String path = Path.GetDirectoryName(filenName);
				//if (!path.IsNullOrWhiteSpace() && !Directory.Exists(path))
				//{
				//	Directory.CreateDirectory(path);
				//}
				filenName.EnsureDirectory(true);

				try
				{
					if (File.Exists(filenName))
					{
						File.Delete(filenName);
					}
					using (var fs = File.Create(filenName))
					{
						stream.CopyTo(fs, 4096);
					}
				}
				catch { }
				finally { stream.Dispose(); }
			}
		}

		#endregion

		#region -- method FileExists --

		/// <summary>判断指定的资源文件是否存在</summary>
		/// <param name="asm"></param>
		/// <param name="filenName"></param>
		/// <returns></returns>
		public static String FileExists(this Assembly asm, String filenName)
		{
			ValidationHelper.ArgumentNull(asm, "asm");
			ValidationHelper.ArgumentNullOrEmpty(filenName, "filenName");

			String[] ss = asm.GetManifestResourceNames();
			if (ss != null && ss.Length > 0)
			{
				String name = String.Empty;

				//找到资源名
				name = ss.FirstOrDefault(e => e == filenName);
				if (name.IsNullOrWhiteSpace())
				{
					name = ss.FirstOrDefault(e => e.EqualIgnoreCase(filenName));
				}
				if (name.IsNullOrWhiteSpace())
				{
					name = ss.FirstOrDefault(e => e.EndsWith(filenName));
				}
				if (!name.IsNullOrWhiteSpace())
				{
					return name;
				}
			}
			return null;
		}

		#endregion

		#region -- method GetFileResource --

		/// <summary>获取文件资源</summary>
		/// <param name="asm"></param>
		/// <param name="filenName"></param>
		/// <param name="checkFileExist"></param>
		/// <returns></returns>
		public static Stream GetFileResource(this Assembly asm, String filenName, Boolean checkFileExist = true)
		{
			if (filenName.IsNullOrWhiteSpace()) { return null; }

			//String name = String.Empty;
			if (asm == null)
			{
				asm = Assembly.GetCallingAssembly();
			}

			//String[] ss = asm.GetManifestResourceNames();
			//if (ss != null && ss.Length > 0)
			//{
			//  // 找到资源名
			//  name = ss.FirstOrDefault(e => e == filenName);
			//  if (name.IsNullOrWhiteSpace())
			//  {
			//    name = ss.FirstOrDefault(e => e.EqualIgnoreCase(filenName));
			//  }
			//  if (name.IsNullOrWhiteSpace())
			//  {
			//    name = ss.FirstOrDefault(e => e.EndsWith(filenName));
			//  }
			//  if (!name.IsNullOrWhiteSpace())
			//  {
			//    return asm.GetManifestResourceStream(name);
			//  }
			//}
			//return null;
			if (checkFileExist)
			{
				var name = FileExists(asm, filenName);
				return (!name.IsNullOrWhiteSpace()) ? asm.GetManifestResourceStream(name) : null;
			}
			else
			{
				return asm.GetManifestResourceStream(filenName);
			}
		}

		/// <summary>获取文件资源</summary>
		/// <param name="asm"></param>
		/// <param name="type">类型</param>
		/// <param name="filenName"></param>
		/// <returns></returns>
		public static Stream GetFileResource(this Assembly asm, Type type, String filenName)
		{
			if (type == null) { return null; }
			if (filenName.IsNullOrWhiteSpace()) { return null; }
			if (asm == null)
			{
				asm = Assembly.GetCallingAssembly();
			}
			Stream s;
			try
			{
				s = asm.GetManifestResourceStream(type, filenName);
			}
			catch { s = null; }
			return s;
		}

		#endregion

		#region -- method GetBitmap --

		/// <summary>获取资源图片</summary>
		/// <param name="asm"></param>
		/// <param name="filenName"></param>
		/// <param name="checkFileExist"></param>
		/// <returns></returns>
		public static Bitmap GetBitmap(this Assembly asm, String filenName, Boolean checkFileExist = true)
		{
			var s = GetFileResource(asm, filenName, checkFileExist);
			return (s != null) ? new Bitmap(s) : null;
		}

		/// <summary>获取资源图片</summary>
		/// <param name="asm"></param>
		/// <param name="type">类型</param>
		/// <param name="filenName"></param>
		/// <returns></returns>
		public static Bitmap GetBitmap(this Assembly asm, Type type, String filenName)
		{
			var s = GetFileResource(asm, type, filenName);
			return (s != null) ? new Bitmap(s) : null;
		}

		#endregion

		#region -- method GetImage --

		/// <summary>获取资源图片</summary>
		/// <param name="asm"></param>
		/// <param name="filenName"></param>
		/// <param name="checkFileExist"></param>
		/// <returns></returns>
		public static Image GetImage(this Assembly asm, String filenName, Boolean checkFileExist = true)
		{
			var s = GetFileResource(asm, filenName, checkFileExist);
			return (s != null) ? Image.FromStream(s) : null;
		}

		/// <summary>获取资源图片</summary>
		/// <param name="asm"></param>
		/// <param name="type">类型</param>
		/// <param name="filenName"></param>
		/// <returns></returns>
		public static Image GetImage(this Assembly asm, Type type, String filenName)
		{
			var s = GetFileResource(asm, type, filenName);
			return (s != null) ? Image.FromStream(s) : null;
		}

		#endregion

		#region -- method GetIcon --

		/// <summary>获取资源Icon</summary>
		/// <param name="asm"></param>
		/// <param name="filenName"></param>
		/// <param name="checkFileExist"></param>
		/// <returns></returns>
		public static Icon GetIcon(this Assembly asm, String filenName, Boolean checkFileExist = true)
		{
			return GetIcon(asm, filenName, 0, 0, checkFileExist);
		}

		/// <summary>获取资源Icon</summary>
		/// <param name="asm"></param>
		/// <param name="filenName"></param>
		/// <param name="size"></param>
		/// <param name="checkFileExist"></param>
		/// <returns></returns>
		public static Icon GetIcon(this Assembly asm, String filenName, Size size, Boolean checkFileExist = true)
		{
			return GetIcon(asm, filenName, size.Width, size.Width, checkFileExist);
		}

		/// <summary>获取资源Icon</summary>
		/// <param name="asm"></param>
		/// <param name="filenName"></param>
		/// <param name="sizeWH"></param>
		/// <param name="checkFileExist"></param>
		/// <returns></returns>
		public static Icon GetIcon(this Assembly asm, String filenName, Int32 sizeWH, Boolean checkFileExist = true)
		{
			return GetIcon(asm, filenName, sizeWH, sizeWH, checkFileExist);
		}

		/// <summary>获取资源Icon</summary>
		/// <param name="asm"></param>
		/// <param name="filenName"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="checkFileExist"></param>
		/// <returns></returns>
		public static Icon GetIcon(this Assembly asm, String filenName, Int32 width, Int32 height, Boolean checkFileExist = true)
		{
			var s = GetFileResource(asm, filenName, checkFileExist);
			return (s != null) ? new Icon(s, width, height) : null;
		}

		/// <summary>获取资源Icon</summary>
		/// <param name="asm"></param>
		/// <param name="type">类型</param>
		/// <param name="filenName"></param>
		/// <returns></returns>
		public static Icon GetIcon(this Assembly asm, Type type, String filenName)
		{
			return GetIcon(asm, type, filenName, 0, 0);
		}

		/// <summary>获取资源Icon</summary>
		/// <param name="asm"></param>
		/// <param name="type">类型</param>
		/// <param name="filenName"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Icon GetIcon(this Assembly asm, Type type, String filenName, Size size)
		{
			return GetIcon(asm, type, filenName, size.Width, size.Height);
		}

		/// <summary>获取资源Icon</summary>
		/// <param name="asm"></param>
		/// <param name="type">类型</param>
		/// <param name="filenName"></param>
		/// <param name="sizeWH"></param>
		/// <returns></returns>
		public static Icon GetIcon(this Assembly asm, Type type, String filenName, Int32 sizeWH)
		{
			return GetIcon(asm, type, filenName, sizeWH, sizeWH);
		}

		/// <summary>获取资源Icon</summary>
		/// <param name="asm"></param>
		/// <param name="type">类型</param>
		/// <param name="filenName"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public static Icon GetIcon(this Assembly asm, Type type, String filenName, Int32 width, Int32 height)
		{
			var s = GetFileResource(asm, type, filenName);
			return (s != null) ? new Icon(s, width, height) : null;
		}

		#endregion
	}
}