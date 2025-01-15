using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.LowLevel;

[Serializable]
public class BuildingDataHolder
{
    public Building.BuildingType type;
    public int x;
    public int y;
    public Quaternion rotation;
    public string keys;
    public string charValues;
    public string typeValues;
}

public class WorldSaver : MonoBehaviour
{
    [SerializeField] private GameObject buildingsMap;
    [SerializeField] private BuildingPlacer buildingPlacer;
    private string saveFilePath = Application.dataPath + "/Resources/Save/save.json";
    private string json = string.Empty;

    #region Read / Write world data
    //public void GetAllMachinesData(InputAction.CallbackContext context)
    //{
    //    if (context.performed)
    //    {
    //        json = string.Empty;
    //        Debug.Log("Writing buildings data to json");
    //        foreach (Transform child in buildingsMap.transform)
    //        {
    //            BuildingDataHolder holder = new BuildingDataHolder();
    //            holder.x = Convert.ToInt32(child.position.x - 0.5);
    //            holder.y = Convert.ToInt32(child.position.z - 0.5);
    //            holder.type = child.GetComponent<Building>().GetBuildingType();
    //            holder.rotation = child.rotation;

    //            Factory factory = child.GetComponent<Factory>();
    //            if (factory != null)
    //            {
    //                var cache = factory.GetFactoryCache();

    //                //var keys = cache.Keys.ToList();

    //                var keys = new List<List<string>>(cache.Keys);
    //                var values = new List<List<Item.Symbol>>(cache.Values);

    //                if (keys.Count > 0 && values.Count > 0)
    //                {
    //                    string allChar = string.Empty;
    //                    string allType = string.Empty;

    //                    //Debug.Log(factory.name + "contains " + keys.Count + " keys and " + values.Count + " values");

    //                    for (int i = 0; i < values.Count; i++)
    //                    {
    //                        string valueCharString = string.Empty;
    //                        string valueTypeString = string.Empty;
    //                        for (int j = 0; j < values[i].Count; j++)
    //                        {
    //                            Debug.Log("Char: " + values[i][j].character + " Type: " + values[i][j].type);
    //                            valueCharString += values[i][j].character.ToString() + ",";
    //                            valueTypeString += values[i][j].type + ",";
    //                        }
    //                        allChar += valueCharString.Remove(valueCharString.Length - 1) + "|";
    //                        allType += valueTypeString.Remove(valueTypeString.Length - 1) + "|";
    //                    }
    //                    allChar = allChar.Remove(allChar.Length - 1);
    //                    allType = allType.Remove(allType.Length - 1);

    //                    string allKeys = string.Empty;

    //                    for (int i = 0; i < keys.Count; i++)
    //                    {
    //                        string tempKeys = string.Empty;

    //                        Debug.Log(keys[i].Count);
    //                        if (keys[i].Count > 0)
    //                        {
    //                            Debug.Log(keys[i].Count);
    //                            Debug.Log(keys[i][0]);
    //                        }

    //                        for (int j = 0; j < keys[i].Count; j++)
    //                        {
    //                            Debug.Log("Key: " + keys[i][j]);
    //                            tempKeys += keys[i][j] + ",";
    //                        }
    //                        allKeys += tempKeys.Remove(tempKeys.Length - 1) + "|";
    //                    }

    //                    allKeys = allKeys.Remove(allKeys.Length - 1);

    //                    holder.keys = allKeys;
    //                    holder.charValues = allChar;
    //                    holder.typeValues = allType;
    //                } 
    //            }
    //            json += JsonUtility.ToJson(holder) + "\n";
    //        }

    //        if (json.Length > 0)
    //        {
    //            json = json.Remove(json.Length - 1);
    //            File.WriteAllText(saveFilePath, json);
    //        }
    //        else
    //        {
    //            Debug.Log("Failed to save the world, there is no building to save !");
    //        }
    //    }
    //}

