using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public class NPCDatabase
{
    static public bool isLoaded { get; private set; }
    static public List<NPCBaseSpecs> Items;

    private static void ValidateDatabase()
    {

        if (Items == null || !isLoaded)
        {
            Items = new List<NPCBaseSpecs>();
            isLoaded = false;
        }
        else { isLoaded = true; }
    }
    public static void LoadDatabase()
    {
        if (isLoaded)
            return;
        LoadDatabaseForce();
    }
    public static void LoadDatabaseForce()
    {
        NPCBaseSpecs[] itemsToLoad = TextReadIn();

        foreach (var item in itemsToLoad)
        {
            if (!Items.Contains(item))
                Items.Add(item);
        }

    }
    public static void ClearDataBase()
    {
        isLoaded = false;
        Items.Clear();

    }
    public static NPCBaseSpecs GetTower(int ID)
    {
        ValidateDatabase();
        LoadDatabase();
        foreach (NPCBaseSpecs item in Items)
        {
            if (item.ID == ID)
                return item;
        }
        return default;
    }
    public static GameObject InstantiateTower(int ID, Vector3 pos, int level)
    {
        ValidateDatabase();
        NPCBaseSpecs towerSpawn = GetTower(ID);

        //Todo Finish Implementation 
        GameObject go = GameObject.Instantiate(towerSpawn.Mesh, pos + new Vector3(0, towerSpawn.OffsetY, 0)
            , Quaternion.identity);

        return go;
    }

    public static GameObject InstantiateTower(int ID, float3 pos, int level)
    {
        Vector3 temp = (Vector3)pos;
        return InstantiateTower(ID, temp, level);
    }

    static NPCBaseSpecs[] TextReadIn()
    {
        TextAsset textFile = Resources.Load("NPCData") as TextAsset;
        var lines = textFile.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        NPCBaseSpecs[] array = new NPCBaseSpecs[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            var parts = lines[i].Split(',');
            array[i] = new NPCBaseSpecs()
            {
                ID = int.Parse(parts[0]),
                name = parts[1],
                Mesh = Resources.Load("Prefabs/" + parts[2]) as GameObject,
                EquippedWeaonIDs = WeaponIDsParser(parts[3]),
                FactionID = int.Parse(parts[4]),
                Level = uint.Parse(parts[5]),
                Threat = int.Parse(parts[6]),
                Protection = int.Parse(parts[7]),
                ViewAngle = int.Parse(parts[8]),
                ViewRadius = int.Parse(parts[9]),
                OffsetY = float.Parse(parts[10]), //Todo reorder data

            };
        }

        return array;
    }
    static int[] WeaponIDsParser(string line)
    {
        var lines = line.Split(';');
        int[] output = new int[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            output[i] = int.Parse(lines[i]);
        }
        return output;
    }
}

public interface iSpec
{
    public int ID { get; set; }

}

public class NPCBaseSpecs : iSpec
{
    public string name;
    public int ID { get; set; }
    public GameObject Mesh;
    public float OffsetY;
    public int[] EquippedWeaonIDs; //InstantiateTower will need to spawn weapons
    public int FactionID;
    public uint Level;
    public int Threat, Protection;
    public int ViewAngle, ViewRadius;


}
