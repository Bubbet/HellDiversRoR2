using EntityStates;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;

namespace HellDiver.Data.Stratagems.Eagle
{
	public class Bomb500kgState : StratagemState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			log.LogInfo("500kg bomb coming right up.");
		}

		public override void Fire()
		{
			base.Fire();
			outer.SetNextStateToMain();
		}
		public override float IncomingDuration => 8;
	}

	public class Bomb500kg : Stratagem, ISkill
	{
		public override StratagemKind kind => StratagemKind.Red;

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skillDef = ScriptableObject.CreateInstance<EagleSkillDef>();
			//TODO finish skillDef
			skillDef.skillName = DevPrefix + "STRATAGEM_EAGLE_500KG_NAME";
			skillDef.skillNameToken = DevPrefix + "STRATAGEM_EAGLE_500KG_NAME";
			skillDef.icon = await LoadAsset<Sprite>("helldivers:0x4215699d6705c2c4");
			skillDef.skillDescriptionToken = DevPrefix + "STRATAGEM_EAGLE_500KG_DESC";
			skillDef.activationStateMachineName = "Stratagem";

			skillDef.beginSkillCooldownOnSkillEnd = true;
			skillDef.baseMaxStock = 2;
			skillDef.baseRechargeInterval = 8;
			skillDef.rechargeStock = 0;

			return skillDef;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(Bomb500kgState) };

		public override Inputs[] inputs =>
			new[] { Inputs.Up, Inputs.Right, Inputs.Down, Inputs.Down, Inputs.Down };
	}
}