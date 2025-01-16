using BepInEx.Bootstrap;
using EntityStates;
using ExtraSkillSlots;
using HellDiver.Data.Grenades;
using HellDiver.Data.Primary;
using HellDiver.Data.Secondary;
using HellDiver.Data.Stratagems;
using RoR2;
using UnityEngine;
using R2API;

namespace HellDiver.Data
{
	public class HellDiver : Concentric, IMaster, IBody, ISurvivor
	{
		private static int? _firstStratagemSlot;

		public static int FirstStratagemSlot
		{
			get
			{
				if (_firstStratagemSlot != null) return _firstStratagemSlot.Value;

				var body = GetBody<HellDiver>().Result;
				_firstStratagemSlot = body.GetComponents<GenericSkill>().TakeWhile(x =>
					!TryGetAssetFromObject(x.skillFamily.defaultSkillDef, out Concentric asset) || asset is not Stratagem).Count();
				return _firstStratagemSlot.Value;
			}
		}

		public static List<GenericSkill> stratagemSkills = new List<GenericSkill>();

		public const int StratagemCount = 4;

		async Task<GameObject> IMaster.BuildObject()
		{
			var master = (await LoadAsset<GameObject>("RoR2/Base/Merc/MercMonsterMaster.prefab"))!.InstantiateClone(
				"HellDiverMaster", true);
			master.GetComponent<CharacterMaster>().bodyPrefab = await this.GetBody();
			return master;
		}

		async Task<GameObject> IBody.BuildObject()
		{
			var bodyPrefab = (await LoadAsset<GameObject>("legacy:Prefabs/CharacterBodies/MercBody"))!
				.InstantiateClone("HellDiverBody");

			var bodyComponent = bodyPrefab.GetComponent<CharacterBody>();
			var bodyHealthComponent = bodyPrefab.GetComponent<HealthComponent>();
			var hellDiverComponent = bodyPrefab.AddComponent<HellDiverBehavior>();

			/*
			#region Setup Model
			var model = await this.GetModel();

			var bodyHurtBoxGroup = model.GetComponentInChildren<HurtBoxGroup>();
			foreach (var hurtBox in bodyHurtBoxGroup.hurtBoxes)
			{
				hurtBox.healthComponent = bodyHealthComponent;
			}

			var bodyModelLocator = bodyPrefab.GetComponent<ModelLocator>();
			UnityEngine.Object.Destroy(bodyModelLocator.modelTransform.gameObject);
			model.transform.parent = bodyModelLocator.modelBaseTransform;
			model.GetComponent<CharacterModel>().body = bodyComponent;
			bodyModelLocator.modelTransform = model.transform;
			//bodyHealthComponent.modelLocator = bodyModelLocator; this isnt even serialized by unity, so its not set in the prefab either

			#endregion
			*/

			var stateMachines = new List<EntityStateMachine>();

			#region Setup StateMachines

			foreach (var toDestroy in bodyPrefab.GetComponents<EntityStateMachine>())
			{
				UnityEngine.Object.Destroy(toDestroy);
			}

			var bodyStateMachine = bodyPrefab.AddComponent<EntityStateMachine>();
			bodyStateMachine.customName = "Body";
			bodyStateMachine.initialStateType = new SerializableEntityStateType(typeof(GenericCharacterMain));
			bodyStateMachine.mainStateType = bodyStateMachine.initialStateType;
			stateMachines.Add(bodyStateMachine);

			var weaponStateMachine = bodyPrefab.AddComponent<EntityStateMachine>();
			weaponStateMachine.customName = "Weapon";
			weaponStateMachine.initialStateType = new SerializableEntityStateType(typeof(Idle));
			weaponStateMachine.mainStateType = weaponStateMachine.initialStateType;
			stateMachines.Add(weaponStateMachine);

			var stratagemStateMachine = bodyPrefab.AddComponent<EntityStateMachine>();
			stratagemStateMachine.customName = "Stratagem";
			stratagemStateMachine.initialStateType = new SerializableEntityStateType(typeof(Idle));
			stratagemStateMachine.mainStateType = stratagemStateMachine.initialStateType;
			stateMachines.Add(stratagemStateMachine);

			var deathBehaviour = bodyPrefab.GetOrAddComponent<CharacterDeathBehavior>();
			deathBehaviour.deathStateMachine = bodyStateMachine;
			deathBehaviour.idleStateMachine = new[] { weaponStateMachine, stratagemStateMachine };

			#endregion

			#region SetupSkills

			foreach (var toDestroy in bodyPrefab.GetComponents<GenericSkill>())
			{
				UnityEngine.Object.Destroy(toDestroy);
			}

			var skillLocator = bodyPrefab.GetComponent<SkillLocator>();

			var primarySkill = bodyPrefab.AddComponent<GenericSkill>();
			primarySkill.skillName = "Primary";
			primarySkill._skillFamily = await GetSkillFamily<PrimaryFamily>();
			skillLocator.primary = primarySkill;

			var secondarySkill = bodyPrefab.AddComponent<GenericSkill>();
			secondarySkill.skillName = "Secondary";
			secondarySkill._skillFamily = await GetSkillFamily<SecondaryFamily>();
			skillLocator.secondary = secondarySkill;

			var utilitySkill = bodyPrefab.AddComponent<GenericSkill>();
			utilitySkill.skillName = "Utility";
			utilitySkill._skillFamily = await GetSkillFamily<CallStratagem>();
			skillLocator.utility = utilitySkill;

			var specialSkill = bodyPrefab.AddComponent<GenericSkill>();
			specialSkill.skillName = "Special";
			specialSkill._skillFamily = await GetSkillFamily<GrenadesFamily>();
			skillLocator.special = specialSkill;

			#endregion

			// TODO Maybe remove the config check from here, because this disables being able to swap from ess and movement based inputs at runtime.
			if (CallStratagem.useExtraSkillsForStratagemCalling.Value &&
			    Chainloader.PluginInfos.ContainsKey(ExtraSkillSlotsPlugin.GUID))
			{
				await SetupExtraSkillSlots(bodyPrefab);
							
				var stratagemDialingStateMachine = bodyPrefab.AddComponent<EntityStateMachine>();
				stratagemDialingStateMachine.customName = "Dialing";
				stratagemDialingStateMachine.initialStateType = new SerializableEntityStateType(typeof(Idle));
				stratagemDialingStateMachine.mainStateType = stratagemDialingStateMachine.initialStateType;
				stateMachines.Add(stratagemDialingStateMachine);
			}

			#region Stratagems

			var stratagemFamily = await GetSkillFamily<StratagemFamily>();
			for (var i = 0; i < StratagemCount; i++)
			{
				var skill = bodyPrefab.AddComponent<GenericSkill>();
				skill.skillName = "Stratagem" + i;
				skill._skillFamily = stratagemFamily;
				stratagemSkills.Add(skill);
			}

			#endregion

			var networkStateMachine = bodyPrefab.GetOrAddComponent<NetworkStateMachine>();
			networkStateMachine.stateMachines = stateMachines.ToArray();

			return bodyPrefab;
		}

