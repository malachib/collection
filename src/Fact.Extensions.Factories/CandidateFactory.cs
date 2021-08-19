using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fact.Extensions.Factories
{
	public struct Candidate<T> : IComparable<Candidate<T>>
		where T : class
	{
		readonly Func<T> factory;

		T instance;

		/// <summary>
		/// Reason why confidence level is what it is
		/// Can be null
		/// </summary>
		public string Reason;

		public readonly ushort Confidence;
		/// <summary>
		/// True means data was adjusted, even as minimally as removing whitespace
		/// False means key was perfectly usable as is
		/// null means no record kept as to which happened
		/// </summary>
		public readonly bool? Coerced;

		public T Instance
		{
			get
			{
				if (instance == default(T) && factory != null) instance = factory();

				return instance;
			}
		}

		public Candidate(T instance, ushort confidence, bool? coerced = null, Func<T> factory = null, string reason = null)
		{
			this.instance = instance;
			Confidence = confidence;
			Coerced = coerced;
			this.factory = factory;
			Reason = reason;
		}

		public int CompareTo(Candidate<T> other)
		{
			var diff = Confidence - other.Confidence;
			return diff;
		}
	}

	/// <summary>
	/// Unlike a regular factory, candidate factories may coerce and adjust keys
	/// in a best attempt to isolate what object to create from them
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="T"></typeparam>
	public interface ICandidateFactory<TKey, T>
		where T : class
	{
		/// <summary>
		/// Given a key and optional context, produce a list of candidates
		/// </summary>
		/// <param name="key"></param>
		/// <param name="context"></param>
		/// <returns>never null.  May not be sorted</returns>
		IEnumerable<Candidate<T>> Create(TKey key, object context = null);
	}


	public static class CandidateFactoryExtensions
	{
		/// <summary>
		/// Choose top confidence candidate
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="T"></typeparam>
		/// <param name="factory"></param>
		/// <param name="key"></param>
		/// <param name="context"></param>
		/// <returns>If no good candidates found, returns a 0 confidence candidate with a null instance</returns>
		public static Candidate<T> Elect<TKey, T>(this ICandidateFactory<TKey, T> factory, TKey key, object context = null)
			where T : class
		{
			IEnumerable<Candidate<T>> candidates = factory.Create(key, context);

			Candidate<T>? candidate = candidates?.Max();

			return candidate ?? new Candidate<T>();
		}
	}
}
