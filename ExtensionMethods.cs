namespace HellDiver
{
	public static class ExtensionMethods
	{
		public static bool SequenceContains<T>(this IEnumerable<T> source, IEnumerable<T> sequence)
		{
			var sourceArray = sequence as T[] ?? sequence.ToArray();
			var sequenceArray = source as T[] ?? source.ToArray();
			return sourceArray.Length <= sequenceArray.Length && sequenceArray.Zip(sourceArray, (arg1, arg2) => (arg1, arg2)).All(pair => pair.arg1!.Equals(pair.arg2));
		}
	}
}