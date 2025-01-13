using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

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
    private string saveFilePath = Application.dataPath + "/Resources/Save/save.json";

    private List<BuildingDataHolder> buildings = new List<BuildingDataHolder>();
    private string json = string.Empty;

    public void GetAllMachinesData(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            json = "{\n";
            int buildingNumber = 0;
            foreach (Transform child in buildingsMap.transform)
            {
                BuildingDataHolder holder = new BuildingDataHolder();
                holder.x = Convert.ToInt32(child.position.x - 0.5);
                holder.y = Convert.ToInt32(child.position.z - 0.5);
                holder.type = child.GetComponent<Building>().GetBuildingType();
                holder.rotation = child.rotation;

                Factory factory = child.GetComponent<Factory>();
                if (factory != null)
                {
                    //Debug.Log("This is a factory !");

                    var cache = factory.GetFactoryCache();

                    var keys = cache.Keys.ToList();
                    var values = cache.Values.ToList();

                    string allChar = string.Empty;
                    string allType = string.Empty;

                    for (int i = 0; i < values.Count; i++)
                    {
                        string valueCharString = string.Empty;
                        string valueTypeString = string.Empty;

                        for (int j = 0; j < values[i].Count; j++)
                        {
                            valueCharString += values[i][j].character.ToString() + ",";
                            valueTypeString += values[i][j].type + ",";
                        }
                        allChar +=  valueCharString.Remove(valueCharString.Length - 1) + "|";
                        allType += valueTypeString.Remove(valueTypeString.Length - 1) + "|";
                    }

                    string allKeys = string.Empty;

                    for (int i = 0; i < keys.Count; i++)
                    {
                        string tempKeys = string.Empty;
                        for (int j = 0;j < keys[i].Count; j++)
                        {
                            tempKeys += keys[i][j] + ",";
                        }
                        allKeys += tempKeys.Remove(tempKeys.Length - 1) + "|";
                    }

                    holder.charValues = allChar;
                    holder.typeValues = allType;
                    holder.keys = allKeys;
                }
                json += "\"building" + buildingNumber + "\":" + JsonUtility.ToJson(holder) + ",\n";
                buildingNumber++;
            }
            json = json.Remove(json.Length - 1);
            json = json.Remove(json.Length - 1);
            json += "}";
            File.WriteAllText(saveFilePath, json);
        }
        
    }
}
