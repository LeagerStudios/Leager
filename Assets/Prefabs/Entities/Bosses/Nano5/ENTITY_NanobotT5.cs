using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ENTITY_NanobotT5 : MonoBehaviour
{
    public Transform head;
    public Transform pulse;
    public Transform legs;
    public GameObject leg;
    public GameObject feet;

    public int legCount = 4;
    public int legSegmentCount = 4;
    public float legLenght = -0.79f;

    void Start()
    {
        Spawn();
    }

    // Update is called once per frame
    void Update()
    {
        AIFrame();
    }

    void AIFrame()
    {

    }

    void Spawn()
    {
        BuildLegs();
    }

    void BuildLegs()
    {
        float degreesPerLeg = 360 / legCount;

        for(int i = 0; i < legCount; i++)
        {
            float direction = degreesPerLeg * i - 45f;
            float x = Mathf.Clamp(-Mathf.Sin(Mathf.Deg2Rad * direction), -0.25f, 0.25f);
            float y = Mathf.Clamp(Mathf.Cos(Mathf.Deg2Rad * direction), -0.4f, 0.4f);

            GameObject newLeg = new GameObject("Leg" + i);
            newLeg.transform.parent = legs;
            newLeg.transform.localPosition = new Vector2(x, y);
            Transform lastLeg = null;

            for(int e = 0; e < legSegmentCount; e++)
            {
                Vector2 legPlacePos = newLeg.transform.position;
                if (lastLeg != null) legPlacePos = lastLeg.GetChild(0).position;
                GameObject legSegment = Instantiate(leg, newLeg.transform, false);
                legSegment.transform.position = legPlacePos;
                legSegment.transform.eulerAngles = Vector3.forward * direction;
                legSegment.SetActive(true);
                lastLeg = legSegment.transform;
            }

            Vector2 placePos = lastLeg.GetChild(0).position;
            GameObject feetSegment = Instantiate(feet, newLeg.transform, false);
            feetSegment.transform.position = placePos;
            feetSegment.transform.eulerAngles = Vector3.forward * direction;
            feetSegment.SetActive(true);
        }
    }
}
