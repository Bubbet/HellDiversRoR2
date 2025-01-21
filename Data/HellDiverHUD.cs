using HellDiver.Data.Stratagems;
using RoR2;
using RoR2.UI;
using TMPro;
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
		private static Sprite? _arrowSprite;

		public static Sprite arrowSprite =>
			_arrowSprite ??= Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texConvertToScrapArrow.png")
				.WaitForCompletion();

		public const int MaxStratagemLength = 8;
		public List<StratagemIcon> skillIcons = new List<StratagemIcon>();
		public HUD hud;
		private GameObject targetBodyObject;
		public Transform stratagemContainer;


		public void BuildRow()
		{
			var skillIcon = Instantiate(hud.skillIcons[0].gameObject);

			var rectTransform = skillIcon.GetComponent<RectTransform>();
			rectTransform.SetParent(stratagemContainer);
			rectTransform.localPosition = new Vector3(0, 74.5f * skillIcons.Count, 0);
			rectTransform.localScale = Vector3.one * 1.6f;

			var bg = (RectTransform)rectTransform.Find("SkillBackgroundPanel");
			DestroyImmediate(bg.GetChild(0).gameObject);
			DestroyImmediate(bg.GetComponent<ContentSizeFitter>());
			DestroyImmediate(bg.GetComponent<HorizontalLayoutGroup>());
			bg.SetSiblingIndex(0);
			bg.pivot = new Vector2(0, 0.5f);
			bg.localScale = new Vector3(4, 4, 1);
			bg.localPosition = new Vector3(-44f, 18f, 0);

			bg.sizeDelta = new Vector2(40, 13);

			var stock = (RectTransform)rectTransform.Find("Skill1StockRoot");
			var stockBg = stock.gameObject.AddComponent<Image>();
			stockBg.color = new Color(0, 0, 0, 0.5f);
			stock.anchorMax = new Vector2(0.5f, 0);
			stock.anchorMin = stock.anchorMax;
			stock.sizeDelta = new Vector2(12, 12);
			stock.localPosition = new Vector3(-2, 8, 0);

			var text = (RectTransform)stock.GetChild(0);
			text.anchorMax = new Vector2(1, 0);
			text.anchorMin = text.anchorMax;
			text.sizeDelta = stock.sizeDelta;
			text.localPosition = new Vector3(0, -8, 0);

			var inputsParent = new GameObject("InputsParent");
			var inputsParentTransform = inputsParent.AddComponent<RectTransform>();
			inputsParent.AddComponent<RectMask2D>();


			var originalCooldownText = rectTransform.Find("CooldownText");
			var cooldown = Instantiate(originalCooldownText);
			var title = Instantiate(originalCooldownText);
			originalCooldownText.GetComponent<HGTextMeshProUGUI>().enabled = false;
			
			var titleTextTransform = (RectTransform)title.transform;
			titleTextTransform.SetParent(inputsParentTransform);
			var cooldownText = (RectTransform)cooldown.transform;
			cooldownText.SetParent(inputsParentTransform);
			
			var infoText = cooldownText.GetComponent<HGTextMeshProUGUI>();
			infoText.alignment = TextAlignmentOptions.Left;
			infoText.fontSize = 3;
			infoText.fontSizeMax = 3;
			cooldownText.sizeDelta = Vector2.one * 10f;
			cooldownText.Align(new Vector2(0, 0.5f));
			cooldownText.localPosition = new Vector3(0f, 2f, 0);
			
			titleTextTransform.name = "skillName";
			titleTextTransform.sizeDelta = Vector2.one * 10f;
			var titleText = titleTextTransform.gameObject.GetComponent<HGTextMeshProUGUI>();
			titleText.alignment = TextAlignmentOptions.Left;
			titleText.fontSize = 3;
			titleText.fontSizeMax = 3;
			titleTextTransform.Align(new Vector2(0, 0.5f));
			titleTextTransform.localPosition = new Vector3(0, 2f, 0);
			
			inputsParentTransform.SetParent(bg);
			inputsParentTransform.localScale = Vector3.one;

			inputsParentTransform.Align(new Vector2(0, 0.5f));
			inputsParentTransform.sizeDelta = new Vector2(25, 15);
			inputsParentTransform.localPosition = new Vector3(12.5f, 0, 0);
			var lg = inputsParentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
			lg.childAlignment = TextAnchor.MiddleLeft;
			lg.childForceExpandHeight = false;


			var inputsContainer = new GameObject("Inputs");
			var inputsContainerTransform = inputsContainer.AddComponent<RectTransform>();

			var inputsLG = inputsContainer.AddComponent<HorizontalLayoutGroup>();
			inputsLG.spacing = 1f;

			inputsContainerTransform.SetParent(inputsParent.transform);
			inputsContainerTransform.localScale = Vector3.one;

			inputsContainerTransform.pivot = new Vector2(0, 0.5f);
			inputsContainerTransform.anchorMin = inputsContainerTransform.pivot;
			inputsContainerTransform.anchorMax = inputsContainerTransform.pivot;
			inputsContainerTransform.sizeDelta = new Vector2(25, 3);
			inputsContainerTransform.localPosition = new Vector3(0, -2, 0);

			var inputObjects = new List<Image>();
			for (var i = 0; i < MaxStratagemLength; i++)
			{
				var inputObject = new GameObject("Input");
				inputObject.transform.SetParent(inputsContainer.transform);
				inputObject.transform.localScale = Vector3.one;
				inputObject.transform.localPosition = Vector3.zero;
				var image = inputObject.AddComponent<Image>();
				image.sprite = arrowSprite;
				image.preserveAspect = true;
				image.color = new Color(0, 0, 0, 0);
				inputObjects.Add(image);
			}

			var stratagemController = skillIcon.AddComponent<StratagemIcon>();
			stratagemController.backgroundTransform = bg;
			stratagemController.inputsParentTransform = inputsParentTransform;
			stratagemController.inputsTransform = inputsContainerTransform;
			stratagemController.text = infoText;
			stratagemController.skillIcon = skillIcon.GetComponent<SkillIcon>();
			stratagemController.inputImages = inputObjects.ToArray();
			stratagemController.stockBg = stockBg;
			stratagemController.skillTitle = titleText;

			skillIcons.Add(stratagemController);
		}

		public static void Build(HUD hud)
		{
			var hellDiverHUD = hud.gameObject.AddComponent<HellDiverHUD>();
			hellDiverHUD.hud = hud;

			var parent = new GameObject("StratagemContainer").transform;
			parent.SetParent(hud.healthBar.transform.parent.parent);
			parent.localScale = Vector3.one;
			parent.localPosition = new Vector3(30, 300, 0);

			hellDiverHUD.stratagemContainer = parent;

			for (var index = 0; index < HellDiver.StratagemCount; index++)
			{
				hellDiverHUD.BuildRow();
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
		public SkillIcon skillIcon;

		private void Awake()
		{
			backgroundOriginalSize = backgroundTransform.sizeDelta;
			inputsOriginalSize = inputsParentTransform.sizeDelta;
			backgroundTransform.sizeDelta = new Vector2(13, 13);
			inputsParentTransform.sizeDelta = new Vector2(0, 13);
		}

		public void SetInputs(IEnumerable<Stratagem.Inputs> inputsEnumerable)
		{
			dialingSkill = skillIcon.targetSkill.GetComponent<SkillLocator>().utility;
			inputs = inputsEnumerable.ToArray();
			stockBg.enabled = skillIcon.targetSkill.maxStock > 1;

			skillTitle.SetText(Language.GetString(skillIcon.targetSkill.skillDef.skillNameToken));
			//skillTitle.transform.localPosition = new Vector3(6f, 2f, 0);

			for (var i = 0; i < inputs.Length; i++)
			{
				var inputObject = inputImages[i];
				inputObject.color = Color.white;
				var transformRotation = inputObject.transform.rotation;
				switch (inputs[i])
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
			}

			for (var i = inputs.Length; i < HellDiverHUD.MaxStratagemLength; i++)
			{
				inputImages[i].color = new Color(0, 0, 0, 0);
			}
		}

		public void Update()
		{
			if (dialingSkill.stateMachine is { state: CallStratagemState stratagem })
			{
				SlideTo(backgroundOriginalSize, inputsOriginalSize);
				if (skillIcon.targetSkill.cooldownRemaining > 0)
				{
					text.enabled = true;
					inputsTransform.gameObject.SetActive(false);

					var span = new TimeSpan(0, 0, (int)skillIcon.targetSkill.cooldownRemaining);
					text.SetText(Language.GetStringFormatted("BUB_STRATAGEM_COOLDOWN", (int)span.TotalMinutes,
						span.Seconds));

					//text.transform.localPosition = new Vector3(10f, 0, 0);
				}
				else if (skillIcon.targetSkill.stock <= 0)
				{
					text.enabled = true;
					inputsTransform.gameObject.SetActive(false);

					text.SetText(Language.GetString("BUB_STRATAGEM_UNAVAILABLE"));
					// TODO Inbound 
					text.transform.localPosition = new Vector3(10f, 0, 0);
				}
				else
				{
					text.enabled = false;
					inputsTransform.gameObject.SetActive(true);

					if (stratagem.dialedInputs.Count > 0)
					{
						reset = false;
						var valid = inputs.SequenceContains(stratagem.dialedInputs);
						var lower = valid ? stratagem.dialedInputs.Count + 1 : 0;
						if (valid)
						{
							for (var i = 0; i < stratagem.dialedInputs.Count; i++)
							{
								inputImages[i].color = Color.white * 0.65f;
							}

							if (stratagem.dialedInputs.Count < inputs.Length)
								inputImages[stratagem.dialedInputs.Count].color = Color.white;
						}

						for (var i = lower; i < inputs.Length; i++)
						{
							inputImages[i].color = Color.white * 0.45f;
						}
					}
					else
					{
						for (var i = 0; i < inputs.Length; i++)
						{
							inputImages[i].color = Color.white * 0.65f;
						}
					}
				}
			}
			else if (!reset)
			{
				for (var i = 0; i < inputs.Length; i++)
				{
					inputImages[i].color = Color.white;
				}

				reset = true;
			}
			else
			{
				SlideTo(new Vector2(13, 13), new Vector2(0, 13));
			}
		}

		private void SlideTo(Vector2 backgroundSize, Vector2 inputsSize)
		{
			backgroundTransform.sizeDelta =
				Vector2.MoveTowards(backgroundTransform.sizeDelta, backgroundSize, slideSpeed);
			inputsParentTransform.sizeDelta =
				Vector2.MoveTowards(inputsParentTransform.sizeDelta, inputsSize, slideSpeed);
		}

		public Stratagem.Inputs[] inputs;
		public GenericSkill dialingSkill;

		public Image[] inputImages;
		private bool reset;
		public static float slideSpeed = 5f;

		public RectTransform backgroundTransform;
		public RectTransform inputsParentTransform;
		private Vector2 backgroundOriginalSize;
		private Vector2 inputsOriginalSize;
		public RectTransform inputsTransform;
		public HGTextMeshProUGUI skillTitle;
		public HGTextMeshProUGUI text;
		public Image stockBg;
	}
}