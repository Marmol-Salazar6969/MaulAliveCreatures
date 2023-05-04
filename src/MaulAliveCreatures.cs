using BepInEx;
using System;
using UnityEngine;
using Menu.Remix.MixedUI;
using static Player;
using Random = UnityEngine.Random;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace MaulAliveCreatures;
[BepInPlugin("MaulAliveCreatures", "Maul Alive Creatures", "1.3")]

public class MaulAliveCreatures : BaseUnityPlugin{

    static bool _initialized;
    static bool mauling;

    public static OptionsMenu optionsMenuInstance;

    private void LogInfo(object ex)
    {
        Logger.LogInfo(ex);
    }

    public void OnEnable(){
        LogInfo("I'M ANGRY, HOW WAKE ME? NOW, Maul Alive Creatures is Wake up!!");
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self){
        orig(self);

        try
        {
            if (_initialized) return;
            _initialized = true;

            On.SlugcatStats.SlugcatCanMaul += SlugcatStats_SlugcatCanMaul;
            On.Player.CanMaulCreature += Player_CanMaulCreature;
            On.Player.CanEatMeat += Player_CanEatMeat;

            On.Player.Grabability += Player_Grabability;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.TossObject += Player_TossObject;
            On.Player.ReleaseGrasp += Player_ReleaseGrasp;

            On.Creature.Violence += Creature_Violence;
            On.Scavenger.Violence += Scavenger_Violence;
            On.Lizard.Violence += Lizard_Violence;
            

            IL.Player.GrabUpdate += Player_GrabUpdate1;

            MachineConnector.SetRegisteredOI("MaulAliveCreatures", optionsMenuInstance = new OptionsMenu());
            Debug.LogWarning("Options menu added new instance");

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

    private bool SlugcatStats_SlugcatCanMaul(On.SlugcatStats.orig_SlugcatCanMaul orig, SlugcatStats.Name slugcatNum)
    {
        orig(slugcatNum);
        if (!OptionsMenu.Dinner.Value)
        {
            return true;
        }
        else
        {
            return orig(slugcatNum);
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
            (crit as Creature).Stun(20);

            return true;

        }
        return false;

    }

    private bool Player_CanEatMeat(On.Player.orig_CanEatMeat orig, Player self, Creature crit)
    {
        orig(self, crit);
        if (!OptionsMenu.Dinner.Value && crit is not Player)
        {
            return true;
        }
        else
        {
            return orig(self, crit);
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

    private void Player_TossObject(On.Player.orig_TossObject orig, Player self, int grasp, bool eu)
    {
        if (!mauling) orig(self, grasp, eu);
    }

    private void Player_ReleaseGrasp(On.Player.orig_ReleaseGrasp orig, Player self, int grasp)
    {
        if (!mauling) orig(self, grasp);
    }

    private void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);

        var room = self.room;
        if (source?.owner is Player player1 && type == Creature.DamageType.Bite && Random.value <= 0.05)
        {
            if (!OptionsMenu.Meat.Value)
            {
                player1.AddQuarterFood();
            }
        }
        if (source?.owner is Player && type == Creature.DamageType.Bite && !OptionsMenu.Sounds.Value)
        {
            room?.PlaySound(CryAboutIt.munch, self.firstChunk);
        }
        if (source?.owner is Player player2 && type == Creature.DamageType.Bite && Random.value <= 0.10 && !OptionsMenu.Sounds.Value)
        {
            room?.PlaySound(CryAboutIt.yummers, self.firstChunk);
            if (!OptionsMenu.Meat.Value)
            {
                player2.AddQuarterFood();
            }
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

    private void Lizard_Violence(On.Lizard.orig_Violence orig, Lizard self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos onAppendagePos, Creature.DamageType type, float damage, float stunBonus)
    {

        orig(self, source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
        var room = self.room;

        if (source?.owner is Player player1 && type == Creature.DamageType.Bite && Random.value <= 0.05)
        {
            if (!OptionsMenu.Meat.Value)
            {
                player1.AddQuarterFood();
            }
        }

        if (source?.owner is Player && type == Creature.DamageType.Bite && !OptionsMenu.Sounds.Value)
        {
            room?.PlaySound(CryAboutIt.munch, self.firstChunk);
        }

        if (source?.owner is Player player2 && type == Creature.DamageType.Bite && Random.value <= 0.10 && !OptionsMenu.Sounds.Value)
        {
            room?.PlaySound(CryAboutIt.yummers, self.firstChunk);
            
            {
                player2.AddQuarterFood();
            }    
        }
    }

    private void Player_GrabUpdate1(ILContext il)
    {
        var cursor = new ILCursor(il);
        try
        {
            if (!cursor.TryGotoNext(MoveType.Before,
                i => i.MatchLdcR4(1f),
                i => i.MatchLdcR4(15f),
                i => i.MatchCallOrCallvirt<Creature>(nameof(Creature.Violence))
                ))
            {
                throw new Exception("FAILED TRYING TO MATCH, NO VALUE HERE???!!");
            }

            cursor.Index += 1;
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<float, Player, float>>((baseDamage, player) =>
            {
                return (OptionsMenu.Yummy.Value);
            });

            Logger.LogWarning("CUSTOM MAUL DAMAGE SET FROM MAUL ALIVE CREATURES");
        }
        catch (Exception ex)
        {
            Debug.LogError($"THE IL HOOK IN PLAYER GRAB UPDATE, DAMAGE, FAILED SO MUCH, SKILL ISSUE!");
            Debug.LogException(ex);
            Debug.LogError(il);
            throw;
        }
    }
}