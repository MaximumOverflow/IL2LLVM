namespace IL2LLVM.Extensions;

public static class EnumerableExtensions
{
	public static TOut[] ArraySelect<TIn, TOut>(this IReadOnlyCollection<TIn> collection, Func<TIn, TOut> fn)
	{
		var i = 0;
		var elements = new TOut[collection.Count];
		foreach (var @in in collection) elements[i++] = fn(@in);
		return elements;
	}
	
	public static TOut[] ArraySelect<TIn, TOut>(this IReadOnlyCollection<TIn> collection, Func<TIn, int, TOut> fn)
	{
		var i = 0;
		var elements = new TOut[collection.Count];
		foreach (var @in in collection) elements[i] = fn(@in, i++);
		return elements;
	}

	public static void Populate<T>(this IList<T> collection, Func<int, T> fn)
		=> Populate(collection, 0, fn);
	
	public static void Populate<T>(this IList<T> collection, int start, Func<int, T> fn)
	{
		for (var i = start; i < collection.Count; i++)
			collection[i] = fn(i);
	}
	
	public static void RevPopulate<T>(this IList<T> collection, Func<int, T> fn)
		=> RevPopulate(collection, 0, fn);
	
	public static void RevPopulate<T>(this IList<T> collection, int start, Func<int, T> fn)
	{
		for (var i = collection.Count - 1; i >= start; i--)
			collection[i] = fn(i);
	}

	public static int IndexOf<T>(this IReadOnlyList<T> collection, T obj) where T : class
	{
		for (var i = 0; i < collection.Count; i++)
			if (collection[i] == obj) return i;

		return -1;
	}
}