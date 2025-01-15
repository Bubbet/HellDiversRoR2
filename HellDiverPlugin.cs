global using static HellDiver.HellDiverPlugin;
global using ConcentricContent;
using BepInEx;
using BepInEx.Logging;
using ExtraSkillSlots;
using RoR2;
using RoR2.ContentManagement;
using System.Security;
using System.Security.Permissions;
using Path = System.IO.Path;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete
[module: UnverifiableCode]

namespace HellDiver
{
	[BepInDependency(ConcentricContentPlugin.Guid)] // "NewlyHatchedDisciple-ConcentricContent-1.0.0"
	[BepInDependency("com.Necrofearfire.HelldiverCaptain")] // "Necrofearfire-Helldiver_Captain-1.0.0"
	[BepInDependency(ExtraSkillSlotsPlugin.GUID, BepInDependency.DependencyFlags.SoftDependency)] // "KingEnderBrine-ExtraSkillSlots-1.6.1"
	[BepInPlugin(GUID, "HellDiver", Version)]
	public class HellDiverPlugin : BaseUnityPlugin
	{
		public static HellDiverPlugin instance;
		public static ManualLogSource log;
		public const string GUID = "dev.bubbet.helldiver";
		public const string Version = "1.8.10";

		public const string DevPrefix = "BUB_";

		public static Dictionary<string, UnityEngine.AssetBundle> bundles =
			new Dictionary<string, UnityEngine.AssetBundle>();

		private void Awake()
		{
			instance = this;
			log = Logger;

			var pluginPath = Path.GetDirectoryName(Info.Location) ??
			                 throw new InvalidOperationException("Failed to find path of plugin.");
			
			Language.collectLanguageRootFolders +=
				folders => folders.Add(Path.Combine(pluginPath, "Language"));
			
			var assetsPath = Path.Join(pluginPath, "Assets");
			if (Directory.Exists(assetsPath))
			{
				var bundlePaths = Directory.EnumerateFiles(assetsPath).Where(x => !x.EndsWith("manifest")).ToArray();
				foreach (var path in bundlePaths)
				{
					UnityEngine.AssetBundle.LoadFromFileAsync(path).completed += operation =>
					{
						bundles[Path.GetFileName(path)] = ((UnityEngine.AssetBundleCreateRequest)operation).assetBundle;
						if (bundles.Count != bundlePaths.Length) return;
						ContentPackProvider.Init();
					};
				}
				if (bundlePaths.Length == 0) ContentPackProvider.Init();
			}
			else
			{
				ContentPackProvider.Init();
			}
		}

		public static Task<T> LoadAsset<T>(string assetPath) where T : UnityEngine.Object
		{
			if (assetPath.StartsWith("addressable:"))
			{
				return UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(assetPath["addressable:".Length..])
					.Task;
			}

			if (assetPath.StartsWith("legacy:"))
			{
				return RoR2.LegacyResourcesAPI.LoadAsync<T>(assetPath["legacy:".Length..]).Task;
			}

			var colinIndex = assetPath.IndexOf(":", StringComparison.Ordinal);
			if (colinIndex <= 0) return UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<T>(assetPath).Task;

			var source = new TaskCompletionSource<T>();
			var handle = bundles[assetPath[..colinIndex]].LoadAssetAsync<T>(assetPath[(colinIndex + 1)..]);
			handle.completed += _ => source.SetResult((T)handle.asset);
			return source.Task;
		}
	}

	public class ContentPackProvider : IContentPackProvider
	{
		private Task<ContentPack> _task;

		private ContentPackProvider()
		{
			_task = Concentric.BuildContentPack(System.Reflection.Assembly.GetExecutingAssembly());
		}

		public static void Init()
		{
			ContentManager.collectContentPackProviders += provider => provider(new ContentPackProvider());
		}

		public System.Collections.IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
		{
			args.ReportProgress(1f);
			yield break;
		}

		public System.Collections.IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
		{
			while (!_task.IsCompleted)
				yield return null;
			if (_task.IsFaulted)
				throw _task.Exception!;

			ContentPack.Copy(_task.Result, args.output);
			args.ReportProgress(1f);
		}

		public System.Collections.IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
		{
			args.ReportProgress(1f);
			yield break;
		}

		public string identifier => instance.Info.Metadata.GUID;
	}
}