using System;

namespace Ochs
{
    public class PoolView
    {
        protected Pool _pool;
        public PoolView(Pool pool)
        {
            _pool = pool;
        }

        public Guid Id => _pool.Id;
        public string Name => _pool.Name;
        public string Location => _pool.Location;
        public string Phase => _pool.Phase?.Name;
        public Guid? PhaseId => _pool.Phase?.Id;

    }
}