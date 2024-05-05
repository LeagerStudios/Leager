using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TechManager : MonoBehaviour, IDragHandler
{
    [Header("Main Stuff")]
    public static TechManager techTree;
    public List<int> unlockedItems = new List<int>();

    [Header("Full Color")]

    public ColorBlock fullColor;

    [Header("Full Color")]
    public ColorBlock notFullColor;
    public bool deployed = false;

    public void Awake()
    {
        techTree = this;
        deployed = false;
    }

    public void Start()
    {
        if (DataSaver.CheckIfFileExists(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/tech.lgrsd"))
        {
            int[] items = ManagingFunctions.ConvertStringToIntArray(DataSaver.LoadStats(Application.persistentDataPath + @"/worlds/" + GameObject.Find("SaveObject").GetComponent<ComponetSaver>().LoadData("worldName")[0] + @"/tech.lgrsd").SavedData);
            StartUnlocks(new List<int>(items));
        }
    }

    public void StartUnlocks(List<int> unlocks)
    {
        foreach (int unlock in unlocks)
            UnlockBlock(unlock);
    }
    
    public void UnlockBlock(int block)
    {
        TechStack[] techStacks = transform.GetComponentsInChildren<TechStack>(true);
        foreach (TechStack techStack in techStacks) techStack.UnlockedItem(block);
        unlockedItems.Add(block);
    }

    public void Update()
    {
        if ((GInput.GetKeyDown(KeyCode.Tab) || (GInput.leagerInput.platform == "Mobile" && GInput.GetKeyDown(KeyCode.Escape))) && (deployed || (!deployed && GameManager.gameManagerReference.InGame)))
        {
            deployed = !deployed;
            GameManager.gameManagerReference.InGame = !deployed;
        }
            

        GetComponent<Image>().enabled = deployed;
        GetComponent<Image>().raycastTarget = deployed;

        transform.parent.GetComponent<Image>().enabled = deployed;
        transform.parent.GetComponent<Image>().raycastTarget = deployed;

        if (deployed)
        {
            transform.parent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
             transform.localScale = Vector3.one * Mathf.Clamp(transform.localScale.x + (Input.mouseScrollDelta.y * 0.1f), 0.5f, 1.5f);
        }
        else
            transform.parent.GetComponent<RectTransform>().anchoredPosition = Vector2.left * 100000;
    }

    public void OnDrag(PointerEventData data)
    {
        GetComponent<RectTransform>().anchoredPosition += data.delta / MenuController.menuController.canvas.scaleFactor;
    }
}
