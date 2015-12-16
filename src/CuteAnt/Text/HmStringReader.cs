using System;
using System.Text;

namespace CuteAnt.Text
{
	/// <summary>String reader.</summary>
	public class HmStringReader
	{
		private String m_originalString = "";
		private String m_sourceString = "";

		/// <summary>Default constructor.</summary>
		/// <param name="source">Source String.</param>
		/// <exception cref="ArgumentNullException">Is raised when <b>source</b> is null.</exception>
		public HmStringReader(String source)
		{
			ValidationHelper.ArgumentNullOrEmpty(source, "source");
			m_originalString = source;
			m_sourceString = source;
		}

		#region -- method AppendString --

		/// <summary>Appends specified String to SourceString.</summary>
		/// <param name="value">String value to append.</param>
		public void AppendString(String value)
		{
			m_sourceString += value;
		}

		#endregion

		#region -- method ReadToFirstChar --

		/// <summary>Reads to first Char, skips white-space(SP,VTAB,HTAB,CR,LF) from the beginning of source String.</summary>
		/// <returns>Returns white-space chars which was readed.</returns>
		public String ReadToFirstChar()
		{
			Int32 whiteSpaces = 0;

			for (Int32 i = 0, len = m_sourceString.Length; i < len; i++)
			{
				if (Char.IsWhiteSpace(m_sourceString[i]))
				{
					whiteSpaces++;
				}
				else
				{
					break;
				}
			}
			String whiteSpaceChars = m_sourceString.Substring(0, whiteSpaces);
			m_sourceString = m_sourceString.Substring(whiteSpaces);
			return whiteSpaceChars;
		}

		#endregion

		#region -- method ReadSpecifiedLength --

		/// <summary>Reads String with specified length. Throws exception if read length is bigger than source String length.</summary>
		/// <param name="length">Number of chars to read.</param>
		/// <returns></returns>
		public String ReadSpecifiedLength(Int32 length)
		{
			ValidationHelper.ArgumentOutOfRangeCondition((m_sourceString.Length < length), "length", "Read {0} can't be bigger than source String !");
			String retVal = m_sourceString.Substring(0, length);
			m_sourceString = m_sourceString.Substring(length);
			return retVal;
		}

		#endregion

		#region -- method QuotedReadToDelimiter --

		/// <summary>
		/// Reads String to specified delimiter or to end of underlying String. Notes: Delimiter in quoted String is skipped.
		/// Delimiter is removed by default.
		/// For example: delimiter = ',', text = '"aaaa,eee",qqqq' - then result is '"aaaa,eee"'.
		/// </summary>
		/// <param name="delimiter">Data delimiter.</param>
		/// <returns></returns>
		public String QuotedReadToDelimiter(Char delimiter)
		{
			return QuotedReadToDelimiter(new Char[] { delimiter });
		}

		/// <summary>
		/// Reads String to specified delimiter or to end of underlying String. Notes: Delimiters in quoted String is skipped.
		/// Delimiter is removed by default.
		/// For example: delimiter = ',', text = '"aaaa,eee",qqqq' - then result is '"aaaa,eee"'.
		/// </summary>
		/// <param name="delimiters">Data delimiters.</param>
		/// <returns></returns>
		public String QuotedReadToDelimiter(Char[] delimiters)
		{
			return QuotedReadToDelimiter(delimiters, true);
		}

		/// <summary>
		/// Reads String to specified delimiter or to end of underlying String. Notes: Delimiters in quoted String is skipped.
		/// For example: delimiter = ',', text = '"aaaa,eee",qqqq' - then result is '"aaaa,eee"'.
		/// </summary>
		/// <param name="delimiters">Data delimiters.</param>
		/// <param name="removeDelimiter">Specifies if delimiter is removed from underlying String.</param>
		/// <returns></returns>
		public String QuotedReadToDelimiter(Char[] delimiters, Boolean removeDelimiter)
		{
			StringBuilder currentSplitBuffer = new StringBuilder(); // Holds active
			Boolean inQuotedString = false;               // Holds flag if position is quoted String or not
			Boolean doEscape = false;

			for (Int32 i = 0; i < m_sourceString.Length; i++)
			{
				Char c = m_sourceString[i];
				if (doEscape)
				{
					currentSplitBuffer.Append(c);

					doEscape = false;
				}
				else if (c == '\\')
				{
					currentSplitBuffer.Append(c);

					doEscape = true;
				}
				else
				{
					// Start/end quoted String area
					if (c == '\"')
					{
						inQuotedString = !inQuotedString;
					}

					// See if Char is delimiter
					Boolean isDelimiter = false;

					foreach (Char delimiter in delimiters)
					{
						if (c == delimiter)
						{
							isDelimiter = true;
							break;
						}
					}

					// Current Char is split Char and it isn't in quoted String, do split
					if (!inQuotedString && isDelimiter)
					{
						String retVal = currentSplitBuffer.ToString();

						// Remove readed String + delimiter from source String
						if (removeDelimiter)
						{
							m_sourceString = m_sourceString.Substring(i + 1);
						}

						// Remove readed String
						else
						{
							m_sourceString = m_sourceString.Substring(i);
						}
						return retVal;
					}
					else
					{
						currentSplitBuffer.Append(c);
					}
				}
			}

			// If we reached so far then we are end of String, return it
			m_sourceString = "";
			return currentSplitBuffer.ToString();
		}

