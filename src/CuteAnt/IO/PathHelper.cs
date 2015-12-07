using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
#if !NET40
using System.Runtime.CompilerServices;
#endif

namespace CuteAnt.IO
{
	/// <summary>路径操作帮助</summary>
	public static class PathHelper
	{
		#region -- Fields --

		private static readonly String _EntryAssemblyLocation;
		private static readonly String _ExecutingAssemblyLocation;
		private static readonly String _AppDomainBaseDirectory;

		#endregion

		#region -- Properties Implementation --

		/// <summary>返回应用程序执行路径</summary>
		public static String ApplicationStartupPath
		{
			get { return _EntryAssemblyLocation; }
		}

		/// <summary>返回应用程序执行路径，同ApplicationStartupPath</summary>
		public static String AppDomainBaseDirectory
		{
			get { return _AppDomainBaseDirectory; }
		}

		/// <summary>返回当前程序集(CuteAnt.dll)所在路径</summary>
		public static String ExecutingAssemblyLocation
		{
			get { return _ExecutingAssemblyLocation; }
		}

		#endregion

		#region -- Constructors --

		static PathHelper()
		{
			// 返回应用程序执行路径
			var entryAssembly = Assembly.GetEntryAssembly();
			_AppDomainBaseDirectory = Runtime.IsWeb ? HttpRuntime.BinDirectory : AppDomain.CurrentDomain.BaseDirectory;
			if (entryAssembly != null)
			{
				_EntryAssemblyLocation = Runtime.IsWeb ? HttpRuntime.BinDirectory : Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + Path.DirectorySeparatorChar;
			}
			else
			{
				_EntryAssemblyLocation = _AppDomainBaseDirectory;
			}

			// 返回当前程序集(CuteAnt.dll)所在路径
			_ExecutingAssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar;
		}

		#endregion

		#region -- method PathFix --

		/// <summary>Fixes path separator, replaces / \ with platform separator Char.</summary>
		/// <param name="path">Path to fix.</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static String PathFix(String path)
		{
			return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
		}

		#endregion

		#region -- method PathCombineFix --

		/// <summary>将两个字符串组合成一个路径</summary>
		/// <param name="path1"></param>
		/// <param name="path2"></param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static String PathCombineFix(String path1, String path2)
		{
			//String path;
#if !NET_3_5_GREATER
			return Combine(path1, path2);
#else
			return Path.Combine(path1, path2);
#endif

			//return Path.GetFullPath(PathFix(path));
		}

		/// <summary>将三个字符串组合成一个路径</summary>
		/// <param name="path1"></param>
		/// <param name="path2"></param>
		/// <param name="path3"></param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static String PathCombineFix(String path1, String path2, String path3)
		{
			//String path;
#if !NET_3_5_GREATER
			return Combine(path1, path2, path3);
#else
			return Path.Combine(path1, path2, path3);
#endif

			//return Path.GetFullPath(PathFix(path));
		}

		/// <summary>将四个字符串组合成一个路径</summary>
		/// <param name="path1"></param>
		/// <param name="path2"></param>
		/// <param name="path3"></param>
		/// <param name="path4"></param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static String PathCombineFix(String path1, String path2, String path3, String path4)
		{
			//String path;
#if !NET_3_5_GREATER
			return Combine(path1, path2, path3, path4);
#else
			return Path.Combine(path1, path2, path3, path4);
#endif

			//return Path.GetFullPath(PathFix(path));
		}

		/// <summary>将字符串数组组合成一个路径</summary>
		/// <param name="paths"></param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static String PathCombineFix(params String[] paths)
		{
			//String path;
#if !NET_3_5_GREATER
			return Combine(paths);
#else
			return Path.Combine(paths);
#endif

			//return Path.GetFullPath(PathFix(path));
		}

		#endregion

