using EntityStates;
using HellDiver.Data.Stratagems;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace HellDiver.Data
{
	public abstract class DirectionState : BaseSkillState
	{
		public abstract Stratagem.Inputs input { get; }

		public override void OnEnter()
		{
			if (skillLocator.GetSkill(SkillSlot.Utility).stateMachine.state is CallStratagemState callStratagemState)
				callStratagemState.Dial(input);
		}
	}

	public class UpState : DirectionState
	{
		public override Stratagem.Inputs input => Stratagem.Inputs.Up;
	}

	public class UpFamily : Concentric, ISkillFamily, ISkill
	{
		public bool HiddenFromCharacterSelect => true;
		IEnumerable<Concentric> ISkillFamily.GetSkillAssets() => new[] { this };

		Task<SkillDef> ISkill.BuildObject()
		{
			var skillDef = ScriptableObject.CreateInstance<SkillDef>();
			return Task.FromResult(skillDef);
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(UpState) };
	}

	public class DownState : DirectionState
	{
		public override Stratagem.Inputs input => Stratagem.Inputs.Down;
	}

	public class DownFamily : Concentric, ISkillFamily, ISkill
	{
		public bool HiddenFromCharacterSelect => true;
		IEnumerable<Concentric> ISkillFamily.GetSkillAssets() => new[] { this };

		Task<SkillDef> ISkill.BuildObject()
		{
			var skillDef = ScriptableObject.CreateInstance<SkillDef>();
			return Task.FromResult(skillDef);
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(DownState) };
	}

	public class LeftState : DirectionState
	{
		public override Stratagem.Inputs input => Stratagem.Inputs.Left;
	}

	public class LeftFamily : Concentric, ISkillFamily, ISkill
	{
		public bool HiddenFromCharacterSelect => true;
		IEnumerable<Concentric> ISkillFamily.GetSkillAssets() => new[] { this };

		Task<SkillDef> ISkill.BuildObject()
		{
			var skillDef = ScriptableObject.CreateInstance<SkillDef>();
			return Task.FromResult(skillDef);
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(LeftState) };
	}

	public class RightState : DirectionState
	{
		public override Stratagem.Inputs input => Stratagem.Inputs.Right;
	}

	public class RightFamily : Concentric, ISkillFamily, ISkill
	{
		public bool HiddenFromCharacterSelect => true;
		IEnumerable<Concentric> ISkillFamily.GetSkillAssets() => new[] { this };

		Task<SkillDef> ISkill.BuildObject()
		{
			var skillDef = ScriptableObject.CreateInstance<SkillDef>();
			return Task.FromResult(skillDef);
		}

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(RightState) };
	}
}