﻿using System;

namespace Bouyei.ProviderFactory
{
    using DbEntityProvider;

    public class LayerOrm : EntityProvider
    {
        public static LayerOrm CreateLayerOrm(string DbConnection = null)
        {
            return new LayerOrm(DbConnection);
        }

        public LayerOrm(string DbConnection = null)
            : base(DbConnection)
        { }
    }
}
