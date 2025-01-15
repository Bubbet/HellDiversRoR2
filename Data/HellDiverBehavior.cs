using HellDiver.Data.Stratagems;
using RoR2;
using UnityEngine;

namespace HellDiver.Data
{
	public class HellDiverBehavior : MonoBehaviour
	{
		public CharacterBody body;
		public (GenericSkill slot, Stratagem stratagem)[] stratagemSkills;

		public void Awake()
		{
			body = GetComponent<CharacterBody>();
			body.onSkillActivatedAuthority += SkillActivated;
		}

		private void SkillActivated(GenericSkill obj)
		{
			stratagemSkills = body.skillLocator.allSkills.Skip(HellDiver.FirstStratagemSlot)
				.Take(HellDiver.StratagemCount).Select(
					slot =>
					{
						if (Concentric.TryGetAssetFromObject(slot.skillDef, out Stratagem stratagem))
							return (slot, stratagem);
						throw new Exception($"Some non stratagem({slot.skillDef}) ended up in the stratagem skill family.");
					}).ToArray();
			body.onSkillActivatedAuthority -= SkillActivated;
		}
	}
}