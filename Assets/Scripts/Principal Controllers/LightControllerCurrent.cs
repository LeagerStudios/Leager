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
    public float[,] renderLights;
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
            UpdateLight(renderQueue.Peek());
        }


        if (renderized && renderQueue.Count > 0)
        {
            transform.position = renderQueue.Peek();
            renderQueue = new Queue<Vector3>();
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

        foreach (float renderLight in renderLights)
        {
            //Vector2Int rendLight = ManagingFunctions.EntireDivision(index, 1/*???*/).ConvertToVector2();
            //rendLight += Vector2Int.one * (lightDist / 2);
            //float intensity = renderLights[index];

            //DrawOn("light", rendLight, intensity);
            //index++;

            //Now this is 100% broken :)
        }
        renderized = true;
        renderizingLight = false;
    }

    private void DrawOn(string toDraw, Vector2Int rendLight, float intensity)
    {
        if (toDraw == "light")
        {
            for (int nx = rendLight.x - lightRadius; nx < rendLight.x + lightRadius + 1; nx++)
            {
                for (int ny = rendLight.y - lightRadius; ny < rendLight.y + lightRadius + 1; ny++)
                {
                    float dist = Vector2.Distance(new Vector2(nx, ny), new Vector2(rendLight.x, rendLight.y));
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
        }
        else if(toDraw == "dot")
        {
            Color gettedColor = lightEcoTexture.GetPixel(rendLight.x, rendLight.y);
            float alphaIntensity = 1 - intensity;

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
                    if (intensity > 1) lightEcoTexture.SetPixel(rendLight.x, rendLight.y, 0, 0, 0, 1f - GameManager.gameManagerReference.dayLuminosity);
                    else lightEcoTexture.SetPixel(rendLight.x, rendLight.y, 0, 0, 0, 1f - intensity);
                }
            }
        }
    }

    private void UpdateLight(Vector3 lightPosition)
    {
        renderizingLight = true;
        if (MenuController.menuController.miniMapOn)
            lightDist = (int)minimapCamera.orthographicSize * 2 + (lightRadius * 3);
        else
            lightDist = 32;


        UpdateLights(lightPosition);

        //ThreadStart lightDrawRef = new ThreadStart(DrawLights);
        //Thread lightRender = new Thread(lightDrawRef);
        //lightRender.Start();
        DrawLights();
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
        renderLights = new float[lightDist, lightDist];

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
                relative.Add(i * 16);
            }

            int e = 0;

            foreach (GameObject loadedChunk in gameChunksToRead)
            {
                ChunkController chunkController = loadedChunk.GetComponent<ChunkController>();

                for (int i = 0; i < chunkController.LightMap.Length; i++)
                {
                    if (chunkController.LightMap[i] > 0)
                    {
                        Vector2 lightPos = (Vector3)lightPosition - (new Vector3(relative[e], 0) + chunkController.TileObject[i].transform.localPosition);
                        renderLights[(int)lightPos.x, (int)lightPos.y] = chunkController.LightMap[i];
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
