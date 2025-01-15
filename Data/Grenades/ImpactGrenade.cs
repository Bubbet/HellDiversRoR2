using EntityStates;
using RoR2.Skills;
using UnityEngine;

namespace HellDiver.Data.Grenades
{
	public class ImpactGrenadeState : BaseSkillState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			log.LogInfo("Boom!");
		}
	}

	public class ImpactGrenade : Concentric, ISkill
	{
		Task<SkillDef> ISkill.BuildObject()
		{
			var skillDef = ScriptableObject.CreateInstance<SkillDef>();
			//TODO finish skillDef
			skillDef.skillName = DevPrefix + "IMPACT_GRENADE_NAME";
			skillDef.skillNameToken = DevPrefix + "IMPACT_GRENADE_NAME";
			//skill.icon = LoadAsset<Sprite>("");
			skillDef.skillDescriptionToken = DevPrefix + "IMPACT_GRENADE_DESC";
			skillDef.activationStateMachineName = "Weapon";
			return Task.FromResult(skillDef);
		}

		public IEnumerable<Type> GetEntityStates() => new[] { typeof(ImpactGrenadeState) };
	}
}