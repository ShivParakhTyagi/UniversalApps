using System;
using System.Net.Http.Headers;
using System.Text;

namespace UwpHttpContentMultipartExtensions
{
    public class InternetMessageFormatHeaderParser
    {
        internal const int MinHeaderSize = 2;
        private int _totalBytesConsumed;
        private int _maxHeaderSize;
        private InternetMessageFormatHeaderParser.HeaderFieldState _headerState;
        private HttpHeaders _headers;
        private InternetMessageFormatHeaderParser.CurrentHeaderFieldStore _currentHeader;
        private readonly bool _ignoreHeaderValidation;

        public InternetMessageFormatHeaderParser(HttpHeaders headers, int maxHeaderSize)
            : this(headers, maxHeaderSize, false)
        {
        }

        public InternetMessageFormatHeaderParser(
            HttpHeaders headers,
            int maxHeaderSize,
            bool ignoreHeaderValidation)
        {
            if (maxHeaderSize < 2)
                throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(maxHeaderSize), (object)maxHeaderSize, (object)2);
            if (headers == null)
                throw Error.ArgumentNull(nameof(headers));
            this._headers = headers;
            this._maxHeaderSize = maxHeaderSize;
            this._ignoreHeaderValidation = ignoreHeaderValidation;
            this._currentHeader = new InternetMessageFormatHeaderParser.CurrentHeaderFieldStore();
        }

        public ParserState ParseBuffer(byte[] buffer, int bytesReady, ref int bytesConsumed)
        {
            if (buffer == null)
                throw Error.ArgumentNull(nameof(buffer));
            ParserState parserState = ParserState.NeedMoreData;
            if (bytesConsumed >= bytesReady)
                return parserState;
            try
            {
                return InternetMessageFormatHeaderParser.ParseHeaderFields(buffer, bytesReady, ref bytesConsumed, ref this._headerState, this._maxHeaderSize, ref this._totalBytesConsumed, this._currentHeader, this._headers, this._ignoreHeaderValidation);
            }
            catch (Exception ex)
            {
                return ParserState.Invalid;
            }
        }

