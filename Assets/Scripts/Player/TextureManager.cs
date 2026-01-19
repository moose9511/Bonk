using System.Collections.Generic;
using UnityEngine;

public class TextureManager : MonoBehaviour
{
    public Material comGunTex;
    public Material legGunTex;
    public Material health1Tex;
    public Material health2Tex;
    public Material health3Tex;
    public Material powerTex;

    public static Dictionary<int, Material> textures = new Dictionary<int, Material>();

    public const int comGunInd = 1, legGunInd = 2, health1Ind = 3, health2Ind = 4, health3Ind = 5, powerInd = 6; 

    private void Awake()
    {
        textures.Add(comGunInd, comGunTex);
        textures.Add(legGunInd, legGunTex);
        textures.Add(health1Ind, health1Tex);
        textures.Add(health2Ind, health2Tex);
        textures.Add(health3Ind, health3Tex);
        textures.Add(powerInd, powerTex);

    }
}
