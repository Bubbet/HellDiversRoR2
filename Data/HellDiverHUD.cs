using HellDiver.Data.Stratagems;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace HellDiver.Data
{
	public class HellDiverHUD : MonoBehaviour
	{
		public StratagemIcon[] skillIcons;
		public HUD hud;
		private GameObject targetBodyObject;

		public static void Build(HUD hud)
		{
			var hellDiverHUD = hud.gameObject.AddComponent<HellDiverHUD>();
			hellDiverHUD.skillIcons = new StratagemIcon[HellDiver.StratagemCount];
			hellDiverHUD.hud = hud;

			for (var index = 0; index < HellDiver.StratagemCount; index++)
			{
				var parent = hud.healthBar.transform.parent.parent;
				var skillIcon = Instantiate(hud.skillIcons[0].gameObject);

				var transform = skillIcon.GetComponent<RectTransform>();
				transform.SetParent(parent);
				transform.localPosition = new Vector3(0, 74.5f * index, 0);
				transform.localScale = Vector3.one * 1.6f;

				var bg = (RectTransform)transform.Find("SkillBackgroundPanel");
				DestroyImmediate(bg.GetChild(0).gameObject);
				DestroyImmediate(bg.GetComponent<ContentSizeFitter>());
				DestroyImmediate(bg.GetComponent<HorizontalLayoutGroup>());
				bg.SetSiblingIndex(0);
				bg.pivot = new Vector2(0, 0.5f);
				bg.localScale = new Vector3(4, 4, 1);
				bg.localPosition = new Vector3(-44f, 18f, 0);

				bg.sizeDelta = new Vector2(38, 13);

				var inputsContainer = new GameObject("Inputs");
				var inputsContainerTransform = inputsContainer.AddComponent<RectTransform>();
				inputsContainerTransform.SetParent(bg);
				inputsContainerTransform.localScale = Vector3.one;
				inputsContainerTransform.offsetMin = new Vector2(12.5f, -13);
				inputsContainerTransform.offsetMax = new Vector2(37, 0);
				inputsContainerTransform.anchorMin = new Vector2(0, 0.5f);
				inputsContainerTransform.anchorMax = inputsContainerTransform.anchorMin;
				inputsContainerTransform.pivot = new Vector2(0, 0.5f);
				inputsContainerTransform.localPosition = new Vector3(13, 0, 0);
				var inputsLG = inputsContainer.AddComponent<HorizontalLayoutGroup>();
				inputsLG.spacing = 1f;

				var stratagemController = skillIcon.AddComponent<StratagemIcon>();
				stratagemController.inputsContainer = inputsContainer;
				stratagemController.skillIcon = skillIcon.GetComponent<SkillIcon>();

				hellDiverHUD.skillIcons[index] = stratagemController;
			}
		}

		public void Update()
		{
			if (hud.targetBodyObject != targetBodyObject)
			{
				targetBodyObject = hud.targetBodyObject;

				var skillLocator = targetBodyObject.GetComponent<SkillLocator>();
				var genericSkills = skillLocator.allSkills.Skip(HellDiver.FirstStratagemSlot)
					.Take(HellDiver.StratagemCount).ToArray();
				var pcmc = hud.targetMaster.playerCharacterMasterController;
				for (var i = 0; i < HellDiver.StratagemCount; i++)
				{
					skillIcons[i].skillIcon.targetSkill = genericSkills[i];
					skillIcons[i].skillIcon.playerCharacterMasterController = pcmc;
					if (
						Concentric.TryGetAssetFromObject(genericSkills[i].skillDef, out Stratagem stratagem))
					{
						skillIcons[i].SetInputs(stratagem.inputs);
					}
				}
			}
		}
	}

	public class StratagemIcon : MonoBehaviour
	{
		public GameObject inputsContainer;
		public SkillIcon skillIcon;

		private void Awake()
		{
			originalPos = inputsContainer.transform.localPosition;
			parentTransform = (RectTransform)inputsContainer.transform.parent;
			originalSize = parentTransform.sizeDelta;
			
			parentTransform.sizeDelta = new Vector2(13, 13);
			inputsContainer.transform.localPosition = new Vector3(-20, 0, 0);

		}

		public void SetInputs(IEnumerable<Stratagem.Inputs> inputsEnumerable)
		{
			genericSkill = skillIcon.targetSkill.GetComponent<SkillLocator>().utility;

			for (var i = 0; i < inputsContainer.transform.childCount; i++)
			{
				DestroyImmediate(inputsContainer.transform.GetChild(i));
			}

			inputObjects.Clear();
			inputs = inputsEnumerable.ToArray();

			foreach (var input in inputs)
			{
				var inputObject = new GameObject();
				inputObject.transform.SetParent(inputsContainer.transform);
				inputObject.transform.localScale = Vector3.one;
				inputObject.transform.localPosition = Vector3.zero;
				var image = inputObject.AddComponent<Image>();
				image.sprite = arrowSprite;
				image.preserveAspect = true;
				var transformRotation = inputObject.transform.rotation;
				switch (input)
				{
					case Stratagem.Inputs.Up:
						transformRotation.eulerAngles = new Vector3(0, 0, 90);
						break;
					case Stratagem.Inputs.Down:
						transformRotation.eulerAngles = new Vector3(0, 0, -90);
						break;
					case Stratagem.Inputs.Left:
						transformRotation.eulerAngles = new Vector3(0, 0, 180);
						break;
					case Stratagem.Inputs.Right:
						break;
					case Stratagem.Inputs.Rest:
					default:
						throw new ArgumentOutOfRangeException();
				}

				inputObject.transform.rotation = transformRotation;
				inputObjects.Add(image);
			}
		}

		public void Update()
		{
			if (genericSkill.stateMachine is { state: CallStratagemState stratagem })
			{
				parentTransform.sizeDelta = Vector2.MoveTowards(parentTransform.sizeDelta, originalSize, slideSpeed);
				inputsContainer.transform.localPosition =
					Vector3.MoveTowards(inputsContainer.transform.localPosition, originalPos, slideSpeed);
				if (stratagem.dialedInputs.Count > 0)
				{
					reset = false;
					var valid = inputs.SequenceContains(stratagem.dialedInputs);
					var lower = valid ? stratagem.dialedInputs.Count : 0;
					if (valid)
					{
						for (var i = 0; i < stratagem.dialedInputs.Count; i++)
						{
							inputObjects[i].color = Color.white;
						}
					}

					for (var i = lower; i < inputs.Length; i++)
					{
						inputObjects[i].color = Color.white * 0.65f;
					}
				}
			}
			else if (!reset)
			{
				foreach (var image in inputObjects)
				{
					image.color = Color.white;
				}

				reset = true;
			}
			else
			{
				parentTransform.sizeDelta = Vector2.MoveTowards(parentTransform.sizeDelta, new Vector2(13, 13), slideSpeed);
				inputsContainer.transform.localPosition =
					Vector3.MoveTowards(inputsContainer.transform.localPosition, new Vector3(-20, 0, 0), slideSpeed);

			}
		}

		private static Sprite? _arrowSprite;
		public Stratagem.Inputs[] inputs;

		[FormerlySerializedAs("callingStateMachine")]
		public GenericSkill genericSkill;

		private List<Image> inputObjects = new List<Image>();
		private bool reset;
		private Vector3 originalPos;
		private RectTransform parentTransform;
		private Vector2 originalSize;
		public static float slideSpeed = 5f;

		public static Sprite arrowSprite =>
			_arrowSprite ??= Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texConvertToScrapArrow.png")
				.WaitForCompletion();
	}
}