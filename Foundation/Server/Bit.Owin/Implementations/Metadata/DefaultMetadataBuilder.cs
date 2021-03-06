﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Bit.Owin.Contracts.Metadata;

namespace Bit.Owin.Implementations.Metadata
{
    public class DefaultMetadataBuilder : IMetadataBuilder
    {
        public virtual ICollection<ObjectMetadata> AllMetadata { get; protected set; } = new Collection<ObjectMetadata>();

        public virtual async Task<IEnumerable<ObjectMetadata>> BuildMetadata()
        {
            return AllMetadata;
        }
    }
}
