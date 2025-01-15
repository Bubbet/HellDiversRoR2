using HellDiver.Data.Stratagems.Eagle;
using HellDiver.Data.Stratagems.Orbital;
using RoR2;

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
			skill.GetComponents<GenericSkill>().TakeWhile(x => x != skill).Count() + 1 - HellDiver.FirstStratagemSlot);
	}
}