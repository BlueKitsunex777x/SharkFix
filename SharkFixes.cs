using BepInEx;
using BlockStoryCore;
using BlockStoryMod;
using HarmonyLib;
using UnityEngine;

[BepInPlugin("com.BlueKitsunex777x.blockstory.sharkfixes", "Shark Fixes", "1.0.0")]
[BepInDependency(Core.Guid)]
public class SharkFix : BaseUnityPlugin
{
    public static bool Enabled = PlayerPrefs.GetInt("BetterInventory_Enabled", 1) != 0;
    
    // the game gives sharks a 3 1 3 block box and a 6 tall body, way bigger than the model,
    // so they snag on blocks and you bump them from far away
    static readonly Vector3 OldBlockBox = new Vector3(3f, 1f, 3f);
    static readonly Vector3 NewBlockBox = new Vector3(1.5f, 1f, 1.5f);
    const float BodyRadius = 0.8f;
    const float BodyHeight = 1f;

    float timer;

    private void Awake()
    {
        if (!BSMKCheck.Passed(this)) return;   // added: only runs on the real BSMK Core
        ModRegistry.Register(new ModInfo
        {
            Name = "Shark Fixes",
            Description = "Changes the layout of the players inventory",
            GetEnabled = () => Enabled,
            SetEnabled = on =>
            {
                if (BSWorld.InWorld) { L.Warn("on/off ignored, in a world"); return; }
                Enabled = on; PlayerPrefs.SetInt("BetterInventory_Enabled", on ? 1 : 0); PlayerPrefs.Save(); L.Info("enabled = " + on);
            },
            HasConfig = false,
        });
    }

    void Update()
    {
        if (!Enabled) return;
        // sharks spawn and despawn all the time, a check once a second is cheap and catches them all
        timer += Time.deltaTime;
        if (timer < 1f) return;
        timer = 0f;

        foreach (var body in FindObjectsByType<CharacterController>(FindObjectsInactive.Include))
        {
            // the loot fish is also called Shark but it has no CharacterController so it never gets here
            if (!body.name.StartsWith("Shark")) continue;
            if (body.radius == BodyRadius) continue;

            body.radius = BodyRadius;
            body.height = BodyHeight;

            var blocks = body.GetComponent<AABBCharacterController>();
            if (blocks == null) continue;
            var box = Traverse.Create(blocks).Field("eRadius");
            if (box.GetValue<Vector3>() == OldBlockBox) box.SetValue(NewBlockBox);
        }
    }
}
