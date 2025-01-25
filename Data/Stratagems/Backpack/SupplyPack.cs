using HellDiver.Data.Stratagems.Eagle;
using RoR2.Skills;
using UnityEngine;

namespace HellDiver.Data.Stratagems.Backpack
{
	public class SupplyPackState : StratagemState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			log.LogInfo("Supply Pack.");
		}

		public override void Fire()
		{
			base.Fire();
			outer.SetNextStateToMain();
		}
		public override float IncomingDuration => 15;
	}
	
	public class SupplyPack : Stratagem, ISkill
	{
		public override StratagemKind kind => StratagemKind.Blue;

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skillDef = ScriptableObject.CreateInstance<SkillDef>();
			//TODO finish skillDef
			skillDef.skillName = DevPrefix + "STRATAGEM_SUPPLY_PACK_NAME";
			skillDef.skillNameToken = DevPrefix + "STRATAGEM_SUPPLY_PACK_NAME";
			skillDef.icon = await LoadAsset<Sprite>("helldivers:0x36668e94dd8998e9");
			skillDef.skillDescriptionToken = DevPrefix + "STRATAGEM_SUPPLY_PACK_DESC";
			skillDef.activationStateMachineName = "Stratagem";

			skillDef.beginSkillCooldownOnSkillEnd = true;
			skillDef.baseMaxStock = 1;
			skillDef.baseRechargeInterval = 480;

			return skillDef;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(SupplyPackState) };

		public override Inputs[] inputs =>
			new[] { Inputs.Down, Inputs.Left, Inputs.Down, Inputs.Up, Inputs.Up, Inputs.Down };
	}
}