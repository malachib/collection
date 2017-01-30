using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fact.Extensions.Factories
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    /// <remarks>
    /// FIX: Thinking maybe this should be called AggregateFactory, sticking with the "type as a suffix" paradigm
    /// </remarks>
    public class FactoryAggregator<TInput, TOutput> : IFactoryWithMeta<TInput, TOutput>
    {
        LinkedList<IFactory<TInput, TOutput>> candidates = new LinkedList<IFactory<TInput, TOutput>>();

        public IFactory<TInput, TOutput> GetCandidate(TInput id)
        {
            return candidates.FirstOrDefault(c => c.CanCreate(id));
            /*
            foreach (var c in candidates)
            {
                if (c.CanCreate(id))
                    return c;
            }

            return null;*/
        }

        internal class Meta
        {
            internal IFactory<TInput, TOutput> candidate;
            internal object meta; // meta associated with the found candidate linked to the ID
#if DEBUG
            internal TInput id;
#endif
        }

        IFactory<TInput, TOutput> GetCandidate(TInput id, out object meta)
        {
            foreach (var c in candidates)
            {
                var c_meta = c as IFactoryWithMeta<TInput, TOutput>;

                if (c_meta != null)
                {
                    if (c_meta.CanCreate(id, out meta))
                        return c;
                }
                else
                {
                    if (c.CanCreate(id))
                    {
                        meta = null;
                        return c;
                    }
                }
            }

            meta = null;
            return null;
        }

        public bool CanCreate(TInput id)
        {
            return GetCandidate(id) != null;
        }


        public bool CanCreate(TInput id, out object meta)
        {
            var c = GetCandidate(id, out meta);
            if (c != null)
            {
                var _meta = new Meta() { meta = meta, candidate = c };
#if DEBUG
                _meta.id = id;
#endif
                meta = _meta;
                return true;
            }
            return false;
        }

        public TOutput Create(TInput id)
        {
            var c = GetCandidate(id);

            if (c != null) return c.Create(id);

            throw new KeyNotFoundException();
        }


        public TOutput Create(TInput id, object meta)
        {
            var _meta = meta as Meta;

            if (_meta != null)
            {
                var c_meta = _meta.candidate as IFactoryWithMeta<TInput, TOutput>;

                if (c_meta != null)
                {
#if DEBUG
                    if (!Object.Equals(_meta.id, id))
                        throw new ArgumentOutOfRangeException();
#endif
                    return c_meta.Create(id, _meta.meta);
                }
                else
                {
                    return _meta.candidate.Create(id);
                }
            }

            var c = GetCandidate(id);

            if (c != null) return c.Create(id);

            throw new KeyNotFoundException();
        }

        public void Add(IFactory<TInput, TOutput> candidate)
        {
            candidates.AddLast(candidate);
        }
    }
}
