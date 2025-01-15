using EntityStates;
using RoR2.Skills;
using UnityEngine;

namespace HellDiver.Data.Secondary
{
	// I dont know what I want to put here yet.
	public class Empty : Concentric, ISkill
	{
		Task<SkillDef> ISkill.BuildObject()
		{
			var skillDef = ScriptableObject.CreateInstance<SkillDef>();
			//TODO finish skillDef
			return Task.FromResult(skillDef);
		}

		public IEnumerable<Type> GetEntityStates() => new[] { typeof(Idle) };
	}
	
	public class SecondaryFamily : Concentric, ISkillFamily
	{
		IEnumerable<Concentric> ISkillFamily.GetSkillAssets() => new[] { GetAsset<Empty>() };
	}
}