		#region -- method ApplicationStartupPathCombine --

#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static String ApplicationStartupPathCombine(String path1)
		{
			return Path.GetFullPath(PathCombineFix(AppDomainBaseDirectory, path1));
		}

#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static String ApplicationStartupPathCombine(String path1, String path2)
		{
			return Path.GetFullPath(PathCombineFix(AppDomainBaseDirectory, path1, path2));
		}

#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static String ApplicationStartupPathCombine(String path1, String path2, String path3)
		{
			return Path.GetFullPath(PathCombineFix(AppDomainBaseDirectory, path1, path2, path3));
		}

#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static String ApplicationStartupPathCombine(params String[] paths)
		{
			ValidationHelper.ArgumentNull(paths, "paths");

			var list = new List<String>(paths.Length + 1);
			list.Add(AppDomainBaseDirectory);
			list.AddRange(paths);
			return Path.GetFullPath(PathCombineFix(list.ToArray()));
		}

		#endregion

		#region -- method DeleteDirectoryFiles --

		/// <summary>删除指定目录的所有文件和子目录</summary>
		/// <param name="TargetDir">操作目录</param>
		/// <param name="delSubDir">如果为true,包含对子目录的操作</param>
		public static void DeleteDirectoryFiles(String TargetDir, Boolean delSubDir)
		{
			foreach (String fileName in Directory.GetFiles(TargetDir))
			{
				File.SetAttributes(fileName, FileAttributes.Normal);
				File.Delete(fileName);
			}
			if (delSubDir)
			{
				DirectoryInfo dir = new DirectoryInfo(TargetDir);

				foreach (DirectoryInfo subDi in dir.GetDirectories())
				{
					DeleteDirectoryFiles(subDi.FullName, true);
					subDi.Delete();
				}
			}
		}

		#endregion

		#region -- method DirectoryExists --

		/// <summary>
		/// Checks if directory exists. If linux, checks with case-insenstively (linux is case-sensitive).
		/// Returns actual dir (In linux it may differ from requested directory, because of case-sensitivity.)
		/// or null if directory doesn't exist.
		/// </summary>
		/// <param name="dirName">Directory to check.</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static String DirectoryExists(String dirName)
		{
			// Windows we can use Directory.Exists
			if (Environment.OSVersion.Platform.ToString().ToLower().IndexOf("win") > -1)
			{
				if (Directory.Exists(dirName))
				{
					return dirName;
				}
			}

			// Unix,Linux we can't trust Directory.Exists value because of case-sensitive file system
			else
			{
				if (Directory.Exists(dirName))
				{
					return dirName;
				}
				else
				{
					// Remove / if path starts with /.
					if (dirName.StartsWith("/"))
					{
						dirName = dirName.Substring(1);
					}

					// Remove / if path ends with /.
					//if(dirName.EndsWith("/")){
					//    dirName = dirName.Substring(0,dirName.Length - 1);
					//}
					String[] pathParts = dirName.Split('/');
					String currentPath = "/";

					// See if dirs path is valid
					for (Int32 i = 0, len = pathParts.Length; i < len; i++)
					{
						Boolean dirExists = false;
						String[] dirs = Directory.GetDirectories(currentPath);

						foreach (String dir in dirs)
						{
							String[] dirParts = dir.Split('/');
							if (pathParts[i].ToLower() == dirParts[dirParts.Length - 1].ToLower())
							{
								currentPath = dir;
								dirExists = true;
								break;
							}
						}
						if (!dirExists)
						{
							return null;
						}
					}
					return currentPath;
				}
			}
			return null;
		}

		#endregion

		#region -- method EnsureDirectory --

		/// <summary>
		/// Ensures that specified folder exists, if not it will be created.
		/// Returns actual dir (In linux it may differ from requested directory, because of case-sensitivity.).
		/// </summary>
		/// <param name="folder">Folder name with path.</param>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static String EnsureDirectory(String folder)
		{
			String normalizedFolder = DirectoryExists(folder);
			if (normalizedFolder == null)
			{
				Directory.CreateDirectory(folder);
				return folder;
			}
			else
			{
				return normalizedFolder;
			}
		}

		#endregion

		#region -- method DirectoryIsExists --

		/// <summary>检测目录是否存在</summary>
		/// <param name="StrPath">路径</param>
		/// <returns></returns>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static Boolean DirectoryIsExists(String StrPath)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(StrPath);
			return dirInfo.Exists;
		}

