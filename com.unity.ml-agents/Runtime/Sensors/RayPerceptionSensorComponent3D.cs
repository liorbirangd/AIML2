using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.MLAgents.Sensors
{
    /// <summary>
    /// A component for 3D Ray Perception.
    /// </summary>
    [AddComponentMenu("ML Agents/Ray Perception Sensor 3D", (int)MenuGroup.Sensors)]
    public class RayPerceptionSensorComponent3D : RayPerceptionSensorComponentBase
    {

        private float GlanceMinAngle = -110f;
        private float GlanceMaxAngle = 110f;



        private float glanceDuration = 0f;
        private float maxGlanceDuration = 5f;
        private float timeBetweenGlances = 1f;
        private float visionAngle = 0f;

        [HideInInspector, SerializeField, FormerlySerializedAs("startVerticalOffset")]
        [Range(-10f, 10f)]
        [Tooltip("Ray start is offset up or down by this amount.")]
        float m_StartVerticalOffset;

        /// <summary>
        /// Ray start is offset up or down by this amount.
        /// </summary>
        public float StartVerticalOffset
        {
            get => m_StartVerticalOffset;
            set { m_StartVerticalOffset = value; UpdateSensor(); }
        }

        [HideInInspector, SerializeField, FormerlySerializedAs("endVerticalOffset")]
        [Range(-10f, 10f)]
        [Tooltip("Ray end is offset up or down by this amount.")]
        float m_EndVerticalOffset;

        /// <summary>
        /// Ray end is offset up or down by this amount.
        /// </summary>
        public float EndVerticalOffset
        {
            get => m_EndVerticalOffset;
            set { m_EndVerticalOffset = value; UpdateSensor(); }
        }


        /// <inheritdoc/>
        public override RayPerceptionCastType GetCastType()
        {
            return RayPerceptionCastType.Cast3D;
        }

        /// <inheritdoc/>
        public override float GetStartVerticalOffset()
        {
            return StartVerticalOffset;
        }

        /// <inheritdoc/>
        public override float GetEndVerticalOffset()
        {
            return EndVerticalOffset;
        }

        void Update()
        {
            HandleGlance();
        }

        private void HandleGlance()
        {
            
            if (timeBetweenGlances > 0f)
            {
                timeBetweenGlances -= Time.deltaTime;
                //Debug.Log($"Counting down timeBetweenGlances: {timeBetweenGlances:F2}");
                ApplyVisionAngle(0f);
                return;
            }

            
            if (glanceDuration <= 0f)
            {
                float glanceChance = Random.Range(0f, 1f);
                //Debug.Log($"Glance chance: {glanceChance:F2}");

                //glance at all
                if (glanceChance < 0.5f)
                {
                    glanceDuration = Random.Range(2f, maxGlanceDuration);

                   
                    if (Random.Range(0f, 1f) < 0.5f)
                    {
                        //glance left
                        visionAngle = Random.Range(-MaxRayDegrees,GlanceMinAngle);
                    }
                    else
                    {
                        //glance right
                        visionAngle = Random.Range(MaxRayDegrees, GlanceMaxAngle);
                    }
                    //Debug.Log($"Starting glance at angle: {visionAngle:F2}");
                }
                else
                {
                    
                    timeBetweenGlances = Random.Range(2f, 10f);
                    //Debug.Log($"No glance occurred. Resetting timeBetweenGlances to: {timeBetweenGlances:F2}");
                    ApplyVisionAngle(0f);
                    return;
                }
            }

            
            if (glanceDuration > 0f)
            {
                //Debug.Log($"Applying glance at angle: {visionAngle:F2} degrees.");
                ApplyVisionAngle(visionAngle);
                glanceDuration -= Time.deltaTime;

                
                if (glanceDuration <= 0f)
                {
                    timeBetweenGlances = Random.Range(2f, 10f);
                    //Debug.Log($"Glance ended. Resetting timeBetweenGlances to: {timeBetweenGlances:F2}");
                }
            }
            else
            {
                
                ApplyVisionAngle(0f);
            }
        }

        private void ApplyVisionAngle(float angle)
        {
            if (glanceDuration > 0f)
            {
                //apply the glance angle to the child only (raycast direction)
                transform.localRotation = Quaternion.Euler(0, angle, 0);
            }
            else
            {
                //reset the child to align with the agent's forward direction
                transform.localRotation = Quaternion.identity;
            }

            
            Vector3 visionDirection = transform.forward;
            //Debug.DrawRay(transform.position, visionDirection * 10f, Color.blue); //glance direction
            //Debug.DrawRay(transform.position, transform.parent.forward * 10f, Color.red); //normal agent looking direction
        }

    }
}
   