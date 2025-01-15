using EntityStates;
using RoR2.Skills;
using UnityEngine;

namespace HellDiver.Data.Primary
{
	public class DominatorState : BaseSkillState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			log.LogInfo("Pew!");
		}
	}

	public class Dominator : Concentric, ISkill
	{
		Task<SkillDef> ISkill.BuildObject()
		{
			var skillDef = ScriptableObject.CreateInstance<SkillDef>();
			//TODO finish skillDef
			skillDef.skillName = DevPrefix + "DOMINATOR_NAME";
			skillDef.skillNameToken = DevPrefix + "DOMINATOR_NAME";
			//skill.icon = LoadAsset<Sprite>("");
			skillDef.skillDescriptionToken = DevPrefix + "DOMINATOR_DESC";
			skillDef.activationStateMachineName = "Weapon";
			return Task.FromResult(skillDef);
		}

		public IEnumerable<Type> GetEntityStates() => new[] { typeof(DominatorState) };
	}
}