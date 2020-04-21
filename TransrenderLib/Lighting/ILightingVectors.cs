using System.Numerics;

namespace Transrender.Lighting
{
    public interface ILightingVectors
    {
        Vector3 GetLightingVector(int projection);
    }
}