		/// <summary>检测目录是否存在</summary>
		/// <param name="StrPath">路径</param>
		/// <param name="Create">如果不存在，是否创建</param>
#if !NET40
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
		public static void DirectoryIsExists(String StrPath, Boolean Create)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(StrPath);

			//return dirInfo.Exists;
			if (!dirInfo.Exists)
			{
				if (Create) { dirInfo.Create(); }
			}
		}

		#endregion

		#region -- method CreateDirectory --

		/// <summary>创建指定目录</summary>
		/// <param name="targetDir"></param>
		public static void CreateDirectory(String targetDir)
		{
			DirectoryInfo dir = new DirectoryInfo(targetDir);
			if (!dir.Exists) { dir.Create(); }
		}

		/// <summary>建立子目录</summary>
		/// <param name="parentDir">目录路径</param>
		/// <param name="subDirName">子目录名称</param>
		public static void CreateDirectory(String parentDir, String subDirName)
		{
			CreateDirectory(PathCombineFix(parentDir, subDirName));
		}

		#endregion

		#region -- method CopyDirectory --

		/// <summary>复制文件夹</summary>
		/// <param name="sourcePath">原始文件夹路径</param>
		/// <param name="destinationPath">新文件夹路径</param>
		/// <param name="overwriteexisting">是否覆盖(默认true)</param>
		/// <returns></returns>
		public static Boolean CopyDirectory(String sourcePath, String destinationPath, Boolean overwriteexisting)
		{
			Boolean ret = false;

			try
			{
				sourcePath = sourcePath.EndsWith(@"\") ? sourcePath : sourcePath + @"\";
				destinationPath = destinationPath.EndsWith(@"\") ? destinationPath : destinationPath + @"\";
				if (Directory.Exists(sourcePath))
				{
					if (Directory.Exists(destinationPath) == false)
					{
						Directory.CreateDirectory(destinationPath);
					}

					foreach (String fls in Directory.GetFiles(sourcePath))
					{
						FileInfo flinfo = new FileInfo(fls);
						flinfo.CopyTo(destinationPath + flinfo.Name, overwriteexisting);
					}

					foreach (String drs in Directory.GetDirectories(sourcePath))
					{
						DirectoryInfo drinfo = new DirectoryInfo(drs);
						if (CopyDirectory(drs, destinationPath + drinfo.Name, overwriteexisting) == false)
						{
							ret = false;
						}
					}
				}
				ret = true;
			}
			catch { ret = false; }
			return ret;
		}

		/// <summary>复制文件夹</summary>
		/// <param name="sourcePath">原始文件夹路径</param>
		/// <param name="destinationPath">新文件夹路径</param>
		public static Boolean CopyDirectory(String sourcePath, String destinationPath)
		{
			return CopyDirectory(sourcePath, destinationPath, true);
		}

		#endregion

		#region -- method ReNameFloder --

		/// <summary>重命名文件夹</summary>
		/// <param name="OldFloderName">原路径文件夹名称</param>
		/// <param name="NewFloderName">新路径文件夹名称</param>
		/// <returns></returns>
		public static Boolean ReNameFloder(String OldFloderName, String NewFloderName)
		{
			try
			{
				if (Directory.Exists(HttpContext.Current.Server.MapPath("//") + OldFloderName))
				{
					Directory.Move(HttpContext.Current.Server.MapPath("//") + OldFloderName, HttpContext.Current.Server.MapPath("//") + NewFloderName);
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		#endregion

		#region -- method DeleteDirectory --

		/// <summary>删除指定目录</summary>
		/// <param name="targetDir">目录路径</param>
		public static void DeleteDirectory(String targetDir)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(targetDir);
			if (dirInfo.Exists)
			{
				DeleteDirectoryFiles(targetDir, true);
				dirInfo.Delete(true);
			}
		}

		#endregion

		#region -- method DeleteSubDirectory --

		/// <summary>删除指定目录的所有子目录,不包括对当前目录文件的删除</summary>
		/// <param name="targetDir">目录路径</param>
		public static void DeleteSubDirectory(String targetDir)
		{
			foreach (String subDir in Directory.GetDirectories(targetDir))
			{
				DeleteDirectory(subDir);
			}
		}

		#endregion

		#region -- method GetLocalDrives --

		/// <summary>获取本地驱动器名列表</summary>
		/// <returns></returns>
		public static String[] GetLocalDrives()
		{
			return Directory.GetLogicalDrives();
		}

		#endregion

		#region -- Combine Methods --

#if !NET_3_5_GREATER

		public static String Combine(String path1, String path2)
		{
			if ((path1 == null) || (path2 == null))
			{
				throw new ArgumentNullException((path1 == null) ? "path1" : "path2");
			}
			CheckInvalidPathChars(path1);
			CheckInvalidPathChars(path2);
			return CombineNoChecks(path1, path2);
		}

		public static String Combine(String path1, String path2, String path3)
		{
			if (((path1 == null) || (path2 == null)) || (path3 == null))
			{
				throw new ArgumentNullException((path1 == null) ? "path1" : ((path2 == null) ? "path2" : "path3"));
			}
			CheckInvalidPathChars(path1);
			CheckInvalidPathChars(path2);
			CheckInvalidPathChars(path3);
			return CombineNoChecks(CombineNoChecks(path1, path2), path3);
		}

		public static String Combine(String path1, String path2, String path3, String path4)
		{
			if (((path1 == null) || (path2 == null)) || ((path3 == null) || (path4 == null)))
			{
				throw new ArgumentNullException((path1 == null) ? "path1" : ((path2 == null) ? "path2" : ((path3 == null) ? "path3" : "path4")));
			}
			CheckInvalidPathChars(path1);
			CheckInvalidPathChars(path2);
			CheckInvalidPathChars(path3);
			CheckInvalidPathChars(path4);
			return CombineNoChecks(CombineNoChecks(CombineNoChecks(path1, path2), path3), path4);
		}

		private static String CombineNoChecks(String path1, String path2)
		{
			if (path2.Length == 0)
			{
				return path1;
			}
			if (path1.Length == 0)
			{
				return path2;
			}
			if (Path.IsPathRooted(path2))
			{
				return path2;
			}
			Char ch = path1[path1.Length - 1];
			if (((ch != Path.DirectorySeparatorChar) && (ch != Path.AltDirectorySeparatorChar)) && (ch != Path.VolumeSeparatorChar))
			{
				return (path1 + Path.DirectorySeparatorChar + path2);
			}
			return (path1 + path2);
		}

		public static String Combine(params String[] paths)
		{
			if (paths == null)
			{
				throw new ArgumentNullException("paths");
			}
			Int32 capacity = 0;
			Int32 num2 = 0;

			for (Int32 i = 0; i < paths.Length; i++)
			{
				if (paths[i] == null)
				{
					throw new ArgumentNullException("paths");
				}
				if (paths[i].Length != 0)
				{
					CheckInvalidPathChars(paths[i]);
					if (Path.IsPathRooted(paths[i]))
					{
						num2 = i;
						capacity = paths[i].Length;
					}
					else
					{
						capacity += paths[i].Length;
					}
					Char ch = paths[i][paths[i].Length - 1];
					if (((ch != Path.DirectorySeparatorChar) && (ch != Path.AltDirectorySeparatorChar)) && (ch != Path.VolumeSeparatorChar))
					{
						capacity++;
					}
				}
			}
			StringBuilder builder = new StringBuilder(capacity);

			for (Int32 j = num2; j < paths.Length; j++)
			{
				if (paths[j].Length != 0)
				{
					if (builder.Length == 0)
					{
						builder.Append(paths[j]);
					}
					else
					{
						Char ch2 = builder[builder.Length - 1];
						if (((ch2 != Path.DirectorySeparatorChar) && (ch2 != Path.AltDirectorySeparatorChar)) && (ch2 != Path.VolumeSeparatorChar))
						{
							builder.Append(Path.DirectorySeparatorChar);
						}
						builder.Append(paths[j]);
					}
				}
			}
			return builder.ToString();
		}

		internal static void CheckInvalidPathChars(String path)
		{
			for (Int32 i = 0; i < path.Length; i++)
			{
				Int32 num2 = path[i];
				if (((num2 == 0x22) || (num2 == 60)) || (((num2 == 0x3e) || (num2 == 0x7c)) || (num2 < 0x20)))
				{
					throw new ArgumentException("Argument_InvalidPathChars");
				}
			}
		}

#endif

		#endregion

		#region -- SD --

		/// <summary>
		/// Gets the normalized version of fileName.
		/// Slashes are replaced with backslashes, backreferences "." and ".." are 'evaluated'.
		/// </summary>
		public static String NormalizePath(String fileName)
		{
			if (fileName.IsNullOrWhiteSpace())
			{
				return fileName;
			}
			Int32 i;
			Boolean isWeb = false;

			for (i = 0; i < fileName.Length; i++)
			{
				if (fileName[i] == '/' || fileName[i] == '\\')
				{
					break;
				}
				if (fileName[i] == ':')
				{
					if (i > 1)
					{
						isWeb = true;
					}
					break;
				}
			}
			Char outputSeparator = isWeb ? '/' : System.IO.Path.DirectorySeparatorChar;
			StringBuilder result = new StringBuilder();
			if (isWeb == false && fileName.StartsWith(@"\\") || fileName.StartsWith("//"))
			{
				i = 2;
				result.Append(outputSeparator);
			}
			else
			{
				i = 0;
			}
			Int32 segmentStartPos = i;

			for (; i <= fileName.Length; i++)
			{
				if (i == fileName.Length || fileName[i] == '/' || fileName[i] == '\\')
				{
					Int32 segmentLength = i - segmentStartPos;

					switch (segmentLength)
					{
						case 0:

							// ignore empty segment (if not in web mode)
							// On unix, don't ignore empty segment if i==0
							if (isWeb || (i == 0 && Environment.OSVersion.Platform == PlatformID.Unix))
							{
								result.Append(outputSeparator);
							}
							break;

						case 1:

							// ignore /./ segment, but append other one-letter segments
							if (fileName[segmentStartPos] != '.')
							{
								if (result.Length > 0) result.Append(outputSeparator);
								result.Append(fileName[segmentStartPos]);
							}
							break;

						case 2:
							if (fileName[segmentStartPos] == '.' && fileName[segmentStartPos + 1] == '.')
							{
								// remove previous segment
								Int32 j;

								for (j = result.Length - 1; j >= 0 && result[j] != outputSeparator; j--) ;
								if (j > 0)
								{
									result.Length = j;
								}
								break;
							}
							else
							{
								// append normal segment
								goto default;
							}
						default:
							if (result.Length > 0) result.Append(outputSeparator);
							result.Append(fileName, segmentStartPos, segmentLength);
							break;
					}
					segmentStartPos = i + 1; // remember start position for next segment
				}
			}
			if (isWeb == false)
			{
				if (result.Length > 0 && result[result.Length - 1] == outputSeparator)
				{
					result.Length -= 1;
				}
				if (result.Length == 2 && result[1] == ':')
				{
					result.Append(outputSeparator);
				}
			}
			return result.ToString();
		}

		public static Boolean IsEqualFileName(String fileName1, String fileName2)
		{
			return String.Equals(NormalizePath(fileName1),
													 NormalizePath(fileName2),
													 StringComparison.OrdinalIgnoreCase);
		}

		public static Boolean IsBaseDirectory(String baseDirectory, String testDirectory)
		{
			if (baseDirectory == null || testDirectory == null)
			{
				return false;
			}
			baseDirectory = NormalizePath(baseDirectory) + Path.DirectorySeparatorChar;
			testDirectory = NormalizePath(testDirectory) + Path.DirectorySeparatorChar;
			return testDirectory.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase);
		}

		public static Boolean IsUrl(String path)
		{
			if (path == null) { throw new ArgumentNullException("path"); }
			return path.IndexOf("://", StringComparison.Ordinal) > 0;
		}

		//public static Boolean IsEqualFileName(FileName fileName1, FileName fileName2)
		//{
		//	return fileName1 == fileName2;
		//}

		//public static String GetCommonBaseDirectory(String dir1, String dir2)
		//{
		//	if (dir1 == null || dir2 == null)
		//	{
		//		return null;
		//	}
		//	if (IsUrl(dir1) || IsUrl(dir2))
		//	{
		//		return null;
		//	}
		//	dir1 = NormalizePath(dir1);
		//	dir2 = NormalizePath(dir2);
		//	String[] aPath = dir1.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		//	String[] bPath = dir2.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		//	StringBuilder result = new StringBuilder();
		//	Int32 indx = 0;

		//	for (; indx < Math.Min(bPath.Length, aPath.Length); ++indx)
		//	{
		//		if (bPath[indx].Equals(aPath[indx], StringComparison.OrdinalIgnoreCase))
		//		{
		//			if (result.Length > 0)
		//			{
		//				result.Append(Path.DirectorySeparatorChar);
		//			}
		//			result.Append(aPath[indx]);
		//		}
		//		else
		//		{
		//			break;
		//		}
		//	}
		//	if (indx == 0)
		//	{
		//		return null;
		//	}
		//	else
		//	{
		//		return result.ToString();
		//	}
		//}

		///// <summary>
		///// Converts a given absolute path and a given base path to a path that leads
		///// from the base path to the absoulte path. (as a relative path)
		///// </summary>
		//public static String GetRelativePath(String baseDirectoryPath, String absPath)
		//{
		//	if (IsUrl(absPath) || IsUrl(baseDirectoryPath))
		//	{
		//		return absPath;
		//	}
		//	baseDirectoryPath = NormalizePath(baseDirectoryPath);
		//	absPath = NormalizePath(absPath);
		//	String[] bPath = baseDirectoryPath.Split(separators);
		//	String[] aPath = absPath.Split(separators);
		//	Int32 indx = 0;

		//	for (; indx < Math.Min(bPath.Length, aPath.Length); ++indx)
		//	{
		//		if (!bPath[indx].Equals(aPath[indx], StringComparison.OrdinalIgnoreCase))
		//		{
		//			break;
		//		}
		//	}
		//	if (indx == 0)
		//	{
		//		return absPath;
		//	}
		//	StringBuilder erg = new StringBuilder();
		//	if (indx == bPath.Length)
		//	{
		//		//				erg.Append('.');
		//		//				erg.Append(Path.DirectorySeparatorChar);
		//	}
		//	else
		//	{
		//		for (Int32 i = indx; i < bPath.Length; ++i)
		//		{
		//			erg.Append("..");
		//			erg.Append(Path.DirectorySeparatorChar);
		//		}
		//	}
		//	erg.Append(String.Join(Path.DirectorySeparatorChar.ToString(), aPath, indx, aPath.Length - indx));
		//	return erg.ToString();
		//}

		///// <summary>Combines baseDirectoryPath with relPath and normalizes the resulting path.</summary>
		//public static String GetAbsolutePath(String baseDirectoryPath, String relPath)
		//{
		//	return NormalizePath(Path.Combine(baseDirectoryPath, relPath));
		//}

		//public static String RenameBaseDirectory(String fileName, String oldDirectory, String newDirectory)
		//{
		//	fileName = NormalizePath(fileName);
		//	oldDirectory = NormalizePath(oldDirectory.TrimEnd(Path.DirectorySeparatorChar,
		//																										Path.AltDirectorySeparatorChar));
		//	newDirectory = NormalizePath(newDirectory.TrimEnd(Path.DirectorySeparatorChar,
		//																										Path.AltDirectorySeparatorChar));
		//	if (IsBaseDirectory(oldDirectory, fileName))
		//	{
		//		if (fileName.Length == oldDirectory.Length)
		//		{
		//			return newDirectory;
		//		}
		//		return Path.Combine(newDirectory, fileName.Substring(oldDirectory.Length + 1));
		//	}
		//	return fileName;
		//}

		//public static void DeepCopy(String sourceDirectory, String destinationDirectory, Boolean overwrite)
		//{
		//	if (!Directory.Exists(destinationDirectory))
		//	{
		//		Directory.CreateDirectory(destinationDirectory);
		//	}

		//	foreach (String fileName in Directory.GetFiles(sourceDirectory))
		//	{
		//		File.Copy(fileName, Path.Combine(destinationDirectory, Path.GetFileName(fileName)), overwrite);
		//	}

		//	foreach (String directoryName in Directory.GetDirectories(sourceDirectory))
		//	{
		//		DeepCopy(directoryName,
		//						 Path.Combine(destinationDirectory,
		//						 Path.GetFileName(directoryName)),
		//						 overwrite);
		//	}
		//}

		//public static List<String> SearchDirectory(
		//		String directory,
		//		String filemask,
		//		Boolean searchSubdirectories,
		//		Boolean ignoreHidden)
		//{
		//	List<String> collection = new List<String>();
		//	SearchDirectory(directory, filemask, collection, searchSubdirectories, ignoreHidden);
		//	return collection;
		//}

		//public static List<String> SearchDirectory(
		//		String directory,
		//		String filemask,
		//		Boolean searchSubdirectories)
		//{
		//	return SearchDirectory(directory, filemask, searchSubdirectories, true);
		//}

		//public static List<String> SearchDirectory(String directory, String filemask)
		//{
		//	return SearchDirectory(directory, filemask, true, true);
		//}

		///// <summary>
		///// Finds all files which are valid to the mask <paramref name="filemask"/> in the path
		///// <paramref name="directory"/> and all subdirectories
		///// (if <paramref name="searchSubdirectories"/> is true).
		///// The found files are added to the List&lt;String&gt;
		///// <paramref name="collection"/>.
		///// If <paramref name="ignoreHidden"/> is true, hidden files and folders are ignored.
		///// </summary>
		//private static void SearchDirectory(
		//		String directory,
		//		String filemask,
		//		List<String> collection,
		//		Boolean searchSubdirectories,
		//		Boolean ignoreHidden)
		//{
		//	// If Directory.GetFiles() searches the 8.3 name as well as the full name so if the filemask is
		//	// "*.xpt" it will return "Template.xpt~"
		//	try
		//	{
		//		Boolean isExtMatch = Regex.IsMatch(filemask, @"^\*\..{3}$");
		//		String ext = null;
		//		String[] file = Directory.GetFiles(directory, filemask);
		//		if (isExtMatch)
		//		{
		//			ext = filemask.Remove(0, 1);
		//		}

		//		foreach (String f in file)
		//		{
		//			if (ignoreHidden && (File.GetAttributes(f) & FileAttributes.Hidden) == FileAttributes.Hidden)
		//			{
		//				continue;
		//			}
		//			if (isExtMatch && Path.GetExtension(f) != ext)
		//			{
		//				continue;
		//			}
		//			collection.Add(f);
		//		}
		//		if (searchSubdirectories)
		//		{
		//			String[] dir = Directory.GetDirectories(directory);

		//			foreach (String d in dir)
		//			{
		//				if (ignoreHidden && (File.GetAttributes(d) & FileAttributes.Hidden) == FileAttributes.Hidden)
		//				{
		//					continue;
		//				}
		//				SearchDirectory(d, filemask, collection, searchSubdirectories, ignoreHidden);
		//			}
		//		}
		//	}
		//	catch (UnauthorizedAccessException)
		//	{
		//		// Ignore exception when access to a directory is denied.
		//		// Fixes SD2-893.
		//	}
		//}

		///// <summary>This method checks if a path (full or relative) is valid.</summary>
		//public static Boolean IsValidPath(String fileName)
		//{
		//	// Fixme: 260 is the hardcoded maximal length for a path on my Windows XP system
		//	//        I can't find a .NET property or method for determining this variable.
		//	if (fileName == null || fileName.Length == 0 || fileName.Length >= MaxPathLength)
		//	{
		//		return false;
		//	}

		//	// platform independend : check for invalid path chars
		//	if (fileName.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
		//	{
		//		return false;
		//	}
		//	if (fileName.IndexOf('?') >= 0 || fileName.IndexOf('*') >= 0)
		//	{
		//		return false;
		//	}
		//	if (!Regex.IsMatch(fileName, fileNameRegEx))
		//	{
		//		return false;
		//	}
		//	if (fileName[fileName.Length - 1] == ' ')
		//	{
		//		return false;
		//	}
		//	if (fileName[fileName.Length - 1] == '.')
		//	{
		//		return false;
		//	}

		//	// platform dependend : Check for invalid file names (DOS)
		//	// this routine checks for follwing bad file names :
		//	// CON, PRN, AUX, NUL, COM1-9 and LPT1-9
		//	String nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
		//	if (nameWithoutExtension != null)
		//	{
		//		nameWithoutExtension = nameWithoutExtension.ToUpperInvariant();
		//	}
		//	if (nameWithoutExtension == "CON" ||
		//			nameWithoutExtension == "PRN" ||
		//			nameWithoutExtension == "AUX" ||
		//			nameWithoutExtension == "NUL")
		//	{
		//		return false;
		//	}
		//	Char ch = nameWithoutExtension.Length == 4 ? nameWithoutExtension[3] : '\0';
		//	return !((nameWithoutExtension.StartsWith("COM") ||
		//						nameWithoutExtension.StartsWith("LPT")) &&
		//						Char.IsDigit(ch));
		//}

		///// <summary>Checks that a single directory name (not the full path) is valid.</summary>
		//[ObsoleteAttribute("Use IsValidDirectoryEntryName instead")]
		//public static Boolean IsValidDirectoryName(String name)
		//{
		//	return IsValidDirectoryEntryName(name);
		//}

		///// <summary>Checks that a single directory name (not the full path) is valid.</summary>
		//public static Boolean IsValidDirectoryEntryName(String name)
		//{
		//	if (!IsValidPath(name))
		//	{
		//		return false;
		//	}
		//	if (name.IndexOfAny(new Char[] { Path.AltDirectorySeparatorChar,
		//																	 Path.DirectorySeparatorChar,
		//																	 Path.VolumeSeparatorChar }) >= 0)
		//	{
		//		return false;
		//	}
		//	if (name.Trim(' ').Length == 0)
		//	{
		//		return false;
		//	}
		//	return true;
		//}

		//public static Boolean IsDirectory(String filename)
		//{
		//	if (!Directory.Exists(filename))
		//	{
		//		return false;
		//	}
		//	FileAttributes attr = File.GetAttributes(filename);
		//	return (attr & FileAttributes.Directory) != 0;
		//}

		////TODO This code is Windows specific
		//private static Boolean MatchN(String src, Int32 srcidx, String pattern, Int32 patidx)
		//{
		//	Int32 patlen = pattern.Length;
		//	Int32 srclen = src.Length;
		//	Char next_char;

		//	for (; ; )
		//	{
		//		if (patidx == patlen)
		//		{
		//			return (srcidx == srclen);
		//		}
		//		next_char = pattern[patidx++];
		//		if (next_char == '?')
		//		{
		//			if (srcidx == src.Length)
		//			{
		//				return false;
		//			}
		//			srcidx++;
		//		}
		//		else if (next_char != '*')
		//		{
		//			if ((srcidx == src.Length) || (src[srcidx] != next_char))
		//			{
		//				return false;
		//			}
		//			srcidx++;
		//		}
		//		else
		//		{
		//			if (patidx == pattern.Length)
		//			{
		//				return true;
		//			}

		//			while (srcidx < srclen)
		//			{
		//				if (MatchN(src, srcidx, pattern, patidx))
		//				{
		//					return true;
		//				}
		//				srcidx++;
		//			}
		//			return false;
		//		}
		//	}
		//}

		//private static Boolean Match(String src, String pattern)
		//{
		//	if (pattern[0] == '*')
		//	{
		//		// common case optimization
		//		Int32 i = pattern.Length;
		//		Int32 j = src.Length;

		//		while (--i > 0)
		//		{
		//			if (pattern[i] == '*')
		//			{
		//				return MatchN(src, 0, pattern, 0);
		//			}
		//			if (j-- == 0)
		//			{
		//				return false;
		//			}
		//			if ((pattern[i] != src[j]) && (pattern[i] != '?'))
		//			{
		//				return false;
		//			}
		//		}
		//		return true;
		//	}
		//	return MatchN(src, 0, pattern, 0);
		//}

		//public static Boolean MatchesPattern(String filename, String pattern)
		//{
		//	filename = filename.ToUpper();
		//	pattern = pattern.ToUpper();
		//	String[] patterns = pattern.Split(';');

		//	foreach (String p in patterns)
		//	{
		//		if (Match(filename, p))
		//		{
		//			return true;
		//		}
		//	}
		//	return false;
		//}

		#endregion
	}
}