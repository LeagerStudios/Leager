﻿using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ManagingFunctions
{
    public static GameManager gameManager = GameManager.gameManagerReference;
    public static GameObject emptyDrop;
    public static GameObject dropContainer;
    public static int lastDropName = 0;


    public static void DropItem(int item, Vector2 dropPosition, Vector2 velocity = default, int amount = 1, float imunityGrab = 0, bool clientIsAuthored = false, string theName = "#null")
    {
        if (item > 0 && amount > 0)
        {
            if (gameManager.isNetworkClient && !clientIsAuthored)
            {
                NetworkController.networkController.DropItem(item, amount, imunityGrab, dropPosition, velocity);
            }
            else
            {
                GameObject newDrop = GameObject.Instantiate(emptyDrop, dropPosition, Quaternion.identity);
                newDrop.transform.SetParent(dropContainer.transform);
                newDrop.GetComponent<SpriteRenderer>().sprite = gameManager.GetComponent<GameManager>().tiles[item];
                newDrop.GetComponent<Rigidbody2D>().velocity = velocity;
                DroppedItemController dropped = newDrop.GetComponent<DroppedItemController>();
                dropped.item = item;
                dropped.amount = amount;
                dropped.imunityGrab = imunityGrab;
                if (theName != "#null")
                {
                    newDrop.name = theName;
                }
                else
                    newDrop.name = lastDropName++.ToString();

                if (gameManager.isNetworkHost)
                {
                    NetworkController.networkController.DropItem(item, amount, imunityGrab, dropPosition, velocity, newDrop.name);
                }
            }
        }

    }

    public static Vector2 ScreenToRectPos(Vector2 screenPos, Transform transform)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, screenPos);

        // convert the screen position to the local anchored position

        return transform.InverseTransformPoint(screenPoint);
    }

    public static int ParseBoolToInt(bool value)
    {
        if (value)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }

    public static Dictionary<string, string> CreateArgsSingle(string key, string value)
    {
        Dictionary<string, string> args = new Dictionary<string, string>
        {
            { key, value }
        };
        return args;
    }

    public static Dictionary<string, string> CreateArgs(string[] keys, string[] values)
    {
        Dictionary<string, string> args = new Dictionary<string, string>();
        for (int i = 0; i < keys.Length; i++)
            args.Add(keys[i], values[i]);
        return args;
    }

    public static bool InsideRanges(Vector2 position, Vector2 min, Vector2 max)
    {
        bool returnn = true;
        if (position.x < min.x) returnn = false;
        if (position.y < min.y) returnn = false;
        if (position.x > max.x) returnn = false;
        if (position.y > max.y) returnn = false;

        return returnn;
    }

    public static Vector2 MoveTowardsTarget(Vector2 current, Vector2 target, float velocity)
    {
        // Calculate the direction from current to target
        Vector2 direction = (target - current).normalized * velocity;

        // Calculate the distance to the target
        float distance = Vector2.Distance(current, target);

        // Move one unit towards the target, but do not overshoot
        if (distance < velocity)
        {
            return target;
        }
        else
        {
            return current + direction;
        }
    }

    public static int CreateIndex(Vector2Int position)
    {
        int x = position.x;
        if(x < 0)
        {
            x += gameManager.WorldWidth * 16;
        }
        else if(x > gameManager.WorldWidth * 16)
        {
            x -= gameManager.WorldWidth * 16;
        }

        return position.x * gameManager.WorldHeight + position.y;
    }

    public static float ClampX(float x)
    {
        return x % (gameManager.WorldWidth * 16);
    }

    public static int FindIndexInArrayOfVector2(Vector2 vectorToFind, Vector2[] list)
    {
        int index = -1;

        foreach(Vector2 element in list)
        {
            if(element == vectorToFind)
            {
                index = System.Array.IndexOf(list, element);
                break;
            }
        }

        return index;
    }

    public static DivisionResult EntireDivision(int divisor, int dividend)
    {
        DivisionResult result = new DivisionResult(0, 0, 0, 0);

        int cocient = Mathf.Abs(divisor / dividend);
        int rest = divisor % dividend;

        result = new DivisionResult(divisor, dividend, cocient, rest);

        return result;
    }

    public static string[] ConvertIntToStringArray(int[] entryArray)
    {
        string[] exitArray = new string[entryArray.Length];

        for (int i = 0; i < entryArray.Length; i++)
        {
            exitArray[i] = entryArray[i].ToString();
        }

        return exitArray;
    }

    public static int[] ConvertStringToIntArray(string[] entryArray)
    {
        int[] exitArray = new int[entryArray.Length];

        for (int i = 0; i < entryArray.Length; i++)
        {
            exitArray[i] = System.Convert.ToInt32(entryArray[i]);
        }

        return exitArray;
    }

    public static void SaveStackBarAndInventory()
    {
        StackBar.SaveStackBar();
        InventoryBar.SaveInventory();
    }

    public static float PointToPivotDown(Vector2 actual,Vector2 to)
    {
        float finalDirection = 0;
        Vector2 final = to - actual;
        //final.x = final.x * -1;
        final.y = final.y * -1;

        finalDirection = Mathf.Rad2Deg * Mathf.Atan2(final.x,final.y);

        return finalDirection;
    }

    public static float PointToPivotUp(Vector2 actual, Vector2 to)
    {
        float finalDirection = 0;
        Vector2 final = to - actual;
        final.x = final.x * -1;
        //final.y = final.y * -1;

        finalDirection = Mathf.Rad2Deg * Mathf.Atan2(final.x, final.y);

        return finalDirection;
    }


    public static float VolumeDistance(float distance, float range)
    {
        float value = 1;

        float clampedValue = Mathf.Clamp(distance, 0, range);

        clampedValue = clampedValue / range;

        value = 1f - clampedValue;

        return value;
    }

    public static string GetRandomString(int length)
    {
        string stringSpace = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        int stringLength = stringSpace.Length;
        StringBuilder randomString = new StringBuilder();

        System.Random random = new System.Random();
        for (int i = 0; i < length; i++)
        {
            int randomIndex = random.Next(0, stringLength);
            randomString.Append(stringSpace[randomIndex]);
        }

        return randomString.ToString();
    }

    public static string GetRandomStringUpper(int length)
    {
        string stringSpace = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        int stringLength = stringSpace.Length;
        StringBuilder randomString = new StringBuilder();

        System.Random random = new System.Random();
        for (int i = 0; i < length; i++)
        {
            int randomIndex = random.Next(0, stringLength);
            randomString.Append(stringSpace[randomIndex]);
        }

        return randomString.ToString();
    }


    public static string GetRandomStringUpperUnvowel(int length)
    {
        string stringSpace = "BCDFGHJKLMNPQRSTVWXYZ";
        int stringLength = stringSpace.Length;
        StringBuilder randomString = new StringBuilder();

        System.Random random = new System.Random();
        for (int i = 0; i < length; i++)
        {
            int randomIndex = random.Next(0, stringLength);
            randomString.Append(stringSpace[randomIndex]);
        }

        return randomString.ToString();
    }

    public static string GetRandomStringNumbers(int length)
    {
        string stringSpace = "0123456789";
        int stringLength = stringSpace.Length;
        StringBuilder randomString = new StringBuilder();

        System.Random random = new System.Random();
        for (int i = 0; i < length; i++)
        {
            int randomIndex = random.Next(0, stringLength);
            randomString.Append(stringSpace[randomIndex]);
        }

        return randomString.ToString();
    }

    public static Color HexToColor(string hex)
    {
        hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
        hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if (hex.Length == 8)
        {
            a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r, g, b, a);
    }

    public static string SerializeAnimatorParameters(Animator animator)
    {
        StringBuilder sb = new StringBuilder();

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            string value = "";

            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    value = animator.GetFloat(param.name).ToString();
                    break;
                case AnimatorControllerParameterType.Int:
                    value = animator.GetInteger(param.name).ToString();
                    break;
                case AnimatorControllerParameterType.Bool:
                    value = animator.GetBool(param.name) ? "1" : "0";
                    break;
                case AnimatorControllerParameterType.Trigger:
                    value = animator.GetBool(param.name) ? "1" : "0"; // Los triggers se guardan como bool
                    break;
            }

            sb.Append($"{param.name},{param.type},{value}:");
        }

        return sb.ToString();
    }

    public static void DeserializeAnimatorParameters(Animator animator, string data)
    {
        string[] parameters = data.Split(':');

        foreach (string param in parameters)
        {
            if (string.IsNullOrEmpty(param)) continue;

            string[] parts = param.Split(',');
            if (parts.Length != 3) continue;

            string name = parts[0];
            AnimatorControllerParameterType type = (AnimatorControllerParameterType)System.Enum.Parse(typeof(AnimatorControllerParameterType), parts[1]);
            string value = parts[2];

            switch (type)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(name, float.Parse(value));
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(name, int.Parse(value));
                    break;
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(name, value == "1");
                    break;
                case AnimatorControllerParameterType.Trigger:
                    if (value == "1") animator.SetTrigger(name);
                    else animator.ResetTrigger(name);
                    break;
            }
        }
    }
}

