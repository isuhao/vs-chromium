﻿// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Linq;
using VsChromium.Core.Ipc.TypedMessages;
using VsChromium.Core.Win32.Memory;
using VsChromium.Server.NativeInterop;

namespace VsChromium.Server.Search {
  public class UTF16FileContents : FileContents {
    private readonly SafeHeapBlockHandle _heap;

    public UTF16FileContents(SafeHeapBlockHandle heap, DateTime utcLastWriteTime)
      : base(utcLastWriteTime) {
      _heap = heap;
    }

    public override long ByteLength { get { return _heap.ByteLength; } }

    private IntPtr Pointer { get { return _heap.Pointer; } }
    private int CharacterCount { get { return (int)this.ByteLength / 2; } }

    public static UTF16StringSearchAlgorithm CreateSearchAlgo(string pattern, NativeMethods.SearchOptions searchOptions) {
      return new StrStrWStringSearchAlgorithm(pattern, searchOptions);
    }

    public override List<FilePositionSpan> Search(SearchContentsData searchContentsData) {
      if (searchContentsData.ParsedSearchString.MainEntry.Text.Length > ByteLength)
        return NoSpans;

      var algo = searchContentsData.UTF16StringSearchAlgo;
      // TODO(rpaquay): We are limited to 2GB for now.
      var result = algo.SearchAll(_heap.Pointer, (int)_heap.ByteLength);
      if (searchContentsData.ParsedSearchString.EntriesBeforeMainEntry.Count == 0 &&
          searchContentsData.ParsedSearchString.EntriesAfterMainEntry.Count == 0) {
        return result.ToList();
      }

      return FilterOnOtherEntries(searchContentsData.ParsedSearchString, algo.MatchCase, result).ToList();
    }

    private unsafe IEnumerable<FilePositionSpan> FilterOnOtherEntries(ParsedSearchString parsedSearchString, bool matchCase, IEnumerable<FilePositionSpan> matches) {
      var start = (char *)Pointers.Add(this.Pointer, 0);
      Func<int, char> getCharacter = position => *(start + position);
      return new TextSourceTextSearch(this.CharacterCount, getCharacter).FilterOnOtherEntries(parsedSearchString, matchCase, matches);
    }
  }
}
