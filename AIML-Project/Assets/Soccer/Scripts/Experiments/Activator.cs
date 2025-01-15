using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activator : MonoBehaviour
{
    public GameObject blueStriker1;
    public GameObject blueStriker2;
    public GameObject purpleStriker1;
    public GameObject purpleStriker2;
    public GameObject targetManager;
    public GameObject hearingManager;
    private string on = "on";
    int hearingSensor = 1;
    int awarenessSystem = 2;

    public void ActivateAllAwarenessSystem(string activate)
    {
        bool setActive = activate.Equals(on);
        ActivateAllStrikersChildObject(setActive, awarenessSystem);
        targetManager.SetActive(setActive);
    }

    public void ActivateAllHearingSensors(string activate)
    {
        bool setActive = activate.Equals(on);
        ActivateAllStrikersChildObject(setActive, hearingSensor);
        hearingManager.SetActive(setActive);
    }

    private void ActivateAllStrikersChildObject(bool activate, int child)
    {
        ActivateChildObject(blueStriker1, activate, child);
        ActivateChildObject(blueStriker2, activate, child);
        ActivateChildObject(purpleStriker1, activate, child);
        ActivateChildObject(purpleStriker2, activate, child);
    }

    public void ActivateBlueStriker1Sensors(string activateHearing, string activateAwareness)
    {
        ActivateStrikerSensors(blueStriker1, activateHearing, activateAwareness);
    }

    public void ActivateBlueStriker2Sensors(string activateHearing, string activateAwareness)
    {
        ActivateStrikerSensors(blueStriker2, activateHearing, activateAwareness);
    }

    public void ActivatePurpleStriker1Sensors(string activateHearing, string activateAwareness)
    {
        ActivateStrikerSensors(purpleStriker1, activateHearing, activateAwareness);
    }

    public void ActivatePurpleStriker2Sensors(string activateHearing, string activateAwareness)
    {
        ActivateStrikerSensors(purpleStriker2, activateHearing, activateAwareness);
    }

    public void ActivateStrikerSensors(GameObject striker, string activateHearing, string activateAwareness)
    {
        if (activateHearing != null) ActivateChildObject(striker, activateHearing.Equals(on), hearingSensor);
        if (activateAwareness != null) ActivateChildObject(striker, activateAwareness.Equals(on), awarenessSystem);
    }


    private void ActivateChildObject(GameObject parent, bool  activate, int child)
    {
        GameObject targetObject = parent.transform.GetChild(child).gameObject;
        targetObject.SetActive(activate);
        //TestActive(targetObject);
    }

    private void TestActive(GameObject targetObject)
    {
        Debug.Log(targetObject.activeSelf);
    }


}
