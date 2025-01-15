using BepInEx.Configuration;
using EntityStates;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace HellDiver.Data.Stratagems
{
	public class CallStratagemState : BaseSkillState
	{
		private Stratagem.Inputs _lastInput;
		public List<Stratagem.Inputs> dialedInputs = new List<Stratagem.Inputs>();
		private Stratagem.Inputs _lastDialedInput;
		private HellDiverBehavior _hellDiverBehavior;
		private bool _wasMoving;

		public override void OnEnter()
		{
			base.OnEnter();
			_hellDiverBehavior = GetComponent<HellDiverBehavior>();

			_wasMoving = inputBank.rawMoveData.sqrMagnitude > 0.1;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (isAuthority && !IsKeyDownAuthority())
			{
				outer.SetNextStateToMain();
				return;
			}

			if (!CallStratagem.useExtraSkillsForStratagemCalling.Value) HandleMoveVector();

			if (_wasMoving)
				if (_lastInput != Stratagem.Inputs.Rest)
					return;
				else
					_wasMoving = false;

			if (_lastInput != Stratagem.Inputs.Rest && _lastDialedInput != _lastInput)
			{
				Dial(_lastInput);
			}
			else if (_lastInput == Stratagem.Inputs.Rest)
			{
				_lastDialedInput = Stratagem.Inputs.Rest;
			}
		}

		public override void OnExit()
		{
			base.OnExit();
			if (!isAuthority) return;

			foreach (var (slot, stratagem) in _hellDiverBehavior.stratagemSkills)
			{
				if (!stratagem.inputs.SequenceEqual(dialedInputs)) continue;
				slot.ExecuteIfReady();
				return;
			}
		}

		private void HandleMoveVector()
		{
			var x = inputBank.rawMoveData.normalized.x;
			var y = inputBank.rawMoveData.normalized.y;

			if (_lastInput != Stratagem.Inputs.Rest &&
			    y < 0.1 && y > -0.1 && x < 0.1 && x > -0.1)
				_lastInput = Stratagem.Inputs.Rest;
			else if (y > 0.90 && x < 0.1 && x > -0.1)
				_lastInput = Stratagem.Inputs.Up;
			else if (y < -0.90 && x < 0.1 && x > -0.1)
				_lastInput = Stratagem.Inputs.Down;
			else if (x < -0.90 && y < 0.1 && y > -0.1)
				_lastInput = Stratagem.Inputs.Left;
			else if (x > 0.90 && y < 0.1 && y > -0.1)
				_lastInput = Stratagem.Inputs.Right;
		}

		public void Dial(Stratagem.Inputs input)
		{
			Util.PlaySound("StratDialSound", gameObject);
			log.LogInfo("Dialed " + input);
			dialedInputs.Add(input);
			_lastDialedInput = input;
		}
	}

	public class CallStratagem : Concentric, ISkill, ISkillFamily
	{
		public static ConfigEntry<bool> useExtraSkillsForStratagemCalling =
			instance.Config.Bind("General", "Use ExtraSkillSlots", false,
				"Set to true if you want to use the extra skills to input the directions when calling stratagems. " +
				"Good for controllers. " +
				"Make sure you have ExtraSkillSlots installed.");

		public bool HiddenFromCharacterSelect => true;

		Task<SkillDef> ISkill.BuildObject()
		{
			var skillDef = ScriptableObject.CreateInstance<SkillDef>();
			//TODO finish skillDef
			skillDef.skillName = DevPrefix + "STRATAGEM_DIALING_NAME";
			skillDef.skillNameToken = DevPrefix + "STRATAGEM_DIALING_NAME";
			//skill.icon = LoadAsset<Sprite>("");
			skillDef.skillDescriptionToken = DevPrefix + "STRATAGEM_DIALING_DESC";
			skillDef.activationStateMachineName = "Body";
			return Task.FromResult(skillDef);
		}

		IEnumerable<Concentric> ISkillFamily.GetSkillAssets() => new[] { this };

		IEnumerable<Type> ISkill.GetEntityStates() => new[] { typeof(CallStratagemState) };
	}
}