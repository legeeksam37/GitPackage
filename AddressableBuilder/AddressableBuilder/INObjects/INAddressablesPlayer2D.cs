using System;
using UnityEngine;

namespace AddressableBuilder
{
    [Serializable]
    public class INAddressablesPlayer2D : INAddressablesObject
    {
        public const string URL_VIDEO_KEY = "urlVideo";
        public const string DISTANCE_TRIGGER = "activeByDistance";
        public const string POSITION_KEY = "position";
        public const string TITLE_KEY = "title";

        public bool isTrigger;
        public string urlVideo;
        public Title titles = new Title();
        
        [HideInInspector]
        public string position;

        public override void Init(string key)
        {
            base.BaseKey = key;
        }
    }
}
