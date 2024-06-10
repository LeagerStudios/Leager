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
    public float legLength = 0.79f;

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
        for(int i = 0; i < legs.childCount; i++)
        {
            Transform leg = legs.GetChild(i);
            Transform foot = leg.GetChild(legSegmentCount);

            Vector2 startPosition = head.transform.position + leg.transform.localPosition;
            Vector2 footLocalPosition = (Vector2)foot.transform.position - startPosition;
            footLocalPosition = Vector2.ClampMagnitude(footLocalPosition, legSegmentCount * legLength);
            foot.transform.position = footLocalPosition + startPosition;
            Vector3 dir = Vector3.forward * ManagingFunctions.PointToPivotDown(foot.transform.position, startPosition);
            foot.eulerAngles = dir;
            float juan2 = Vector2.Distance(startPosition, foot.transform.position);

            for (int e = leg.childCount - 1; e >= 0; e--)
            {
                Transform indexSegment = leg.GetChild(e);

                if(e != legSegmentCount)
                {
                    float juan = 1f / legSegmentCount * e;
                    indexSegment.transform.position = Vector2.Lerp(startPosition, foot.transform.position, juan);
                    indexSegment.eulerAngles = dir;
                    indexSegment.localScale = new Vector3(1, juan2 / (legSegmentCount * legLength), 1);
                }
            }
        }
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
            float x = Mathf.Clamp(-Mathf.Sin(Mathf.Deg2Rad * direction), -0.2f, 0.2f);
            float y = Mathf.Clamp(Mathf.Cos(Mathf.Deg2Rad * direction), -0.35f, 0.35f);

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