    public void WriteAllMachineData(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            json = string.Empty;
            BuildingDataHolder holder = new BuildingDataHolder();

            foreach(Transform child in buildingsMap.transform)
            {

                if (child.GetComponent<Building>().GetBuildingType() == Building.BuildingType.Filler)
                {
                    continue;
                }

                // Get the new position, type and rotation in the holder
                holder.x = Convert.ToInt32(child.position.x - 0.5f);
                holder.y = Convert.ToInt32(child.position.z - 0.5f);
                holder.type = child.GetComponent<Building>().GetBuildingType();
                holder.rotation = child.rotation;

                // In case the building we are dealing with is a factory, we get the cache
                if ((int)holder.type >= 8 && (int)holder.type < 17)
                {
                    var cache = child.GetComponent<Factory>().GetFactoryCache();

                    holder.keys = GetKeyFromCache(cache);

                    string[] res = GetSymboleFromCache(cache);

                    if (res != null)
                    {
                        holder.charValues = res[0];
                        holder.typeValues = res[1];
                    }
                    else
                    {
                        holder.charValues = string.Empty;
                        holder.typeValues = string.Empty;
                    }
                }
                else
                {
                    holder.keys = null;
                    holder.charValues = null;
                    holder.typeValues = null;
                }
                json += JsonUtility.ToJson(holder);
                json += "\n";
            }

            if (json.Length > 0)
            {
                json = json.Remove(json.Length - 1);
                Debug.Log("Successfully saved the world!");
                File.WriteAllText(saveFilePath, json);
            }
            else
            {
                Debug.Log("Failed to save the world, there is no building to save !");
            }
        }
    }

    private string GetKeyFromCache(Dictionary<List<string>, List<Item.Symbol>> cache)
    {
        string keys = string.Empty;

        List<List<string>> keyList = new List<List<string>>(cache.Keys);

        if (keyList.Count == 0)
        {
            return keys;
        }

        for (int i = 0; i < keyList.Count; i++)
        {

            string tempKey = string.Empty;

            for (int j = 0; j < keyList[i].Count; j++)
            {
                keys += keyList[i][j] + ",";
            }
            keys = keys.Remove(keys.Length - 1);
            keys += "|";
        }
        keys = keys.Remove(keys.Length - 1);

        return keys;
    }

    private string[] GetSymboleFromCache(Dictionary<List<string>, List<Item.Symbol>> cache)
    {
        string charVal = string.Empty;
        string typeVal = string.Empty;

        List<List<Item.Symbol>> values = new List<List<Item.Symbol>>(cache.Values);

        if (values.Count == 0) 
        { 
            return null; 
        }

        for (int i = 0; i < values.Count; i++)
        {
            for (int j = 0; j < values[i].Count; j++)
            {
                charVal += values[i][j].character + ",";
                typeVal += values[i][j].type + ",";
            }
            charVal = charVal.Remove(charVal.Length - 1);
            typeVal = typeVal.Remove(typeVal.Length - 1);
            charVal += "|";
            typeVal += "|";
        }
        charVal = charVal.Remove(charVal.Length - 1);
        typeVal = typeVal.Remove(typeVal.Length - 1);

        string[] res = {charVal, typeVal};

        return res;
    }

    public List<BuildingDataHolder> ReadAllMachineData()
    {
        
        Debug.Log("Reading buildings data from json");

        List<BuildingDataHolder> buildings = new List<BuildingDataHolder>();
        
        if (!File.Exists(saveFilePath))
        {
            return null;
        }

        using(StreamReader reader = new StreamReader(saveFilePath))
        {
            string line = string.Empty;
            //int count = 0;

            while ((line = reader.ReadLine()) != null)
            {
                //count++;
                //Debug.Log(line);
                BuildingDataHolder buildingData = JsonUtility.FromJson<BuildingDataHolder>(line);
                buildings.Add(buildingData);
            }
            //Debug.Log(count);
        }   
        return buildings;
    }
    #endregion

    //public void RebuildWorld(InputAction.CallbackContext context)
    //{
    //    if (context.performed)
    //    {
    //        List<BuildingDataHolder> buildings = ReadAllMachineData();

    //        for (int i = 0; i < buildings.Count; i++) 
    //        {
    //            Vector3Int buildingPos = new Vector3Int(buildings[i].x, buildings[i].y, 0);
    //            buildingPlacer.PlaceBuildingAtPosition(buildings[i].type, buildingPos, buildings[i].rotation);
    //            if ((int)buildings[i].type >= 8 && (int)buildings[i].type < 17)
    //            {
    //                BuildingTile tile = BuildingManager.Instance.buildingTilemap.GetTile<BuildingTile>(buildingPos);
    //                Factory factory = tile.building.GetComponent<Factory>();

    //                List<string> inputs = new List<string>();
    //                List<Item.Symbol> outputs = new List<Item.Symbol>();

    //                string[] splitKeys = buildings[i].keys.Split("|");
    //                string[] splitChars = buildings[i].charValues.Split("|");
    //                string[] splitTypes = buildings[i].typeValues.Split("|");

    //                for (int j = 0; j < splitKeys.Length; j++)
    //                {
    //                    inputs.Clear();
    //                    outputs.Clear();

    //                    inputs = splitKeys[j].Split(",").ToList<string>();

    //                    string[] splitChar = splitChars[j].Split(",");
    //                    string[] splitType = splitTypes[j].Split(",");

    //                    Debug.Log(splitChar.Length);

    //                    for (int k = 0; k < splitChar.Length; k++)
    //                    {
    //                        Item.Symbol symbol = new Item.Symbol();
    //                        Debug.Log(splitChar[k]);
    //                        symbol.character = splitChar[k].ToCharArray()[0];
    //                        if (splitType[k] == "Hiragana")
    //                        {
    //                            symbol.type = Item.SymbolType.Hiragana;
    //                        }
    //                        else if (splitType[k] == "Katakana")
    //                        {
    //                            symbol.type = Item.SymbolType.Katakana;
    //                        }
    //                        else if (splitType[k] == "Kanji")
    //                        {
    //                            symbol.type = Item.SymbolType.Kanji;
    //                        }
    //                        outputs.Add(symbol);
    //                    }

    //                    Debug.Log("Key: ");
    //                    for (int k = 0; k < inputs.Count; k++)
    //                    {
    //                        Debug.Log(inputs[k] + " ");
    //                    }
    //                    Debug.Log("Valeurs: ");
    //                    for (int k = 0; k < outputs.Count; k++)
    //                    {
    //                        Debug.Log(outputs[k].character + " " + outputs[k].type);
    //                    }

    //                    factory.AddToCache(inputs, outputs);
    //                }
    //            }
    //        }
    //    }
    //}

    public void BuildWorldFromData(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            List<BuildingDataHolder> buildings = ReadAllMachineData();

            if (buildings == null)
            {
                Debug.Log("No save file to load");
                return;
            }

            for (int i = 0; i < buildings.Count; i++)
            {
                //Debug.Log("x: " + buildings[i].x + " y: " + buildings[i].y);
                Vector3Int buildingPosition = new Vector3Int(buildings[i].x, buildings[i].y, 0);
                buildingPlacer.PlaceBuildingAtPosition(buildings[i].type, buildingPosition, buildings[i].rotation);

                if ((int)buildings[i].type >= 8 && (int)buildings[i].type < 17)
                {
                    BuildingTile buildingTile = BuildingManager.Instance.buildingTilemap.GetTile<BuildingTile>(buildingPosition);
                    Factory factory = buildingTile.building.GetComponent<Factory>();

                    // Add the cache back from the holder to the actual factory

                    var res = ExtractCacheFromHolder(buildings[i]);

                    if (res.Count == 1 && res[0].Item1 == null && res[0].Item2 == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < res.Count; j++)
                    {
                        factory.AddToCache(res[j].Item1, res[j].Item2);
                    }
                }
            }
        }
    }

    private List<(List<string>,List<Item.Symbol>)> ExtractCacheFromHolder(BuildingDataHolder holder)
    {
        List<(List<string>, List<Item.Symbol>)> res = new List<(List<string>, List<Item.Symbol>)>();

        List<string> inputs = new List<string>();
        List<Item.Symbol> outputs = new List<Item.Symbol>();

        string[] keyList = holder.keys.Split("|");
        string[] charList = holder.charValues.Split("|");
        string[] typeList = holder.typeValues.Split("|");

        for (int i =0; i < keyList.Length; i++)
        {
            res.Add(ExtractOneCacheFromHolder(keyList[i], charList[i], typeList[i]));
        }

        return res;
    }

    private (List<string>, List<Item.Symbol>) ExtractOneCacheFromHolder(string key, string chara, string type)
    {
        List<string> keyList = keyList = key.Split(",").ToList<string>();

        List<Item.Symbol> symList = new List<Item.Symbol>();

        string[] charaSplit = chara.Split(",");
        string[] typeSplit = type.Split(",");

        if (charaSplit[0] == string.Empty)
        {
            Debug.Log("Empty cache");
            return (null, null);
        }

        for (int i = 0; i < charaSplit.Length; i++)
        {
            Debug.Log(charaSplit[i]);
            Item.Symbol sym = new Item.Symbol();
            sym.character = charaSplit[i].ToCharArray()[0];

            if (typeSplit[i] == "Hiragana")
            {
                sym.type = Item.SymbolType.Hiragana;
            }
            else if (typeSplit[i] == "Katakana")
            {
                sym.type = Item.SymbolType.Katakana;
            }
            else if (charaSplit[i] == "Kanji")
            {
                sym.type = Item.SymbolType.Katakana;
            }
            symList.Add(sym);
        }
        return (keyList, symList);
    }
}
