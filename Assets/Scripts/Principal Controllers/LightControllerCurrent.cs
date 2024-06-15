using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public static class LightController
{
    public static LightControllerCurrent lightController;
}

public class LightControllerCurrent : MonoBehaviour
{
    public GameObject[] loadedChunks;
    public Camera minimapCamera;
    public UnityEngine.UI.Dropdown lightStyleDropdown;
    public Dictionary<Vector2, float> renderLights = new Dictionary<Vector2, float>();
    public List<Vector2> temp1 = new List<Vector2>();
    public List<float> temp2 = new List<float>();

    public Vector3 previousPosition;

    public int lightDist = 30;
    public int lightStyle = 0;
    public int lightRadius = 5;
    public EcoTexture lightEcoTexture;
    public bool renderizingLight = false;
    public bool renderized = false;
    public Vector2Int renderQueue;
    public Vector2 renderizedTexturePosition;
    public bool pendingRender = false;

    void Start()
    {
        LightController.lightController = this;
        lightEcoTexture = new EcoTexture(lightDist, lightDist);
        Texture2D lightTexture = lightEcoTexture.Export(FilterMode.Point);
        Sprite illuminationMap = Sprite.Create(lightTexture, new Rect(0f, 0f, lightTexture.width, lightTexture.height), new Vector2(0f, 0f), 1);
        GetComponent<SpriteRenderer>().sprite = illuminationMap;

        if (DataSaver.LoadStats(Application.persistentDataPath + @"/settings/lightstyle.lgrsd").SavedData[0] == "0")
        {
            lightStyleDropdown.value = 0;
        }
        else
        {
            lightStyleDropdown.value = 1;
        }
    }

    void Update()
    {
        if (!renderizingLight && !renderized)
        {
            Vector2 globalPosition = Vector2Int.RoundToInt(Camera.main.transform.position);
            transform.position = globalPosition - Vector2.one * 0.5f;

            if (transform.position != previousPosition)
            {
                AddRenderQueue(globalPosition);
                transform.position = previousPosition;
            }

            if (pendingRender)
            {
                pendingRender = false;
                renderizingLight = true;
                UpdateLight((Vector2)renderQueue);
            }
        }
        else if (renderized)
        {
            transform.position = renderizedTexturePosition;
            RenderizeTexture();
        }

        previousPosition = transform.position;
    }

    public void AddRenderQueue(Vector2 renderPosition)
    {
        renderQueue = Vector2Int.RoundToInt(renderPosition);
        pendingRender = true;
    }

    public void DrawLights()
    {
        for(int i = 0; i < temp1.Count; i++)
        {
            renderLights.Add(temp1[i], temp2[i]);
        }

        lightEcoTexture = new EcoTexture(lightDist, lightDist);
        lightEcoTexture.FillWith(Color.black);

        int index = 0;

        foreach (KeyValuePair<Vector2, float> renderLight in renderLights)
        {
            Vector2 rendLight = renderLight.Key;
            float intensity = renderLight.Value;

            Color thisColorLight = lightEcoTexture.GetPixel((int)rendLight.x, (int)rendLight.y);
            bool didPoint = false;
            if (intensity > 1f)
            {
                float secondIntensity = -1f;
                if (renderLights.TryGetValue(rendLight + Vector2.down, out secondIntensity))
                {
                    if (secondIntensity > 1f)
                        if (renderLights.TryGetValue(rendLight + Vector2.left, out secondIntensity))
                            if (secondIntensity > 1f)
                                if (renderLights.TryGetValue(rendLight + Vector2.up, out secondIntensity))
                                    if (secondIntensity > 1f)
                                        if (renderLights.TryGetValue(rendLight + Vector2.right, out secondIntensity))
                                        {
                                            lightEcoTexture.SetPixel((int)rendLight.x, (int)rendLight.y, 0, 0, 0, 1f - GameManager.gameManagerReference.dayLuminosity);
                                            didPoint = true;
                                        }

                }
            }

            if (!didPoint)
            {
                for (int nx = (int)rendLight.x - lightRadius; nx < (int)rendLight.x + lightRadius + 1; nx++)
                {
                    for (int ny = (int)rendLight.y - lightRadius; ny < (int)rendLight.y + lightRadius + 1; ny++)
                    {
                        float dist = Vector2.Distance(new Vector2(nx, ny), new Vector2((int)rendLight.x, (int)rendLight.y));
                        if (dist < lightDist)
                        {
                            Color gettedColor = lightEcoTexture.GetPixel(nx, ny);
                            dist = Mathf.Clamp(dist, 0f, lightRadius);
                            float alphaIntensity = dist / lightRadius;
                            if (intensity > 1)
                            {
                                alphaIntensity = (1 - alphaIntensity) * GameManager.gameManagerReference.dayLuminosity;
                            }
                            else
                            {
                                alphaIntensity = (1 - alphaIntensity) * intensity;
                            }
                            alphaIntensity = 1 - alphaIntensity;


                            if (gettedColor.r >= 0f)
                            {
                                if (gettedColor.a > alphaIntensity)
                                {
                                    if (dist > 1.5f)
                                    {
                                        Color color = Color.black;
                                        color.a = alphaIntensity;
                                        lightEcoTexture.SetPixel(nx, ny, color.r, color.g, color.b, color.a);
                                    }
                                    else if (intensity > 1) lightEcoTexture.SetPixel(nx, ny, 0, 0, 0, 1f - GameManager.gameManagerReference.dayLuminosity);
                                    else lightEcoTexture.SetPixel(nx, ny, 0, 0, 0, 1f - intensity);
                                }
                            }
                        }
                    }
                }
                index++;
            }
        }
        renderized = true;
        renderizingLight = false;
    }

