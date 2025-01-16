using HellDiver.Data.Stratagems.Eagle;
using HellDiver.Data.Stratagems.Orbital;
using RoR2;
using RoR2.Skills;

namespace HellDiver.Data.Stratagems
{
	public abstract class Stratagem : Concentric
	{
		public abstract IEnumerable<Inputs> inputs { get; }

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
		IEnumerable<Concentric> ISkillFamily.GetSkillAssets() =>
			new Stratagem[] { GetAsset<Bomb500kg>(), GetAsset<RailCannon>() };

		public string GetNameToken(GenericSkill skill) => Language.GetStringFormatted("BUB_LOADOUT_STRATAGEM",
			HellDiver.stratagemSkills.TakeWhile(x => x != skill).Count() + 1);

		public string GetViewableNameOverride(GenericSkill skill) =>
			SkillCatalog.GetSkillFamilyName(skill.skillFamily.catalogIndex) +
			HellDiver.stratagemSkills.TakeWhile(x => x != skill).Count();
	}
}