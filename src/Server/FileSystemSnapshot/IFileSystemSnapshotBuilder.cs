// Copyright 2013 The Chromium Authors. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.

using System.Collections.Generic;
using VsChromium.Core.Files;
using VsChromium.Server.FileSystemNames;

namespace VsChromium.Server.FileSystemSnapshot {
  public interface IFileSystemSnapshotBuilder {
    FileSystemTreeSnapshot Compute(
      IFileSystemNameFactory fileNameFactory, 
      FileSystemTreeSnapshot oldSnapshot,
      FullPathChanges pathChanges /* may be null */,
      List<FullPath> rootFiles, 
      int version);
  }
}