		public async Task SetupExtraSkillSlots(GameObject bodyPrefab)
		{
			var extraSkillLocator = bodyPrefab.GetOrAddComponent<ExtraSkillLocator>();

			var upSkill = bodyPrefab.AddComponent<GenericSkill>();
			upSkill.hideInCharacterSelect = true;
			upSkill.skillName = "Up";
			upSkill._skillFamily = await GetSkillFamily<UpFamily>();
			extraSkillLocator.extraFirst = upSkill;

			var downSkill = bodyPrefab.AddComponent<GenericSkill>();
			downSkill.hideInCharacterSelect = true;
			downSkill.skillName = "Down";
			downSkill._skillFamily = await GetSkillFamily<DownFamily>();
			extraSkillLocator.extraSecond = downSkill;

			var leftSkill = bodyPrefab.AddComponent<GenericSkill>();
			leftSkill.hideInCharacterSelect = true;
			leftSkill.skillName = "Left";
			leftSkill._skillFamily = await GetSkillFamily<LeftFamily>();
			extraSkillLocator.extraThird = leftSkill;

			var rightSkill = bodyPrefab.AddComponent<GenericSkill>();
			rightSkill.hideInCharacterSelect = true;
			rightSkill.skillName = "Right";
			rightSkill._skillFamily = await GetSkillFamily<RightFamily>();
			extraSkillLocator.extraFourth = rightSkill;
		}

		async Task<SurvivorDef> ISurvivor.BuildObject()
		{
			var survivor = ScriptableObject.CreateInstance<SurvivorDef>();
			//survivor.primaryColor = ;
			survivor.displayNameToken = DevPrefix + "HELLDIVER_NAME";
			survivor.descriptionToken = DevPrefix + "HELLDIVER_DESCRIPTION";
			survivor.outroFlavorToken = DevPrefix + "HELLDIVER_OUTRO_FLAVOR";
			survivor.mainEndingEscapeFailureFlavorToken = DevPrefix + "HELLDIVER_OUTRO_FAILURE";
			survivor.desiredSortPosition = 100f;

			survivor.bodyPrefab = await this.GetBody();

			return survivor;
		}
	}
}