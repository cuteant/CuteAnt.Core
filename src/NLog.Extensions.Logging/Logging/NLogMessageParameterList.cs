﻿using System;
using System.Collections;
using System.Collections.Generic;
using NLog.MessageTemplates;

namespace NLog.Extensions.Logging
{
    /// <summary>
    /// Converts Microsoft Extension Logging ParameterList into NLog MessageTemplate ParameterList
    /// </summary>
    internal class NLogMessageParameterList : IList<MessageTemplateParameter>
    {
#if NET40
        private readonly IList<KeyValuePair<string, object>> _parameterList;
#else
        private readonly IReadOnlyList<KeyValuePair<string, object>> _parameterList;
#endif
        private static readonly NLogMessageParameterList EmptyList = new NLogMessageParameterList(new KeyValuePair<string, object>[0]);
        private static readonly NLogMessageParameterList OriginalMessageList = new NLogMessageParameterList(new[] { new KeyValuePair<string, object>(NLogLogger.OriginalFormatPropertyName, string.Empty) });

        public bool HasOriginalMessage => _originalMessageIndex.HasValue;
        private readonly int? _originalMessageIndex;

        public bool HasComplexParameters => _hasMessageTemplateCapture || _isMixedPositional;
        private readonly bool _hasMessageTemplateCapture;
        private readonly bool _isMixedPositional;

        public bool IsPositional => _isPositional;
        private readonly bool _isPositional;

#if NET40
        public NLogMessageParameterList(IList<KeyValuePair<string, object>> parameterList)
#else
        public NLogMessageParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList)
#endif
        {
            if (IsValidParameterList(parameterList, out _originalMessageIndex, out _hasMessageTemplateCapture, out _isMixedPositional, out _isPositional))
            {
                _parameterList = parameterList;
            }
            else
            {
                _parameterList = CreateValidParameterList(parameterList);
            }
        }

        /// <summary>
        /// Create a <see cref="NLogMessageParameterList"/> if <paramref name="parameterList"/> has values, otherwise <c>null</c>
        /// </summary>
        /// <remarks>
        /// The LogMessageParameterList-constructor initiates all the parsing/scanning
        /// </remarks>
#if NET40
        public static NLogMessageParameterList TryParse(IList<KeyValuePair<string, object>> parameterList)
#else
        public static NLogMessageParameterList TryParse(IReadOnlyList<KeyValuePair<string, object>> parameterList)
#endif
        {
            if (parameterList.Count > 1 || (parameterList.Count == 1 && parameterList[0].Key != NLogLogger.OriginalFormatPropertyName))
            {
                return new NLogMessageParameterList(parameterList);
            }
            else if (parameterList.Count == 1)
            {
                return OriginalMessageList; // Skip allocation
            }
            else
            {
                return EmptyList;           // Skip allocation
            }
        }

        public bool HasMessageTemplateSyntax(bool parseMessageTemplates)
        {
            return HasOriginalMessage && (HasComplexParameters || (parseMessageTemplates && Count > 0));
        }

#if NET40
        public string GetOriginalMessage(IList<KeyValuePair<string, object>> messageProperties)
#else
        public string GetOriginalMessage(IReadOnlyList<KeyValuePair<string, object>> messageProperties)
#endif
        {
            if (_originalMessageIndex < messageProperties?.Count)
            {
                return messageProperties[_originalMessageIndex.Value].Value as string;
            }
            return null;
        }

        /// <summary>
        /// Verify that the input parameterList contains non-empty key-values and the orignal-format-property at the end
        /// </summary>
#if NET40
        private static bool IsValidParameterList(IList<KeyValuePair<string, object>> parameterList, out int? originalMessageIndex, out bool hasMessageTemplateCapture, out bool isMixedPositional, out bool isPositional)
#else
        private static bool IsValidParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList, out int? originalMessageIndex, out bool hasMessageTemplateCapture, out bool isMixedPositional, out bool isPositional)
#endif
        {
            hasMessageTemplateCapture = false;
            isMixedPositional = false;
            isPositional = false;
            originalMessageIndex = null;
            bool? firstParameterIsPositional = null;
            for (int i = 0; i < parameterList.Count; ++i)
            {
                string parameterKey;
                try
                {
                    parameterKey = parameterList[i].Key;
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new FormatException($"Invalid format string. Expected {parameterList.Count - 1} format parameters, but failed to lookup parameter index {i}", ex);
                }
                if (string.IsNullOrEmpty(parameterKey))
                {
                    originalMessageIndex = null;
                    return false;
                }

                char firstChar = parameterKey[0];
                if (GetCaptureType(firstChar) != CaptureType.Normal)
                {
                    hasMessageTemplateCapture = true;
                }
                else if (parameterKey == NLogLogger.OriginalFormatPropertyName)
                {
                    if (originalMessageIndex.HasValue)
                    {
                        originalMessageIndex = null;
                        return false;
                    }

                    originalMessageIndex = i;
                }
                else
                {
                    if (!firstParameterIsPositional.HasValue)
                        firstParameterIsPositional = char.IsDigit(firstChar);
                    else if (char.IsDigit(firstChar) != firstParameterIsPositional)
                        isMixedPositional = true;
                }
            }

            if (firstParameterIsPositional == true && !isMixedPositional)
                isPositional = true;

            return true;
        }

        /// <summary>
        /// Extract all valid properties from the input parameterList, and return them in a newly allocated list
        /// </summary>
#if NET40
        private static IList<KeyValuePair<string, object>> CreateValidParameterList(IList<KeyValuePair<string, object>> parameterList)
#else
        private static IReadOnlyList<KeyValuePair<string, object>> CreateValidParameterList(IReadOnlyList<KeyValuePair<string, object>> parameterList)
#endif
        {
            var validParameterList = new List<KeyValuePair<string, object>>(parameterList.Count);
            for (int i = 0; i < parameterList.Count; ++i)
            {
                var paramPair = parameterList[i];
                if (string.IsNullOrEmpty(paramPair.Key))
                    continue;

                if (paramPair.Key == NLogLogger.OriginalFormatPropertyName)
                {
                    continue;
                }

                validParameterList.Add(parameterList[i]);
            }
            return validParameterList;
        }

        public MessageTemplateParameter this[int index]
        {
            get
            {
                if (index >= _originalMessageIndex)
                    index += 1;

                var parameter = _parameterList[index];
                var parameterName = parameter.Key;
                var capture = GetCaptureType(parameterName[0]);
                if (capture != CaptureType.Normal)
                    parameterName = parameterName.Substring(1);
                return new MessageTemplateParameter(parameterName, parameter.Value, null, capture);
            }
            set => throw new NotSupportedException();
        }

        private static CaptureType GetCaptureType(char firstChar)
        {
            if (firstChar == '@')
                return CaptureType.Serialize;
            else if (firstChar == '$')
                return CaptureType.Stringify;
            else
                return CaptureType.Normal;
        }


        public int Count => _parameterList.Count - (_originalMessageIndex.HasValue ? 1 : 0);

        public bool IsReadOnly => true;

        public void Add(MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(MessageTemplateParameter[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; ++i)
                array[i + arrayIndex] = this[i];
        }

        public IEnumerator<MessageTemplateParameter> GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
                yield return this[i];
        }

        public int IndexOf(MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(MessageTemplateParameter item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
