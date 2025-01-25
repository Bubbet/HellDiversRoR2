using EntityStates;
using RoR2.Skills;
using UnityEngine;

namespace HellDiver.Data.Stratagems.Orbital
{
	public class RailCannonState : StratagemState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			log.LogInfo("Rail cannon firing, seek cover.");
		}

		public override void Fire()
		{
			base.Fire();
			outer.SetNextStateToMain();
		}

		public override float IncomingDuration => 12;
	}
	
	public class RailCannon : Stratagem, ISkill
	{
		public override StratagemKind kind => StratagemKind.Red;
		public override Inputs[] inputs =>
			new[] { Inputs.Right, Inputs.Up, Inputs.Down, Inputs.Down, Inputs.Right };

		async Task<SkillDef> ISkill.BuildObject()
		{
			var skillDef = ScriptableObject.CreateInstance<SkillDef>();
			skillDef.skillName = DevPrefix + "STRATAGEM_RAIL_CANNON_NAME";
			skillDef.skillNameToken = DevPrefix + "STRATAGEM_RAIL_CANNON_NAME";
			skillDef.icon = await LoadAsset<Sprite>("helldivers:0x81bc00dd2ee52bf0");
			skillDef.skillDescriptionToken = DevPrefix + "STRATAGEM_RAIL_CANNON_DESC";
			skillDef.activationStateMachineName = "Stratagem";

			skillDef.beginSkillCooldownOnSkillEnd = true;
			skillDef.baseRechargeInterval = 210;
			
			return skillDef;
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(RailCannonState) };
	}
}