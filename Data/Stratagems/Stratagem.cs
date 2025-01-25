using EntityStates;
using EntityStates.Engi.EngiMissilePainter;
using HellDiver.Data.Stratagems.Eagle;
using HellDiver.Data.Stratagems.Orbital;
using RoR2;
using RoR2.Skills;
using System.Reflection;
using UnityEngine;

namespace HellDiver.Data.Stratagems
{
	public interface IStratagemState
	{
		public float IncomingDuration { get; }
		public float GetTMinusImpact();
	}

	public abstract class StratagemState : BaseSkillState, IStratagemState
	{
		private bool _fired;
		public virtual void Fire() { }

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (!_fired && GetTMinusImpact() <= 0)
			{
				Fire();
				_fired = true;
			}
		}

		public abstract float IncomingDuration { get; }
		public float GetTMinusImpact() => Mathf.Max(0, IncomingDuration - fixedAge);
	}

	public abstract class Stratagem : Concentric
	{
		public abstract StratagemKind kind { get; }
		public abstract Inputs[] inputs { get; }

		public enum Inputs
		{
			Rest,
			Up,
			Down,
			Left,
			Right,
		}
	}

	public class StratagemFamily : Concentric, ISkillFamily
	{
		private static List<Stratagem>? _stratagemAssets;

		public static List<Stratagem> StratagemAssets
		{
			get
			{
				if (_stratagemAssets != null) return _stratagemAssets;

				var stratType = typeof(Stratagem);
				_stratagemAssets = stratType.Assembly.GetTypes()
					.Where(x => !x.IsAbstract && stratType.IsAssignableFrom(x))
					.Select(GetAsset)
					.Cast<Stratagem>()
					.ToList();

				return _stratagemAssets;
			}
		}

		IEnumerable<Concentric> ISkillFamily.GetSkillAssets() => StratagemAssets;

		public string GetNameToken(GenericSkill skill) => Language.GetStringFormatted("BUB_LOADOUT_STRATAGEM",
			HellDiver.stratagemSkills.TakeWhile(x => x != skill).Count() + 1);

		public string GetViewableNameOverride(GenericSkill skill) =>
			SkillCatalog.GetSkillFamilyName(skill.skillFamily.catalogIndex) +
			HellDiver.stratagemSkills.TakeWhile(x => x != skill).Count();
	}
}