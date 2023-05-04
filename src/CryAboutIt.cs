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