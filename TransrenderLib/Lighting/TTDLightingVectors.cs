using System.Numerics;

namespace Transrender.Lighting
{
    public class TTDLightingVectors : ILightingVectors
    {
        public Vector3 GetLightingVector(int projection)
        {
            return _lightingVectors[projection];
        }

        private Vector3[] _lightingVectors = new[]
        {
            new Vector3 { X = 0, Y = -1, Z = -2},
            new Vector3 { X = -1, Y = -1, Z = -2},
            new Vector3 { X = -1, Y = 0, Z = -2},
            new Vector3 { X = -1, Y = 1, Z = -2},
            new Vector3 { X = 0, Y = 1, Z = -2},
            new Vector3 { X = 1, Y = 1, Z = -2},
            new Vector3 { X = 1, Y = 0, Z = -2},
            new Vector3 { X = 1, Y = -1, Z = -2}
        };
    }
}