		#endregion

		#region -- method ReadWord --

		/// <summary>
		/// Reads word from String. Returns null if no word is available.
		/// Word reading begins from first Char, for example if SP"text", then space is trimmed.
		/// </summary>
		/// <returns></returns>
		public String ReadWord()
		{
			return ReadWord(true);
		}

		/// <summary>
		/// Reads word from String. Returns null if no word is available.
		/// Word reading begins from first Char, for example if SP"text", then space is trimmed.
		/// </summary>
		/// <param name="unQuote">Specifies if quoted String word is unquoted.</param>
		/// <returns></returns>
		public String ReadWord(Boolean unQuote)
		{
			return ReadWord(unQuote, new Char[] { ' ', ',', ';', '{', '}', '(', ')', '[', ']', '<', '>', '\r', '\n' }, false);
		}

		/// <summary>
		/// Reads word from String. Returns null if no word is available.
		/// Word reading begins from first Char, for example if SP"text", then space is trimmed.
		/// </summary>
		/// <param name="unQuote">Specifies if quoted String word is unquoted.</param>
		/// <param name="wordTerminatorChars">Specifies chars what terminate word.</param>
		/// <param name="removeWordTerminator">Specifies if work terminator is removed.</param>
		/// <returns></returns>
		public String ReadWord(Boolean unQuote, Char[] wordTerminatorChars, Boolean removeWordTerminator)
		{
			// Always start word reading from first Char.
			this.ReadToFirstChar();
			if (this.Available == 0)
			{
				return null;
			}

			// quoted word can contain any Char, " must be escaped with \
			// unqouted word can conatin any Char except: SP VTAB HTAB,{}()[]<>
			if (m_sourceString.StartsWith("\""))
			{
				if (unQuote)
				{
					return StringHelper.UnQuoteString(QuotedReadToDelimiter(wordTerminatorChars, removeWordTerminator));
				}
				else
				{
					return QuotedReadToDelimiter(wordTerminatorChars, removeWordTerminator);
				}
			}
			else
			{
				Int32 wordLength = 0;

				for (Int32 i = 0; i < m_sourceString.Length; i++)
				{
					Char c = m_sourceString[i];
					Boolean isTerminator = false;

					foreach (Char terminator in wordTerminatorChars)
					{
						if (c == terminator)
						{
							isTerminator = true;
							break;
						}
					}
					if (isTerminator)
					{
						break;
					}
					wordLength++;
				}
				String retVal = m_sourceString.Substring(0, wordLength);
				if (removeWordTerminator)
				{
					if (m_sourceString.Length >= wordLength + 1)
					{
						m_sourceString = m_sourceString.Substring(wordLength + 1);
					}
				}
				else
				{
					m_sourceString = m_sourceString.Substring(wordLength);
				}
				return retVal;
			}
		}

		#endregion

		#region -- method ReadParenthesized --

		/// <summary>
		/// Reads parenthesized value. Supports {},(),[],&lt;&gt; parenthesis.
		/// Throws exception if there isn't parenthesized value or closing parenthesize is missing.
		/// </summary>
		/// <returns></returns>
		public String ReadParenthesized()
		{
			ReadToFirstChar();
			Char startingChar = ' ';
			Char closingChar = ' ';
			if (m_sourceString.StartsWith("{"))
			{
				startingChar = '{';
				closingChar = '}';
			}
			else if (m_sourceString.StartsWith("("))
			{
				startingChar = '(';
				closingChar = ')';
			}
			else if (m_sourceString.StartsWith("["))
			{
				startingChar = '[';
				closingChar = ']';
			}
			else if (m_sourceString.StartsWith("<"))
			{
				startingChar = '<';
				closingChar = '>';
			}
			else
			{
				throw new ArgumentException("No parenthesized value '" + m_sourceString + "' !");
			}
			Boolean inQuotedString = false; // Holds flag if position is quoted String or not
			Boolean skipNextChar = false;
			Int32 closingCharIndex = -1;
			Int32 nestedStartingCharCounter = 0;

			for (Int32 i = 1; i < m_sourceString.Length; i++)
			{
				// Skip this Char.
				if (skipNextChar)
				{
					skipNextChar = false;
				}

				// We have Char escape '\', skip next Char.
				else if (m_sourceString[i] == '\\')
				{
					skipNextChar = true;
				}

				// Start/end quoted String area
				else if (m_sourceString[i] == '\"')
				{
					inQuotedString = !inQuotedString;
				}

				// We need to skip parenthesis in quoted String
				else if (!inQuotedString)
				{
					// There is nested parenthesis
					if (m_sourceString[i] == startingChar)
					{
						nestedStartingCharCounter++;
					}

					// Closing Char
					else if (m_sourceString[i] == closingChar)
					{
						// There isn't nested parenthesis closing chars left, this is closing Char what we want
						if (nestedStartingCharCounter == 0)
						{
							closingCharIndex = i;
							break;
						}

						// This is nested parenthesis closing Char
						else
						{
							nestedStartingCharCounter--;
						}
					}
				}
			}
			ValidationHelper.ArgumentCondition((closingCharIndex == -1), "There is no closing parenthesize for '" + m_sourceString + "' !");
			String retVal = m_sourceString.Substring(1, closingCharIndex - 1);
			m_sourceString = m_sourceString.Substring(closingCharIndex + 1);
			return retVal;
		}

