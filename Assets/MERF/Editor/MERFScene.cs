using System;

public enum MERFScene {
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

    public static string String(this MERFScene scene) {
        return scene.ToString().ToLower();
    }

    public static MERFScene ToEnum(string value) {
        return (MERFScene)Enum.Parse(typeof(MERFScene), value, true);
    }
}