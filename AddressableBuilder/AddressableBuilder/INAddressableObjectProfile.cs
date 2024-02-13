using UnityEngine;

namespace AddressableBuilder
{
    public abstract class INAddressableObjectProfile : ScriptableObject
    {
        public Mesh drawMesh;
        public string textureGizmosName = string.Empty;
    }
}
