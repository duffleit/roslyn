﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax
{
    //This used to implement IEnumerable and have more functionality, but it was not tested or used.
    internal struct SyntaxDiagnosticInfoList
    {
        private readonly GreenNode _node;

        internal SyntaxDiagnosticInfoList(GreenNode node)
        {
            _node = node;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_node);
        }

        internal bool Any(Func<DiagnosticInfo, bool> predicate)
        {
            var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (predicate(enumerator.Current))
                    return true;
            }

            return false;
        }

        public struct Enumerator
        {
            private struct NodeIteration
            {
                internal GreenNode Node;
                internal int DiagnosticIndex;
                internal int SlotIndex;

                internal NodeIteration(GreenNode node)
                {
                    this.Node = node;
                    this.SlotIndex = -1;
                    this.DiagnosticIndex = -1;
                }
            }

            private NodeIteration[] _stack;
            private int _count;
            private DiagnosticInfo _current;

            internal Enumerator(GreenNode node)
            {
                _current = null;
                _stack = null;
                _count = 0;
                if (node != null && node.ContainsDiagnostics)
                {
                    _stack = new NodeIteration[8];
                    this.PushNodeOrToken(node);
                }
            }

            public bool MoveNext()
            {
                while (_count > 0)
                {
                    var diagIndex = _stack[_count - 1].DiagnosticIndex;
                    var node = _stack[_count - 1].Node;
                    var diags = node.GetDiagnostics();
                    if (diagIndex < diags.Length - 1)
                    {
                        diagIndex++;
                        _current = diags[diagIndex];
                        _stack[_count - 1].DiagnosticIndex = diagIndex;
                        return true;
                    }

                    var slotIndex = _stack[_count - 1].SlotIndex;
                tryAgain:
                    if (slotIndex < node.SlotCount - 1)
                    {
                        slotIndex++;
                        var child = node.GetSlot(slotIndex);
                        if (child == null || !child.ContainsDiagnostics)
                        {
                            goto tryAgain;
                        }

                        _stack[_count - 1].SlotIndex = slotIndex;
                        this.PushNodeOrToken(child);
                    }
                    else
                    {
                        this.Pop();
                    }
                }

                return false;
            }

            private void PushNodeOrToken(GreenNode node)
            {
                var token = node as SyntaxToken;
                if (token != null)
                {
                    this.PushToken(token);
                }
                else
                {
                    this.Push(node);
                }
            }

            private void PushToken(SyntaxToken token)
            {
                var trailing = token.GetTrailingTrivia();
                if (trailing != null)
                {
                    this.Push(trailing);
                }

                this.Push(token);
                var leading = token.GetLeadingTrivia();
                if (leading != null)
                {
                    this.Push(leading);
                }
            }

            private void Push(GreenNode node)
            {
                if (_count >= _stack.Length)
                {
                    var tmp = new NodeIteration[_stack.Length * 2];
                    Array.Copy(_stack, tmp, _stack.Length);
                    _stack = tmp;
                }

                _stack[_count] = new NodeIteration(node);
                _count++;
            }

            private void Pop()
            {
                _count--;
            }

            public DiagnosticInfo Current
            {
                get { return _current; }
            }
        }
    }
}
