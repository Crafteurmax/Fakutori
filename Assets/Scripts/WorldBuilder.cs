using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;

using UnityEngine.Tilemaps;

public class WorldBuilder : MonoBehaviour
{
    public static WorldBuilder Instance { get; private set; }
    public bool isTheWorldComplete;

    public Tilemap map;
    [SerializeField] private bool isProcedural;

    private int chunkSize = 64;
    private int nbChunkX = 6;
    private int nbChunkY = 6;

    [SerializeField] private int seed;
    [SerializeField, Range(0f, 1f)] private float offset;

    private string levelsPrefabFolder = Application.dataPath + "/Resources/Levels/";

    [SerializeField] private GameObject a ,  i,  u,  e,  o;
    [SerializeField] private GameObject ka, ki, ku, ke, ko;
    [SerializeField] private GameObject sa, si, su, se, so;
    [SerializeField] private GameObject ta, ti, tu, te, to;
    [SerializeField] private GameObject na, ni, nu, ne, no;
    [SerializeField] private GameObject ha, hi, hu, he, ho;
    [SerializeField] private GameObject ma, mi, mu, me, mo;
    [SerializeField] private GameObject ya,     yu,     yo;
    [SerializeField] private GameObject ra, ri, ru, re, ro;
    [SerializeField] private GameObject wa,  n,         wo;

    private List<List<Tile>> tiles = new List<List<Tile>>();

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateTileList();
        StartCoroutine(GenerateWorld());  
    }


    private void GenerateTileList()
    {
        for (int x = 0; x < 10; x++) 
        {
            tiles.Add(new List<Tile>());
            for(int y = 0; y < 5; y++) tiles[x].Add(ScriptableObject.CreateInstance<Tile>());
        }

        tiles[0][0].gameObject = a;
        tiles[0][1].gameObject = i;
        tiles[0][2].gameObject = u;
        tiles[0][3].gameObject = e;
        tiles[0][4].gameObject = o;

        tiles[1][0].gameObject = ka;
        tiles[1][1].gameObject = ki;
        tiles[1][2].gameObject = ku;
        tiles[1][3].gameObject = ke;
        tiles[1][4].gameObject = ko;

        tiles[2][0].gameObject = sa;
        tiles[2][1].gameObject = si;
        tiles[2][2].gameObject = su;
        tiles[2][3].gameObject = se;
        tiles[2][4].gameObject = so;

        tiles[3][0].gameObject = ta;
        tiles[3][1].gameObject = ti;
        tiles[3][2].gameObject = tu;
        tiles[3][3].gameObject = te;
        tiles[3][4].gameObject = to;

        tiles[4][0].gameObject = na;
        tiles[4][1].gameObject = ni;
        tiles[4][2].gameObject = nu;
        tiles[4][3].gameObject = ne;
        tiles[4][4].gameObject = no;

        tiles[5][0].gameObject = ha;
        tiles[5][1].gameObject = hi;
        tiles[5][2].gameObject = hu;
        tiles[5][3].gameObject = he;
        tiles[5][4].gameObject = ho;

        tiles[6][0].gameObject = ma;
        tiles[6][1].gameObject = mi;
        tiles[6][2].gameObject = mu;
        tiles[6][3].gameObject = me;
        tiles[6][4].gameObject = mo;

        tiles[7][0].gameObject = ya;
        tiles[7][2].gameObject = yu;
        tiles[7][4].gameObject = yo;

        tiles[8][0].gameObject = ra;
        tiles[8][1].gameObject = ri;
        tiles[8][2].gameObject = ru;
        tiles[8][3].gameObject = re;
        tiles[8][4].gameObject = ro;

        tiles[9][0].gameObject = wa;
        tiles[9][1].gameObject = n;
        tiles[9][4].gameObject = wo;
    }

    private IEnumerator GenerateProceduralWorld()
    {
        FastNoiseLite ConsonneNoise = GenerateConsonneRepartition();
        FastNoiseLite SpotNoise = GenerateSpotRepartition();


        for (int xChunk = -(nbChunkX/2); xChunk < nbChunkX/2; xChunk++)
        {
            for (int yChunk = -(nbChunkY/2); yChunk < nbChunkY/2; yChunk++)
            {
                for (int x = xChunk * chunkSize; x < (xChunk + 1) * chunkSize; x++)
                {
                    for (int y = yChunk * chunkSize; y < (yChunk + 1) * chunkSize; y++)
                    {
                        Vector3Int pos = new Vector3Int(x, y);
                        float noiseValue = (ConsonneNoise.GetNoise(x, y) + 1) / 2;
                        for (int vowel = 0; vowel < 5; vowel++)
                            if ((SpotNoise.GetNoise(x + vowel * 100, y + vowel * 100) + 1) / 2 < 1 - offset)
                                map.SetTile(pos, ChoseTile(vowel, noiseValue));
                    }
                }
                yield return null;

            }
        }
    }

    private Tile ChoseTile(int y, float randomValue)
    {
        // assuming that that you can place all hiragana on a 10*5 grid for 10 consonnes and 5 voyelles
        // https://fr.wikipedia.org/wiki/Hiragana#/media/Fichier:Table_hiragana.svg
        // excluding wi and we that are deprecated and puting n at the wi place
        // if you get one of 1 of the 4 blanc place, juste return null

        // we normalise randomValue

        int ecart = 1;
        int x = (int) Mathf.Floor(randomValue * 10 * ecart);
        if (x % ecart != 0) return null; 
        x /= ecart;

        return tiles[x][y];
    }

    private FastNoiseLite GenerateConsonneRepartition()
    {
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        noise.SetSeed(seed);
        noise.SetFrequency(0.015f);

        noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
        noise.SetFractalOctaves(4);

        noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Manhattan);
        noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);
        noise.SetCellularJitter(0.3f);

        return noise;
    }

    private FastNoiseLite GenerateSpotRepartition()
    {
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        noise.SetSeed(seed + 1);
        noise.SetFrequency(0.02f);

        noise.SetFractalType(FastNoiseLite.FractalType.None);

        noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.EuclideanSq);
        noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Div);

        return noise;
    }

    private IEnumerator GeneratePrebaWorld(string worldPath)
    {
        string rawData = System.IO.File.ReadAllText(levelsPrefabFolder + worldPath + ".csv");
        string[] rawDataArray = rawData.Split(new string[] { ",", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        int offsetToCenter = 0;

        for(int i = 4; i < rawDataArray.Length; i += 4)
        {
            Vector3Int pos = new Vector3Int(int.Parse(rawDataArray[i]) - offsetToCenter, int.Parse(rawDataArray[i+1])-offsetToCenter);
            int ConsonnelId = int.Parse(rawDataArray[i + 2]);
            int vowelId = int.Parse(rawDataArray[i + 3]);
            map.SetTile(pos, tiles[ConsonnelId][vowelId]);
            yield return null;
        }
    }

    private IEnumerator GenerateWorld()
    {
        isTheWorldComplete = false;
        Time.timeScale = 0f;
        if (isProcedural) yield return StartCoroutine(GenerateProceduralWorld());
        else if (LevelData.mapName != null) 
            yield return StartCoroutine(GeneratePrebaWorld(LevelData.mapName + "_map"));
        else yield return StartCoroutine(GeneratePrebaWorld("default_map"));
        Time.timeScale = 1f;
        isTheWorldComplete = true;
    }

}
