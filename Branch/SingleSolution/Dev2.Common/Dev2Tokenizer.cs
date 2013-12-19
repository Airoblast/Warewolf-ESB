﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.StringTokenizer.Interfaces;

namespace Dev2.Common
{
    internal class Dev2Tokenizer : IDev2Tokenizer, IDisposable
    {

        private readonly char[] _tokenParts;
        readonly CharEnumerator _charEnumerator;
        private readonly IList<IDev2SplitOp> _ops;
        private readonly bool _isReversed;
        private readonly int _masterLen;

        private int _opPointer;
        private int _startIdx;
        private bool _hasMoreOps;

        private bool _useEnumerator;

        private bool _disposing;

        internal Dev2Tokenizer(string candiateString, IList<IDev2SplitOp> ops, bool reversed)
        {
            // only build if we are using a non-single token op set ;)
            
            _ops = ops;
            _isReversed = reversed;
            _useEnumerator = CanUseEnumerator();
            _masterLen = candiateString.Length;

            // we need the char array :( - non optomized
            if(!_useEnumerator)
            {
                _tokenParts = candiateString.ToCharArray();
            }
            else
            {
                _charEnumerator = candiateString.GetEnumerator();    
            }
            

            _opPointer = 0;
            _hasMoreOps = true;
            

            if (!_isReversed)
            {
                _startIdx = 0;
            }
            else
            {
                _startIdx = _tokenParts.Length - 1;
            }
        }

        #region Private Method

        /// <summary>
        /// Determines whether this instance [can use enumerator].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance [can use enumerator]; otherwise, <c>false</c>.
        /// </returns>
        private bool CanUseEnumerator()
        {
            bool result = false;

            if(_ops != null)
            {
                // are all the ops token based?!
                if(_ops.Count( op=> op.CanUseEnumerator(_isReversed)) == _ops.Count)
                {
                    result = true;
                }
            }

            return result;
        }

        /// <summary>
        /// Moves the op pointer.
        /// </summary>
        private void MoveOpPointer()
        {
            _opPointer++;

            if (_opPointer >= _ops.Count)
            {
                _opPointer = 0;
            }
        }

        /// <summary>
        /// Moves the start index.
        /// </summary>
        /// <param name="newOffSet">The new off set.</param>
        private void MoveStartIndex(int newOffSet)
        {
            if (!_isReversed)
            {
                _startIdx += newOffSet;
            }
            else
            {
                _startIdx -= newOffSet;
            }
        }

        /// <summary>
        /// Determines whether [has more data].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [has more data]; otherwise, <c>false</c>.
        /// </returns>
        private bool HasMoreData()
        {
            bool result;

            if(!_isReversed)
            {
                result = (_startIdx < _masterLen);
            }
            else
            {
                result = (_startIdx >= 0);
            }

            return result;
        }

        /// <summary>
        /// Remainders to string.
        /// </summary>
        /// <returns></returns>
        private string RemainderToString()
        {
            StringBuilder result = new StringBuilder();

            if(!_useEnumerator)
            {
                for(int i = _startIdx; i < _tokenParts.Length; i++)
                {
                    result.Append(_tokenParts[i]);
                }
            }
            else
            {
                try
                {
                    while(_charEnumerator.MoveNext())
                    {
                        result.Append(_charEnumerator.Current);
                    }
                }
                catch (Exception)
                {
                    // _charEnumerator will return null reference exception when done ;)
                }
            }

            MoveStartIndex(result.Length);
            _hasMoreOps = false;

            return result.ToString();
        }

        #endregion

        /// <summary>
        /// Determines whether [has more ops].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [has more ops]; otherwise, <c>false</c>.
        /// </returns>
        public bool HasMoreOps()
        {
            return _hasMoreOps;
        }

        /// <summary>
        /// Nexts the token.
        /// </summary>
        /// <returns></returns>
        public string NextToken()
        {
            string result = string.Empty;

            try
            {
                // we can be smart about the operations ;)
                if(_useEnumerator)
                {
                    result = _ops[_opPointer].ExecuteOperation(_charEnumerator, _startIdx, _masterLen, _isReversed);
                }
                else
                {
                    result = _ops[_opPointer].ExecuteOperation(_tokenParts, _startIdx, _isReversed);
                }

                MoveStartIndex((result.Length + _ops[_opPointer].OpLength()));
                MoveOpPointer();
                // check to see if there is data to fetch still?
                _hasMoreOps = (!_ops[_opPointer].IsFinalOp() & HasMoreData());
            }
            catch
            {
                // error, return remaining portion of the string
                result = RemainderToString();
                throw;
            }

            return result;
        }


        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposing)
            {
                if (disposing)
                {
                    _charEnumerator.Dispose();
                }

                // shared cleanup logic
                _disposing = true;
            }
        }
    }
}
