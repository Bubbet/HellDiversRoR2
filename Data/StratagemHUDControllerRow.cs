using HellDiver.Data.Stratagems;
using RoR2;
using System.Collections;
using System.Runtime.InteropServices.ComTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HellDiver.Data
{
	public class StratagemHUDController : MonoBehaviour
	{
		[Header("Game Variables")] [SerializeField]
		private bool incoming;

		public bool Incoming
		{
			get => incoming;
			set
			{
				incoming = value;
				icon.fillOrigin = incoming ? 0 : 1;
				border.fillOrigin = icon.fillOrigin;
			}
		}

		[Range(0, 1), SerializeField] private float fillLevel;

		public float FillLevel
		{
			get => fillLevel;
			set
			{
				fillLevel = value;
				icon.fillAmount = 1 - fillLevel;
				border.fillAmount = icon.fillAmount;
				var barOffset = incoming ? 1.75f : 0;
				var cooldownBarSizeDelta = cooldownBar.sizeDelta;
				cooldownBarSizeDelta.y = Mathf.Max(0,
					Mathf.Min(58, (incoming ? 1 - fillLevel : fillLevel) * 58 + barOffset));
				cooldownBar.sizeDelta = cooldownBarSizeDelta;
			}
		}

		[SerializeField] private StratagemKind kind;

		public StratagemKind Kind
		{
			get => kind;
			set
			{
				kind = value;
				var color = StratagemKindColors[kind];
				icon.color = color;
				border.color = color;
			}
		}

		[SerializeField] private int stock;

		public int Stock
		{
			get => stock;
			set
			{
				if (stock != value)
				{
					var textEnabled = value != 0;
					stockParent.SetActive(textEnabled);
					if (textEnabled) stockText.SetText(value.ToString());
				}

				stock = value;
			}
		}

		[SerializeField] private bool visible;

		public bool Visible
		{
			get => visible;
			set
			{
				if (visible != value)
					StartCoroutine(Slide(value ? slideChild.Item1.localPosition.x * widthMult + slideChild.Item2.preferredWidth : 0,
						0.2f));
				visible = value;
			}
		}

		public GenericSkill utilitySkill;
		public GenericSkill skill;

		public GenericSkill Skill
		{
			get => skill;
			set
			{
				utilitySkill = value.GetComponent<SkillLocator>().utility;
				skill = value;
				icon.sprite = skill.icon;
				cooldownIcon.sprite = skill.icon;
				title.SetText(Language.GetString(skill.skillNameToken));
				if (!Concentric.TryGetAssetFromObject(skill.skillDef, out Stratagem strat)) return;
				stratagem = strat;
				Kind = stratagem.kind;

				FillLevel = 0;

				for (var i = 0; i < stratagem.inputs.Length; i++)
				{
					var input = inputImages[i];
					var background = backgroundInputImages[i];
					input.enabled = true;
					background.enabled = true;
					var transformRotation = input.transform.rotation;
					switch (stratagem.inputs[i])
					{
						case Stratagem.Inputs.Left:
							transformRotation.eulerAngles = new Vector3(0, 0, 90);
							break;
						case Stratagem.Inputs.Right:
							transformRotation.eulerAngles = new Vector3(0, 0, -90);
							break;
						case Stratagem.Inputs.Down:
							transformRotation.eulerAngles = new Vector3(0, 0, 180);
							break;
						case Stratagem.Inputs.Up:
							transformRotation.eulerAngles = new Vector3(0, 0, 0);
							break;
						case Stratagem.Inputs.Rest:
						default:
							throw new ArgumentOutOfRangeException();
					}

					input.transform.rotation = transformRotation;
					background.transform.rotation = transformRotation;
				}

				for (var i = stratagem.inputs.Length; i < HellDiverHUD.MaxStratagemLength; i++)
				{
					inputImages[i].enabled = false;
					backgroundInputImages[i].enabled = false;
				}
			}
		}


		[Header("Prefab References")] public Image icon;
		public Image cooldownIcon;
		public Image border;
		public Image soloBackground;
		public LayoutElement slide;
		public RectTransform cooldownBar;
		public GameObject inputContainer;
		public List<Image> inputImages = new List<Image>();
		public List<Image> backgroundInputImages = new List<Image>();
		public TextMeshProUGUI title;
		public TextMeshProUGUI subText;
		public TextMeshProUGUI stockText;
		public GameObject stockParent;
		public CanvasGroup canvasGroup;

		[Header("Runtime Fields")] [NonSerialized]
		public (RectTransform, LayoutGroup) slideChild;

		private bool _readyRunning;

		public static Dictionary<StratagemKind, Color> StratagemKindColors = new Dictionary<StratagemKind, Color>
		{
			{ StratagemKind.Blue, ExtensionMethods.ParseHtmlColor("#4FB3D0") },
			{ StratagemKind.Red, ExtensionMethods.ParseHtmlColor("#D56052") },
			{ StratagemKind.Green, ExtensionMethods.ParseHtmlColor("#679552") },
			{ StratagemKind.Yellow, ExtensionMethods.ParseHtmlColor("#C9B269") },
		};

		public Stratagem stratagem;
		public bool reset;
		private TextState textState = TextState.Done;
		private float textTimer;
		private float infoDisplayDuration = 1f;
		private float widthMult = 2f;

		// Start is called before the first frame update
		void Start()
		{
			var child = slide.transform.GetChild(0);
			slideChild = ((RectTransform)child, child.GetComponent<LayoutGroup>());
		}

		public void Ready()
		{
			if (_readyRunning) return;
			StartCoroutine(ReadyAnimation());
		}

		public IEnumerator ReadyAnimation()
		{
			if (_readyRunning) yield break;
			_readyRunning = true;

			for (var i = 0; i < inputImages.Count; i++)
			{
				inputImages[i].enabled = false;
				backgroundInputImages[i].enabled = false;
			}

			for (var i = 0; i < stratagem.inputs.Length; i++)
			{
				inputImages[i].color = new Color32(208, 212, 195, 255);
				inputImages[i].enabled = true;
				backgroundInputImages[i].enabled = true;
				yield return new WaitForSeconds(0.2f);
			}

			_readyRunning = false;
		}

		IEnumerator Slide(float targetWidth, float duration)
		{
			var startWidth = slide.preferredWidth;
			var timeElapsed = 0f;

			// Animate using cubic easing (ease-in-out)
			while (timeElapsed < duration)
			{
				// Calculate the normalized time (0 to 1)
				var t = timeElapsed / duration;

				// Apply cubic ease-in-out interpolation
				var cubicT = EaseInOutCubic(t);

				// Interpolate between the start and target width based on the cubic curve
				slide.preferredWidth = Mathf.Lerp(startWidth, targetWidth, cubicT);

				timeElapsed += Time.deltaTime;

				// Wait for the next frame
				yield return null;
			}

			// Ensure the final target width is set
			slide.preferredWidth = targetWidth;
		}

		private void OnValidate()
		{
			Start();
			slide.preferredWidth = visible ? slideChild.Item1.localPosition.x * widthMult + slideChild.Item2.preferredWidth : 0;
		}

		// Cubic ease-in-out function
		float EaseInOutCubic(float t)
		{
			return t < 0.5f ? 4 * Mathf.Pow(t, 3) : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
		}

		enum TextState
		{
			Dialing,
			Activating,
			Inbound,
			Impact,
			Cooldown,
			CooldownEnter,
			CooldownExit,
			Ready,
			Done,
			ReadyAnimation,
			Unavailable
		}

		public void Update()
		{
			textTimer -= Time.deltaTime;

			var dialing = false;
			if (utilitySkill.stateMachine is { state: CallStratagemState calling })
			{
				dialing = true;
				var inputsLength = stratagem.inputs.Length;
				var dialedAmount = calling.dialedInputs.Count;
				var valid = stratagem.inputs.SequenceContains(calling.dialedInputs);

				if (textState is TextState.Done or TextState.Dialing && valid && dialedAmount < inputsLength)
				{
					textState = TextState.Dialing;
					for (var i = 0; i < inputsLength; i++)
					{
						var image = inputImages[i];

						// disabled rgb(106, 110, 115)
						// dialed rgb(140, 144, 141)
						// next rgb(255, 255, 238)
						// not yet dialed rgb(196, 195, 175)
						// nothing entered rgb(208, 212, 195)

						Color color = (dialedAmount, valid) switch
						{
							(0, _) => // nothing entered
								new Color32(208, 212, 195, 255),
							(var d, true) when i == d => // next
								new Color32(255, 255, 238, 255),
							(var d, true) when i < d => // dialed
								new Color32(140, 144, 141, 255),
							(var d, true) when i > d => // not yet dialed
								new Color32(196, 195, 175, 255),
							(_, false) => // disabled
								new Color32(106, 110, 115, 255),
							_ => default
						};

						image.color = color;
					}
				}

				if (textState == TextState.Dialing && valid && inputsLength == dialedAmount)
				{
					textState = TextState.Activating;
					subText.SetText(Language.GetString(DevPrefix + "STRATAGEM_ACTIVATING"));
					subText.gameObject.SetActive(true);
					inputContainer.SetActive(false);
				}

				/* text states
						activating
						inbound T-00:00
						impact
						cooldown T-00:00
						ready
						
						unavailable
						*/
			}
			else if (textState == TextState.Dialing)
			{
				textState = TextState.Done;
			}

			if (skill.stateMachine != null && skill.stateMachine.state.GetType() == skill.activationState.stateType &&
			    skill.stateMachine.state is IStratagemState state)
			{
				var tMinus = state.GetTMinusImpact();
				switch (textState)
				{
					case TextState.Activating:
						textState = TextState.Inbound;
						break;
					case TextState.Inbound when tMinus <= 0:
						textTimer = infoDisplayDuration;
						textState = TextState.Impact;
						break;
					case TextState.Inbound:
						var span = TimeSpan.FromSeconds(tMinus);
						subText.SetText(Language.GetStringFormatted(DevPrefix + "STRATAGEM_INBOUND",
							span.Minutes, span.Seconds));
						FillLevel = 1f - tMinus / state.IncomingDuration;
						Incoming = true;
						break;
				}
			}

			switch (textState)
			{
				case TextState.Impact when textTimer <= 0 && skill is { stock: 0, cooldownRemaining: <= 0 }:
				case TextState.Done when skill is { stock: 0, cooldownRemaining: <= 0 }:
					subText.SetText(Language.GetString(DevPrefix + "STRATAGEM_UNAVAILABLE"));
					textState = TextState.Unavailable;
					break;
				case TextState.Impact when textTimer <= 0:
					textState = TextState.CooldownEnter;
					break;
				case TextState.Impact:
					subText.SetText(Language.GetString(DevPrefix + "STRATAGEM_IMPACT"));
					break;
				case TextState.CooldownEnter when skill.cooldownRemaining <= 10f:
					textState = TextState.CooldownExit;
					break;
				case TextState.CooldownEnter
					when skill.rechargeStopwatch >= 5f:
					textState = TextState.Cooldown;
					break;
				case TextState.CooldownExit when skill.cooldownRemaining <= 0:
					textTimer = infoDisplayDuration;
					textState = TextState.Ready;
					subText.SetText(Language.GetString(DevPrefix + "STRATAGEM_READY"));
					break;
				case TextState.Cooldown when skill.cooldownRemaining <= 5:
					textState = TextState.CooldownExit;
					break;
				case TextState.CooldownExit:
				case TextState.CooldownEnter:
				case TextState.Cooldown:
					var spanCooldown = TimeSpan.FromSeconds(skill.cooldownRemaining);
					subText.SetText(Language.GetStringFormatted(DevPrefix + "STRATAGEM_COOLDOWN", spanCooldown.Minutes,
						spanCooldown.Seconds));
					FillLevel = skill.cooldownRemaining / skill.CalculateFinalRechargeInterval();
					Incoming = false;
					break;
				case TextState.Ready when textTimer <= 0:
					textState = TextState.ReadyAnimation;
					subText.gameObject.SetActive(false);
					inputContainer.SetActive(true);
					Ready();
					break;
				case TextState.ReadyAnimation when !_readyRunning:
					textState = TextState.Done;
					break;
				case TextState.Unavailable when skill.stock > 0:
					// TODO figure out what this should be
					textState = TextState.Done;
					break;
			}

			Visible = dialing || textState != TextState.Done && textState != TextState.Cooldown &&
				textState != TextState.Unavailable;
		}
	}
}