﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;

#if !CODE_STYLE
using Microsoft.CodeAnalysis.Options;
#endif

namespace Microsoft.CodeAnalysis.Formatting.Rules
{
    /// <summary>
    /// Represents a next operation to run in a continuation style chaining.
    /// </summary>
    internal readonly struct NextAction<TArgument>
    {
        private readonly int _index;
        private readonly SyntaxNode _node;
        private readonly ActionCache<TArgument> _actionCache;

        public NextAction(int index, SyntaxNode node, in ActionCache<TArgument> actionCache)
        {
            _index = index;
            _node = node;
            _actionCache = actionCache;
        }

        public void Invoke(List<TArgument> arguments)
        {
            _actionCache.Continuation(_index, arguments, _node, _actionCache);
        }
    }

    internal readonly struct NextSuppressOperationAction
    {
        private readonly ImmutableArray<AbstractFormattingRule> _formattingRules;
        private readonly int _index;
        private readonly SyntaxNode _node;
        private readonly OptionSet _optionSet;
        private readonly List<SuppressOperation> _list;

        public NextSuppressOperationAction(
            ImmutableArray<AbstractFormattingRule> formattingRules,
            int index,
            SyntaxNode node,
            OptionSet optionSet,
            List<SuppressOperation> list)
        {
            _formattingRules = formattingRules;
            _index = index;
            _node = node;
            _optionSet = optionSet;
            _list = list;
        }

        private NextSuppressOperationAction NextAction
            => new NextSuppressOperationAction(_formattingRules, _index + 1, _node, _optionSet, _list);

        public void Invoke()
        {
            // If we have no remaining handlers to execute, then we'll execute our last handler
            if (_index >= _formattingRules.Length)
            {
                return;
            }
            else
            {
                // Call the handler at the index, passing a continuation that will come back to here with index + 1
                _formattingRules[_index].AddSuppressOperations(_list, _node, _optionSet, NextAction);
                return;
            }
        }
    }
}
