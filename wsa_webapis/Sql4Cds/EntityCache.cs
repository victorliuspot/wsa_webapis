﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;

namespace Sql4Cds
{
    static class EntityCache
    {
        private static IDictionary<IOrganizationService, EntityMetadata[]> _cache = new Dictionary<IOrganizationService, EntityMetadata[]>();
        private static ISet<IOrganizationService> _loading = new HashSet<IOrganizationService>();

        public static EntityMetadata[] GetEntities(IOrganizationService org)
        {
            if (!_cache.TryGetValue(org, out var entities))
            {
                entities = ((RetrieveMetadataChangesResponse)org.Execute(new RetrieveMetadataChangesRequest
                {
                    Query = new EntityQueryExpression
                    {
                        Properties = new MetadataPropertiesExpression
                        {
                            PropertyNames =
                            {
                                nameof(EntityMetadata.LogicalName),
                                nameof(EntityMetadata.DisplayName),
                                nameof(EntityMetadata.Description)
                            }
                        }
                    }
                })).EntityMetadata
                .ToArray();

                _cache[org] = entities;
            }

            return entities;
        }

        public static bool TryGetEntities(IOrganizationService org, out EntityMetadata[] entities)
        {
            if (_cache.TryGetValue(org, out entities))
                return true;

            if (_loading.Add(org))
                Task.Run(() => GetEntities(org));

            return false;
        }
    }
}