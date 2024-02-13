using System;
using UnityEngine;

namespace AddressableBuilder
{
    [Serializable]
    public class INAddressablesDoor : INAddressablesObject
    {
        public const string URL_IMAGE_KEY = "urlImage";

        public const string IS_ADDRESSABLE_KEY          = "AddressableBuilder";
        public const string ACTION_KEY                  = "buttonAction";
        public const string TITLE_KEY                   = "title";
        public const string POSITION_DOOR_KEY           = "positionDoor";
        public const string POSITION_PLAYER_KEY         = "positionPlayer";
        public const string DEFAULT_ACTION_KEY          = "Orange";
        public const string DEFAULT_POSITION_PLAYER_KEY = "Orange_Camera_Dummy";
        public const string CUSTOM_PREFAB_PATH          = "CustomPrefabPath";

        public string urlImage;
        public string customPrefabPath;
        [HideInInspector]
        public bool addressableBuilder = false;
        public string goToScene;
        [HideInInspector]
        public string positionDoor;
        public Title titles = new Title();
        public string targetPlayerPosition;

        public override void Init(string key)
        {
            base.BaseKey = key;
        }
    }

    [Serializable]
    public class Title
    {
        public const string FR_KEY = "FR";
        public const string EN_KEY = "EN";
        public const string ES_KEY = "ES";

        public string FR = "Entrer dans le salon";
        public string EN = "Enter the lounge";
        public string ES = "Entra en la tienda";
    }
}
