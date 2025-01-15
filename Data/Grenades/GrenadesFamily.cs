namespace HellDiver.Data.Grenades
{
	public class GrenadesFamily : Concentric, ISkillFamily
	{
		IEnumerable<Concentric> ISkillFamily.GetSkillAssets() => new[] { GetAsset<ImpactGrenade>() };
	}
}