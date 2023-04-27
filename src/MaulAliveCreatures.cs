using BepInEx;
using System;
using UnityEngine;
using Menu.Remix.MixedUI;
using static Player;
using Random = UnityEngine.Random;
using static MoreSlugcats.SingularityBomb;
using System.Collections;

namespace MaulAliveCreatures;
[BepInPlugin("MaulAliveCreatures", "Maul Alive Creatures", "1.2")]

public class MaulAliveCreatures : BaseUnityPlugin{

    static bool _initialized;
    private OptionsMenu optionsMenuInstance;

    static bool mauling;
    public DynamicSoundLoop scavcry;


    private void LogInfo(object ex)
    {
        Logger.LogInfo(ex);
    }

    public void OnEnable(){
        LogInfo("I'M ANGRY, HOW WAKE ME? NOW, Maul Alive Creatures is Wake up!!");
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self){
        optionsMenuInstance = new OptionsMenu();
        orig(self);

        try
        {
            if (_initialized) return;
            _initialized = true;

            //Hooks for mauling
            On.Player.CanMaulCreature += Player_CanMaulCreature;
            On.Player.Grabability += Player_Grabability;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.TossObject += Player_TossObject;
            On.Player.ReleaseGrasp += Player_ReleaseGrasp;

            //Hooks for sounds
            On.Creature.Violence += Creature_Violence;
            On.Scavenger.Violence += Scavenger_Violence;
            On.Lizard.Violence += Lizard_Violence;

            MachineConnector.SetRegisteredOI("MaulAliveCreatures", optionsMenuInstance);
            CryAboutIt.RegisterValues();

        }
        catch (Exception ex)
        {
            Debug.Log($"Remix Menu: Hook_OnModsInit options failed init error {optionsMenuInstance}{ex}");
            Logger.LogError(ex);
            Logger.LogMessage("WHOOPS something go wrong");
        }
        finally
        {
            orig.Invoke(self);
        }
    }



    private void Lizard_Violence(On.Lizard.orig_Violence orig, Lizard self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos onAppendagePos, Creature.DamageType type, float damage, float stunBonus)
    {
        orig(self, source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
        var room = self.room;
        if (source?.owner is Player player1 && type == Creature.DamageType.Bite && Random.value <= 0.05)
        {
            player1.AddQuarterFood();
        }
        if (source?.owner is Player && type == Creature.DamageType.Bite && !OptionsMenu.Sounds.Value)
        {
            room?.PlaySound(CryAboutIt.munch, self.firstChunk);
        }
        if (source?.owner is Player player2 && type == Creature.DamageType.Bite && Random.value <= 0.10 && !OptionsMenu.Sounds.Value)
        {
            room?.PlaySound(CryAboutIt.yummers, self.firstChunk);
            player2.AddQuarterFood();
        }
    }

    private void Scavenger_Violence(On.Scavenger.orig_Violence orig, Scavenger self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        var room = self.room;
        if (source?.owner is Player && type == Creature.DamageType.Bite && !OptionsMenu.Sounds.Value)
        {
            room.PlaySound(CryAboutIt.scavcry, self.firstChunk);
        }
    }

    private void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        var room = self.room;
        if (source?.owner is Player player1 && type == Creature.DamageType.Bite && Random.value <= 0.05)
        {
            player1.AddQuarterFood();
        }
        if (source?.owner is Player && type == Creature.DamageType.Bite && !OptionsMenu.Sounds.Value)
        {
            room?.PlaySound(CryAboutIt.munch, self.firstChunk);
        }
        if (source?.owner is Player player2 && type == Creature.DamageType.Bite && Random.value <= 0.10 && !OptionsMenu.Sounds.Value)
        {
            room?.PlaySound(CryAboutIt.yummers, self.firstChunk);
            player2.AddQuarterFood();
        }

    }

    private ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (obj is Player player && (player.isSlugpup || player.isNPC) || obj is JetFish)
        {
            return orig(self, obj);
        }
        if (obj is Creature && obj is not JetFish && obj is not PoleMimic && obj is not BigEel && obj is not Player)
        {
            return ObjectGrabability.Drag;
        }
        else if (obj is Scavenger || obj is BigEel)
        {
            return ObjectGrabability.OneHand;
        }
        else
        {
            return orig(self, obj);
        }
    }

    private void Player_ReleaseGrasp(On.Player.orig_ReleaseGrasp orig, Player self, int grasp)
    {
        if(!mauling /*|| ((self.room.game.IsStorySession || self.room.game.IsArenaSession) && !(ModManager.CoopAvailable && !RWCustom.Custom.rainWorld.options.friendlyFire))*/) orig(self, grasp);
    }

    private void Player_TossObject(On.Player.orig_TossObject orig, Player self, int grasp, bool eu)
    {
        if (!mauling /*|| ((self.room.game.IsStorySession || self.room.game.IsArenaSession) && !(ModManager.CoopAvailable && !RWCustom.Custom.rainWorld.options.friendlyFire))*/) orig(self, grasp, eu);
    }

    private void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
    {
        mauling = self.maulTimer > 20;
        try
        {
            orig(self, eu);
        }
        finally
        {
            mauling = false; 
        }

    }

     
    private bool Player_CanMaulCreature(On.Player.orig_CanMaulCreature orig, Player self, Creature crit)
    {

        if ((self.room.game.IsStorySession || self.room.game.IsArenaSession) && (ModManager.CoopAvailable && !RWCustom.Custom.rainWorld.options.friendlyFire))
        {
            return false;
        }

        
        if ((crit is not null) && !crit.dead && (crit is not IPlayerEdible || !crit.Stunned || (crit.Stunned && (crit is Cicada) && crit is not Player)))
        {
            (self.grasps[0].grabbed as Creature).Stun(20);

            return true;
            
        }
        return false;

    }
}

public static class CryAboutIt
{
    public static void RegisterValues()
    {
        scavcry = new SoundID("scavcry", true);
        munch = new SoundID("munch", true);
        yummers = new SoundID("yummers", true);

    }
    public static void UnregisterValues()
    {
        Unregister(scavcry);
        Unregister(munch);
        Unregister(yummers);

    }

    private static void Unregister<T>(ExtEnum<T> extEnum) where T : ExtEnum<T>
    {
        extEnum?.Unregister();
    }

    public static SoundID scavcry;
    public static SoundID munch;
    public static SoundID yummers;

}

public class OptionsMenu : OptionInterface
{
    public static OptionsMenu instance = new();

    public static Configurable<bool> Sounds;


    public OptionsMenu()
    {
        Sounds = config.Bind("Sounds", false);
    }

    public override void Initialize()
    {
        var opTab1 = new OpTab(this, "Options");
        Tabs = new[] { opTab1 };

        OpContainer tab1Container = new OpContainer(new Vector2(0, 0));
        opTab1.AddItems(tab1Container);

        UIelement[] UIArrayElements2 = new UIelement[]
        {
                new OpCheckBox(Sounds, 10, 540),
                new OpLabel(45f, 540f, "Disable the sounds"),
        };
        opTab1.AddItems(UIArrayElements2);
    }

}