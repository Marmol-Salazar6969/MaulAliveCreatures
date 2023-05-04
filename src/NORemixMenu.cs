using BepInEx;
using System;
using UnityEngine;
using Menu.Remix.MixedUI;
using static Player;
using Random = UnityEngine.Random;
using MonoMod.Cil;
using Mono.Cecil.Cil;

public class OptionsMenu : OptionInterface
{
    public static Configurable<bool> Sounds;
    public static Configurable<bool> Meat;
    public static Configurable<bool> Dinner;
    public static Configurable<float> Yummy;

    public OptionsMenu()
    {
        Sounds = config.Bind("Sounds", false);
        Meat = config.Bind("Meat", false);
        Dinner = config.Bind("Dinner", false);
        Yummy = config.Bind("Yummy", 1f, new ConfigAcceptableRange<float>(1f, 10000f));
    }

    public override void Initialize()
    {
        var opTab1 = new OpTab(this, "Options");
        Tabs = new[] { opTab1 };
        Debug.LogWarning("Options new tab");

        OpContainer tab1Container = new(new Vector2(0, 0));
        opTab1.AddItems(tab1Container);

        Debug.LogWarning("Added button");
        UIelement[] UIArrayElements2 = new UIelement[]
        {
                new OpCheckBox(Sounds, 10, 540),
                new OpLabel(45f, 540f, "Disable the Sounds"),
                new OpCheckBox(Meat, 10, 500),
                new OpLabel(45f, 500f, "Disable the YUMMERS DUMMERS"),
                new OpCheckBox(Dinner, 10, 460),
                new OpLabel(45f, 460f, "Disable everyone can eat the dinner"),
                new OpFloatSlider(Yummy, new Vector2(5, 245), 200, 2, true){max = 10000f, min = 1f, hideLabel = false},
                new OpLabel(45f, 420f, "How ANGRY are you?"),
        };

        opTab1.AddItems(UIArrayElements2);
        Debug.LogWarning("Added items to remix menu");
    }

}