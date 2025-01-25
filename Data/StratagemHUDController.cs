using HellDiver.Data.Stratagems;
using LeTai.Asset.TranslucentImage;
using RoR2;
using RoR2.UI;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HellDiver.Data
{
	public class StratagemHUDLoader : Concentric, IGenericObject
	{
		public GameObject HUD;
		public static Material StratagemMaterial;

		public override async Task Initialize()
		{
			await base.Initialize();

			StratagemMaterial = await LoadAsset<Material>("helldivers:ChannelExtractorMat");

			HUD = await LoadAsset<GameObject>("RoR2/Base/UI/HUDSimple.prefab");
			var hud = HUD.GetComponent<HUD>();
			var stratagem = await this.GetGenericObject();

			stratagem.GetComponent<StratagemHUD>().HUD = hud;
			stratagem.transform.SetParent(hud.healthBar.transform.parent.parent);
			stratagem.transform.localScale = Vector3.one * 0.85f;
			stratagem.transform.localPosition = new Vector3(0, 300, 0);
		}

		async Task<GameObject> IGenericObject.BuildObject()
		{
			var stratagems = await LoadAsset<GameObject>("helldivers:StratagemContainer");
			stratagems.transform.GetChild(0).gameObject.AddComponent<TranslucentImage>();
			return stratagems;
		}
	}

	public enum StratagemKind
	{
		Blue,
		Red,
		Green,
		Yellow
	}

	public class StratagemHUD : MonoBehaviour
	{
		[Header("Game Variables")] [NonSerialized]
		public List<StratagemHUDController> Rows = new List<StratagemHUDController>();

		[Header("Prefab References")] public Image background;
		public RectTransform parentTransform;
		public GameObject rowPrefab;
		public HUD HUD;
		private GameObject targetBodyObject;
		private GenericSkill utilitySkill;
		private bool wasOpen;
		private float OffsetsAndIconSize = 26 + 58;

		private void Update()
		{
			if (HUD.targetBodyObject != targetBodyObject)
			{
				targetBodyObject = HUD.targetBodyObject;
				foreach (var row in Rows)
				{
					Destroy(gameObject);
				}

				var skillLocator = targetBodyObject.GetComponent<SkillLocator>();
				utilitySkill = skillLocator.utility;

				for (var i = 0; i < HellDiver.StratagemCount; i++)
				{
					var row = Instantiate(rowPrefab, parentTransform);
					row.transform.localScale = Vector3.one;
					var rowComponent = row.GetComponent<StratagemHUDController>();

					rowComponent.Skill = skillLocator.allSkills[HellDiver.FirstStratagemSlot + i];
					Rows.Add(rowComponent);
				}
			}


			if (utilitySkill.stateMachine is { state: CallStratagemState })
			{
				var backgroundTransform = (RectTransform)background.transform;
				var sizeDelta = backgroundTransform.sizeDelta;
				sizeDelta.x = OffsetsAndIconSize + Rows.Max(x => x.slide.preferredWidth);
				backgroundTransform.sizeDelta = sizeDelta;
				background.enabled = true;
			}
			else { background.enabled = false; }
			/*
			else if (!Rows.Any(x => x.Visible) && Rows.All(x => x.slide.preferredWidth == 0))
			{
				background.enabled = false;
			}*/
		}
	}
}