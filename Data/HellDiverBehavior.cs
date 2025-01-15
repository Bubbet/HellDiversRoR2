using RoR2;
using UnityEngine;

namespace HellDiver.Data
{
	public class HellDiverBehavior : MonoBehaviour
	{
		public CharacterBody body;
		public GenericSkill[] stratagemSkills;

		public void Awake()
		{
			body = GetComponent<CharacterBody>();
			stratagemSkills = body.skillLocator.allSkills.Skip(HellDiver.FirstStratagemSlot).Take(4).ToArray();
		}
	}
}