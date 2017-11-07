#if DESKTOPCLR
using System;
using System.IO;
using System.Text;

namespace CuteAnt.IO
{
  /// <summary>常用工具类——文件操作类</summary>
  public static class FileHelper
  {
    #region -- Fields --

    private static readonly Char[] separators = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar };
    private const String fileNameRegEx = @"^([a-zA-Z]:)?[^:]+$";

    // This is an arbitrary limitation built into the .NET Framework.
    // Windows supports paths up to 32k length.
    public static readonly Int32 MaxPathLength = 260;

    private const String PATH_SPLIT_CHAR = "\\";

    #endregion

    #region -- 文件操作 --

    #region - method FileIsExists -

    /// <summary>返回文件是否存在</summary>
    /// <param name="filename">文件名</param>
    /// <returns>是否存在</returns>
    public static Boolean FileIsExists(String filename)
    {
      return File.Exists(filename);
    }

    #endregion

    #region - method FileExists -

    /// <summary>
    /// Checks if file exists. If linux, checks with case-insenstively (linux is case-sensitive).
    /// Returns actual file (In linux it may differ from requested file, because of case-sensitivity.)
    /// or null if file doesn't exist.
    /// </summary>
    /// <param name="fileName">File to check.</param>
    /// <returns></returns>
    public static String FileExists(String fileName)
    {
      // Windows we can use File.Exists
      if (Environment.OSVersion.Platform.ToString().ToLower().IndexOf("win") > -1)
      {
        if (File.Exists(fileName))
        {
          return fileName;
        }
      }

      // Unix,Linux we can't trust File.Exists value because of case-sensitive file system
      else
      {
        if (File.Exists(fileName))
        {
          return fileName;
        }
        else
        {
          // Remove / if path starts with /.
          if (fileName.StartsWith("/"))
          {
            fileName = fileName.Substring(1);
          }
          String[] pathParts = fileName.Split('/');
          String currentPath = "/";

          // See if dirs path is valid
          for (Int32 i = 0, len = pathParts.Length; i < len - 1; i++)
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

          // Check that file exists
          String[] files = Directory.GetFiles(currentPath);

          foreach (String file in files)
          {
            if (pathParts[pathParts.Length - 1].ToLower() == Path.GetFileName(file).ToLower())
            {
              return file;
            }
          }
        }
      }
      return null;
    }

    #endregion

    #region - method IsImgFilename -

    /// <summary>判断文件名是否为浏览器可以直接显示的图片文件名</summary>
    /// <param name="filename">文件名</param>
    /// <returns>是否可以直接显示</returns>
    public static Boolean IsImgFilename(String filename)
    {
      filename = filename.Trim();
      if (filename.EndsWith(".") || filename.IndexOf(".") == -1)
      {
        return false;
      }
      String extname = filename.Substring(filename.LastIndexOf(".") + 1).ToLower();
      return (extname == "jpg" || extname == "jpeg" || extname == "png" || extname == "bmp" || extname == "gif");
    }

    #endregion

    #region - method IsHiddenFile -

    /// <summary>判断是否是隐藏文件</summary>
    /// <param name="path">文件路径</param>
    /// <returns></returns>
    public static Boolean IsHiddenFile(String path)
    {
      FileAttributes MyAttributes = File.GetAttributes(path);
      String MyFileType = MyAttributes.ToString();
      return (MyFileType.LastIndexOf("Hidden") != -1); //是否隐藏文件
    }

    #endregion

    #region - method CopyFiles -

    /// <summary>复制指定目录的所有文件</summary>
    /// <param name="sourceDir">原始目录</param>
    /// <param name="targetDir">目标目录</param>
    /// <param name="overWrite">如果为true,覆盖同名文件,否则不覆盖</param>
    /// <param name="copySubDir">如果为true,包含目录,否则不包含</param>
    public static void CopyFiles(String sourceDir, String targetDir, Boolean overWrite, Boolean copySubDir)
    {
      //复制当前目录文件
      foreach (String sourceFileName in Directory.GetFiles(sourceDir))
      {
        String targetFileName = Path.Combine(targetDir, sourceFileName.Substring(sourceFileName.LastIndexOf(PATH_SPLIT_CHAR) + 1));
        if (File.Exists(targetFileName))
        {
          if (overWrite == true)
          {
            File.SetAttributes(targetFileName, FileAttributes.Normal);
            File.Copy(sourceFileName, targetFileName, overWrite);
          }
        }
        else
        {
          File.Copy(sourceFileName, targetFileName, overWrite);
        }
      }
    }

