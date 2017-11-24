using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CuteAnt.Text
{
  /// <summary>常用工具类——字符串类</summary>
  public static class StringHelper
  {
    #region -- 属性 --

    private static readonly UTF8Encoding _UTF8NoBOM = new UTF8Encoding(false);

    /// <summary>不提供 Unicode 字节顺序标记，检测到无效的编码时不引发异常</summary>
    public static UTF8Encoding UTF8NoBOM
    {
      get { return _UTF8NoBOM; }
    }

    private static readonly UTF8Encoding _SecureUTF8NoBOM = new UTF8Encoding(false, true);

    /// <summary>不提供 Unicode 字节顺序标记，检测到无效的编码时引发异常</summary>
    public static UTF8Encoding SecureUTF8NoBOM
    {
      get { return _SecureUTF8NoBOM; }
    }

    private static readonly UTF8Encoding _SecureUTF8 = new UTF8Encoding(true, true);

    /// <summary>提供 Unicode 字节顺序标记，检测到无效的编码时引发异常</summary>
    public static UTF8Encoding SecureUTF8
    {
      get { return _SecureUTF8; }
    }

    #endregion

    #region -- methods from LumiSoft.Net --

    #region - method QuoteString -

    /// <summary>Qoutes String and escapes fishy('\',"') chars.</summary>
    /// <param name="text">Text to quote.</param>
    /// <returns></returns>
    public static String QuoteString(String text)
    {
      //// String is already quoted-String.
      //if (text != null && text.StartsWith("\"") && text.EndsWith("\""))
      //{
      //	return text;
      //}

      var retVal = StringBuilderCache.Acquire();
      retVal.Append("\"");
      for (Int32 i = 0; i < text.Length; i++)
      {
        Char c = text[i];
        if (c == '\\')
        {
          retVal.Append("\\\\");
        }
        else if (c == '\"')
        {
          retVal.Append("\\\"");
        }
        else
        {
          retVal.Append(c);
        }
      }
      retVal.Append("\"");

      return StringBuilderCache.GetStringAndRelease(retVal);
    }

    #endregion

    #region - method UnQuoteString -

    /// <summary>Unquotes and unescapes escaped chars specified text. For example "xxx" will become to 'xxx', "escaped quote \"", will become to escaped 'quote "'.</summary>
    /// <param name="text">Text to unquote.</param>
    /// <returns></returns>
    public static String UnQuoteString(String text)
    {
      Int32 startPosInText = 0;
      Int32 endPosInText = text.Length;

      //--- Trim. We can't use standard String.Trim(), it's slow. ----//
      for (Int32 i = 0; i < endPosInText; i++)
      {
        Char c = text[i];
        if (c == ' ' || c == '\t')
        {
          startPosInText++;
        }
        else
        {
          break;
        }
      }

      for (Int32 i = endPosInText - 1; i > 0; i--)
      {
        Char c = text[i];
        if (c == ' ' || c == '\t')
        {
          endPosInText--;
        }
        else
        {
          break;
        }
      }

      //--------------------------------------------------------------//
      // All text trimmed
      if ((endPosInText - startPosInText) <= 0)
      {
        return "";
      }

      // Remove starting and ending quotes.
      if (text[startPosInText] == '\"')
      {
        startPosInText++;
      }
      if (text[endPosInText - 1] == '\"')
      {
        endPosInText--;
      }

      // Just '"'
      if (endPosInText == startPosInText - 1)
      {
        return "";
      }
      Char[] chars = new Char[endPosInText - startPosInText];
      Int32 posInChars = 0;
      Boolean charIsEscaped = false;

      for (Int32 i = startPosInText; i < endPosInText; i++)
      {
        Char c = text[i];

        // Escaping Char
        if (!charIsEscaped && c == '\\')
        {
          charIsEscaped = true;
        }

        // Escaped Char
        else if (charIsEscaped)
        {
          // TODO: replace \n,\r,\t,\v ???
          chars[posInChars] = c;
          posInChars++;
          charIsEscaped = false;
        }

        // Normal Char
        else
        {
          chars[posInChars] = c;
          posInChars++;
          charIsEscaped = false;
        }
      }
      return new String(chars, 0, posInChars);
    }

    #endregion

    #region - method EscapeString -

    /// <summary>Escapes specified chars in the specified String.</summary>
    /// <param name="text">Text to escape.</param>
    /// <param name="charsToEscape">Chars to escape.</param>
    public static String EscapeString(String text, Char[] charsToEscape)
    {
      // Create worst scenario buffer, assume all chars must be escaped
      Char[] buffer = new Char[text.Length * 2];
      Int32 nChars = 0;

      foreach (Char c in text)
      {
        foreach (Char escapeChar in charsToEscape)
        {
          if (c == escapeChar)
          {
            buffer[nChars] = '\\';
            nChars++;
            break;
          }
        }
        buffer[nChars] = c;
        nChars++;
      }
      return new String(buffer, 0, nChars);
    }

    #endregion

    #region - method UnEscapeString -

    /// <summary>Unescapes all escaped chars.</summary>
    /// <param name="text">Text to unescape.</param>
    /// <returns></returns>
    public static String UnEscapeString(String text)
    {
      // Create worst scenarion buffer, non of the chars escaped.
      Char[] buffer = new Char[text.Length];
      Int32 nChars = 0;
      Boolean escapedCahr = false;

      foreach (Char c in text)
      {
        if (!escapedCahr && c == '\\')
        {
          escapedCahr = true;
        }
        else
        {
          buffer[nChars] = c;
          nChars++;
          escapedCahr = false;
        }
      }
      return new String(buffer, 0, nChars);
    }

    #endregion

    #region - method SplitQuotedString -

    /// <summary>
    /// Splits String into String arrays. This split method won't split qouted strings, but only text outside of qouted String.
    /// For example: '"text1, text2",text3' will be 2 parts: "text1, text2" and text3.
    /// </summary>
    /// <param name="text">Text to split.</param>
    /// <param name="splitChar">Char that splits text.</param>
    /// <returns></returns>
    public static String[] SplitQuotedString(String text, Char splitChar)
    {
      return SplitQuotedString(text, splitChar, false);
    }

    /// <summary>
    /// Splits String into String arrays. This split method won't split qouted strings, but only text outside of qouted String.
    /// For example: '"text1, text2",text3' will be 2 parts: "text1, text2" and text3.
    /// </summary>
    /// <param name="text">Text to split.</param>
    /// <param name="splitChar">Char that splits text.</param>
    /// <param name="unquote">If true, splitted parst will be unqouted if they are qouted.</param>
    /// <returns></returns>
    public static String[] SplitQuotedString(String text, Char splitChar, Boolean unquote)
    {
      return SplitQuotedString(text, splitChar, unquote, Int32.MaxValue);
    }

    /// <summary>
    /// Splits String into String arrays. This split method won't split qouted strings, but only text outside of qouted String.
    /// For example: '"text1, text2",text3' will be 2 parts: "text1, text2" and text3.
    /// </summary>
    /// <param name="text">Text to split.</param>
    /// <param name="splitChar">Char that splits text.</param>
    /// <param name="unquote">If true, splitted parst will be unqouted if they are qouted.</param>
    /// <param name="count">Maximum number of substrings to return.</param>
    /// <returns>Returns splitted String.</returns>
    /// <exception cref="ArgumentNullException">Is raised when <b>text</b> is null reference.</exception>
    public static String[] SplitQuotedString(String text, Char splitChar, Boolean unquote, Int32 count)
    {
      if (text == null)
      {
        throw new ArgumentNullException("text");
      }
      List<String> splitParts = new List<String>();  // Holds splitted parts
      Int32 startPos = 0;
      Boolean inQuotedString = false;               // Holds flag if position is quoted String or not
      Char lastChar = '0';

      for (Int32 i = 0; i < text.Length; i++)
      {
        Char c = text[i];

        // We have exceeded maximum allowed splitted parts.
        if ((splitParts.Count + 1) >= count)
        {
          break;
        }

        // We have quoted String start/end.
        if (lastChar != '\\' && c == '\"')
        {
          inQuotedString = !inQuotedString;
        }

        // We have escaped or normal Char.
        //else{
        // We igonre split Char in quoted-String.
        if (!inQuotedString)
        {
          // We have split Char, do split.
          if (c == splitChar)
          {
            if (unquote)
            {
              splitParts.Add(UnQuoteString(text.Substring(startPos, i - startPos)));
            }
            else
            {
              splitParts.Add(text.Substring(startPos, i - startPos));
            }

            // Store new split part start position.
            startPos = i + 1;
          }
        }

        //else{
        lastChar = c;
      }

      // Add last split part to splitted parts list
      if (unquote)
      {
        splitParts.Add(UnQuoteString(text.Substring(startPos, text.Length - startPos)));
      }
      else
      {
        splitParts.Add(text.Substring(startPos, text.Length - startPos));
      }
      return splitParts.ToArray();
    }

    #endregion

    #region - method QuotedIndexOf -

    /// <summary>
    /// Gets first index of specified Char. The specified Char in quoted String is skipped.
    /// Returns -1 if specified Char doesn't exist.
    /// </summary>
    /// <param name="text">Text in what to check.</param>
    /// <param name="indexChar">Char what index to get.</param>
    /// <returns></returns>
    public static Int32 QuotedIndexOf(String text, Char indexChar)
    {
      Int32 retVal = -1;
      Boolean inQuotedString = false; // Holds flag if position is quoted String or not

      for (Int32 i = 0; i < text.Length; i++)
      {
        Char c = text[i];
        if (c == '\"')
        {
          // Start/end quoted String area
          inQuotedString = !inQuotedString;
        }

        // Current Char is what index we want and it isn't in quoted String, return it's index
        if (!inQuotedString && c == indexChar)
        {
          return i;
        }
      }
      return retVal;
    }

    #endregion

    #region - method SplitString -

    /// <summary>Splits String into String arrays.</summary>
    /// <param name="text">Text to split.</param>
    /// <param name="splitChar">Char Char that splits text.</param>
    /// <returns></returns>
    public static String[] SplitString(String text, Char splitChar)
    {
      ArrayList splitParts = new ArrayList();  // Holds splitted parts
      Int32 lastSplitPoint = 0;
      Int32 textLength = text.Length;

      for (Int32 i = 0; i < textLength; i++)
      {
        if (text[i] == splitChar)
        {
          // Add current currentSplitBuffer value to splitted parts list
          splitParts.Add(text.Substring(lastSplitPoint, i - lastSplitPoint));
          lastSplitPoint = i + 1;
        }
      }

      // Add last split part to splitted parts list
      if (lastSplitPoint <= textLength)
      {
        splitParts.Add(text.Substring(lastSplitPoint));
      }
      String[] retVal = new String[splitParts.Count];
      splitParts.CopyTo(retVal, 0);
      return retVal;
    }

    #endregion

    #region - method IsToken -

    /// <summary>Gets if specified String is valid "token" value.</summary>
    /// <param name="value">String value to check.</param>
    /// <returns>Returns true if specified String value is valid "token" value.</returns>
    /// <exception cref="ArgumentNullException">Is raised if <b>value</b> is null.</exception>
    public static Boolean IsToken(String value)
    {
      if (value == null)
      {
        throw new ArgumentNullException(value);
      }
      /* This syntax is taken from rfc 3261, but token must be universal so ... .
          token    =  1*(alphanum / "-" / "." / "!" / "%" / "*" / "_" / "+" / "`" / "'" / "~" )
          alphanum = ALPHA / DIGIT
          ALPHA    =  %x41-5A / %x61-7A   ; A-Z / a-z
          DIGIT    =  %x30-39             ; 0-9
      */
      Char[] tokenChars = new Char[] { '-', '.', '!', '%', '*', '_', '+', '`', '\'', '~' };

      foreach (Char c in value)
      {
        // We don't have letter or digit, so we only may have token Char.
        if (!((c >= 0x41 && c <= 0x5A) || (c >= 0x61 && c <= 0x7A) || (c >= 0x30 && c <= 0x39)))
        {
          Boolean validTokenChar = false;

          foreach (Char tokenChar in tokenChars)
          {
            if (c == tokenChar)
            {
              validTokenChar = true;
              break;
            }
          }
          if (!validTokenChar)
          {
            return false;
          }
        }
      }
      return true;
    }

    #endregion

    #endregion

    #region -- LD编辑距离算法 --

    /// <summary>编辑距离搜索，从词组中找到最接近关键字的若干匹配项</summary>
    /// <remarks>
    /// 算法代码由@Aimeast 独立完成。http://www.cnblogs.com/Aimeast/archive/2011/09/05/2167844.html
    /// </remarks>
    /// <param name="key">关键字</param>
    /// <param name="words">词组</param>
    /// <returns></returns>
    public static String[] LevenshteinSearch(String key, String[] words)
    {
      if (string.IsNullOrWhiteSpace(key)) return new String[0];
      String[] keys = key.Split(new Char[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);

      foreach (String item in keys)
      {
        Int32 maxDist = (item.Length - 1) / 2;
        var q = from str in words
                where item.Length <= str.Length
                    && Enumerable.Range(0, maxDist + 1)
                    .Any(dist =>
                    {
                      return Enumerable.Range(0, Math.Max(str.Length - item.Length - dist + 1, 0))
                          .Any(f =>
                          {
                            return LevenshteinDistance(item, str.Substring(f, item.Length + dist)) <= maxDist;
                          });
                    })
                orderby str
                select str;
        words = q.ToArray();
      }
      return words;
    }

    /// <summary>编辑距离</summary>
    /// <remarks>
    /// 又称Levenshtein距离（也叫做Edit Distance），是指两个字串之间，由一个转成另一个所需的最少编辑操作次数。
    /// 许可的编辑操作包括将一个字符替换成另一个字符，插入一个字符，删除一个字符。
    /// 算法代码由@Aimeast 独立完成。http://www.cnblogs.com/Aimeast/archive/2011/09/05/2167844.html
    /// </remarks>
    /// <param name="str1"></param>
    /// <param name="str2"></param>
    /// <returns></returns>
    public static Int32 LevenshteinDistance(String str1, String str2)
    {
      Int32 n = str1.Length;
      Int32 m = str2.Length;
      Int32[,] C = new Int32[n + 1, m + 1];
      Int32 i, j, x, y, z;

      for (i = 0; i <= n; i++)
        C[i, 0] = i;

      for (i = 1; i <= m; i++)
        C[0, i] = i;

      for (i = 0; i < n; i++)

        for (j = 0; j < m; j++)
        {
          x = C[i, j + 1] + 1;
          y = C[i + 1, j] + 1;
          if (str1[i] == str2[j])
            z = C[i, j];
          else
            z = C[i, j] + 1;
          C[i + 1, j + 1] = Math.Min(Math.Min(x, y), z);
        }
      return C[n, m];
    }

    #endregion

    #region -- LCS算法 --

    /// <summary>最长公共子序列搜索，从词组中找到最接近关键字的若干匹配项</summary>
    /// <remarks>
    /// 算法代码由@Aimeast 独立完成。http://www.cnblogs.com/Aimeast/archive/2011/09/05/2167844.html
    /// </remarks>
    /// <param name="key"></param>
    /// <param name="words"></param>
    /// <returns></returns>
    public static String[] LCSSearch(String key, String[] words)
    {
      if (string.IsNullOrWhiteSpace(key) || words == null || words.Length == 0) return new String[0];
      String[] keys = key
                          .Split(new Char[] { ' ', '\u3000' }, StringSplitOptions.RemoveEmptyEntries)
                          .OrderBy(s => s.Length)
                          .ToArray();

      //var q = from sentence in items.AsParallel()
      var q = from word in words
              let MLL = LCSDistance(word, keys)
              where MLL >= 0
              orderby (MLL + 0.5) / word.Length, word
              select word;
      return q.ToArray();
    }

    /// <summary>
    /// 最长公共子序列问题是寻找两个或多个已知数列最长的子序列。
    /// 一个数列 S，如果分别是两个或多个已知数列的子序列，且是所有符合此条件序列中最长的，则 S 称为已知序列的最长公共子序列。
    /// The longest common subsequence (LCS) problem is to find the longest subsequence common to all sequences in a set of sequences (often just two). Note that subsequence is different from a substring, see substring vs. subsequence. It is a classic computer science problem, the basis of diff (a file comparison program that outputs the differences between two files), and has applications in bioinformatics.
    /// </summary>
    /// <remarks>
    /// 算法代码由@Aimeast 独立完成。http://www.cnblogs.com/Aimeast/archive/2011/09/05/2167844.html
    /// </remarks>
    /// <param name="word"></param>
    /// <param name="keys">多个关键字。长度必须大于0，必须按照字符串长度升序排列。</param>
    /// <returns></returns>
    public static Int32 LCSDistance(String word, String[] keys)
    {
      Int32 sLength = word.Length;
      Int32 result = sLength;
      Boolean[] flags = new Boolean[sLength];
      Int32[,] C = new Int32[sLength + 1, keys[keys.Length - 1].Length + 1];

      //Int32[,] C = new Int32[sLength + 1, words.Select(s => s.Length).Max() + 1];
      foreach (String key in keys)
      {
        Int32 wLength = key.Length;
        Int32 first = 0, last = 0;
        Int32 i = 0, j = 0, LCS_L;

        //foreach 速度会有所提升，还可以加剪枝
        for (i = 0; i < sLength; i++)

          for (j = 0; j < wLength; j++)
            if (word[i] == key[j])
            {
              C[i + 1, j + 1] = C[i, j] + 1;
              if (first < C[i, j])
              {
                last = i;
                first = C[i, j];
              }
            }
            else
              C[i + 1, j + 1] = Math.Max(C[i, j + 1], C[i + 1, j]);
        LCS_L = C[i, j];
        if (LCS_L <= wLength >> 1)
          return -1;

        while (i > 0 && j > 0)
        {
          if (C[i - 1, j - 1] + 1 == C[i, j])
          {
            i--;
            j--;
            if (!flags[i])
            {
              flags[i] = true;
              result--;
            }
            first = i;
          }
          else if (C[i - 1, j] == C[i, j])
            i--;
          else// if (C[i, j - 1] == C[i, j])
            j--;
        }
        if (LCS_L <= (last - first + 1) >> 1)
          return -1;
      }
      return result;
    }

    #endregion
  }
}