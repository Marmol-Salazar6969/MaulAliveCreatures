using BepInEx;
using System.Security.Permissions;
using System.Security;
using System;
using UnityEngine;
using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MoreSlugcats;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.Rendering;
using System.Xml;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;

namespace MaulAliveCreatures
{
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
}
