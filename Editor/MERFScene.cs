using System;

public enum MERFScene {
    Custom = -1,
    Gardenvase,
    Bicycle,
    KitchenLego,
    Stump,
    OfficeBonsai,
    FullLivingRoom,
    KitchenCounter,
    TreehillFlower,
}

public static class MERFSceneExtensions {

    public static string LowerCaseName(this MERFScene scene) {
        return scene.ToString().ToLower();
    }

    public static string Name(this MERFScene scene) {
        return scene.ToString();
    }

    public static MERFScene ToEnum(string value) {
        return (MERFScene)Enum.Parse(typeof(MERFScene), value, true);
    }
}