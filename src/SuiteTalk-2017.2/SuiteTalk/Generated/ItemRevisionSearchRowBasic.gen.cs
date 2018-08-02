//~ Generated by SearchStubs/SearchRowBasic

using System;
using System.Collections.Generic;

namespace SuiteTalk
{
    partial class ItemRevisionSearchRowBasic
    {
        // TODO: Make this Lazy and Weak referenced so that it can be garbaged collected
        private static readonly Lazy<ColumnFactory> columnFactoryLoader = new Lazy<ColumnFactory>(() => new ColumnFactory());

        public override void SetColumns(string[] columnNames)
        {
            var factory = columnFactoryLoader.Value;
            for (int i = 0; i < columnNames.Length; i++)
            {
                factory.BuildColumn(this, columnNames[i]);
            }
        }

        class ColumnFactory:  ColumnFactory<ItemRevisionSearchRowBasic>
        {
            protected override Dictionary<string, Action<ItemRevisionSearchRowBasic>> InitializeColumnBuilders()
            {
                return new Dictionary<string, Action<ItemRevisionSearchRowBasic>> {
                    { "effectiveDate", r => r.@effectiveDate = new [] { new SearchColumnDateField { customLabel = "effectiveDate" } } },
                    { "externalId", r => r.@externalId = new [] { new SearchColumnSelectField { customLabel = "externalId" } } },
                    { "internalId", r => r.@internalId = new [] { new SearchColumnSelectField { customLabel = "internalId" } } },
                    { "isInactive", r => r.@isInactive = new [] { new SearchColumnBooleanField { customLabel = "isInactive" } } },
                    { "item", r => r.@item = new [] { new SearchColumnSelectField { customLabel = "item" } } },
                    { "memo", r => r.@memo = new [] { new SearchColumnStringField { customLabel = "memo" } } },
                    { "name", r => r.@name = new [] { new SearchColumnStringField { customLabel = "name" } } },
                    { "obsoleteDate", r => r.@obsoleteDate = new [] { new SearchColumnDateField { customLabel = "obsoleteDate" } } },
                };
            }
        }
    }
}