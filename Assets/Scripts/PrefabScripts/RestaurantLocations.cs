using UnityEngine;
using Extensions.CSharp;

// TODO this is a janky hot fix!!! Please delete me and add a better
// container class for this! :(

public sealed class RestaurantLocations : MonoBehaviour
{
    [SerializeField] private Transform[] inspectionLocations = default;
    [SerializeField] private Transform[] foodPrepLocations = default;
    public Vector3[] FoodPrepLocations =>
        foodPrepLocations.ArrayInto((Transform t) => t.position);
    public Vector3[] InspectionLocations =>
        inspectionLocations.ArrayInto((Transform t) => t.position);
}