class MainFunctions : MonoBehaviour
{
    [SerializeField] GameObject dropReference;
    [SerializeField] GameObject dropContainerReference;

    private void Awake()
    {
        ManagingFunctions.emptyDrop = dropReference;
        ManagingFunctions.dropContainer = dropContainerReference;
        ManagingFunctions.gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

}

public struct DivisionResult
{
    public float divisor;
    public float dividend;
    public int cocient;
    public int rest;

    public DivisionResult(float Div, float divid, int coci, int res)
    {
        divisor = Div;
        dividend = divid;
        cocient = coci;
        rest = res;
    }

    public Vector2 ConvertToVector2()
    {
        return new Vector2(cocient, rest);
    }

}

[System.Serializable]
public class SerializableColor
{
    public float _r;
    public float _g;
    public float _b;
    public float _a;

    public SerializableColor()
    {
      
    }

    public SerializableColor(Color c)
    {
        _r = c.r;
        _g = c.g;
        _b = c.b;
        _a = c.a;
    }


    public void AssignColor(Color c)
    {
        _r = c.r;
        _g = c.g;
        _b = c.b;
        _a = c.a;
    }

    public Color GetColor()
    {
        return new Color(_r, _g, _b, _a);
    }

    public void SetColor(Color value)
    {
        _r = value.r;
        _g = value.g;
        _b = value.b;
        _a = value.a;
    }
}

//static class CallBackFunctions
//{
//    public static List<string> callbacks = new List<string>();
//    private static List<object> caller = new List<object>();
//    private static List<object> callBacker = new List<object>();

//    public static object GetAnotherObject(int indexCallback, bool isCaller)
//    {
//        object objReturn;

//        if (!isCaller)
//        {
//            objReturn = caller[indexCallback];
//        }
//        else
//        {
//            objReturn = callBacker[indexCallback];
//        }

//        return objReturn;
//    }

//    public static void AddNewCallBack(object obj1, object obj2, string callBackText)
//    {

//        callbacks.Add(callBackText);
//        caller.Add(obj1);
//        callBacker.Add(obj2);
//    }

//    public static int MyCallBackIdx(object me)
//    {
//        int idx = -1;

//        if (caller.IndexOf(me) == -1)
//        {
//            if (callBacker.IndexOf(me) == -1)
//            {
//                Debug.LogAssertion("Callback error: ");
//            }
//            else
//            {
//                idx = callBacker.IndexOf(me);
//            }
//        }
//        else
//        {
//            idx = caller.IndexOf(me);
//        }

//        return idx;
//    }
//}

//public interface ICallBack
//{
//    void CallUpdate(GameObject callerName);
//}