        private static ParserState ParseHeaderFields(
            byte[] buffer,
            int bytesReady,
            ref int bytesConsumed,
            ref InternetMessageFormatHeaderParser.HeaderFieldState requestHeaderState,
            int maximumHeaderLength,
            ref int totalBytesConsumed,
            InternetMessageFormatHeaderParser.CurrentHeaderFieldStore currentField,
            HttpHeaders headers,
            bool ignoreHeaderValidation)
        {
            int num1 = bytesConsumed;
            ParserState parserState = ParserState.DataTooBig;
            int num2 = maximumHeaderLength <= 0 ? int.MaxValue : maximumHeaderLength - totalBytesConsumed + num1;
            if (bytesReady < num2)
            {
                parserState = ParserState.NeedMoreData;
                num2 = bytesReady;
            }
            switch (requestHeaderState)
            {
                case InternetMessageFormatHeaderParser.HeaderFieldState.Name:
                    int index1 = bytesConsumed;
                    while (buffer[bytesConsumed] != (byte)58)
                    {
                        if (buffer[bytesConsumed] == (byte)13)
                        {
                            if (!currentField.IsEmpty())
                            {
                                parserState = ParserState.Invalid;
                                goto label_29;
                            }
                            else
                            {
                                requestHeaderState = InternetMessageFormatHeaderParser.HeaderFieldState.AfterCarriageReturn;
                                if (++bytesConsumed != num2)
                                    goto case InternetMessageFormatHeaderParser.HeaderFieldState.AfterCarriageReturn;
                                else
                                    goto label_29;
                            }
                        }
                        else if (++bytesConsumed == num2)
                        {
                            string str = Encoding.UTF8.GetString(buffer, index1, bytesConsumed - index1);
                            currentField.Name.Append(str);
                            goto label_29;
                        }
                    }
                    if (bytesConsumed > index1)
                    {
                        string str = Encoding.UTF8.GetString(buffer, index1, bytesConsumed - index1);
                        currentField.Name.Append(str);
                    }
                    requestHeaderState = InternetMessageFormatHeaderParser.HeaderFieldState.Value;
                    if (++bytesConsumed == num2)
                        break;
                    goto case InternetMessageFormatHeaderParser.HeaderFieldState.Value;
                case InternetMessageFormatHeaderParser.HeaderFieldState.Value:
                    int index2 = bytesConsumed;
                    while (buffer[bytesConsumed] != (byte)13)
                    {
                        if (++bytesConsumed == num2)
                        {
                            string str = Encoding.UTF8.GetString(buffer, index2, bytesConsumed - index2);
                            currentField.Value.Append(str);
                            goto label_29;
                        }
                    }
                    if (bytesConsumed > index2)
                    {
                        string str = Encoding.UTF8.GetString(buffer, index2, bytesConsumed - index2);
                        currentField.Value.Append(str);
                    }
                    requestHeaderState = InternetMessageFormatHeaderParser.HeaderFieldState.AfterCarriageReturn;
                    if (++bytesConsumed == num2)
                        break;
                    goto case InternetMessageFormatHeaderParser.HeaderFieldState.AfterCarriageReturn;
                case InternetMessageFormatHeaderParser.HeaderFieldState.AfterCarriageReturn:
                    if (buffer[bytesConsumed] != (byte)10)
                    {
                        parserState = ParserState.Invalid;
                        break;
                    }
                    if (currentField.IsEmpty())
                    {
                        parserState = ParserState.Done;
                        ++bytesConsumed;
                        break;
                    }
                    requestHeaderState = InternetMessageFormatHeaderParser.HeaderFieldState.FoldingLine;
                    if (++bytesConsumed == num2)
                        break;
                    goto case InternetMessageFormatHeaderParser.HeaderFieldState.FoldingLine;
                case InternetMessageFormatHeaderParser.HeaderFieldState.FoldingLine:
                    if (buffer[bytesConsumed] != (byte)32 && buffer[bytesConsumed] != (byte)9)
                    {
                        currentField.CopyTo(headers, ignoreHeaderValidation);
                        requestHeaderState = InternetMessageFormatHeaderParser.HeaderFieldState.Name;
                        if (bytesConsumed != num2)
                            goto case InternetMessageFormatHeaderParser.HeaderFieldState.Name;
                        else
                            break;
                    }
                    else
                    {
                        currentField.Value.Append(' ');
                        requestHeaderState = InternetMessageFormatHeaderParser.HeaderFieldState.Value;
                        if (++bytesConsumed != num2)
                            goto case InternetMessageFormatHeaderParser.HeaderFieldState.Value;
                        else
                            break;
                    }
            }
            label_29:
            totalBytesConsumed += bytesConsumed - num1;
            return parserState;
        }

        private enum HeaderFieldState
        {
            Name,
            Value,
            AfterCarriageReturn,
            FoldingLine,
        }

        private class CurrentHeaderFieldStore
        {
            private static readonly char[] _linearWhiteSpace = new char[2]
            {
                ' ',
                '\t'
            };
            private readonly StringBuilder _name = new StringBuilder(128);
            private readonly StringBuilder _value = new StringBuilder(2048);
            private const int DefaultFieldNameAllocation = 128;
            private const int DefaultFieldValueAllocation = 2048;

            public StringBuilder Name
            {
                get
                {
                    return this._name;
                }
            }

            public StringBuilder Value
            {
                get
                {
                    return this._value;
                }
            }

            public void CopyTo(HttpHeaders headers, bool ignoreHeaderValidation)
            {
                string name = this._name.ToString();
                string str = this._value.ToString().Trim(InternetMessageFormatHeaderParser.CurrentHeaderFieldStore._linearWhiteSpace);
                if (ignoreHeaderValidation)
                    headers.TryAddWithoutValidation(name, str);
                else
                    headers.Add(name, str);
                this.Clear();
            }

            public bool IsEmpty()
            {
                if (this._name.Length == 0)
                    return this._value.Length == 0;
                return false;
            }

            private void Clear()
            {
                this._name.Clear();
                this._value.Clear();
            }
        }
    }
}