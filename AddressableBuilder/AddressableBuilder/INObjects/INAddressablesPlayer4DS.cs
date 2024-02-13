using System;
using UnityEngine;

namespace AddressableBuilder
{
    [Serializable]
    public class INAddressablesPlayer4DS : INAddressablesObject
    {
       public const string  URL_VIDEO_KEY = "urlVideo";
       public const string  POSITION_KEY = "position";

        public string urlVideo;
        [HideInInspector]
        public string position;

        public override void Init(string key)
        {
            base.BaseKey = key;
        }    
    }
}
