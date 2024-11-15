using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using NineSolsAPI;
using UnityEngine;
using TMPro;
using RCGMaker.Core;
using System.Collections.Generic;

namespace HPNumbers;

[BepInDependency(NineSolsAPICore.PluginGUID)]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class HPNumbers : BaseUnityPlugin {
    private float YiHealth = 0;
    private TMP_Text hpNumber = null!;

    private MonsterBase monster = null!;
    private TMP_Text bossHPNumber = null!;
    private float bossHealth = 0;

    private TMP_Text bossInternalHPNumber = null!;
    private float bossInternalHealth = 0;
    private Color internalDamageColor;

    private ConfigEntry<Vector2> yiHPPos = null!;
    private ConfigEntry<Vector2> bossHPPos = null!;
    private ConfigEntry<Vector2> bossInternalPos = null!;
    private void Awake() {
        Log.Init(Logger);
        RCGLifeCycle.DontDestroyForever(gameObject);
        
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

        internalDamageColor = new Color(0.5f, 1.0f, 1.0f, 1.0f);

        //Player
        yiHPPos = Config.Bind("General", "YiHPPosition", new Vector2(340, 750), "Position for Yi's HP number");
        hpNumber = ShowHealth(YiHealth.ToString("F0"), Color.black);
        hpNumber.rectTransform.position = yiHPPos.Value;
        hpNumber.SetActive(false);

        //Boss
        //monster = MonsterManager.Instance.FindClosestMonster();
        bossHPPos = Config.Bind("General", "BossHPPosition", new Vector2(850, 70), "Position for Boss HP number");
        bossInternalPos = Config.Bind("General", "BossInternalHPPosition", new Vector2(930, 70), "Position for Boss Internal HP number");
        
        bossHPNumber = ShowHealth(bossHealth.ToString("F0"), Color.white);
        bossHPNumber.rectTransform.position = bossHPPos.Value;
        bossInternalHPNumber = ShowHealth(bossInternalHealth.ToString("F0"), internalDamageColor);
        bossInternalHPNumber.rectTransform.position = bossInternalPos.Value;

        bossHPNumber.SetActive(false);
        bossInternalHPNumber.SetActive(false);
    } 

    private void Update() {
        //Player HP
        if (Player.i != null) {
            YiHealth = Player.i.health.currentValue;
            hpNumber.SetActive(true);
            hpNumber.text = YiHealth.ToString("F0");
        }
        if (YiHealth < 8) {
            hpNumber.color = Color.white;
        } 
        else {
            hpNumber.color = Color.black;
        }

        //Boss HP
        monster = GetBossInstance();
        if (monster != null && monster.IsActive == true) {
            bossHealth = monster.postureSystem.CurrentHealthValue;
            bossHPNumber.SetActive(true);
            bossInternalHealth = monster.postureSystem.CurrentInternalInjury;
            bossInternalHPNumber.SetActive(true);

            bossHPNumber.text = bossHealth.ToString("F0");
            bossInternalHPNumber.text = bossInternalHealth.ToString("F0");
        }
        else {
            bossHPNumber.SetActive(false);
            bossInternalHPNumber.SetActive(false);
        }      
        //Custom Positions
        hpNumber.rectTransform.position = yiHPPos.Value;
        bossHPNumber.rectTransform.position = bossHPPos.Value;
        bossInternalHPNumber.rectTransform.position = bossInternalPos.Value;
        
        //Hiding texts upon death
        if (YiHealth <= 0) {
            hpNumber.SetActive(false);
            bossHPNumber.SetActive(false);
            bossInternalHPNumber.SetActive(false);
        } else if (bossHealth == 0 && bossInternalHealth == 0) { // 
            bossHPNumber.SetActive(false);
            bossInternalHPNumber.SetActive(false);
        }
    }
    
    private static TMP_Text ShowHealth(string str, Color color) {
        var canvas = NineSolsAPICore.FullscreenCanvas.transform;
        var textGo = new GameObject();
        textGo.transform.SetParent(canvas);

        var text = (TMP_Text)textGo.AddComponent<TextMeshProUGUI>();
        text.fontSize = 19f;
        text.text = str;
        text.color = color;
        return text;
    }

    private MonsterBase GetBossInstance() {
        foreach (var boss in MonsterManager.Instance.monsterDict.Values) {
            if (boss.tag == "Boss" && boss.name != "_depre_StealthGameMonster_伏羲 Variant(Clone)" && boss.name != "StealthGameMonster_新女媧 Variant") {
                return boss;
            }
        }
        return null;
    }


    private void OnDestroy() {
        // Make sure to clean up resources here to support hot reloading
        Destroy(hpNumber);
        Destroy(bossHPNumber);
        Destroy(bossInternalHPNumber);
    }
}