    #endregion

    #region - method MoveFiles -

    /// <summary>移动指定目录的所有文件</summary>
    /// <param name="sourceDir">原始目录</param>
    /// <param name="targetDir">目标目录</param>
    /// <param name="overWrite">如果为true,覆盖同名文件,否则不覆盖</param>
    /// <param name="moveSubDir">如果为true,包含目录,否则不包含</param>
    public static void MoveFiles(String sourceDir, String targetDir, Boolean overWrite, Boolean moveSubDir)
    {
      //移动当前目录文件
      foreach (String sourceFileName in Directory.GetFiles(sourceDir))
      {
        String targetFileName = Path.Combine(targetDir, sourceFileName.Substring(sourceFileName.LastIndexOf(PATH_SPLIT_CHAR) + 1));
        if (File.Exists(targetFileName))
        {
          if (overWrite == true)
          {
            File.SetAttributes(targetFileName, FileAttributes.Normal);
            File.Delete(targetFileName);
            File.Move(sourceFileName, targetFileName);
          }
        }
        else
        {
          File.Move(sourceFileName, targetFileName);
        }
      }
      if (moveSubDir)
      {
        foreach (String sourceSubDir in Directory.GetDirectories(sourceDir))
        {
          String targetSubDir = Path.Combine(targetDir, sourceSubDir.Substring(sourceSubDir.LastIndexOf(PATH_SPLIT_CHAR) + 1));
          if (!Directory.Exists(targetSubDir))
            Directory.CreateDirectory(targetSubDir);
          MoveFiles(sourceSubDir, targetSubDir, overWrite, true);
          Directory.Delete(sourceSubDir);
        }
      }
    }

    #endregion

    #region - method DeleteFiles -

    /// <summary>删除指定目录下的指定文件</summary>
    /// <param name="TargetFileDir">指定文件的目录</param>
    public static void DeleteFiles(String TargetFileDir)
    {
      File.Delete(TargetFileDir);
    }

    #endregion

    #region - method BackupFile -

    /// <summary>备份文件,当目标文件存在时覆盖</summary>
    /// <param name="sourceFileName">源文件名</param>
    /// <param name="destFileName">目标文件名</param>
    /// <returns>操作是否成功</returns>
    public static Boolean BackupFile(String sourceFileName, String destFileName)
    {
      return BackupFile(sourceFileName, destFileName, true);
    }

