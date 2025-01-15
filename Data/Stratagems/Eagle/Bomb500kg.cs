using EntityStates;
using RoR2.Skills;
using UnityEngine;

namespace HellDiver.Data.Stratagems.Eagle
{
	public class Bomb500kgState : BaseSkillState
	{
		public override void OnEnter()
		{
			base.OnEnter();
			log.LogInfo("500kg bomb coming right up.");
		}
	}

	public class Bomb500kg : Stratagem, ISkill
	{
		Task<SkillDef> ISkill.BuildObject()
		{
			var skillDef = ScriptableObject.CreateInstance<SkillDef>();
			//TODO finish skillDef
			skillDef.skillName = DevPrefix + "STRATAGEM_EAGLE_500KG_NAME";
			skillDef.skillNameToken = DevPrefix + "STRATAGEM_EAGLE_500KG_NAME";
			//skill.icon = LoadAsset<Sprite>("");
			skillDef.skillDescriptionToken = DevPrefix + "STRATAGEM_EAGLE_500KG_DESC";
			skillDef.activationStateMachineName = "Stratagem";
			return Task.FromResult(skillDef);
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(Bomb500kgState) };

		public override IEnumerable<Inputs> inputs =>
			new[] { Inputs.Up, Inputs.Right, Inputs.Down, Inputs.Down, Inputs.Down };
	}
}