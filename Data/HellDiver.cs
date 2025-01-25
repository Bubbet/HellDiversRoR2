using BepInEx.Bootstrap;
using EntityStates;
using ExtraSkillSlots;
using HellDiver.Data.Grenades;
using HellDiver.Data.Primary;
using HellDiver.Data.Secondary;
using HellDiver.Data.Stratagems;
using HelldiverCaptain;
using RoR2;
using UnityEngine;
using R2API;
using RoR2.Skills;

namespace HellDiver.Data
{
	public class HellDiver : Concentric, IMaster, IBody, ISurvivor, IModel, IBodyDisplay, ISkin, IEntityStates
	{
		private static int? _firstStratagemSlot;

		public static int FirstStratagemSlot
		{
			get
			{
				if (_firstStratagemSlot != null) return _firstStratagemSlot.Value;

				var body = GetBody<HellDiver>().Result;
				_firstStratagemSlot = body.GetComponents<GenericSkill>().TakeWhile(x =>
					!TryGetAssetFromObject(x.skillFamily.defaultSkillDef, out Concentric asset) ||
					asset is not Stratagem).Count();
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

			#endregion

			var stateMachines = new List<EntityStateMachine>();

			#region Setup StateMachines

			foreach (var toDestroy in bodyPrefab.GetComponents<EntityStateMachine>())
			{
				UnityEngine.Object.Destroy(toDestroy);
			}

			var bodyStateMachine = bodyPrefab.AddComponent<EntityStateMachine>();
			bodyStateMachine.customName = "Body";
			bodyStateMachine.initialStateType = new SerializableEntityStateType(typeof(HellDiverMain));
			bodyStateMachine.mainStateType = bodyStateMachine.initialStateType;
			stateMachines.Add(bodyStateMachine);

			var weaponStateMachine = bodyPrefab.AddComponent<EntityStateMachine>();
			weaponStateMachine.customName = "Weapon";
			weaponStateMachine.initialStateType = new SerializableEntityStateType(typeof(Idle));
			weaponStateMachine.mainStateType = weaponStateMachine.initialStateType;
			stateMachines.Add(weaponStateMachine);

			var deathBehaviour = bodyPrefab.GetOrAddComponent<CharacterDeathBehavior>();
			deathBehaviour.deathStateMachine = bodyStateMachine;
			deathBehaviour.idleStateMachine = new[] { weaponStateMachine };

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
				var stratagemStateMachine = bodyPrefab.AddComponent<EntityStateMachine>();
				stratagemStateMachine.customName = "Stratagem" + i;
				stratagemStateMachine.initialStateType = new SerializableEntityStateType(typeof(Idle));
				stratagemStateMachine.mainStateType = stratagemStateMachine.initialStateType;
				stateMachines.Add(stratagemStateMachine);

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

			survivor.displayPrefab = await this.GetBodyDisplay();
			survivor.bodyPrefab = await this.GetBody();

			return survivor;
		}

		async Task<GameObject> IBodyDisplay.BuildObject()
		{
			var model = await this.GetModel();
			var displayModel = model.InstantiateClone("HellDiverDisplay", false);
			return displayModel;
		}

		async Task<GameObject> IModel.BuildObject()
		{
			var model = (await LoadAsset<GameObject>("RoR2/Base/Captain/CaptainBody.prefab")).transform
				.Find("ModelBase/mdlCaptain").gameObject.InstantiateClone("mdlHellDiver", false);
			var mesh = HelldiverCaptainPlugin.assetBundle.LoadAsset<Mesh>(
				"Assets\\SkinMods\\HelldiverCaptain\\Meshes\\Helldiver.mesh");

			var modelComponent = model.GetComponent<CharacterModel>();
			model.GetComponent<Animator>().SetBool("isGrounded", true);
			var firstInfo = modelComponent.baseRendererInfos[0];
			((SkinnedMeshRenderer)firstInfo.renderer).sharedMesh = mesh;

			for (var i = 1; i < modelComponent.baseRendererInfos.Length; i++)
			{
				UnityEngine.Object.DestroyImmediate(modelComponent.baseRendererInfos[i].renderer.gameObject);
			}

			modelComponent.baseRendererInfos = new[] { firstInfo };

			return model;
		}

		IEnumerable<Concentric> IModel.GetSkins() => new[] { this };

		async Task<SkinDef> ISkin.BuildObject()
		{
			var model = await this.GetModel();
			return (SkinDef)ScriptableObject.CreateInstance(typeof(SkinDef), obj =>
			{
				var skinDef = (SkinDef)obj;
				ISkin.AddDefaults(ref skinDef);
				skinDef.name = "HellDiverDefaultSkinDef";
				skinDef.nameToken = DevPrefix + "HELLDIVER_DEFAULT_SKIN_NAME";
				//skinDef.icon = icon;

				skinDef.rootObject = model;
				var modelRendererInfos = model.GetComponent<CharacterModel>().baseRendererInfos;
				var rendererInfos = new CharacterModel.RendererInfo[modelRendererInfos.Length];
				modelRendererInfos.CopyTo(rendererInfos, 0);
				skinDef.rendererInfos = rendererInfos;
			});
		}

		IEnumerable<Type> IEntityStates.GetEntityStates() => new[] { typeof(HellDiverMain) };
	}

	public class HellDiverMain : GenericCharacterMain
	{
		public static List<GameObject> subscribedBodies = new List<GameObject>();

		public override void OnEnter()
		{
			base.OnEnter();


			if (subscribedBodies.Contains(gameObject)) return;
			foreach (var skill in skillLocator.allSkills.Skip(HellDiver.FirstStratagemSlot)
				         .Take(HellDiver.StratagemCount))
			{
				skill.customStateMachineResolver +=
					ResolveStratagemStates;
			}

			subscribedBodies.Add(gameObject);
		}

		// ReSharper disable once RedundantAssignment
		public static void ResolveStratagemStates(GenericSkill genericSkill, SkillDef _, ref EntityStateMachine machine)
		{
			machine = EntityStateMachine.FindByCustomName(genericSkill.gameObject, genericSkill.skillName);
		}
	}
}