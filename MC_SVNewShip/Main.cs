
using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MC_SVNewShip
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string pluginGuid = "mc.starvalor.newship";
        public const string pluginName = "SV New Ship";
        public const string pluginVersion = "1.0.0";

        private static GameObject shipGO;
        private static Sprite thumbnail;
        private static bool configured = false;

        private static BepInEx.Logging.ManualLogSource log = BepInEx.Logging.Logger.CreateLogSource("NewShip");

        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Main));
            
            string pluginfolder = System.IO.Path.GetDirectoryName(GetType().Assembly.Location);
            string bundleName = "ship";
            AssetBundle assets = AssetBundle.LoadFromFile($"{pluginfolder}\\{bundleName}");
            shipGO = assets.LoadAsset<GameObject>("Assets/Ship.prefab");
            thumbnail = assets.LoadAsset<Sprite>("Assets/Thumbnail.png");
        }

        private static void Configure()
        {
            ConfigureWeapons();
            ConfigureShip();
            configured = true;
        }

        private static void ConfigureWeapons()
        {
            Transform weapons = shipGO.transform.Find("WeaponSlots");

            WeaponTurret turret = weapons.GetChild(0).gameObject.AddComponent<WeaponTurret>();
            turret.turretIndex = 0;
            turret.type = WeaponTurretType.limitedArch;
            turret.spinalMount = false;
            turret.spriteName = ""; // "", "dual_gun", "tri_gun", "quad_gun", "five_gun", "spinal_gun"
            turret.degreesLimit = 10f;
            turret.turnSpeed = 20f;
            turret.totalSpace = 0;
            turret.maxInstalledWeapons = 0;
            turret.weaponMods = new WeaponStatsModifier();
            turret.baseWeaponMods = null;            
            turret.extraBarrels = null; // Array of extra guntip game object transforms
            turret.turnAlong = null; // Array of other transforms to rotate with the "main" turret - used for batteries where a single weapons slot is used to control multiple turret models
            turret.iconTranslate = Vector2.zero; // Tweak position of turret icon
            turret.GetBaseAngles();

            turret = weapons.GetChild(1).gameObject.AddComponent<WeaponTurret>();
            turret.turretIndex = 1;
            turret.type = WeaponTurretType.limitedArch;
            turret.spinalMount = false;
            turret.spriteName = ""; // "", "dual_gun", "tri_gun", "quad_gun", "five_gun", "spinal_gun"
            turret.degreesLimit = 180f;
            turret.turnSpeed = 20f;
            turret.totalSpace = 0;
            turret.maxInstalledWeapons = 0;
            turret.weaponMods = new WeaponStatsModifier();
            turret.baseWeaponMods = null;
            turret.extraBarrels = null; // Array of extra guntip game object transforms
            turret.turnAlong = null; // Array of other transforms to rotate with the "main" turret - used for batteries where a single weapons slot is used to control multiple turret models
            turret.iconTranslate = new Vector2(); // Tweak position of turret icon
            turret.GetBaseAngles();

            turret = weapons.GetChild(2).gameObject.AddComponent<WeaponTurret>();
            turret.turretIndex = 2;
            turret.type = WeaponTurretType.limitedArch;
            turret.spinalMount = false;
            turret.spriteName = ""; // "", "dual_gun", "tri_gun", "quad_gun", "five_gun", "spinal_gun"
            turret.degreesLimit = 180f;
            turret.turnSpeed = 20f;
            turret.totalSpace = 0;
            turret.maxInstalledWeapons = 0;
            turret.weaponMods = new WeaponStatsModifier();
            turret.baseWeaponMods = null;
            turret.extraBarrels = null; // Array of extra guntip game object transforms
            turret.turnAlong = null; // Array of other transforms to rotate with the "main" turret - used for batteries where a single weapons slot is used to control multiple turret models
            turret.iconTranslate = Vector2.one; // Tweak position of turret icon
            turret.GetBaseAngles();
        }

        private static void ConfigureShip()
        {
            ShipModel shipModel = shipGO.AddComponent<ShipModel>();
            shipModel.data = new ShipModelData()
            {
                //Backend
                id = 0,
                image = thumbnail,
                drawScale = 1.0f,
                extraSurFXScale = Vector3.one,
                sizeScale = 1.0f,
                sortPower = 1,
                weaponSlotsGO = shipGO.transform.Find("WeaponSlots"),

                //Stats - general
                agility = 10,
                armor = 100,
                level = 1,
                manufacturer = TFaction.Independent,
                mass = 9,
                rarity = (int)ItemRarity.Common_1,
                shipModelName = "The Blob",
                shipClass = ShipClassLevel.Corvette,
                shipRole = ShipRole.Freighter,
                speed = 11,
                craftingMaterials = new List<CraftMaterial>(),
                modelBonus = new ShipBonus[0],

                //Stats - capacity
                cargoSpace = 12,
                equipSpace = 300,
                hangarDroneSpace = 14,
                hangarShipSpace = 15,
                passengers = 16,
                weaponSpace = 17,
                crewSpace = new CrewSeat[] {
                new CrewSeat() { minRequired = 0, position = CrewPosition.Co_Pilot, space = 1 },
                new CrewSeat() { minRequired = 1, position = CrewPosition.Engineer, space = 2 },
                new CrewSeat() { minRequired = 0, position = CrewPosition.Gunner, space = 1 }
                },

                //Trading
                sellChance = 100,
                factions = new TFaction[] { TFaction.Independent, TFaction.Miners, TFaction.Pirates, TFaction.Rebels, TFaction.Tecnomancers, TFaction.Traders, TFaction.Venghi },
                repReq = new ReputationRequisite() { factionIndex = (int)TFaction.Independent, repNeeded = 0 },
            };            
        }

        [HarmonyPatch(typeof(ShipDB), nameof(ShipDB.LoadDatabaseForce))]
        [HarmonyPostfix]
        private static void ShipDBLoadForce_Post(List<ShipModelData> ___shipModels)
        {
            if (!configured)
                Configure();

            ShipModelData smd = shipGO.GetComponent<ShipModel>().data;
            smd.id = ___shipModels.Count + 1;
            ___shipModels.Add(smd);
            ShipDB.SortList();
        }

        [HarmonyPatch(typeof(ObjManager), nameof(ObjManager.GetShip))]
        [HarmonyPrefix]
        public static bool ObjManagerGetShip_Pre(string str, ref GameObject __result)
        {
            if (!configured)
                Configure();

            Int32.TryParse(str.Substring(str.Length - 2, 2), out int id);
            if (id == shipGO.GetComponent<ShipModel>().data.id)
            {
                __result = shipGO;
                return false;
            }
            else
                return true;
        }

        [HarmonyPatch(typeof(MarketSystem), nameof(MarketSystem.GenerateMarket))]
        [HarmonyPostfix]
        private static void MarketSystemGenerateMarket_Post(List<MarketItem> __result)
        {
            ShipModelData smd = shipGO.GetComponent<ShipModel>().data;
            MarketItem marketItem = new MarketItem(4, smd.id, smd.rarity, 1, null);
            marketItem.lastStockCount = marketItem.stock;
            __result.Add(marketItem);
            MarketSystem.SortMarket(__result);            
        }
    }
}
