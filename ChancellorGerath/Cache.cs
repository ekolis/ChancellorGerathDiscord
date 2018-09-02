using System;
using System.Collections.Generic;

namespace ChancellorGerath
{
	public static class Cache
	{
		public static ISet<ICache> All { get; } = new HashSet<ICache>();

		/// <summary>
		/// Reloads all caches' data.
		/// </summary>
		public static void ReloadAll()
		{
			foreach (var cache in All)
				cache.Reload();
		}
	}

	/// <summary>
	/// A cache of data that can be lazy-loaded and reloaded on demand.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Cache<T> : ICache
	{
		public Cache(T data)
		{
			isLoaded = true;
			this.data = data;
			Cache.All.Add(this);
		}

		public Cache(Func<T> loader)
		{
			this.loader = loader;
			Cache.All.Add(this);
		}

		/// <summary>
		/// The data stored in the cache. Loads if necessary.
		/// </summary>
		public T Data
		{
			get
			{
				if (!isLoaded)
				{
					data = loader();
					isLoaded = true;
				}
				return data;
			}
		}

		object ICache.Data => Data;
		private T data;
		private bool isLoaded = false;
		private Func<T> loader;

		public static implicit operator Cache<T>(T obj)
		{
			return new Cache<T>(obj);
		}

		public static implicit operator Cache<T>(Func<T> f)
		{
			return new Cache<T>(f);
		}

		public static implicit operator Func<T>(Cache<T> cache)
		{
			return cache.loader ?? (() => cache.Data);
		}

		public static implicit operator T(Cache<T> cache)
		{
			return cache.Data;
		}

		/// <summary>
		/// Reloads the data.
		/// </summary>
		/// <returns>true if data exists, otherwise false.</returns>
		public bool Reload()
		{
			if (loader != null)
			{
				loader();
				return true;
			}
			else
				return data != null;
		}
	}

	public interface ICache
	{
		object Data { get; }

		bool Reload();
	}
}