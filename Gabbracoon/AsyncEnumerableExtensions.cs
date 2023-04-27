using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabbracoon
{
	public static class AsyncEnumerableExtensions
	{
		public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> items,
			CancellationToken cancellationToken = default) {
			var results = new List<T>();
			await foreach (var item in items.WithCancellation(cancellationToken)
											.ConfigureAwait(false)) {
				results.Add(item);
			}
			return results;
		}

		public static async Task<Dictionary<Key, Value>> ToDictionaryAsync<T, Key, Value>(this IAsyncEnumerable<T> items, Func<T, (Key, Value)> func,
	CancellationToken cancellationToken = default) {
			var results = new Dictionary<Key,Value>();
			await foreach (var item in items.WithCancellation(cancellationToken)
											.ConfigureAwait(false)) {
				var newElement = func(item);
				results.Add(newElement.Item1, newElement.Item2);
			}
			return results;
		}
	}
}