    /// <summary>备份文件</summary>
    /// <param name="sourceFileName">源文件名</param>
    /// <param name="destFileName">目标文件名</param>
    /// <param name="overwrite">当目标文件存在时是否覆盖</param>
    /// <returns>操作是否成功</returns>
    public static Boolean BackupFile(String sourceFileName, String destFileName, Boolean overwrite)
    {
      if (!File.Exists(sourceFileName))
      {
        throw new FileNotFoundException(sourceFileName + "文件不存在！");
      }
      if (!overwrite && File.Exists(destFileName))
      {
        return false;
      }

      try
      {
        File.Copy(sourceFileName, destFileName, true);
        return true;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    #endregion

    #region - method RestoreFile -

    /// <summary>恢复文件</summary>
    /// <param name="backupFileName">备份文件名</param>
    /// <param name="targetFileName">要恢复的文件名</param>
    /// <param name="backupTargetFileName">要恢复文件再次备份的名称,如果为null,则不再备份恢复文件</param>
    /// <returns>操作是否成功</returns>
    public static Boolean RestoreFile(String backupFileName, String targetFileName, String backupTargetFileName)
    {
      try
      {
        if (!File.Exists(backupFileName))
        {
          throw new FileNotFoundException(backupFileName + "文件不存在！");
        }
        if (backupTargetFileName != null)
        {
          if (!File.Exists(targetFileName))
          {
            throw new FileNotFoundException(targetFileName + "文件不存在！无法备份此文件！");
          }
          else
          {
            File.Copy(targetFileName, backupTargetFileName, true);
          }
        }
        File.Delete(targetFileName);
        File.Copy(backupFileName, targetFileName);
      }
      catch (Exception e)
      {
        throw e;
      }
      return true;
    }

    public static Boolean RestoreFile(String backupFileName, String targetFileName)
    {
      return RestoreFile(backupFileName, targetFileName, null);
    }

    #endregion

    #region - method GetFileWriteTime -

    /// <summary>获取文件最后修改时间</summary>
    /// <param name="FileUrl">文件真实路径</param>
    /// <returns></returns>
    public static DateTime GetFileWriteTime(String FileUrl)
    {
      return File.GetLastWriteTime(FileUrl);
    }

    #endregion

    #region - method ConvertSimpleFileName -

    /// <summary>转换长文件名为短文件名</summary>
    /// <param name="fullname"></param>
    /// <param name="repstring"></param>
    /// <param name="leftnum"></param>
    /// <param name="rightnum"></param>
    /// <param name="charnum"></param>
    /// <returns></returns>
    public static String ConvertSimpleFileName(String fullname, String repstring, Int32 leftnum, Int32 rightnum, Int32 charnum)
    {
      String simplefilename = "", leftstring = "", rightstring = "", filename = "";
      String extname = GetFileExtName(fullname);
      if (extname.IsNullOrWhiteSpace())
      {
        throw new Exception("字符串不含有扩展名信息");
      }
      Int32 filelength = 0, dotindex = 0;

      dotindex = fullname.LastIndexOf('.');
      filename = fullname.Substring(0, dotindex);
      filelength = filename.Length;
      if (dotindex > charnum)
      {
        leftstring = filename.Substring(0, leftnum);
        rightstring = filename.Substring(filelength - rightnum, rightnum);
        if (repstring.IsNullOrWhiteSpace())
        {
          simplefilename = leftstring + rightstring + "." + extname;
        }
        else
        {
          simplefilename = leftstring + repstring + rightstring + "." + extname;
        }
      }
      else
      {
        simplefilename = fullname;
      }
      return simplefilename;
    }

    #endregion

    #region - method GetFileExtName -

    /// <summary>获取指定文件的扩展名</summary>
    /// <param name="fileName">指定文件名</param>
    /// <returns>扩展名</returns>
    public static String GetFileExtName(String fileName)
    {
      // return Path.GetExtension(PathFileName);
      //String[] array = filename.Trim().Split('.');
      //Array.Reverse(array);
      //return array[0].ToString();
      if (fileName.IsNullOrWhiteSpace() || fileName.IndexOf('.') <= 0)
      {
        return "";
      }
      fileName = fileName.ToLower().Trim();
      return fileName.Substring(fileName.LastIndexOf('.'), fileName.Length - fileName.LastIndexOf('.'));
    }

    #endregion

    #region - 获取文件大小并以B，KB，GB，TB方式表示[+2 重载] -

    /// <summary>获取文件大小并以B，KB，GB，TB方式表示</summary>
    /// <param name="File">文件(FileInfo类型)</param>
    /// <returns></returns>
    public static String GetFileSize(FileInfo File)
    {
      String Result = "";
      Int64 FileSize = File.Length;
      if (FileSize >= 1024L * 1024L * 1024L)
      {
        if (FileSize / (1024L * 1024L * 1024L * 1024L) >= 1024)
        {
          Result = String.Format("{0:############0.00} TB", (Double)FileSize / (Double)(1024L * 1024L * 1024L * 1024L));
        }
        else
        {
          Result = String.Format("{0:####0.00} GB", (Double)FileSize / (Double)(1024L * 1024L * 1024L));
        }
      }
      else if (FileSize >= 1024L * 1024L)
      {
        Result = String.Format("{0:####0.00} MB", (Double)FileSize / (Double)(1024L * 1024L));
      }
      else if (FileSize >= 1024L)
      {
        Result = String.Format("{0:####0.00} KB", (Double)FileSize / (Double)1024L);
      }
      else
      {
        Result = String.Format("{0:####0.00} Bytes", FileSize);
      }
      return Result;
    }

    /// <summary>获取文件大小并以B，KB，GB，TB方式表示</summary>
    /// <param name="FilePath">文件的具体路径</param>
    /// <returns></returns>
    public static String GetFileSize(String FilePath)
    {
      return GetFileSize(new FileInfo(FilePath));
    }

    #endregion

    #region - method FindNoUTF8File -

    /// <summary>返回指定目录下的非 UTF8 字符集文件</summary>
    /// <param name="Path">路径</param>
    /// <returns>文件名的字符串数组</returns>
    public static String[] FindNoUTF8File(String Path)
    {
      StringBuilder filelist = new StringBuilder();
      DirectoryInfo Folder = new DirectoryInfo(Path);
      FileInfo[] subFiles = Folder.GetFiles();

      for (Int32 j = 0; j < subFiles.Length; j++)
      {
        if (subFiles[j].Extension.ToLower().Equals(".htm"))
        {
          FileStream fs = new FileStream(subFiles[j].FullName, FileMode.Open, FileAccess.Read);
          Boolean bUtf8 = IsUTF8(fs);
          fs.Close();
          if (!bUtf8)
          {
            filelist.Append(subFiles[j].FullName);
            filelist.Append("\r\n");
          }
        }
      }
      return filelist.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
    }

    //0000 0000-0000 007F - 0xxxxxxx  (ascii converts to 1 octet!)
    //0000 0080-0000 07FF - 110xxxxx 10xxxxxx    ( 2 octet format)
    //0000 0800-0000 FFFF - 1110xxxx 10xxxxxx 10xxxxxx (3 octet format)
    /// <summary>判断文件流是否为UTF8字符集</summary>
    /// <param name="sbInputStream">文件流</param>
    /// <returns>判断结果</returns>
    private static Boolean IsUTF8(FileStream sbInputStream)
    {
      Int32 i;
      Byte cOctets;  // octets to go in this UTF-8 encoded character
      Byte chr;
      Boolean bAllAscii = true;
      Int64 iLen = sbInputStream.Length;
      cOctets = 0;

      for (i = 0; i < iLen; i++)
      {
        chr = (Byte)sbInputStream.ReadByte();
        if ((chr & 0x80) != 0) bAllAscii = false;
        if (cOctets == 0)
        {
          if (chr >= 0x80)
          {
            do
            {
              chr <<= 1;
              cOctets++;
            }

            while ((chr & 0x80) != 0);
            cOctets--;
            if (cOctets == 0) { return false; }
          }
        }
        else
        {
          if ((chr & 0xC0) != 0x80)
          {
            return false;
          }
          cOctets--;
        }
      }
      if (cOctets > 0)
      {
        return false;
      }
      if (bAllAscii)
      {
        return false;
      }
      return true;
    }

    #endregion

    #region - method ReadFileAsString -

    /// <summary>读取文本文件,使用utf-8编码</summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static String ReadFileAsString(String filePath)
    {
      if (File.Exists(filePath))
      {
        var result = String.Empty;
        StreamReader sr = null;
        try
        {
          sr = new StreamReader(filePath, Encoding.UTF8);
          result = sr.ReadToEnd();
        }
        catch
        {
          result = String.Empty;
        }
        finally
        {
          sr.Close();
          sr.Dispose();
        }
        return result;
      }
      else
      {
        return String.Empty;
      }
    }

    #endregion

    #region - method SaveStringAsFile -

    /// <summary>保存字符串文本文件,使用utf-8编码</summary>
    /// <param name="filePath"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static bool SaveStringAsFile(String filePath, String content)
    {
      var result = true;
      StreamWriter sw = null;
      try
      {
        sw = new StreamWriter(filePath, false, Encoding.UTF8);
        sw.Write(content);
        result = true;
      }
      catch
      {
        result = false;
      }
      finally
      {
        if (sw != null)
        {
          sw.Close();
          sw.Dispose();
        }
      }
      return result;
    }

    #endregion

    #endregion
  }
}
#endif