    private void UpdateLight(Vector3 lightPosition)
    {
        renderizingLight = true;
        if (MenuController.menuController.miniMapOn)
            lightDist = (int)minimapCamera.orthographicSize / 2 * 4 + (lightRadius * 3);
        else
            lightDist = 40;


        UpdateLights(lightPosition);
        renderizedTexturePosition = lightPosition + (Vector3)Vector2.one * 0.5f;

            ThreadStart lightDrawRef = new ThreadStart(DrawLights);
            Thread lightRender = new Thread(lightDrawRef);
            lightRender.Start();
    }

    public void RenderizeTexture()
    {
        Texture2D lightTexture;
        if (lightStyle == 0)
        {
            lightTexture = lightEcoTexture.Export(FilterMode.Point);
        }
        else
        {
            lightTexture = lightEcoTexture.Export(FilterMode.Trilinear);
        }
        Sprite illuminationMap = Sprite.Create(lightTexture, new Rect(0.0f, 0.0f, lightTexture.width, lightTexture.height), new Vector2(0.5f, 0.5f), 1, 0, SpriteMeshType.FullRect);
        GetComponent<SpriteRenderer>().sprite = illuminationMap;
        renderized = false;
    }

    private void UpdateLights(Vector2 lightPosition)
    {
        loadedChunks = GameManager.gameManagerReference.LoadedChunks.ToArray();
        renderLights = new Dictionary<Vector2, float>(lightDist*lightDist*2);
        temp1 = new List<Vector2>();
        temp2 = new List<float>();

        if (loadedChunks.Length > 0)
        {
            List<GameObject> gameChunksToRead = new List<GameObject>();
            int playerChunk = (int)Mathf.Floor(GameManager.gameManagerReference.player.transform.position.x / 16f);

            for (int i = playerChunk - MenuController.menuController.chunksOnEachSide; i < playerChunk + MenuController.menuController.chunksOnEachSide; i++)
            {
                int chunkToLoad = i;
                if (chunkToLoad < 0)
                {
                    chunkToLoad += GameManager.gameManagerReference.WorldWidth;
                }
                if (chunkToLoad >= GameManager.gameManagerReference.WorldWidth)
                {
                    chunkToLoad -= GameManager.gameManagerReference.WorldWidth;
                }

                gameChunksToRead.Add(GameManager.gameManagerReference.chunkContainer.transform.GetChild(chunkToLoad).gameObject);
            }

            int e = 0;
            Vector2 abc = Vector2.one * (lightDist / 2);
            Vector2 min = lightPosition - (Vector2.one * (lightDist + lightRadius * 2));
            Vector2 max = lightPosition + (Vector2.one * (lightDist + lightRadius * 2));
            int worldHeight = GameManager.gameManagerReference.WorldHeight;

            foreach (GameObject loadedChunk in gameChunksToRead)
            {
                ChunkController chunkController = loadedChunk.GetComponent<ChunkController>();
                int tileX = 0;
                int tileY = 0;

                if (!chunkController.loading)
                    if (chunkController.currentX + 16 > transform.position.x - (lightDist + lightRadius * 2) && chunkController.currentX < transform.position.x + (lightDist + lightRadius * 2))
                        for (int i = 0; i < chunkController.LightMap.Length; i++)
                        {
                            if (chunkController.LightMap[i] > 0)
                            {
                                Vector2 thePosition = new Vector2(tileX + chunkController.currentX, tileY);

                                if (ManagingFunctions.InsideRanges(thePosition, min, max))
                                {
                                    Vector2 lightPos = lightPosition - thePosition;

                                    temp1.Add(lightPos + abc);
                                    temp2.Add(chunkController.LightMap[i]);
                                }

                            }

                            tileY++;
                            if(tileY == worldHeight)
                            {
                                tileX++;
                                tileY = 0;
                            }
                        }

                e++;
            }
        }
    }

    public void SetLightStyle(int style)
    {
        lightStyle = style;
        DataSaver.SaveStats(new string[] { style + "" }, Application.persistentDataPath + @"/settings/lightstyle.lgrsd");



        AddRenderQueue(GameManager.gameManagerReference.player.transform.position);
    }
}

public class EcoTexture
{
    int width = 0;
    int height = 0;
    float[] r;
    float[] g;
    float[] b;
    float[] a;

    public EcoTexture(int texWidth, int texHeight)
    {
        width = texWidth;
        height = texHeight;
        r = new float[width * height];
        g = new float[width * height];
        b = new float[width * height];
        a = new float[width * height];
    }

    public void SetPixel(int x, int y, float pr, float pg, float pb, float pa)
    {
        if((x >= 0 && y >= 0) && (x < width && y < height))
        {
            int index = (y * width) + x;
            r[index] = pr;
            g[index] = pg;
            b[index] = pb;
            a[index] = pa;
        }
    }

    public void FillWith(Color color)
    {
        for(int i = 0; i < width * height; i++)
        {
            r[i] = color.r;
            g[i] = color.g;
            b[i] = color.b;
            a[i] = color.a;
        }
    }

    public Color GetPixel(int x, int y)
    {
        if ((x >= 0 && y >= 0) && (x < width && y < height))
        {
            int idx = (y * width) + x;

            return new Color(r[idx], g[idx], b[idx], a[idx]);
        }
        else
        {
            return new Color(-1f, -1f, -1f, -1f);
        }
    }

    public Texture2D Export(FilterMode filterMode)
    {
        Texture2D texture2D = new Texture2D(width, height);
        Color[] coolArray = new Color[width * height];
        for(int i = 0;i < width * height; i++)
        {
            coolArray[i] = new Color(r[i], g[i], b[i], a[i]);
        }
        texture2D.SetPixels(coolArray);
        texture2D.filterMode = filterMode;
        texture2D.Apply();
        return texture2D;
    }
}
