using System.Collections.Generic;
using UnityEngine;
using Debug.Gizmos;
using Extensions.CSharp;

namespace Generators
{
    /// <summary>
    /// Generates objects in a two dimensional hexagonal grid pattern.
    /// </summary>
    public sealed class HexPatternGenerator : Generator
    {
        #region Inspector Fields
        [Tooltip("Apothem length of the hexagon.")]
        [SerializeField] private float apothem = 1f;
        [Tooltip("Tiles along the apothem dimension.")]
        [SerializeField] private int tilesAlongApothem = 2;
        [Tooltip("Tiles along the apothem perpendicular dimension.")]
        [SerializeField] private int tilesAlongRadius = 2;
        private void OnValidate()
        {
            apothem.LowerLimit(0f);
            tilesAlongApothem.LowerLimit(1);
            tilesAlongRadius.LowerLimit(1);
        }
        #endregion
#if UNITY_EDITOR
        #region Editor Gizmo Drawing
        private void OnDrawGizmosSelected()
        {
            // Draw empty's at the locations that the generator
            // will generate at.
            Gizmos.color = Color.yellow;
            List<Vector3> locations = GetLocations();
            foreach (Vector3 location in locations)
                GizmosHelper.DrawEmpty(location);
        }
        #endregion
#endif
        #region Accessors
        /// <summary>
        /// The length between each pattern element.
        /// </summary>
        public float Distance => apothem * 2f;
        /// <summary>
        /// Number of tiles created along the local X axis.
        /// </summary>
        public int TilesX => tilesAlongApothem;
        /// <summary>
        /// Number of tiles created along the local Z axis.
        /// </summary>
        public int TilesZ => tilesAlongRadius;
        #endregion
        #region Generate Implementation
        /// <summary>
        /// Generates the hexagonal pattern of GameObjects from the pattern template.
        /// </summary>
        public override void Generate()
        {
            // Retrieve the locations to generate at.
            List<Vector3> locations = GetLocations();
            GameObject[] generatedObjects = new GameObject[locations.Count];
            // Instantiate the GameObjects.
            for (int i = 0; i < locations.Count; i++)
            {
                generatedObjects[i] = Instantiate(patternTemplate);
                generatedObjects[i].transform.position = locations[i];
            }
            // Expose this latest generated collection.
            LastGeneratedObjects = generatedObjects;
        }
        #endregion
        #region Utility Functions
        private List<Vector3> GetLocations()
        {
            // Figure out the grid step in each direction
            // using trigonometry.
            float stepX = apothem * 2f;
            float stepZ = stepX * Mathf.Tan(30f * Mathf.Deg2Rad) * 1.5f;
            // Generate the location pattern in columns and rows.
            List<Vector3> locations = new List<Vector3>();
            for (int z = 0; z < tilesAlongRadius; z++)
            {
                for (int x = 0; x < tilesAlongApothem; x++)
                {
                    Vector3 location = transform.position;
                    location += transform.right * stepX * x
                        + transform.forward * stepZ * z;
                    // Every other row is offset by half a unit
                    // along the apothem axis.
                    if (z % 2 == 1)
                        location += transform.right * stepX * 0.5f;
                    locations.Add(location);
                }
            }
            return locations;
        }
        #endregion
    }
}
