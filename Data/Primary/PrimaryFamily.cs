namespace HellDiver.Data.Primary
{
	public class PrimaryFamily : Concentric, ISkillFamily
	{
		IEnumerable<Concentric> ISkillFamily.GetSkillAssets() => new[] { GetAsset<Dominator>() };
	}
}