using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using ButterflyGame;

public static class SetupButterflyGame
{
    private const string TEXTURES_DIR = "Assets/Textures";
    private const string MATERIALS_DIR = "Assets/Materials";
    private const string PREFABS_DIR = "Assets/Prefabs";
    private const string SCENES_DIR = "Assets/Scenes";
    private const string SCENE_PATH = "Assets/Scenes/GardenScene.unity";

    [MenuItem("Butterfly Game/Build Complete Game")]
    public static void BuildCompleteGame()
    {
        Debug.Log("[SetupButterflyGame] Starting full automated game generation...");

        EnsureDirectories();
        GenerateAllTextures();
        Dictionary<string, Material> materials = CreateAllMaterials();
        GameObject[] butterflyPrefabs = BuildButterflyPrefabs(materials);
        GameObject playerPrefab = BuildBoyCharacterPrefab(materials);

        CreateMainScene(materials, butterflyPrefabs, playerPrefab);

        Debug.Log("[SetupButterflyGame] Build complete! GardenScene is ready to play.");
    }

    private static void EnsureDirectories()
    {
        if (!AssetDatabase.IsValidFolder(TEXTURES_DIR)) AssetDatabase.CreateFolder("Assets", "Textures");
        if (!AssetDatabase.IsValidFolder(MATERIALS_DIR)) AssetDatabase.CreateFolder("Assets", "Materials");
        if (!AssetDatabase.IsValidFolder(PREFABS_DIR)) AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(SCENES_DIR)) AssetDatabase.CreateFolder("Assets", "Scenes");
        AssetDatabase.Refresh();
    }

    #region Texture Generation

    private static void GenerateAllTextures()
    {
        CreateTextureFile("Grass_Albedo.png", GenerateGrassTexture(512, 512));
        CreateTextureFile("Cobblestone_Albedo.png", GenerateCobblestoneTexture(512, 512));
        CreateTextureFile("Bark_Albedo.png", GenerateBarkTexture(256, 512));
        CreateTextureFile("Foliage_Albedo.png", GenerateFoliageTexture(256, 256));
        CreateTextureFile("Flower_Petals.png", GenerateFlowerTexture(256, 256));
        CreateTextureFile("Wood_Pole.png", GenerateWoodPoleTexture(128, 256));
        CreateTextureFile("Net_Mesh.png", GenerateNetMeshTexture(256, 256));

        // Butterfly Wing Textures for all 5 Species
        CreateTextureFile("Monarch_Wing.png", GenerateMonarchWingTexture(512, 512));
        CreateTextureFile("BlueMorpho_Wing.png", GenerateBlueMorphoWingTexture(512, 512));
        CreateTextureFile("Emerald_Wing.png", GenerateEmeraldWingTexture(512, 512));
        CreateTextureFile("Sulphur_Wing.png", GenerateSulphurWingTexture(512, 512));
        CreateTextureFile("CrimsonRose_Wing.png", GenerateCrimsonWingTexture(512, 512));

        // Boy Character Textures
        CreateTextureFile("Boy_Skin.png", GenerateBoySkinTexture(256, 256));
        CreateTextureFile("Boy_Shirt.png", GenerateBoyShirtTexture(256, 256));
        CreateTextureFile("Boy_Denim.png", GenerateBoyDenimTexture(256, 256));
        CreateTextureFile("Boy_Sneakers.png", GenerateBoySneakersTexture(256, 256));
        CreateTextureFile("Boy_Hair.png", GenerateBoyHairTexture(256, 256));

        // UI Sprites
        CreateTextureFile("UI_Panel.png", GenerateRoundedRectTexture(256, 256, 32, new Color(0.08f, 0.14f, 0.12f, 0.88f), new Color(0.25f, 0.65f, 0.45f, 0.8f)));
        CreateTextureFile("UI_Button.png", GenerateRoundedRectTexture(256, 80, 16, new Color(0.18f, 0.55f, 0.32f, 0.95f), new Color(0.45f, 0.85f, 0.55f, 1f)));

        AssetDatabase.Refresh();
    }

    private static void CreateTextureFile(string filename, Texture2D tex)
    {
        string path = Path.Combine(TEXTURES_DIR, filename);
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        Object.DestroyImmediate(tex);
    }

    private static Texture2D GenerateGrassTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color baseDark = new Color(0.20f, 0.42f, 0.16f);
        Color baseLight = new Color(0.38f, 0.68f, 0.22f);
        Color flowerYellow = new Color(0.85f, 0.82f, 0.25f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float n1 = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                float n2 = Mathf.PerlinNoise(x * 0.20f, y * 0.20f) * 0.3f;
                float blend = Mathf.Clamp01(n1 + n2);
                Color c = Color.Lerp(baseDark, baseLight, blend);

                // Blade flecks
                if ((x * 17 + y * 31) % 43 == 0)
                {
                    c = Color.Lerp(c, baseLight * 1.25f, 0.5f);
                }

                // Tiny clover/flower specks
                if ((x * 47 + y * 73) % 499 == 0)
                {
                    c = flowerYellow;
                }

                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateCobblestoneTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color stoneColor1 = new Color(0.60f, 0.58f, 0.54f);
        Color stoneColor2 = new Color(0.48f, 0.46f, 0.44f);
        Color groutColor = new Color(0.30f, 0.28f, 0.25f);
        Color mossGreen = new Color(0.32f, 0.45f, 0.22f);

        int stoneSize = 64;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int sx = x % stoneSize;
                int sy = (y + (x / stoneSize % 2) * (stoneSize / 2)) % stoneSize;

                bool isEdge = (sx < 4 || sx > stoneSize - 4 || sy < 4 || sy > stoneSize - 4);
                float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f);

                Color c;
                if (isEdge)
                {
                    c = groutColor;
                    if (noise > 0.6f) c = Color.Lerp(c, mossGreen, 0.65f);
                }
                else
                {
                    float centerDist = Mathf.Sqrt(Mathf.Pow(sx - stoneSize / 2f, 2) + Mathf.Pow(sy - stoneSize / 2f, 2)) / (stoneSize / 2f);
                    c = Color.Lerp(stoneColor1, stoneColor2, Mathf.Clamp01(centerDist * 0.8f + noise * 0.4f));
                }
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateBarkTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color darkBark = new Color(0.24f, 0.16f, 0.10f);
        Color lightBark = new Color(0.42f, 0.32f, 0.22f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Vertical wood grain streaks
                float n = Mathf.PerlinNoise(x * 0.15f, y * 0.02f);
                float nFiner = Mathf.PerlinNoise(x * 0.4f, y * 0.08f) * 0.25f;
                Color c = Color.Lerp(darkBark, lightBark, Mathf.Clamp01(n + nFiner));
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateFoliageTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color leafDark = new Color(0.18f, 0.38f, 0.14f);
        Color leafBright = new Color(0.36f, 0.65f, 0.20f);
        Color leafYellow = new Color(0.55f, 0.72f, 0.22f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.12f, y * 0.12f);
                float fleck = Mathf.PerlinNoise(x * 0.35f, y * 0.35f);
                Color c = Color.Lerp(leafDark, leafBright, n);
                if (fleck > 0.65f) c = Color.Lerp(c, leafYellow, 0.4f);
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateFlowerTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color petalRed = new Color(0.92f, 0.22f, 0.32f);
        Color petalYellow = new Color(0.96f, 0.78f, 0.18f);
        Color petalPurple = new Color(0.68f, 0.32f, 0.88f);
        Color centerYellow = new Color(1.0f, 0.88f, 0.20f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Quadrant flower varieties
                Color c = petalRed;
                if (x < w / 2 && y >= h / 2) c = petalYellow;
                else if (x >= w / 2 && y >= h / 2) c = petalPurple;
                else if (x < w / 2 && y < h / 2) c = petalRed;
                else c = new Color(0.95f, 0.95f, 0.95f); // White daisy

                // Inner gradient
                float lx = (x % (w / 2)) / (w / 4f) - 1f;
                float ly = (y % (h / 2)) / (h / 4f) - 1f;
                float r = Mathf.Sqrt(lx * lx + ly * ly);

                if (r < 0.25f) c = centerYellow;
                else c = Color.Lerp(c, c * 0.8f, r * 0.5f);

                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateWoodPoleTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color wood1 = new Color(0.65f, 0.48f, 0.30f);
        Color wood2 = new Color(0.50f, 0.36f, 0.22f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.2f, y * 0.04f);
                tex.SetPixel(x, y, Color.Lerp(wood1, wood2, n));
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateNetMeshTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color threadColor = new Color(0.95f, 0.95f, 0.95f, 0.85f);
        Color holeColor = new Color(0.95f, 0.95f, 0.95f, 0.12f);

        int cellSize = 16;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int mx = x % cellSize;
                int my = y % cellSize;
                bool isThread = (mx < 2 || my < 2 || mx == cellSize - 1 || my == cellSize - 1);
                tex.SetPixel(x, y, isThread ? threadColor : holeColor);
            }
        }
        tex.Apply();
        return tex;
    }

    // Butterfly Wing Textures: Realistic anatomical patterns
    private static Texture2D GenerateMonarchWingTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color orange1 = new Color(0.98f, 0.48f, 0.08f);
        Color orange2 = new Color(0.90f, 0.35f, 0.05f);
        Color blackVein = new Color(0.08f, 0.06f, 0.06f);
        Color whiteSpot = new Color(0.96f, 0.96f, 0.96f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float u = (float)x / w;
                float v = (float)y / h;

                // Wing outer border check
                float distFromBorder = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
                bool isBorder = distFromBorder < 0.12f;

                // Vein pattern using radial angles and sinusoidal waves
                float angle = Mathf.Atan2(v - 0.2f, u - 0.1f);
                float vein = Mathf.Sin(angle * 7f) * Mathf.Cos(u * 14f + v * 8f);

                Color c;
                if (isBorder)
                {
                    c = blackVein;
                    // White decorative dots along margin
                    if (distFromBorder > 0.03f && distFromBorder < 0.08f)
                    {
                        if ((x * 11 + y * 13) % 29 == 0) c = whiteSpot;
                    }
                }
                else if (Mathf.Abs(vein) > 0.75f)
                {
                    c = blackVein;
                }
                else
                {
                    c = Color.Lerp(orange1, orange2, v * 0.7f + u * 0.3f);
                }
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateBlueMorphoWingTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color deepBlue = new Color(0.06f, 0.25f, 0.85f);
        Color brightCyan = new Color(0.20f, 0.75f, 0.98f);
        Color blackMargin = new Color(0.05f, 0.05f, 0.08f);
        Color eyeSpot = new Color(0.85f, 0.70f, 0.30f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float u = (float)x / w;
                float v = (float)y / h;

                float distFromEdge = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
                bool isBorder = distFromEdge < 0.14f;

                float sheen = Mathf.Sin(u * 18f) * 0.15f + Mathf.Cos(v * 12f) * 0.15f;
                Color iridescentBlue = Color.Lerp(deepBlue, brightCyan, Mathf.Clamp01(v + sheen));

                Color c = isBorder ? blackMargin : iridescentBlue;
                // Subtle eye spot
                float eyeDist = Vector2.Distance(new Vector2(u, v), new Vector2(0.75f, 0.35f));
                if (eyeDist < 0.05f) c = eyeSpot;

                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateEmeraldWingTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color emerald = new Color(0.08f, 0.85f, 0.45f);
        Color darkVelvet = new Color(0.06f, 0.10f, 0.08f);
        Color yellowDust = new Color(0.65f, 0.92f, 0.35f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float u = (float)x / w;
                float v = (float)y / h;

                // Striking green diagonal sash band
                float band = Mathf.Abs(u - v * 0.8f - 0.2f);
                Color c;
                if (band < 0.16f)
                {
                    c = Color.Lerp(emerald, yellowDust, band / 0.16f);
                }
                else
                {
                    c = darkVelvet;
                }
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateSulphurWingTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color brightYellow = new Color(0.98f, 0.88f, 0.15f);
        Color deepYellow = new Color(0.92f, 0.72f, 0.08f);
        Color blackStripe = new Color(0.12f, 0.10f, 0.08f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float u = (float)x / w;
                float v = (float)y / h;

                // Tiger stripes
                float stripe = Mathf.Sin(u * 22f + v * 6f);
                Color c = Color.Lerp(brightYellow, deepYellow, v);
                if (stripe > 0.65f && u > 0.2f && u < 0.85f)
                {
                    c = blackStripe;
                }

                float distEdge = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
                if (distEdge < 0.06f) c = blackStripe;

                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateCrimsonWingTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color rubyCrimson = new Color(0.85f, 0.12f, 0.22f);
        Color deepBurgundy = new Color(0.55f, 0.06f, 0.14f);
        Color darkBorder = new Color(0.08f, 0.04f, 0.06f);
        Color pinkSpot = new Color(0.95f, 0.65f, 0.75f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float u = (float)x / w;
                float v = (float)y / h;

                float distEdge = Mathf.Min(Mathf.Min(u, 1f - u), Mathf.Min(v, 1f - v));
                Color c = Color.Lerp(rubyCrimson, deepBurgundy, u);
                if (distEdge < 0.12f)
                {
                    c = darkBorder;
                    if (distEdge > 0.04f && (x * 13 + y * 7) % 31 == 0) c = pinkSpot;
                }

                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    // Realistic Boy Character Textures
    private static Texture2D GenerateBoySkinTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color skinTone = new Color(0.94f, 0.78f, 0.68f);
        Color blush = new Color(0.92f, 0.65f, 0.60f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.1f;
                Color c = Color.Lerp(skinTone, blush, n);
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateBoyShirtTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color shirtTeal = new Color(0.15f, 0.58f, 0.62f);
        Color shirtStripe = new Color(0.95f, 0.95f, 0.95f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool isStripe = (y % 40) < 6;
                Color c = isStripe ? shirtStripe : shirtTeal;
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateBoyDenimTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color denimDark = new Color(0.20f, 0.32f, 0.48f);
        Color denimLight = new Color(0.30f, 0.44f, 0.60f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float weave = ((x + y) % 4 == 0) ? 0.3f : 0f;
                Color c = Color.Lerp(denimDark, denimLight, weave);
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateBoySneakersTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color redCanvas = new Color(0.85f, 0.20f, 0.20f);
        Color whiteRubber = new Color(0.95f, 0.95f, 0.95f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color c = (y < h * 0.35f) ? whiteRubber : redCanvas;
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateBoyHairTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color darkBrown = new Color(0.25f, 0.16f, 0.10f);
        Color chestnut = new Color(0.40f, 0.26f, 0.16f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.25f, y * 0.05f);
                tex.SetPixel(x, y, Color.Lerp(darkBrown, chestnut, n));
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GenerateRoundedRectTexture(int w, int h, int cornerRadius, Color fill, Color border)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, true);
        Color clear = new Color(0, 0, 0, 0);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int cx = (x < cornerRadius) ? cornerRadius : (x >= w - cornerRadius) ? w - cornerRadius - 1 : x;
                int cy = (y < cornerRadius) ? cornerRadius : (y >= h - cornerRadius) ? h - cornerRadius - 1 : y;

                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (dist > cornerRadius)
                {
                    tex.SetPixel(x, y, clear);
                }
                else if (dist > cornerRadius - 2.5f)
                {
                    tex.SetPixel(x, y, border);
                }
                else
                {
                    tex.SetPixel(x, y, fill);
                }
            }
        }
        tex.Apply();
        return tex;
    }

    #endregion

    #region Materials Setup

    private static Dictionary<string, Material> CreateAllMaterials()
    {
        Dictionary<string, Material> dict = new Dictionary<string, Material>();
        Shader standardShader = Shader.Find("Standard");
        if (standardShader == null) standardShader = Shader.Find("Diffuse");

        // Helper to load or create
        Material MakeMat(string name, string texName, float smoothness, float metallic, bool isCutout = false)
        {
            string path = Path.Combine(MATERIALS_DIR, name + ".mat");
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(standardShader);
                AssetDatabase.CreateAsset(mat, path);
            }

            string texPath = Path.Combine(TEXTURES_DIR, texName);
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex != null)
            {
                mat.mainTexture = tex;
            }

            mat.SetFloat("_Glossiness", smoothness);
            mat.SetFloat("_Metallic", metallic);

            if (isCutout)
            {
                mat.SetFloat("_Mode", 1); // Cutout
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.EnableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 2450;
            }

            EditorUtility.SetDirty(mat);
            dict[name] = mat;
            return mat;
        }

        MakeMat("Mat_Grass", "Grass_Albedo.png", 0.15f, 0.0f);
        MakeMat("Mat_Cobblestone", "Cobblestone_Albedo.png", 0.25f, 0.05f);
        MakeMat("Mat_Bark", "Bark_Albedo.png", 0.1f, 0.0f);
        MakeMat("Mat_Foliage", "Foliage_Albedo.png", 0.25f, 0.0f);
        MakeMat("Mat_Flower", "Flower_Petals.png", 0.35f, 0.0f);
        MakeMat("Mat_WoodPole", "Wood_Pole.png", 0.4f, 0.05f);
        MakeMat("Mat_NetMesh", "Net_Mesh.png", 0.1f, 0.0f, true);

        // Butterfly Wing Materials
        MakeMat("Mat_Monarch", "Monarch_Wing.png", 0.6f, 0.05f);
        MakeMat("Mat_BlueMorpho", "BlueMorpho_Wing.png", 0.75f, 0.15f);
        MakeMat("Mat_Emerald", "Emerald_Wing.png", 0.7f, 0.1f);
        MakeMat("Mat_Sulphur", "Sulphur_Wing.png", 0.55f, 0.05f);
        MakeMat("Mat_CrimsonRose", "CrimsonRose_Wing.png", 0.65f, 0.05f);

        // Boy Character Materials
        MakeMat("Mat_BoySkin", "Boy_Skin.png", 0.25f, 0.0f);
        MakeMat("Mat_BoyShirt", "Boy_Shirt.png", 0.15f, 0.0f);
        MakeMat("Mat_BoyDenim", "Boy_Denim.png", 0.1f, 0.0f);
        MakeMat("Mat_BoySneakers", "Boy_Sneakers.png", 0.4f, 0.0f);
        MakeMat("Mat_BoyHair", "Boy_Hair.png", 0.2f, 0.0f);

        // Net Metal Ring Material
        string ringPath = Path.Combine(MATERIALS_DIR, "Mat_NetRing.mat");
        Material ringMat = AssetDatabase.LoadAssetAtPath<Material>(ringPath);
        if (ringMat == null)
        {
            ringMat = new Material(standardShader);
            AssetDatabase.CreateAsset(ringMat, ringPath);
        }
        ringMat.color = new Color(0.85f, 0.75f, 0.35f); // Brass gold
        ringMat.SetFloat("_Glossiness", 0.85f);
        ringMat.SetFloat("_Metallic", 0.85f);
        dict["Mat_NetRing"] = ringMat;

        AssetDatabase.SaveAssets();
        return dict;
    }

    #endregion

    #region Butterfly Prefabs Generation

    private static GameObject[] BuildButterflyPrefabs(Dictionary<string, Material> materials)
    {
        ButterflySpecies[] speciesList = new ButterflySpecies[]
        {
            ButterflySpecies.MonarchOrange,
            ButterflySpecies.BlueMorpho,
            ButterflySpecies.EmeraldSwallowtail,
            ButterflySpecies.SulphurYellow,
            ButterflySpecies.CrimsonRose
        };

        string[] matKeys = new string[]
        {
            "Mat_Monarch",
            "Mat_BlueMorpho",
            "Mat_Emerald",
            "Mat_Sulphur",
            "Mat_CrimsonRose"
        };

        GameObject[] prefabs = new GameObject[speciesList.Length];

        for (int i = 0; i < speciesList.Length; i++)
        {
            string prefabPath = Path.Combine(PREFABS_DIR, "Butterfly_" + speciesList[i].ToString() + ".prefab");

            GameObject root = new GameObject("Butterfly_" + speciesList[i].ToString());

            // Add Sphere Collider for catching detection
            SphereCollider sc = root.AddComponent<SphereCollider>();
            sc.radius = 0.45f;
            sc.isTrigger = true;

            // Add ButterflyAI
            ButterflyAI ai = root.AddComponent<ButterflyAI>();
            ai.SetSpecies(speciesList[i]);

            // Create Body Hierarchy
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(0.06f, 0.18f, 0.06f);
            body.transform.localRotation = Quaternion.Euler(75f, 0f, 0f);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            Renderer bodyRend = body.GetComponent<Renderer>();
            bodyRend.sharedMaterial = materials["Mat_Bark"]; // Dark insect body

            // Antennae
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject ant = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ant.name = (side == -1) ? "Antenna_L" : "Antenna_R";
                ant.transform.SetParent(body.transform, false);
                ant.transform.localPosition = new Vector3(side * 0.15f, 0.95f, 0.1f);
                ant.transform.localRotation = Quaternion.Euler(25f, side * 30f, 0f);
                ant.transform.localScale = new Vector3(0.1f, 0.4f, 0.1f);
                Object.DestroyImmediate(ant.GetComponent<Collider>());
                ant.GetComponent<Renderer>().sharedMaterial = materials["Mat_Bark"];
            }

            // Left Wing Pivot
            GameObject leftWingPivot = new GameObject("Wing_Left_Pivot");
            leftWingPivot.transform.SetParent(root.transform, false);
            leftWingPivot.transform.localPosition = new Vector3(-0.04f, 0.02f, 0f);

            GameObject leftWing = CreateWingQuad("Wing_Left_Mesh", materials[matKeys[i]], false);
            leftWing.transform.SetParent(leftWingPivot.transform, false);
            leftWing.transform.localPosition = new Vector3(-0.24f, 0f, 0f);
            leftWing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            leftWing.transform.localScale = new Vector3(0.48f, 0.38f, 1f);

            // Right Wing Pivot
            GameObject rightWingPivot = new GameObject("Wing_Right_Pivot");
            rightWingPivot.transform.SetParent(root.transform, false);
            rightWingPivot.transform.localPosition = new Vector3(0.04f, 0.02f, 0f);

            GameObject rightWing = CreateWingQuad("Wing_Right_Mesh", materials[matKeys[i]], true);
            rightWing.transform.SetParent(rightWingPivot.transform, false);
            rightWing.transform.localPosition = new Vector3(0.24f, 0f, 0f);
            rightWing.transform.localRotation = Quaternion.Euler(90f, 180f, 0f);
            rightWing.transform.localScale = new Vector3(0.48f, 0.38f, 1f);

            ai.SetWingTransforms(leftWingPivot.transform, rightWingPivot.transform);

            // Save Prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            prefabs[i] = prefab;
            Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        return prefabs;
    }

    private static GameObject CreateWingQuad(string name, Material mat, bool flipUv)
    {
        GameObject go = new GameObject(name);
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;

        Mesh mesh = new Mesh();
        mesh.name = name + "_Mesh";

        // Double sided planar quad
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
            // Back face
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0)
        };

        Vector2 u0 = flipUv ? new Vector2(1, 0) : new Vector2(0, 0);
        Vector2 u1 = flipUv ? new Vector2(0, 0) : new Vector2(1, 0);
        Vector2 u2 = flipUv ? new Vector2(1, 1) : new Vector2(0, 1);
        Vector2 u3 = flipUv ? new Vector2(0, 1) : new Vector2(1, 1);

        Vector2[] uvs = new Vector2[]
        {
            u0, u1, u2, u3,
            u1, u0, u3, u2
        };

        int[] triangles = new int[]
        {
            0, 2, 1,
            2, 3, 1,
            4, 5, 6,
            6, 5, 7
        };

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        mf.sharedMesh = mesh;
        return go;
    }

    #endregion

    #region Boy Character & Net Generation

    private static GameObject BuildBoyCharacterPrefab(Dictionary<string, Material> materials)
    {
        string prefabPath = Path.Combine(PREFABS_DIR, "BoyExplorerCharacter.prefab");

        GameObject root = new GameObject("BoyCharacter");

        // CharacterController configuration
        CharacterController cc = root.AddComponent<CharacterController>();
        cc.height = 1.45f;
        cc.radius = 0.35f;
        cc.center = new Vector3(0f, 0.725f, 0f);
        cc.stepOffset = 0.3f;

        PlayerMovement pm = root.AddComponent<PlayerMovement>();
        CharacterAnimator ca = root.AddComponent<CharacterAnimator>();
        ButterflyCatcher bc = root.AddComponent<ButterflyCatcher>();

        // Build Rigged Humanoid Hierarchy
        // 1. Pelvis / Hips
        GameObject hips = new GameObject("Hips");
        hips.transform.SetParent(root.transform, false);
        hips.transform.localPosition = new Vector3(0f, 0.76f, 0f);

        GameObject pelvisMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pelvisMesh.name = "Pelvis_Mesh";
        pelvisMesh.transform.SetParent(hips.transform, false);
        pelvisMesh.transform.localScale = new Vector3(0.28f, 0.18f, 0.20f);
        Object.DestroyImmediate(pelvisMesh.GetComponent<Collider>());
        pelvisMesh.GetComponent<Renderer>().sharedMaterial = materials["Mat_BoyDenim"];

        // 2. Spine & Torso
        GameObject spine = new GameObject("Spine");
        spine.transform.SetParent(hips.transform, false);
        spine.transform.localPosition = new Vector3(0f, 0.14f, 0f);

        GameObject torsoMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        torsoMesh.name = "Torso_Mesh";
        torsoMesh.transform.SetParent(spine.transform, false);
        torsoMesh.transform.localPosition = new Vector3(0f, 0.18f, 0f);
        torsoMesh.transform.localScale = new Vector3(0.32f, 0.36f, 0.22f);
        Object.DestroyImmediate(torsoMesh.GetComponent<Collider>());
        torsoMesh.GetComponent<Renderer>().sharedMaterial = materials["Mat_BoyShirt"];

        // 3. Head & Neck
        GameObject head = new GameObject("Head");
        head.transform.SetParent(spine.transform, false);
        head.transform.localPosition = new Vector3(0f, 0.42f, 0f);

        GameObject headMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        headMesh.name = "Head_Mesh";
        headMesh.transform.SetParent(head.transform, false);
        headMesh.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        headMesh.transform.localScale = new Vector3(0.24f, 0.26f, 0.24f);
        Object.DestroyImmediate(headMesh.GetComponent<Collider>());
        headMesh.GetComponent<Renderer>().sharedMaterial = materials["Mat_BoySkin"];

        // Boy Hair cap
        GameObject hairMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hairMesh.name = "Hair_Cap";
        hairMesh.transform.SetParent(head.transform, false);
        hairMesh.transform.localPosition = new Vector3(0f, 0.18f, -0.02f);
        hairMesh.transform.localScale = new Vector3(0.26f, 0.18f, 0.26f);
        Object.DestroyImmediate(hairMesh.GetComponent<Collider>());
        hairMesh.GetComponent<Renderer>().sharedMaterial = materials["Mat_BoyHair"];

        // 4. Left Arm & Hand
        GameObject leftArm = new GameObject("Left_Arm");
        leftArm.transform.SetParent(spine.transform, false);
        leftArm.transform.localPosition = new Vector3(-0.22f, 0.32f, 0f);

        GameObject lArmMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        lArmMesh.name = "Left_Arm_Mesh";
        lArmMesh.transform.SetParent(leftArm.transform, false);
        lArmMesh.transform.localPosition = new Vector3(0f, -0.16f, 0f);
        lArmMesh.transform.localScale = new Vector3(0.10f, 0.18f, 0.10f);
        Object.DestroyImmediate(lArmMesh.GetComponent<Collider>());
        lArmMesh.GetComponent<Renderer>().sharedMaterial = materials["Mat_BoyShirt"];

        GameObject lHandMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lHandMesh.name = "Left_Hand_Mesh";
        lHandMesh.transform.SetParent(leftArm.transform, false);
        lHandMesh.transform.localPosition = new Vector3(0f, -0.34f, 0f);
        lHandMesh.transform.localScale = new Vector3(0.11f, 0.11f, 0.11f);
        Object.DestroyImmediate(lHandMesh.GetComponent<Collider>());
        lHandMesh.GetComponent<Renderer>().sharedMaterial = materials["Mat_BoySkin"];

        // 5. Right Arm & Hand (Holds Butterfly Net!)
        GameObject rightArm = new GameObject("Right_Arm");
        rightArm.transform.SetParent(spine.transform, false);
        rightArm.transform.localPosition = new Vector3(0.22f, 0.32f, 0f);

        GameObject rArmMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rArmMesh.name = "Right_Arm_Mesh";
        rArmMesh.transform.SetParent(rightArm.transform, false);
        rArmMesh.transform.localPosition = new Vector3(0f, -0.16f, 0f);
        rArmMesh.transform.localScale = new Vector3(0.10f, 0.18f, 0.10f);
        Object.DestroyImmediate(rArmMesh.GetComponent<Collider>());
        rArmMesh.GetComponent<Renderer>().sharedMaterial = materials["Mat_BoyShirt"];

        GameObject rightHand = new GameObject("Right_Hand");
        rightHand.transform.SetParent(rightArm.transform, false);
        rightHand.transform.localPosition = new Vector3(0f, -0.34f, 0f);

        GameObject rHandMesh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rHandMesh.name = "Right_Hand_Mesh";
        rHandMesh.transform.SetParent(rightHand.transform, false);
        rHandMesh.transform.localScale = new Vector3(0.11f, 0.11f, 0.11f);
        Object.DestroyImmediate(rHandMesh.GetComponent<Collider>());
        rHandMesh.GetComponent<Renderer>().sharedMaterial = materials["Mat_BoySkin"];

        // 6. Butterfly Net Model attached firmly to Right Hand
        GameObject netRoot = new GameObject("ButterflyNet");
        netRoot.transform.SetParent(rightHand.transform, false);
        netRoot.transform.localPosition = new Vector3(0f, 0f, 0f);
        netRoot.transform.localRotation = Quaternion.Euler(70f, 0f, 15f);

        // Net Pole Handle (Bamboo / polished wood)
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Net_Pole";
        pole.transform.SetParent(netRoot.transform, false);
        pole.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        pole.transform.localScale = new Vector3(0.035f, 0.55f, 0.035f);
        Object.DestroyImmediate(pole.GetComponent<Collider>());
        pole.GetComponent<Renderer>().sharedMaterial = materials["Mat_WoodPole"];

        // Net Metal Ring Hoop
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Net_Hoop";
        ring.transform.SetParent(netRoot.transform, false);
        ring.transform.localPosition = new Vector3(0f, 1.05f, 0f);
        ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        ring.transform.localScale = new Vector3(0.55f, 0.02f, 0.55f);
        Object.DestroyImmediate(ring.GetComponent<Collider>());
        ring.GetComponent<Renderer>().sharedMaterial = materials["Mat_NetRing"];

        // Net Translucent Cone Mesh Bag
        GameObject netBag = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        netBag.name = "Net_Mesh_Bag";
        netBag.transform.SetParent(ring.transform, false);
        netBag.transform.localPosition = new Vector3(0f, -0.35f, 0f);
        netBag.transform.localScale = new Vector3(0.92f, 1.4f, 0.92f);
        Object.DestroyImmediate(netBag.GetComponent<Collider>());
        netBag.GetComponent<Renderer>().sharedMaterial = materials["Mat_NetMesh"];

        // Net Hoop Center for detection
        GameObject hoopCenter = new GameObject("Hoop_Center_Point");
        hoopCenter.transform.SetParent(ring.transform, false);
        bc.SetNetHoopCenter(hoopCenter.transform);

        // 7. Left Leg
        GameObject leftLeg = new GameObject("Left_Leg");
        leftLeg.transform.SetParent(hips.transform, false);
        leftLeg.transform.localPosition = new Vector3(-0.11f, -0.08f, 0f);

        GameObject lThighMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        lThighMesh.name = "Left_Thigh_Mesh";
        lThighMesh.transform.SetParent(leftLeg.transform, false);
        lThighMesh.transform.localPosition = new Vector3(0f, -0.16f, 0f);
        lThighMesh.transform.localScale = new Vector3(0.12f, 0.18f, 0.12f);
        Object.DestroyImmediate(lThighMesh.GetComponent<Collider>());
        lThighMesh.GetComponent<Renderer>().sharedMaterial = materials["Mat_BoyDenim"];

        GameObject leftShin = new GameObject("Left_Shin");
        leftShin.transform.SetParent(leftLeg.transform, false);
        leftShin.transform.localPosition = new Vector3(0f, -0.34f, 0f);

        GameObject lShinMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        lShinMesh.name = "Left_Shin_Mesh";
        lShinMesh.transform.SetParent(leftShin.transform, false);
        lShinMesh.transform.localPosition = new Vector3(0f, -0.16f, 0f);
        lShinMesh.transform.localScale = new Vector3(0.10f, 0.17f, 0.10f);
        Object.DestroyImmediate(lShinMesh.GetComponent<Collider>());
        lShinMesh.GetComponent<Renderer>().sharedMaterial = materials["Mat_BoySkin"];

        GameObject lFootMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lFootMesh.name = "Left_Shoe_Mesh";
        lFootMesh.transform.SetParent(leftShin.transform, false);
        lFootMesh.transform.localPosition = new Vector3(0f, -0.32f, 0.05f);
        lFootMesh.transform.localScale = new Vector3(0.12f, 0.08f, 0.22f);
        Object.DestroyImmediate(lFootMesh.GetComponent<Collider>());
        lFootMesh.GetComponent<Renderer>().sharedMaterial = materials["Mat_BoySneakers"];

        // 8. Right Leg
        GameObject rightLeg = new GameObject("Right_Leg");
        rightLeg.transform.SetParent(hips.transform, false);
        rightLeg.transform.localPosition = new Vector3(0.11f, -0.08f, 0f);

        GameObject rThighMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rThighMesh.name = "Right_Thigh_Mesh";
        rThighMesh.transform.SetParent(rightLeg.transform, false);
        rThighMesh.transform.localPosition = new Vector3(0f, -0.16f, 0f);
        rThighMesh.transform.localScale = new Vector3(0.12f, 0.18f, 0.12f);
        Object.DestroyImmediate(rThighMesh.GetComponent<Collider>());
        rThighMesh.GetComponent<Renderer>().sharedMaterial = materials["Mat_BoyDenim"];

        GameObject rightShin = new GameObject("Right_Shin");
        rightShin.transform.SetParent(rightLeg.transform, false);
        rightShin.transform.localPosition = new Vector3(0f, -0.34f, 0f);

        GameObject rShinMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rShinMesh.name = "Right_Shin_Mesh";
        rShinMesh.transform.SetParent(rightShin.transform, false);
        rShinMesh.transform.localPosition = new Vector3(0f, -0.16f, 0f);
        rShinMesh.transform.localScale = new Vector3(0.10f, 0.17f, 0.10f);
        Object.DestroyImmediate(rShinMesh.GetComponent<Collider>());
        rShinMesh.GetComponent<Renderer>().sharedMaterial = materials["Mat_BoySkin"];

        GameObject rFootMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rFootMesh.name = "Right_Shoe_Mesh";
        rFootMesh.transform.SetParent(rightShin.transform, false);
        rFootMesh.transform.localPosition = new Vector3(0f, -0.32f, 0.05f);
        rFootMesh.transform.localScale = new Vector3(0.12f, 0.08f, 0.22f);
        Object.DestroyImmediate(rFootMesh.GetComponent<Collider>());
        rFootMesh.GetComponent<Renderer>().sharedMaterial = materials["Mat_BoySneakers"];

        // Bind limbs to CharacterAnimator
        ca.BindLimbs(
            hips.transform, spine.transform, head.transform,
            leftArm.transform, rightArm.transform,
            leftLeg.transform, rightLeg.transform,
            leftShin.transform, rightShin.transform,
            rightHand.transform
        );

        // Save Prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        return prefab;
    }

    #endregion

    #region Main Scene Assembly

    private static void CreateMainScene(Dictionary<string, Material> materials, GameObject[] butterflyPrefabs, GameObject playerPrefab)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Directional Sun & Atmosphere
        GameObject sunObj = new GameObject("Sun_DirectionalLight");
        Light sunLight = sunObj.AddComponent<Light>();
        sunObj.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
        GardenAtmosphere atmosphere = sunObj.AddComponent<GardenAtmosphere>();
        atmosphere.ApplyAtmosphere();

        // 2. Realistic Garden Environment
        GameObject envRoot = new GameObject("Garden_Environment");

        // Ground Plane (48m x 48m) with grass material and collider
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Garden_Ground";
        ground.transform.SetParent(envRoot.transform, false);
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(4.8f, 1f, 4.8f);
        ground.GetComponent<Renderer>().sharedMaterial = materials["Mat_Grass"];
        ground.GetComponent<Collider>().material = null; // Standard physics

        // Winding Stone Garden Paths
        BuildGardenWalkways(envRoot.transform, materials);

        // Trees, Bushes, Flowerbeds
        BuildGardenVegetation(envRoot.transform, materials);

        // Garden Perimeter Boundaries (Wooden Fence + Invisible Wall Colliders)
        BuildGardenBoundaries(envRoot.transform, materials);

        // 3. Player Character Instance
        GameObject playerInstance = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        playerInstance.name = "BoyExplorer";
        playerInstance.transform.position = new Vector3(0f, 0.05f, 0f);

        // 4. Smooth Third-Person Camera
        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.Skybox;
        cam.fieldOfView = 60f;
        camObj.AddComponent<AudioListener>();

        CameraFollow camFollow = camObj.AddComponent<CameraFollow>();
        camFollow.SetTarget(playerInstance.transform);
        camObj.transform.position = new Vector3(0f, 2.4f, -4.6f);
        camObj.transform.rotation = Quaternion.Euler(16f, 0f, 0f);

        PlayerMovement pm = playerInstance.GetComponent<PlayerMovement>();
        if (pm != null) pm.SetCameraTransform(camObj.transform);

        // 5. Butterfly Spawner
        GameObject spawnerObj = new GameObject("ButterflySpawner");
        ButterflySpawner spawner = spawnerObj.AddComponent<ButterflySpawner>();
        spawner.SetPrefabs(butterflyPrefabs);

        // 6. Sound Manager
        GameObject soundObj = new GameObject("SoundManager");
        soundObj.AddComponent<SoundManager>();

        // 7. Score Manager
        GameObject scoreObj = new GameObject("ScoreManager");
        scoreObj.AddComponent<ScoreManager>();

        // 8. Game Manager
        GameObject gameMgrObj = new GameObject("GameManager");
        GameManager gm = gameMgrObj.AddComponent<GameManager>();

        // 9. Canvas UI Setup
        BuildFullCanvasUI(materials);

        // Configure Scene in Build Settings
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        SetSceneInBuildSettings(SCENE_PATH);

        Debug.Log("[SetupButterflyGame] Scene saved successfully to: " + SCENE_PATH);
    }

    private static void BuildGardenWalkways(Transform parent, Dictionary<string, Material> materials)
    {
        GameObject pathsRoot = new GameObject("Stone_Paths");
        pathsRoot.transform.SetParent(parent, false);

        // Cross and circular path paving stones
        for (float z = -20f; z <= 20f; z += 1.8f)
        {
            float curve = Mathf.Sin(z * 0.15f) * 2.5f;
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.name = "PathStone_N";
            stone.transform.SetParent(pathsRoot.transform, false);
            stone.transform.position = new Vector3(curve, 0.015f, z);
            stone.transform.localScale = new Vector3(2.4f, 0.03f, 1.5f);
            stone.transform.rotation = Quaternion.Euler(0f, Random.Range(-4f, 4f), 0f);
            Object.DestroyImmediate(stone.GetComponent<Collider>());
            stone.GetComponent<Renderer>().sharedMaterial = materials["Mat_Cobblestone"];
        }

        for (float x = -20f; x <= 20f; x += 1.8f)
        {
            float curve = Mathf.Cos(x * 0.15f) * 2.2f;
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.name = "PathStone_E";
            stone.transform.SetParent(pathsRoot.transform, false);
            stone.transform.position = new Vector3(x, 0.015f, curve);
            stone.transform.localScale = new Vector3(1.5f, 0.03f, 2.4f);
            stone.transform.rotation = Quaternion.Euler(0f, Random.Range(-4f, 4f), 0f);
            Object.DestroyImmediate(stone.GetComponent<Collider>());
            stone.GetComponent<Renderer>().sharedMaterial = materials["Mat_Cobblestone"];
        }
    }

    private static void BuildGardenVegetation(Transform parent, Dictionary<string, Material> materials)
    {
        GameObject vegRoot = new GameObject("Vegetation");
        vegRoot.transform.SetParent(parent, false);

        // 1. Realistic Garden Trees
        Vector3[] treePositions = new Vector3[]
        {
            new Vector3(-16f, 0f, -16f),
            new Vector3(16f, 0f, -15f),
            new Vector3(-17f, 0f, 16f),
            new Vector3(17f, 0f, 17f),
            new Vector3(-10f, 0f, -18f),
            new Vector3(10f, 0f, -18f),
            new Vector3(-18f, 0f, -8f),
            new Vector3(18f, 0f, 8f),
            new Vector3(-8f, 0f, 18f),
            new Vector3(8f, 0f, 18f),
            new Vector3(-18f, 0f, 6f),
            new Vector3(14f, 0f, -6f)
        };

        for (int i = 0; i < treePositions.Length; i++)
        {
            BuildRealisticTree(vegRoot.transform, treePositions[i], materials, i);
        }

        // 2. Flowering Bushes and Flowerbeds
        Vector3[] bushPositions = new Vector3[]
        {
            new Vector3(-6f, 0f, -6f),
            new Vector3(6f, 0f, -6f),
            new Vector3(-6f, 0f, 6f),
            new Vector3(6f, 0f, 6f),
            new Vector3(-12f, 0f, 0f),
            new Vector3(12f, 0f, 0f),
            new Vector3(0f, 0f, -12f),
            new Vector3(0f, 0f, 12f),
            new Vector3(-14f, 0f, -10f),
            new Vector3(14f, 0f, 10f),
            new Vector3(-10f, 0f, 14f),
            new Vector3(10f, 0f, -14f)
        };

        for (int i = 0; i < bushPositions.Length; i++)
        {
            BuildFlowerBed(vegRoot.transform, bushPositions[i], materials, i);
        }
    }

    private static void BuildRealisticTree(Transform parent, Vector3 pos, Dictionary<string, Material> materials, int index)
    {
        GameObject tree = new GameObject("Tree_" + index);
        tree.transform.SetParent(parent, false);
        tree.transform.position = pos;

        float scaleMod = Random.Range(0.85f, 1.25f);

        // Trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.SetParent(tree.transform, false);
        trunk.transform.localPosition = new Vector3(0f, 2.5f * scaleMod, 0f);
        trunk.transform.localScale = new Vector3(0.6f * scaleMod, 2.5f * scaleMod, 0.6f * scaleMod);
        trunk.GetComponent<Renderer>().sharedMaterial = materials["Mat_Bark"];

        // Canopy Layer 1 (Lower large)
        GameObject canopy1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy1.name = "Canopy_Lower";
        canopy1.transform.SetParent(tree.transform, false);
        canopy1.transform.localPosition = new Vector3(0f, 4.6f * scaleMod, 0f);
        canopy1.transform.localScale = new Vector3(4.2f * scaleMod, 2.8f * scaleMod, 4.2f * scaleMod);
        Object.DestroyImmediate(canopy1.GetComponent<Collider>());
        canopy1.GetComponent<Renderer>().sharedMaterial = materials["Mat_Foliage"];

        // Canopy Layer 2 (Upper crown)
        GameObject canopy2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy2.name = "Canopy_Upper";
        canopy2.transform.SetParent(tree.transform, false);
        canopy2.transform.localPosition = new Vector3(0.2f, 6.2f * scaleMod, -0.1f);
        canopy2.transform.localScale = new Vector3(3.2f * scaleMod, 2.5f * scaleMod, 3.2f * scaleMod);
        Object.DestroyImmediate(canopy2.GetComponent<Collider>());
        canopy2.GetComponent<Renderer>().sharedMaterial = materials["Mat_Foliage"];
    }

    private static void BuildFlowerBed(Transform parent, Vector3 center, Dictionary<string, Material> materials, int index)
    {
        GameObject bed = new GameObject("FlowerBed_" + index);
        bed.transform.SetParent(parent, false);
        bed.transform.position = center;

        // Bush foliage sphere
        GameObject bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bush.name = "Bush_Dome";
        bush.transform.SetParent(bed.transform, false);
        bush.transform.localPosition = new Vector3(0f, 0.45f, 0f);
        bush.transform.localScale = new Vector3(2.2f, 1.1f, 2.2f);
        Object.DestroyImmediate(bush.GetComponent<Collider>());
        bush.GetComponent<Renderer>().sharedMaterial = materials["Mat_Foliage"];

        // Colorful Flower Petal Clusters
        for (int f = 0; f < 8; f++)
        {
            float angle = f * (Mathf.PI * 2f / 8f) + Random.Range(-0.2f, 0.2f);
            float dist = Random.Range(0.6f, 1.1f);
            Vector3 fPos = new Vector3(Mathf.Cos(angle) * dist, 0.55f + Random.Range(0f, 0.2f), Mathf.Sin(angle) * dist);

            GameObject flower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            flower.name = "Flower_" + f;
            flower.transform.SetParent(bed.transform, false);
            flower.transform.localPosition = fPos;
            flower.transform.localScale = new Vector3(0.35f, 0.02f, 0.35f);
            flower.transform.localRotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 360f), Random.Range(-15f, 15f));
            Object.DestroyImmediate(flower.GetComponent<Collider>());
            flower.GetComponent<Renderer>().sharedMaterial = materials["Mat_Flower"];
        }
    }

    private static void BuildGardenBoundaries(Transform parent, Dictionary<string, Material> materials)
    {
        GameObject fenceRoot = new GameObject("Perimeter_Fences_And_Boundaries");
        fenceRoot.transform.SetParent(parent, false);

        float limit = 23f;

        // 4 Boundary Colliders to keep character and camera contained
        void AddWall(string name, Vector3 pos, Vector3 size)
        {
            GameObject wall = new GameObject(name);
            wall.transform.SetParent(fenceRoot.transform, false);
            wall.transform.position = pos;
            BoxCollider col = wall.AddComponent<BoxCollider>();
            col.size = size;
        }

        AddWall("Wall_North", new Vector3(0f, 2f, limit), new Vector3(limit * 2f, 4f, 1f));
        AddWall("Wall_South", new Vector3(0f, 2f, -limit), new Vector3(limit * 2f, 4f, 1f));
        AddWall("Wall_East", new Vector3(limit, 2f, 0f), new Vector3(1f, 4f, limit * 2f));
        AddWall("Wall_West", new Vector3(-limit, 2f, 0f), new Vector3(1f, 4f, limit * 2f));

        // Decorative Wooden Fence Posts around perimeter
        for (float x = -limit + 2f; x <= limit - 2f; x += 3.5f)
        {
            CreateFencePost(fenceRoot.transform, new Vector3(x, 0.5f, limit - 0.5f), materials["Mat_WoodPole"]);
            CreateFencePost(fenceRoot.transform, new Vector3(x, 0.5f, -limit + 0.5f), materials["Mat_WoodPole"]);
        }

        for (float z = -limit + 2f; z <= limit - 2f; z += 3.5f)
        {
            CreateFencePost(fenceRoot.transform, new Vector3(limit - 0.5f, 0.5f, z), materials["Mat_WoodPole"]);
            CreateFencePost(fenceRoot.transform, new Vector3(-limit + 0.5f, 0.5f, z), materials["Mat_WoodPole"]);
        }
    }

    private static void CreateFencePost(Transform parent, Vector3 pos, Material mat)
    {
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = "FencePost";
        post.transform.SetParent(parent, false);
        post.transform.position = pos;
        post.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);
        Object.DestroyImmediate(post.GetComponent<Collider>());
        post.GetComponent<Renderer>().sharedMaterial = mat;
    }

    #endregion

    #region Canvas UI Setup

    private static void BuildFullCanvasUI(Dictionary<string, Material> materials)
    {
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Ensure EventSystem exists
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        Sprite panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Path.Combine(TEXTURES_DIR, "UI_Panel.png"));
        Sprite btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(Path.Combine(TEXTURES_DIR, "UI_Button.png"));
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        UIManager uiManager = canvasObj.AddComponent<UIManager>();

        // 1. HUD Panel
        GameObject hud = new GameObject("HUD_Panel");
        hud.transform.SetParent(canvasObj.transform, false);
        RectTransform hudRect = hud.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0f, 1f);
        hudRect.anchorMax = new Vector2(1f, 1f);
        hudRect.pivot = new Vector2(0.5f, 1f);
        hudRect.sizeDelta = new Vector2(0f, 100f);

        // Header Background Bar
        Image hudBg = hud.AddComponent<Image>();
        hudBg.color = new Color(0.06f, 0.12f, 0.09f, 0.75f);

        // Score Text
        Text scoreTxt = CreateText(hud.transform, "ScoreText", "SCORE: 0", font, 34, TextAnchor.MiddleLeft, new Vector2(40f, -50f), new Vector2(280f, 60f), new Color(1.0f, 0.88f, 0.25f));

        // Timer Text (Center Top)
        Text timerTxt = CreateText(hud.transform, "TimerText", "TIME: 60s", font, 38, TextAnchor.MiddleCenter, new Vector2(0f, -50f), new Vector2(240f, 60f), Color.white);
        timerTxt.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        timerTxt.rectTransform.anchorMax = new Vector2(0.5f, 1f);

        // Caught Text
        Text caughtTxt = CreateText(hud.transform, "CaughtText", "CAUGHT: 0", font, 34, TextAnchor.MiddleRight, new Vector2(-160f, -50f), new Vector2(280f, 60f), new Color(0.45f, 0.95f, 0.65f));
        caughtTxt.rectTransform.anchorMin = new Vector2(1f, 1f);
        caughtTxt.rectTransform.anchorMax = new Vector2(1f, 1f);

        // Pause Button
        Button pauseBtn = CreateButton(hud.transform, "PauseButton", "PAUSE", font, btnSprite, new Vector2(-50f, -50f), new Vector2(110f, 48f), 18);
        pauseBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 1f);
        pauseBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);

        // Controls reminder bottom hint
        Text hintTxt = CreateText(hud.transform, "HintText", "[WASD] Move   |   [Shift] Run   |   [Space] Catch Butterfly   |   [Esc] Pause", font, 20, TextAnchor.MiddleCenter, new Vector2(0f, 40f), new Vector2(900f, 40f), new Color(1f, 1f, 1f, 0.85f));
        hintTxt.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        hintTxt.rectTransform.anchorMax = new Vector2(0.5f, 0f);

        // 2. Start Menu Panel
        GameObject startMenu = CreateModalPanel(canvasObj.transform, "StartMenu_Panel", panelSprite, new Vector2(620f, 480f));
        CreateText(startMenu.transform, "Title", "REALISTIC BUTTERFLY CATCHER", font, 36, TextAnchor.MiddleCenter, new Vector2(0f, 160f), new Vector2(580f, 60f), new Color(1.0f, 0.90f, 0.35f));
        CreateText(startMenu.transform, "SubTitle", "Explore the lush 3D garden, approach colorful butterflies,\nand swing your net with precision before time expires!", font, 20, TextAnchor.MiddleCenter, new Vector2(0f, 75f), new Vector2(540f, 80f), Color.white);
        CreateText(startMenu.transform, "Controls", "CONTROLS:\n• W / A / S / D: Walk & Turn\n• Left Shift: Sprint\n• Spacebar: Catch Butterfly with Net\n• Right-Click & Drag: Look Around", font, 20, TextAnchor.MiddleLeft, new Vector2(0f, -40f), new Vector2(440f, 110f), new Color(0.85f, 0.95f, 0.85f));
        Button startBtn = CreateButton(startMenu.transform, "StartButton", "START GARDEN HUNT", font, btnSprite, new Vector2(0f, -170f), new Vector2(340f, 62f), 24);

        // 3. Pause Menu Panel
        GameObject pauseMenu = CreateModalPanel(canvasObj.transform, "PauseMenu_Panel", panelSprite, new Vector2(480f, 380f));
        CreateText(pauseMenu.transform, "PauseTitle", "GAME PAUSED", font, 38, TextAnchor.MiddleCenter, new Vector2(0f, 110f), new Vector2(420f, 60f), Color.white);
        Button resumeBtn = CreateButton(pauseMenu.transform, "ResumeButton", "RESUME", font, btnSprite, new Vector2(0f, 10f), new Vector2(280f, 55f), 22);
        Button pauseRestartBtn = CreateButton(pauseMenu.transform, "RestartButton", "RESTART GAME", font, btnSprite, new Vector2(0f, -70f), new Vector2(280f, 55f), 22);

        // 4. Game Over Panel
        GameObject gameOver = CreateModalPanel(canvasObj.transform, "GameOver_Panel", panelSprite, new Vector2(580f, 500f));
        CreateText(gameOver.transform, "OverTitle", "TIME'S UP!", font, 44, TextAnchor.MiddleCenter, new Vector2(0f, 170f), new Vector2(500f, 65f), new Color(1.0f, 0.35f, 0.35f));
        Text finalScoreTxt = CreateText(gameOver.transform, "FinalScore", "FINAL SCORE: 0", font, 32, TextAnchor.MiddleCenter, new Vector2(0f, 85f), new Vector2(500f, 50f), new Color(1.0f, 0.90f, 0.30f));
        Text finalCaughtTxt = CreateText(gameOver.transform, "FinalCaught", "BUTTERFLIES CAUGHT: 0", font, 26, TextAnchor.MiddleCenter, new Vector2(0f, 30f), new Vector2(500f, 50f), Color.white);
        Text rankTxt = CreateText(gameOver.transform, "RankText", "GARDEN ENTHUSIAST", font, 22, TextAnchor.MiddleCenter, new Vector2(0f, -35f), new Vector2(500f, 50f), new Color(0.4f, 0.95f, 0.65f));
        Button gameOverRestartBtn = CreateButton(gameOver.transform, "RestartButton", "PLAY AGAIN", font, btnSprite, new Vector2(0f, -150f), new Vector2(320f, 64f), 24);

        // Wire UIManager
        uiManager.SetReferences(
            hud, startMenu, pauseMenu, gameOver,
            scoreTxt, timerTxt, caughtTxt, pauseBtn,
            finalScoreTxt, finalCaughtTxt, rankTxt, gameOverRestartBtn,
            startBtn, resumeBtn, pauseRestartBtn
        );
    }

    private static GameObject CreateModalPanel(Transform parent, string name, Sprite sprite, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;

        Image img = panel.AddComponent<Image>();
        if (sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
        }
        img.color = new Color(0.08f, 0.16f, 0.12f, 0.92f);

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.35f, 0.75f, 0.45f, 0.6f);
        outline.effectDistance = new Vector2(2f, -2f);

        return panel;
    }

    private static Text CreateText(Transform parent, string name, string content, Font font, int size, TextAnchor alignment, Vector2 pos, Vector2 dimensions, Color color)
    {
        GameObject tObj = new GameObject(name);
        tObj.transform.SetParent(parent, false);

        RectTransform rt = tObj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = dimensions;

        Text txt = tObj.AddComponent<Text>();
        txt.text = content;
        txt.font = font;
        txt.fontSize = size;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = alignment;
        txt.color = color;

        Shadow shadow = tObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return txt;
    }

    private static Button CreateButton(Transform parent, string name, string label, Font font, Sprite btnSprite, Vector2 pos, Vector2 size, int fontSize)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        if (btnSprite != null)
        {
            img.sprite = btnSprite;
            img.type = Image.Type.Sliced;
        }
        img.color = new Color(0.20f, 0.65f, 0.35f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.20f, 0.65f, 0.35f, 1f);
        cb.highlightedColor = new Color(0.28f, 0.82f, 0.44f, 1f);
        cb.pressedColor = new Color(0.12f, 0.48f, 0.24f, 1f);
        btn.colors = cb;

        // Button Label
        CreateText(btnObj.transform, "Label", label, font, fontSize, TextAnchor.MiddleCenter, Vector2.zero, size, Color.white);

        return btn;
    }

    private static void SetSceneInBuildSettings(string scenePath)
    {
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = scenes;
    }

    #endregion
}
