using System;
using System.Collections.Generic;

namespace Fact.Extensions.Factories
{
    public class DelegateFactory<TInput, TOutput> : IFactory<TInput, TOutput>
    {
        readonly Func<TInput, bool> canCreate;
        readonly Func<TInput, TOutput> create;

        public DelegateFactory(Func<TInput, bool> canCreate, Func<TInput, TOutput> create)
        {
            this.canCreate = canCreate;
            this.create = create;
        }

        public bool CanCreate(TInput id)
        {
            return canCreate(id);
        }

        public TOutput Create(TInput id)
        {
            return create(id);
        }
    }
}