		#endregion

		#region -- method ReadToEnd --

		/// <summary>Reads all remaining String, returns null if no chars left to read.</summary>
		/// <returns></returns>
		public String ReadToEnd()
		{
			if (this.Available == 0)
			{
				return null;
			}
			String retVal = m_sourceString;
			m_sourceString = "";
			return retVal;
		}

		#endregion

		#region -- method RemoveFromEnd --

		/// <summary>Removes specified count of chars from the end of the source String.</summary>
		/// <param name="count">Char count.</param>
		/// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
		public void RemoveFromEnd(Int32 count)
		{
			ValidationHelper.ArgumentOutOfRangeCondition(count < 0, "count", "Argument '{0}' value must be >= 0.");
			m_sourceString = m_sourceString.Substring(0, m_sourceString.Length - count);
		}

		#endregion

		#region -- method StartsWith --

		/// <summary>Gets if source String starts with specified value. Compare is case-sensitive.</summary>
		/// <param name="value">Start String value.</param>
		/// <returns>Returns true if source String starts with specified value.</returns>
		public Boolean StartsWith(String value)
		{
			return m_sourceString.StartsWith(value);
		}

		/// <summary>Gets if source String starts with specified value.</summary>
		/// <param name="value">Start String value.</param>
		/// <param name="case_sensitive">Specifies if compare is case-sensitive.</param>
		/// <returns>Returns true if source String starts with specified value.</returns>
		public Boolean StartsWith(String value, Boolean case_sensitive)
		{
			if (case_sensitive)
			{
				return m_sourceString.StartsWith(value);
			}
			else
			{
				return m_sourceString.ToLower().StartsWith(value.ToLower());
			}
		}

		#endregion

		#region -- method EndsWith --

		/// <summary>Gets if source String ends with specified value. Compare is case-sensitive.</summary>
		/// <param name="value">Start String value.</param>
		/// <returns>Returns true if source String ends with specified value.</returns>
		public Boolean EndsWith(String value)
		{
			return m_sourceString.EndsWith(value);
		}

		/// <summary>Gets if source String ends with specified value.</summary>
		/// <param name="value">Start String value.</param>
		/// <param name="case_sensitive">Specifies if compare is case-sensitive.</param>
		/// <returns>Returns true if source String ends with specified value.</returns>
		public Boolean EndsWith(String value, Boolean case_sensitive)
		{
			if (case_sensitive)
			{
				return m_sourceString.EndsWith(value);
			}
			else
			{
				return m_sourceString.EndsWith(value, StringComparison.InvariantCultureIgnoreCase);
			}
		}

		#endregion

		#region -- method StartsWithWord --

		/// <summary>
		/// Gets if current source String starts with word. For example if source String starts with
		/// whiter space or parenthesize, this method returns false.
		/// </summary>
		/// <returns></returns>
		public Boolean StartsWithWord()
		{
			if (m_sourceString.Length == 0)
			{
				return false;
			}
			if (Char.IsWhiteSpace(m_sourceString[0]))
			{
				return false;
			}
			if (Char.IsSeparator(m_sourceString[0]))
			{
				return false;
			}
			Char[] wordTerminators = new Char[] { ' ', ',', ';', '{', '}', '(', ')', '[', ']', '<', '>', '\r', '\n' };

			foreach (Char c in wordTerminators)
			{
				if (c == m_sourceString[0])
				{
					return false;
				}
			}
			return true;
		}

		#endregion

		#region -- Properties Implementation --

		/// <summary>Gets how many chars are available for reading.</summary>
		public Int64 Available
		{
			get { return m_sourceString.Length; }
		}

		/// <summary>Gets original String passed to class constructor.</summary>
		public String OriginalString
		{
			get { return m_originalString; }
		}

		/// <summary>Gets currently remaining String.</summary>
		public String SourceString
		{
			get { return m_sourceString; }
		}

		/// <summary>Gets position in original String.</summary>
		public Int32 Position
		{
			get { return m_originalString.Length - m_sourceString.Length; }
		}

		#endregion
	}
}