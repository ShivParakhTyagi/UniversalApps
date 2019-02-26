using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace UwpHttpContentMultipartExtensions
{
    internal class MimeMultipartParser
    {
        private static readonly ArraySegment<byte> _emptyBodyPart = new ArraySegment<byte>(new byte[0]);
        internal const int MinMessageSize = 10;
        private const int MaxBoundarySize = 256;
        private const byte HTAB = 9;
        private const byte SP = 32;
        private const byte CR = 13;
        private const byte LF = 10;
        private const byte Dash = 45;
        private long _totalBytesConsumed;
        private long _maxMessageSize;
        private MimeMultipartParser.BodyPartState _bodyPartState;
        private string _boundary;
        private MimeMultipartParser.CurrentBodyPartStore _currentBoundary;

        public MimeMultipartParser(string boundary, long maxMessageSize)
        {
            if (maxMessageSize < 10L)
                throw Error.ArgumentMustBeGreaterThanOrEqualTo(nameof(maxMessageSize), (object)maxMessageSize, (object)10);
            if (string.IsNullOrWhiteSpace(boundary))
                throw Error.ArgumentNull(nameof(boundary));
            if (boundary.Length > 246)
                throw Error.ArgumentMustBeLessThanOrEqualTo(nameof(boundary), (object)boundary.Length, (object)246);
            if (boundary.EndsWith(" ", StringComparison.Ordinal))
                throw Error.Argument(nameof(boundary), "Resources.MimeMultipartParserBadBoundary");
            this._maxMessageSize = maxMessageSize;
            this._boundary = boundary;
            this._currentBoundary = new MimeMultipartParser.CurrentBodyPartStore(this._boundary);
            this._bodyPartState = MimeMultipartParser.BodyPartState.AfterFirstLineFeed;
        }

        public bool IsWaitingForEndOfMessage
        {
            get
            {
                if (this._bodyPartState == MimeMultipartParser.BodyPartState.AfterBoundary && this._currentBoundary != null)
                    return this._currentBoundary.IsFinal;
                return false;
            }
        }

        public bool CanParseMore(int bytesRead, int bytesConsumed)
        {
            return bytesConsumed < bytesRead || bytesRead == 0 && this.IsWaitingForEndOfMessage;
        }

        public MimeMultipartParser.State ParseBuffer(
            byte[] buffer,
            int bytesReady,
            ref int bytesConsumed,
            out ArraySegment<byte> remainingBodyPart,
            out ArraySegment<byte> bodyPart,
            out bool isFinalBodyPart)
        {
            if (buffer == null)
                throw Error.ArgumentNull(nameof(buffer));
            remainingBodyPart = MimeMultipartParser._emptyBodyPart;
            bodyPart = MimeMultipartParser._emptyBodyPart;
            isFinalBodyPart = false;
            MimeMultipartParser.State state;
            try
            {
                state = MimeMultipartParser.ParseBodyPart(buffer, bytesReady, ref bytesConsumed, ref this._bodyPartState, this._maxMessageSize, ref this._totalBytesConsumed, this._currentBoundary);
            }
            catch (Exception ex)
            {
                state = MimeMultipartParser.State.Invalid;
            }
            remainingBodyPart = this._currentBoundary.GetDiscardedBoundary();
            bodyPart = this._currentBoundary.BodyPart;
            if (state == MimeMultipartParser.State.BodyPartCompleted)
            {
                isFinalBodyPart = this._currentBoundary.IsFinal;
                this._currentBoundary.ClearAll();
            }
            else
                this._currentBoundary.ClearBodyPart();
            return state;
        }

        private static MimeMultipartParser.State ParseBodyPart(
            byte[] buffer,
            int bytesReady,
            ref int bytesConsumed,
            ref MimeMultipartParser.BodyPartState bodyPartState,
            long maximumMessageLength,
            ref long totalBytesConsumed,
            MimeMultipartParser.CurrentBodyPartStore currentBodyPart)
        {
            int offset1 = bytesConsumed;
            if (bytesReady == 0 && bodyPartState == MimeMultipartParser.BodyPartState.AfterBoundary && currentBodyPart.IsFinal)
                return MimeMultipartParser.State.BodyPartCompleted;
            MimeMultipartParser.State state = MimeMultipartParser.State.DataTooBig;
            long num = maximumMessageLength <= 0L ? long.MaxValue : maximumMessageLength - totalBytesConsumed + (long)bytesConsumed;
            if (num == 0L)
                return MimeMultipartParser.State.DataTooBig;
            if ((long)bytesReady <= num)
            {
                state = MimeMultipartParser.State.NeedMoreData;
                num = (long)bytesReady;
            }
            currentBodyPart.ResetBoundaryOffset();
            switch (bodyPartState)
            {
                case MimeMultipartParser.BodyPartState.BodyPart:
                    while (buffer[bytesConsumed] != (byte)13)
                    {
                        if ((long)++bytesConsumed == num)
                            goto label_54;
                    }
                    currentBodyPart.AppendBoundary((byte)13);
                    bodyPartState = MimeMultipartParser.BodyPartState.AfterFirstCarriageReturn;
                    if ((long)++bytesConsumed == num)
                        break;
                    goto case MimeMultipartParser.BodyPartState.AfterFirstCarriageReturn;
                case MimeMultipartParser.BodyPartState.AfterFirstCarriageReturn:
                    if (buffer[bytesConsumed] != (byte)10)
                    {
                        currentBodyPart.ResetBoundary();
                        bodyPartState = MimeMultipartParser.BodyPartState.BodyPart;
                        goto case MimeMultipartParser.BodyPartState.BodyPart;
                    }
                    else
                    {
                        currentBodyPart.AppendBoundary((byte)10);
                        bodyPartState = MimeMultipartParser.BodyPartState.AfterFirstLineFeed;
                        if ((long)++bytesConsumed == num)
                            break;
                        goto case MimeMultipartParser.BodyPartState.AfterFirstLineFeed;
                    }
                case MimeMultipartParser.BodyPartState.AfterFirstLineFeed:
                    if (buffer[bytesConsumed] == (byte)13)
                    {
                        currentBodyPart.ResetBoundary();
                        currentBodyPart.AppendBoundary((byte)13);
                        bodyPartState = MimeMultipartParser.BodyPartState.AfterFirstCarriageReturn;
                        if ((long)++bytesConsumed != num)
                            goto case MimeMultipartParser.BodyPartState.AfterFirstCarriageReturn;
                        else
                            break;
                    }
                    else if (buffer[bytesConsumed] != (byte)45)
                    {
                        currentBodyPart.ResetBoundary();
                        bodyPartState = MimeMultipartParser.BodyPartState.BodyPart;
                        goto case MimeMultipartParser.BodyPartState.BodyPart;
                    }
                    else
                    {
                        currentBodyPart.AppendBoundary((byte)45);
                        bodyPartState = MimeMultipartParser.BodyPartState.AfterFirstDash;
                        if ((long)++bytesConsumed == num)
                            break;
                        goto case MimeMultipartParser.BodyPartState.AfterFirstDash;
                    }
                case MimeMultipartParser.BodyPartState.AfterFirstDash:
                    if (buffer[bytesConsumed] != (byte)45)
                    {
                        currentBodyPart.ResetBoundary();
                        bodyPartState = MimeMultipartParser.BodyPartState.BodyPart;
                        goto case MimeMultipartParser.BodyPartState.BodyPart;
                    }
                    else
                    {
                        currentBodyPart.AppendBoundary((byte)45);
                        bodyPartState = MimeMultipartParser.BodyPartState.Boundary;
                        if ((long)++bytesConsumed == num)
                            break;
                        goto case MimeMultipartParser.BodyPartState.Boundary;
                    }
                case MimeMultipartParser.BodyPartState.Boundary:
                    int offset2 = bytesConsumed;
                    while (buffer[bytesConsumed] != (byte)13)
                    {
                        if ((long)++bytesConsumed == num)
                        {
                            if (currentBodyPart.AppendBoundary(buffer, offset2, bytesConsumed - offset2))
                            {
                                if (currentBodyPart.IsBoundaryComplete())
                                {
                                    bodyPartState = MimeMultipartParser.BodyPartState.AfterBoundary;
                                    goto label_54;
                                }
                                else
                                    goto label_54;
                            }
                            else
                            {
                                currentBodyPart.ResetBoundary();
                                bodyPartState = MimeMultipartParser.BodyPartState.BodyPart;
                                goto label_54;
                            }
                        }
                    }
                    if (bytesConsumed > offset2 && !currentBodyPart.AppendBoundary(buffer, offset2, bytesConsumed - offset2))
                    {
                        currentBodyPart.ResetBoundary();
                        bodyPartState = MimeMultipartParser.BodyPartState.BodyPart;
                        goto case MimeMultipartParser.BodyPartState.BodyPart;
                    }
                    else
                        goto case MimeMultipartParser.BodyPartState.AfterBoundary;
                case MimeMultipartParser.BodyPartState.AfterBoundary:
                    if (buffer[bytesConsumed] == (byte)45 && !currentBodyPart.IsFinal)
                    {
                        currentBodyPart.AppendBoundary((byte)45);
                        if ((long)++bytesConsumed == num)
                        {
                            bodyPartState = MimeMultipartParser.BodyPartState.AfterSecondDash;
                            break;
                        }
                        goto case MimeMultipartParser.BodyPartState.AfterSecondDash;
                    }
                    else
                    {
                        int offset3 = bytesConsumed;
                        while (buffer[bytesConsumed] != (byte)13)
                        {
                            if ((long)++bytesConsumed == num)
                            {
                                if (!currentBodyPart.AppendBoundary(buffer, offset3, bytesConsumed - offset3))
                                {
                                    currentBodyPart.ResetBoundary();
                                    bodyPartState = MimeMultipartParser.BodyPartState.BodyPart;
                                    goto label_54;
                                }
                                else
                                    goto label_54;
                            }
                        }
                        if (bytesConsumed > offset3 && !currentBodyPart.AppendBoundary(buffer, offset3, bytesConsumed - offset3))
                        {
                            currentBodyPart.ResetBoundary();
                            bodyPartState = MimeMultipartParser.BodyPartState.BodyPart;
                            goto case MimeMultipartParser.BodyPartState.BodyPart;
                        }
                        else if (buffer[bytesConsumed] == (byte)13)
                        {
                            currentBodyPart.AppendBoundary((byte)13);
                            if ((long)++bytesConsumed == num)
                            {
                                bodyPartState = MimeMultipartParser.BodyPartState.AfterSecondCarriageReturn;
                                break;
                            }
                            goto case MimeMultipartParser.BodyPartState.AfterSecondCarriageReturn;
                        }
                        else
                        {
                            currentBodyPart.ResetBoundary();
                            bodyPartState = MimeMultipartParser.BodyPartState.BodyPart;
                            goto case MimeMultipartParser.BodyPartState.BodyPart;
                        }
                    }
                case MimeMultipartParser.BodyPartState.AfterSecondDash:
                    if (buffer[bytesConsumed] == (byte)45)
                    {
                        currentBodyPart.AppendBoundary((byte)45);
                        ++bytesConsumed;
                        if (currentBodyPart.IsBoundaryComplete())
                        {
                            bodyPartState = MimeMultipartParser.BodyPartState.AfterBoundary;
                            state = MimeMultipartParser.State.NeedMoreData;
                            break;
                        }
                        currentBodyPart.ResetBoundary();
                        if ((long)bytesConsumed != num)
                            goto case MimeMultipartParser.BodyPartState.BodyPart;
                        else
                            break;
                    }
                    else
                    {
                        currentBodyPart.ResetBoundary();
                        bodyPartState = MimeMultipartParser.BodyPartState.BodyPart;
                        goto case MimeMultipartParser.BodyPartState.BodyPart;
                    }
                case MimeMultipartParser.BodyPartState.AfterSecondCarriageReturn:
                    if (buffer[bytesConsumed] != (byte)10)
                    {
                        currentBodyPart.ResetBoundary();
                        bodyPartState = MimeMultipartParser.BodyPartState.BodyPart;
                        goto case MimeMultipartParser.BodyPartState.BodyPart;
                    }
                    else
                    {
                        currentBodyPart.AppendBoundary((byte)10);
                        ++bytesConsumed;
                        bodyPartState = MimeMultipartParser.BodyPartState.BodyPart;
                        if (currentBodyPart.IsBoundaryComplete())
                        {
                            state = MimeMultipartParser.State.BodyPartCompleted;
                            break;
                        }
                        currentBodyPart.ResetBoundary();
                        if ((long)bytesConsumed != num)
                            goto case MimeMultipartParser.BodyPartState.BodyPart;
                        else
                            break;
                    }
            }
            label_54:
            if (offset1 < bytesConsumed)
            {
                int boundaryDelta = currentBodyPart.BoundaryDelta;
                if (boundaryDelta > 0 && state != MimeMultipartParser.State.BodyPartCompleted)
                    currentBodyPart.HasPotentialBoundaryLeftOver = true;
                int count = bytesConsumed - offset1 - boundaryDelta;
                currentBodyPart.BodyPart = new ArraySegment<byte>(buffer, offset1, count);
            }
            totalBytesConsumed += (long)(bytesConsumed - offset1);
            return state;
        }

        private enum BodyPartState
        {
            BodyPart,
            AfterFirstCarriageReturn,
            AfterFirstLineFeed,
            AfterFirstDash,
            Boundary,
            AfterBoundary,
            AfterSecondDash,
            AfterSecondCarriageReturn,
        }

        private enum MessageState
        {
            Boundary,
            BodyPart,
            CloseDelimiter,
        }

        public enum State
        {
            NeedMoreData,
            BodyPartCompleted,
            Invalid,
            DataTooBig,
        }

        [DebuggerDisplay("{DebuggerToString()}")]
        private class CurrentBodyPartStore
        {
            private byte[] _boundaryStore = new byte[256];
            private byte[] _referenceBoundary = new byte[256];
            private byte[] _boundary = new byte[256];
            private ArraySegment<byte> _bodyPart = MimeMultipartParser._emptyBodyPart;
            private bool _isFirst = true;
            private const int InitialOffset = 2;
            private int _boundaryStoreLength;
            private int _referenceBoundaryLength;
            private int _boundaryLength;
            private bool _isFinal;
            private bool _releaseDiscardedBoundary;
            private int _boundaryOffset;

            public CurrentBodyPartStore(string referenceBoundary)
            {
                this._referenceBoundary[0] = (byte)13;
                this._referenceBoundary[1] = (byte)10;
                this._referenceBoundary[2] = (byte)45;
                this._referenceBoundary[3] = (byte)45;
                this._referenceBoundaryLength = 4 + Encoding.UTF8.GetBytes(referenceBoundary, 0, referenceBoundary.Length, this._referenceBoundary, 4);
                this._boundary[0] = (byte)13;
                this._boundary[1] = (byte)10;
                this._boundaryLength = 2;
            }

            public bool HasPotentialBoundaryLeftOver { get; set; }

            public int BoundaryDelta
            {
                get
                {
                    if (this._boundaryLength - this._boundaryOffset <= 0)
                        return this._boundaryLength;
                    return this._boundaryLength - this._boundaryOffset;
                }
            }

            public ArraySegment<byte> BodyPart
            {
                get
                {
                    return this._bodyPart;
                }
                set
                {
                    this._bodyPart = value;
                }
            }

            public bool IsFinal
            {
                get
                {
                    return this._isFinal;
                }
            }

            public void ResetBoundaryOffset()
            {
                this._boundaryOffset = this._boundaryLength;
            }

            public void ResetBoundary()
            {
                if (this.HasPotentialBoundaryLeftOver)
                {
                    Buffer.BlockCopy((Array)this._boundary, 0, (Array)this._boundaryStore, 0, this._boundaryOffset);
                    this._boundaryStoreLength = this._boundaryOffset;
                    this.HasPotentialBoundaryLeftOver = false;
                    this._releaseDiscardedBoundary = true;
                }
                this._boundaryLength = 0;
                this._boundaryOffset = 0;
            }

            public void AppendBoundary(byte data)
            {
                this._boundary[this._boundaryLength++] = data;
            }

            public bool AppendBoundary(byte[] data, int offset, int count)
            {
                if (this._boundaryLength + count > this._referenceBoundaryLength + 6)
                    return false;
                int boundaryLength = this._boundaryLength;
                Buffer.BlockCopy((Array)data, offset, (Array)this._boundary, this._boundaryLength, count);
                this._boundaryLength += count;
                for (int index = Math.Min(this._boundaryLength, this._referenceBoundaryLength); boundaryLength < index; ++boundaryLength)
                {
                    if ((int)this._boundary[boundaryLength] != (int)this._referenceBoundary[boundaryLength])
                        return false;
                }
                return true;
            }

            public ArraySegment<byte> GetDiscardedBoundary()
            {
                if (this._boundaryStoreLength <= 0 || !this._releaseDiscardedBoundary)
                    return MimeMultipartParser._emptyBodyPart;
                ArraySegment<byte> arraySegment = new ArraySegment<byte>(this._boundaryStore, 0, this._boundaryStoreLength);
                this._boundaryStoreLength = 0;
                return arraySegment;
            }

            public bool IsBoundaryValid()
            {
                int num = 0;
                if (this._isFirst)
                    num = 2;
                int index;
                for (index = num; index < this._referenceBoundaryLength; ++index)
                {
                    if ((int)this._boundary[index] != (int)this._referenceBoundary[index])
                        return false;
                }
                bool flag = false;
                if (this._boundary[index] == (byte)45 && this._boundary[index + 1] == (byte)45)
                {
                    flag = true;
                    index += 2;
                }
                for (; index < this._boundaryLength - 2; ++index)
                {
                    if (this._boundary[index] != (byte)32 && this._boundary[index] != (byte)9)
                        return false;
                }
                this._isFinal = flag;
                this._isFirst = false;
                return true;
            }

            public bool IsBoundaryComplete()
            {
                return this.IsBoundaryValid() && this._boundaryLength >= this._referenceBoundaryLength && (this._boundaryLength != this._referenceBoundaryLength + 1 || this._boundary[this._referenceBoundaryLength] != (byte)45);
            }

            public void ClearBodyPart()
            {
                this.BodyPart = MimeMultipartParser._emptyBodyPart;
            }

            public void ClearAll()
            {
                this._releaseDiscardedBoundary = false;
                this.HasPotentialBoundaryLeftOver = false;
                this._boundaryLength = 0;
                this._boundaryOffset = 0;
                this._boundaryStoreLength = 0;
                this._isFinal = false;
                this.ClearBodyPart();
            }

            private string DebuggerToString()
            {
                return string.Format((IFormatProvider)CultureInfo.InvariantCulture, "Expected: {0} *** Current: {1}", (object)Encoding.UTF8.GetString(this._referenceBoundary, 0, this._referenceBoundaryLength), (object)Encoding.UTF8.GetString(this._boundary, 0, this._boundaryLength));
            }
        }
    }
}