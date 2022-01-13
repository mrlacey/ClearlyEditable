// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;

namespace ClearlyEditable
{
    public static class StringExtensions
    {
        public static bool ContainsAnyOf(this string value, params string[] filters)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            if (filters.Length > 0)
            {
                return filters.Any(f => !string.IsNullOrWhiteSpace(f) && value.ToUpperInvariant().Contains(f.ToUpperInvariant()));
            }

            return false;
        }
    }
}
