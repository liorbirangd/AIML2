using UnityEngine;

class TrackedTarget
{
    public DetectableTarget Detectable;
    public Vector3 RawPosition;

    public float LastSensedTime = -1f;

    public float Awareness; //0 = not aware (culled)

    //0-1 = rough idea (no location)
    //1-2 = liekly target (location)
    //2 = fully detected
    public bool UpdateAwareness(DetectableTarget target, Vector3 position, float awareness, float minAwareness)
    {
        var oldAwareness = Awareness;

        if (target != null)
            Detectable = target;
        RawPosition = position;
        LastSensedTime = Time.time;
        Awareness = Mathf.Clamp(Mathf.Max(Awareness, minAwareness) + awareness, 0f, 2f);

        if (oldAwareness < 2f && Awareness >= 2f)
            return true;
        if (oldAwareness < 1f && Awareness >= 1f)
            return true;
        if (oldAwareness <= 0f && Awareness >= 0f)
            return true;

        return false;
    }

    public bool DecayAwareness(float decayTime, float amount)
    {
        //Detected to recently
        if ((Time.time - LastSensedTime) < decayTime)
            return false;

        var oldAwareness = Awareness;

        Awareness -= amount;

        if (oldAwareness >= 2f && Awareness < 2f)
            return true;
        if (oldAwareness >= 1f && Awareness < 1f)
            return true;

        return Awareness <= 0f;
    }
}