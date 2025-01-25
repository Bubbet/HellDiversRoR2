using HarmonyLib;
using HellDiver.Data;
using HellDiver.Data.Stratagems;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HellDiver
{
	[HarmonyPatch]
	public static class HarmonyPatches
	{
		private static SkillFamily.Variant currentVariant;

		[HarmonyILManipulator,
		 HarmonyPatch(typeof(LoadoutPanelController.Row), nameof(LoadoutPanelController.Row.FromSkillSlot))]
		public static void GrabCurrentVariant(ILContext il)
		{
			var c = new ILCursor(il);
			c.GotoNext(x => x.MatchLdfld<SkillFamily>(nameof(SkillFamily.variants)));
			var variantIndex = -1;
			c.GotoNext(x => x.MatchStloc(out variantIndex));
			c.GotoNext(
				x => x.MatchCallOrCallvirt<LoadoutPanelController.Row>(nameof(LoadoutPanelController.Row.AddButton)));
			c.Emit(OpCodes.Ldloc, variantIndex);
			c.EmitDelegate(SetMaterial);
		}

		public static void SetMaterial(ref SkillFamily.Variant variant)
		{
			currentVariant = variant;
		}

		[HarmonyPostfix,
		 HarmonyPatch(typeof(LoadoutPanelController.Row), nameof(LoadoutPanelController.Row.AddButton))]
		public static void ReplaceButtonMaterial(LoadoutPanelController.Row __instance)
		{
			if (!Concentric.TryGetAssetFromObject(currentVariant.skillDef, out Concentric concentric) || concentric is not Stratagem stratagem) return;
			var image = __instance.rowData[^1].button.image;
			image.material = StratagemHUDLoader.StratagemMaterial;
			image.color = StratagemHUDController.StratagemKindColors[stratagem.kind];
		}
	}
}