using UnityEngine;

class TrackedTarget
{
    public DetectableTarget Detectable;
    public Vector3 RawPosition;

    public float lastSensedTime;

    public float awareness; //0 = not aware (culled)

    public TrackedTarget(DetectableTarget detectable, Vector3 rawPosition)
    {
        Detectable = detectable;
        RawPosition = rawPosition;
        lastSensedTime = -1f;
        awareness = 0;
    }

    //0-1 = rough idea (no location)
    //1-2 = liekly target (location)
    //2 = fully detected
    public void UpdateAwareness(DetectableTarget target, Vector3 position, float awareness, float minAwareness)
    {
        var oldAwareness = this.awareness;

        if (target != null)
            Detectable = target;
        if (awareness >= 1)
            RawPosition = position;
        lastSensedTime = Time.time;
        this.awareness = Mathf.Clamp(Mathf.Max(this.awareness, minAwareness) + awareness, 0f, 2f);
    }

    public void DecayAwareness(float decayTime, float amount)
    {
        //Detected to recently
        if ((Time.time - lastSensedTime) < decayTime)
            return;
        awareness -= amount;
    }
}