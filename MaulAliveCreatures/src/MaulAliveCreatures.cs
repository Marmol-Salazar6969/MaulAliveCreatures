using BepInEx;
using System;
using UnityEngine;
using static Player;
using Random = UnityEngine.Random;

namespace MaulAliveCreatures;
[BepInPlugin("MaulAliveCreatures", "Maul Alive Creatures", "1.1")]

public class MaulAliveCreatures : BaseUnityPlugin{

    static bool mauling;

    public DynamicSoundLoop scavcry;

    private void LogInfo(object data){
        Logger.LogInfo(data);
    }

    public void OnEnable(){
        LogInfo("I'M ANGRY, HOW WAKE ME? NOW, Maul Alive Creatures is Wake up!!");
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;

    }
    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self){
        try{
            //Hooks default for maul logic
            On.Player.CanMaulCreature += Player_CanMaulCreature;
            On.Player.Grabability += Player_Grabability;
            On.Player.HeavyCarry += Player_HeavyCarry;
            On.Player.GrabUpdate += Player_GrabUpdate;
            On.Player.TossObject += Player_TossObject;
            On.Player.ReleaseGrasp += Player_ReleaseGrasp;
            On.Creature.Violence += Creature_Violence;
            

            //Hooks for sounds
            CryAboutIt.RegisterValues();
            On.Scavenger.Violence += Scavenger_Violence;
            On.Lizard.Violence += Lizard_Violence;
        }
        catch (Exception data){
            LogInfo(data);
            throw;
        }
        finally{
            orig.Invoke(self);
        }
    }

    private void Lizard_Violence(On.Lizard.orig_Violence orig, Lizard self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos onAppendagePos, Creature.DamageType type, float damage, float stunBonus)
    {
        orig(self, source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
        var room = self.room;
        if (source?.owner is Player player && type == Creature.DamageType.Bite && Random.value <= 0.10)
        {
            room.PlaySound(CryAboutIt.yummers, self.firstChunk);
            player.AddQuarterFood();
        }

        else if (source?.owner is Player && type == Creature.DamageType.Bite)
        {

            room.PlaySound(CryAboutIt.munch, self.firstChunk);
        }
    }

    //All the world in my hand!!
    private ObjectGrabability Player_Grabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        
        if (obj is Creature && obj is not PoleMimic && obj is not BigEel && obj is not Player)
        {
            //Debug.Log("I'M THE DOCTOR AND THIS IS THE SAW");
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

    //NEW CHEWING GUM SCAVENGERS!!!
    private void Scavenger_Violence(On.Scavenger.orig_Violence orig, Scavenger self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        var room = self.room;
        if (source?.owner is Player && type == Creature.DamageType.Bite)
        {
            room.PlaySound(CryAboutIt.scavcry, self.firstChunk);
        }
    }

    //I MAUL YOU GIVE ME MY FOOD!!!
    private void Creature_Violence(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
    {
        orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        var room = self.room;

        if (source?.owner is Player player && type == Creature.DamageType.Bite && Random.value <= 0.10)
        {
            //Debug.Log("GET YOUR FOOD FOOL");
            room.PlaySound(CryAboutIt.yummers, self.firstChunk);
            player.AddQuarterFood();
        }

        else if (source?.owner is Player && type == Creature.DamageType.Bite)
        {
             
            room.PlaySound(CryAboutIt.munch, self.firstChunk);
        }
    }

    //i'M NOT LETTING YOU TO SCAPE
    private void Player_ReleaseGrasp(On.Player.orig_ReleaseGrasp orig, Player self, int grasp)
    {
        if(!mauling)
            orig(self, grasp);
    }

    //YOU CAN´T SCAPE
    private void Player_TossObject(On.Player.orig_TossObject orig, Player self, int grasp, bool eu)
    {
        if (!mauling)
            orig(self, grasp, eu);
    }

    //JA JA JA YOU CAN'T SCAPE FROM MY GRAB
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

    //JA YOU ARE LIGHT 
    private bool Player_HeavyCarry(On.Player.orig_HeavyCarry orig, Player self, PhysicalObject obj)
    {
        //Vampiric strenght
        var ratioed = 7f;
        if (obj.TotalMass <= self.TotalMass * ratioed)
        {
            if (ModManager.CoopAvailable && obj is Player player)
            {
                //Debug.Log("IT'S NOR A PLAYER??");
                return !player.isSlugpup;
            }
            //Debug.Log("IT'S FAKE YOU ARE FAKE, ALL OF US ARE FAKE");
            return false;
        }
        //Debug.Log("Calling ORIG YOU CODE IS NOT WORKING AT ALL");
        return orig(self, obj);
    }


    //I'm a baby that mauls everything
    private bool Player_CanMaulCreature(On.Player.orig_CanMaulCreature orig, Player self, Creature crit)
    {
        //var room = self.room;
        //var pos = self.mainBodyChunk.pos;
        //var color = self.ShortCutColor();

        if ((crit is not null) && !crit.dead && (crit is not IPlayerEdible || !crit.Stunned || (crit.Stunned && (crit is Cicada) && crit is not Player)))
        {
            if (crit is Player player && player.isSlugpup)
            {
                //Debug.Log("NO MORE SLUGPUPS FOR DINNER");
                return false;
            }
            //Debug.Log("IT'S TRUE...");

            (self.grasps[0].grabbed as Creature).Stun(20);
            /*int MaulTimes = 0;
            bool CheckTrue = true;
            for (int i = 1; i <= 100; i++)
            {
                if (CheckTrue) // if Maul is true
                {
                    MaulTimes++; // increment the true count
                }

                if (MaulTimes >= 25) // if the true count is greater than or equal to 25
                {
                    self.AddQuarterFood(); // call the function the food function
                    MaulTimes = 0; // reset the true count to 0
                }
            }*/
            return true;
        }
        /*room.AddObject(new SparkFlash(pos, 300f, new Color(0f, 0f, 1f)));
        room.AddObject(new Explosion(room, self, pos, 7, 250f, 6.2f, 2f, 280f, 0.25f, self, 0.7f, 160f, 1f));
        room.AddObject(new Explosion(room, self, pos, 7, 2000f, 4f, 0f, 400f, 0.25f, self, 0.3f, 200f, 1f));
        room.AddObject(new Explosion.ExplosionLight(pos, 280f, 1f, 7, color));
        room.AddObject(new Explosion.ExplosionLight(pos, 230f, 1f, 3, new Color(1f, 1f, 1f)));
        room.AddObject(new Explosion.ExplosionLight(pos, 2000f, 2f, 60, color));
        room.AddObject(new ShockWave(pos, 350f, 0.485f, 300, highLayer: true));
        room.AddObject(new ShockWave(pos, 2000f, 0.185f, 180));*/

        //Debug.Log("Calling orig");
        return orig(self, crit);

    }
}