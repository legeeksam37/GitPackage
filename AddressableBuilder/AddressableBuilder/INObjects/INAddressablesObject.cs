using UnityEngine;

namespace AddressableBuilder
{
    public abstract class INAddressablesObject : MonoBehaviour
    {
        private string baseKey = string.Empty;

        public string BaseKey
        {
            get
            {
                return baseKey;
            }
            set
            {
                if (baseKey != value)
                    baseKey = value;
            }
        }

        public abstract void Init(string key);
    }

    public class INAddressableType
    {
        public const string URL_CATALOG_KEY = "urlCatalog";
        public const string NAME_KEY = "name";
        public const string TYPE_KEY = "type";

        public string Name;
        public string UrlCatalog;
        public string Type;
    }
}
