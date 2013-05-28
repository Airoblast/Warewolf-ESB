﻿using System;
using System.Collections.Generic;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.Hosting
{
    public interface IResourceIterator
    {
        void IterateAll(Guid workspaceID, Func<ResourceIteratorResult, bool> action, params ResourceDelimiter[] delimiters);
        void Iterate(IEnumerable<string> folders, Guid workspaceID, Func<ResourceIteratorResult, bool> action, params ResourceDelimiter[] delimiters);
    }
}