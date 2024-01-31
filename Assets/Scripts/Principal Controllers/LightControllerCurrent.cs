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
    public Vector3 previousPosition;

    public int lightDist = 30;
    public int lightStyle = 0;
    public int lightRadius = 5;
    public EcoTexture lightEcoTexture;
    public bool renderizingLight = false;
    public bool renderized = false;
    public Queue<Vector3> renderQueue = new Queue<Vector3>();

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
        loadedChunks = GameManager.gameManagerReference.LoadedChunks;
        if (!renderizingLight && !renderized && renderQueue.Count < 1)
        {
            Vector2 globalPosition = ManagingFunctions.RoundVector2(Camera.main.transform.position);
            transform.position = globalPosition - Vector2.one * 0.5f;

            if (transform.position != previousPosition)
            {
                AddRenderQueue(globalPosition);
                transform.position = previousPosition;
            }

            
        }



        if (renderQueue.Count > 0 && !renderizingLight && !renderized)
        {
            renderizingLight = true;
            Vector3 x = renderQueue.Peek();
            renderQueue = new Queue<Vector3>();
            renderQueue.Enqueue(x);
            UpdateLight(x);
        }


        if (renderized && renderQueue.Count > 0)
        {
            transform.position = renderQueue.Peek();
            renderQueue.Dequeue();
            RenderizeTexture();
        }
        else if(renderized && renderQueue.Count < 1)
        {
            renderized = false;
            renderizingLight = false;
        }

        previousPosition = transform.position;
    }

    public void AddRenderQueue(Vector3 renderPosition)
    {
        renderQueue.Enqueue(ManagingFunctions.RoundVector2(renderPosition) - Vector2.one * 0.5f);
    }

    public void DrawLights()
    {
        lightEcoTexture = new EcoTexture(lightDist, lightDist);
        lightEcoTexture.FillWith(Color.black);

        int index = 0;

        foreach (KeyValuePair<Vector2, float> renderLight in renderLights)
        {
            Vector2 rendLight = renderLight.Key;
            rendLight += Vector2.one * (lightDist / 2);
            float intensity = renderLight.Value;
            Color thisColorLight = lightEcoTexture.GetPixel((int)rendLight.x, (int)rendLight.y);
            bool didPoint = false;
            if (intensity > 1)
            {
                float secondIntensity = -1f;
                if (renderLights.TryGetValue(rendLight - Vector2.one, out secondIntensity))
                {
                    if (secondIntensity > 1)
                        if (renderLights.TryGetValue(rendLight - Vector2.left, out secondIntensity))
                            if (secondIntensity > 1)
                                if (renderLights.TryGetValue(rendLight - Vector2.right, out secondIntensity))
                                {
                                    lightEcoTexture.SetPixel((int)rendLight.x, (int)rendLight.y, 0, 0, 0, 1f - GameManager.gameManagerReference.dayLuminosity);
                                    didPoint = true;
                                }
                                    
                }
            }
            if(!didPoint)
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
            lightDist = 34;


        UpdateLights(lightPosition);

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
        Sprite illuminationMap = Sprite.Create(lightTexture, new Rect(0.0f, 0.0f, lightTexture.width, lightTexture.height), new Vector2(0.5f, 0.5f), 1);
        GetComponent<SpriteRenderer>().sprite = illuminationMap;
        renderized = false;
    }

    private void UpdateLights(Vector2 lightPosition)
    {
        renderLights = new Dictionary<Vector2, float>();

        if (loadedChunks.Length > 0)
        {
            List<GameObject> gameChunksToRead = new List<GameObject>();
            List<int> relative = new List<int>();
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
                //Debug.LogError(i * 16);
                relative.Add(i * 16);
            }

            int e = 0;

            foreach (GameObject loadedChunk in gameChunksToRead)
            {
                ChunkController chunkController = loadedChunk.GetComponent<ChunkController>();
                if (!chunkController.loading)
                    for (int i = 0; i < chunkController.LightMap.Length; i++)
                    {
                        if (chunkController.LightMap[i] > 0)
                        {
                            if (chunkController.TileObject[i] != null)
                                if (Vector2.Distance(chunkController.TileObject[i].transform.position, lightPosition) < lightDist + lightRadius * 2)
                                {
                                    Vector2 lightPos = (Vector3)lightPosition - (new Vector3(relative[e], 0) + chunkController.TileObject[i].transform.localPosition);
                                    renderLights.Add(lightPos, chunkController.LightMap[i]);
                                }
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
            int index = (x * height) + y;
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
            int idx = (x * height) + y;

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
