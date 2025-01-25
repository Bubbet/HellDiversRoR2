using UnityEngine;

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

		public static void Align(this RectTransform rect, Vector2 vector2)
		{
			rect.pivot = new Vector2(0, 0.5f);
			rect.anchorMin = rect.pivot;
			rect.anchorMax = rect.pivot;
		}

		public static Color ParseHtmlColor(string hex)
		{
			return ColorUtility.TryParseHtmlString(hex, out var color) ? color : throw new Exception("Failed to parse infallible color.");
		}
	}
}