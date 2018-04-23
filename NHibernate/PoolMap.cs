﻿using FluentNHibernate.Mapping;

namespace Ochs
{
    public class PoolMap : ClassMap<Pool>
    {
        public PoolMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            HasMany(x => x.Matches).Inverse();
            HasManyToMany(x => x.Fighters);
            References(x => x.Phase).Not.LazyLoad();
            Map(x => x.Location);
        }
    }
}