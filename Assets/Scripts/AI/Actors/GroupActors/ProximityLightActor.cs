using UnityEngine;

namespace AI_PROG_SP21.AI.Actors.GroupActors
{
    /// <summary>
    /// An actor that dims a light source when
    /// in the proximity of other actors.
    /// </summary>
    public sealed class ProximityLightActor : GroupActor<ProximityLightActor>
    {
        #region Inspector Fields
        [Tooltip("The light to modulate intensity on.")]
        [SerializeField] private Light proximityLight = null;
        [Tooltip("The closest distance where the light is adjusted.")]
        [SerializeField] private float minDistance = 1f;
        [Tooltip("The furthest distance where the light is adjusted.")]
        [SerializeField] private float maxDistance = 1f;
        [Tooltip("The light intensity at the nearest proximity.")]
        [SerializeField] private float minIntensity = 1f;
        [Tooltip("The light intensity at the furthest proximity.")]
        [SerializeField] private float maxIntensity = 1f;
        #endregion
        #region MonoBehaviour Implementation
        private void Update()
        {
            // Find the actor in closest proximity.
            // TODO maybe this should be a utility method
            // on the base class.
            float closestActorDistance = float.MaxValue;
            foreach (ProximityLightActor otherActor in OtherActors)
            {
                float distance = Vector3.Distance(
                    transform.position, otherActor.transform.position);
                if (distance < closestActorDistance)
                    closestActorDistance = distance;
            }
            // Set the light intensity based on actor proximity.
            // TODO should be extension method for map here.
            proximityLight.intensity = Mathf.Lerp(
                minIntensity, 
                maxIntensity, 
                Mathf.Clamp01(
                    Mathf.InverseLerp(
                        minDistance, 
                        maxDistance, 
                        closestActorDistance)));
        }
        #endregion
    }
}
