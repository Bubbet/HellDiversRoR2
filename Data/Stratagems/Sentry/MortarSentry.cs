using RoR2.Skills;
using UnityEngine;

namespace HellDiver.Data.Stratagems.Sentry
{
	public class MortarSentryState : StratagemState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			log.LogInfo("Mortar Sentry.");
		}

		public override void Fire()
		{
			base.Fire();
			outer.SetNextStateToMain();
		}
		public override float IncomingDuration => 15;
	}
	
	public class MortarSentry : Stratagem, ISkill
	{
		public override StratagemKind kind => StratagemKind.Green;

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skillDef = ScriptableObject.CreateInstance<SkillDef>();
			//TODO finish skillDef
			skillDef.skillName = DevPrefix + "STRATAGEM_MORTAR_SENTRY_NAME";
			skillDef.skillNameToken = DevPrefix + "STRATAGEM_MORTAR_SENTRY_NAME";
			skillDef.icon = await LoadAsset<Sprite>("helldivers:0xd4dface72e3999f0");
			skillDef.skillDescriptionToken = DevPrefix + "STRATAGEM_MORTAR_SENTRY_DESC";
			skillDef.activationStateMachineName = "Stratagem";

			skillDef.beginSkillCooldownOnSkillEnd = true;
			skillDef.baseMaxStock = 1;
			skillDef.baseRechargeInterval = 180;

			return skillDef;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(MortarSentryState) };

		public override Inputs[] inputs =>
			new[] { Inputs.Down, Inputs.Up, Inputs.Right, Inputs.Right, Inputs.Down };
	}
}