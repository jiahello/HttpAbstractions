// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.WebUtilities
{
    public static class ByteRangeHelper
    {
        // 14.35.1 Byte Ranges - If a syntactically valid byte-range-set includes at least one byte-range-spec whose
        // first-byte-pos is less than the current length of the entity-body, or at least one suffix-byte-range-spec
        // with a non-zero suffix-length, then the byte-range-set is satisfiable.
        // Adjusts ranges to be absolute and within bounds.
        public static IList<RangeItemHeaderValue> NormalizeRanges(ICollection<RangeItemHeaderValue> ranges, long length)
        {
            var normalizedRanges = new List<RangeItemHeaderValue>(ranges.Count);
            if (length == 0)
            {
                return normalizedRanges;
            }
            foreach (var range in ranges)
            {
                var start = range.From;
                var end = range.To;

                // X-[Y]
                if (start.HasValue)
                {
                    if (start.Value >= length)
                    {
                        // Not satisfiable, skip/discard.
                        continue;
                    }
                    if (!end.HasValue || end.Value >= length)
                    {
                        end = length - 1;
                    }
                }
                else
                {
                    // suffix range "-X" e.g. the last X bytes, resolve
                    if (end.Value == 0)
                    {
                        // Not satisfiable, skip/discard.
                        continue;
                    }

                    var bytes = Math.Min(end.Value, length);
                    start = length - bytes;
                    end = start + bytes - 1;
                }
                normalizedRanges.Add(new RangeItemHeaderValue(start.Value, end.Value));
            }
            return normalizedRanges;
        }
    }
}
