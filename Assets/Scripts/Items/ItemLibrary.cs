using UnityEngine;

namespace TowerGame.Items
{
    /// <summary>
    /// Central registry for every reward in the run.  We build the ScriptableObjects
    /// at runtime (instead of shipping .asset files) so the project can open and run
    /// the moment Unity imports the C# scripts.
    /// </summary>
    public static class ItemLibrary
    {
        private static StaminaPotionItem _staminaPotion;
        private static IronSwordItem _ironSword;
        private static FireboltRuneItem _fireboltRune;
        private static DashBootsItem _dashBoots;
        private static TowerAmuletItem _towerAmulet;
        private static VitalityCharmItem _vitalityCharm;
        private static GritBandItem _gritBand;
        private static PredatorCharmItem _predatorCharm;
        private static readonly FloorKeyItem[] _floorKeys = new FloorKeyItem[12];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            _staminaPotion = null;
            _ironSword = null;
            _fireboltRune = null;
            _dashBoots = null;
            _towerAmulet = null;
            _vitalityCharm = null;
            _gritBand = null;
            _predatorCharm = null;
            for (int i = 0; i < _floorKeys.Length; i++) _floorKeys[i] = null;
        }

        public static StaminaPotionItem StaminaPotion()
        {
            if (_staminaPotion == null)
            {
                _staminaPotion = ScriptableObject.CreateInstance<StaminaPotionItem>();
                _staminaPotion.displayName = "Stamina Potion";
                _staminaPotion.description = "Restores stamina and a little HP. Press F3 to drink.";
                _staminaPotion.spriteColor = new Color(0.35f, 0.85f, 0.45f);
                _staminaPotion.usage = ItemUsage.Active;
                _staminaPotion.consumable = false;
            }
            return _staminaPotion;
        }

        public static IronSwordItem IronSword()
        {
            if (_ironSword == null)
            {
                _ironSword = ScriptableObject.CreateInstance<IronSwordItem>();
                _ironSword.displayName = "Iron Sword";
                _ironSword.description = "+25% damage on melee and ranged (passive).";
                _ironSword.spriteColor = new Color(0.85f, 0.85f, 0.9f);
                _ironSword.usage = ItemUsage.Passive;
            }
            return _ironSword;
        }

        public static FireboltRuneItem FireboltRune()
        {
            if (_fireboltRune == null)
            {
                _fireboltRune = ScriptableObject.CreateInstance<FireboltRuneItem>();
                _fireboltRune.displayName = "Firebolt Rune";
                _fireboltRune.description = "Upgrades your right-click ranged bolt into a faster, harder-hitting firebolt.";
                _fireboltRune.spriteColor = new Color(1f, 0.55f, 0.15f);
                _fireboltRune.usage = ItemUsage.Passive;
            }
            return _fireboltRune;
        }

        public static DashBootsItem DashBoots()
        {
            if (_dashBoots == null)
            {
                _dashBoots = ScriptableObject.CreateInstance<DashBootsItem>();
                _dashBoots.displayName = "Dash Boots";
                _dashBoots.description = "Space to dash (costs stamina).";
                _dashBoots.spriteColor = new Color(0.35f, 0.25f, 0.55f);
                _dashBoots.usage = ItemUsage.Passive;
            }
            return _dashBoots;
        }

        public static TowerAmuletItem TowerAmulet()
        {
            if (_towerAmulet == null)
            {
                _towerAmulet = ScriptableObject.CreateInstance<TowerAmuletItem>();
                _towerAmulet.displayName = "Tower Amulet";
                _towerAmulet.description = "+50% attack, +50 max HP. Tower cleared!";
                _towerAmulet.spriteColor = new Color(0.55f, 0.95f, 0.95f);
                _towerAmulet.usage = ItemUsage.Passive;
            }
            return _towerAmulet;
        }

        public static VitalityCharmItem VitalityCharm()
        {
            if (_vitalityCharm == null)
            {
                _vitalityCharm = ScriptableObject.CreateInstance<VitalityCharmItem>();
                _vitalityCharm.displayName = "Vitality Charm";
                _vitalityCharm.description = "+35 max HP and heal for the same (passive).";
                _vitalityCharm.spriteColor = new Color(0.95f, 0.45f, 0.55f);
                _vitalityCharm.usage = ItemUsage.Passive;
            }
            return _vitalityCharm;
        }

        public static GritBandItem GritBand()
        {
            if (_gritBand == null)
            {
                _gritBand = ScriptableObject.CreateInstance<GritBandItem>();
                _gritBand.displayName = "Grit Band";
                _gritBand.description = "+12 stamina regen per second (passive).";
                _gritBand.spriteColor = new Color(0.55f, 0.65f, 0.95f);
                _gritBand.usage = ItemUsage.Passive;
            }
            return _gritBand;
        }

        public static PredatorCharmItem PredatorCharm()
        {
            if (_predatorCharm == null)
            {
                _predatorCharm = ScriptableObject.CreateInstance<PredatorCharmItem>();
                _predatorCharm.displayName = "Predator Charm";
                _predatorCharm.description = "+15% damage on melee and ranged (passive).";
                _predatorCharm.spriteColor = new Color(0.75f, 0.35f, 0.95f);
                _predatorCharm.usage = ItemUsage.Passive;
            }
            return _predatorCharm;
        }

        public static FloorKeyItem FloorKey(int floor)
        {
            if (floor < 1 || floor >= _floorKeys.Length) floor = 1;
            if (_floorKeys[floor] == null)
            {
                var key = ScriptableObject.CreateInstance<FloorKeyItem>();
                key.floorNumber = floor;
                key.displayName = $"Floor {floor} Key";
                key.description = "Opens the stairway to the next floor.";
                key.spriteColor = new Color(1f, 0.85f, 0.2f);
                key.usage = ItemUsage.Key;
                _floorKeys[floor] = key;
            }
            return _floorKeys[floor];
        }
    }
}
