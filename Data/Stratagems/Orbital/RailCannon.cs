using EntityStates;
using RoR2.Skills;
using UnityEngine;

namespace HellDiver.Data.Stratagems.Orbital
{
	public class RailCannonState : BaseSkillState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			log.LogInfo("Rail cannon firing, seek cover.");
		}
	}
	
	public class RailCannon : Stratagem, ISkill
	{
		public override IEnumerable<Inputs> inputs =>
			new[] { Inputs.Right, Inputs.Up, Inputs.Down, Inputs.Down, Inputs.Right };

		Task<SkillDef> ISkill.BuildObject()
		{
			var skillDef = ScriptableObject.CreateInstance<SkillDef>();
			skillDef.skillName = DevPrefix + "STRATAGEM_RAIL_CANNON_NAME";
			skillDef.skillNameToken = DevPrefix + "STRATAGEM_RAIL_CANNON_NAME";
			//skill.icon = LoadAsset<Sprite>("");
			skillDef.skillDescriptionToken = DevPrefix + "STRATAGEM_RAIL_CANNON_DESC";
			skillDef.activationStateMachineName = "Stratagem";
			return Task.FromResult(skillDef);
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(RailCannonState) };
	}
}