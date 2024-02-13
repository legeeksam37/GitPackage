using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AddressableBuilder
{
    public static class INAddressableObjectType
    {
        public const string DefaultDoorName = "Door";
        public const string DefaultSpawnName = "Spawn";
        public const string DefaultPlayer4DSName = "Player4DS";
        public const string DefaultPlayer2DName = "Player2D";
        public const string DefaultPlayer360Name = "Player360";

        public enum INObjectType
        {
            Door = 0,
            Spawn = 1,
            Player4DS = 2,
            Player2D = 3,
            Player360 = 4
        }

        public static void Reindex(INObjectType type)
        {
            switch(type)
            {
                case INObjectType.Player4DS:
                    INAddressablesObject[] player4DS = GameObject.FindObjectsOfType<INAddressablesPlayer4DS>(true);
                    AddIndexName(player4DS);
                    break;
                case INObjectType.Player2D:
                    INAddressablesObject[] player2D = GameObject.FindObjectsOfType<INAddressablesPlayer2D>(true);
                    AddIndexName(player2D);
                    break;
                case INObjectType.Door:
                    INAddressablesObject[] doors = GameObject.FindObjectsOfType<INAddressablesDoor>(true);
                    AddIndexName(doors);
                    break;
                case INObjectType.Spawn:
                    INAddressablesObject[] spawn = GameObject.FindObjectsOfType<INAddressablesSpawn>(true);
                    AddIndexName(spawn);
                    break;
                case INObjectType.Player360:
                    INAddressablesObject[] player360 = GameObject.FindObjectsOfType<INAddressablePlayer360>(true);
                    AddIndexName(player360);
                    break;
            }
        }

        public static void ReindexAll()
        {
            INObjectType[] enumValues = (INObjectType[])Enum.GetValues(typeof(INObjectType));

            foreach (INObjectType value in enumValues)
                Reindex(value);
        }

        private static void AddIndexName(INAddressablesObject[] objects)
        {
            if (objects == null || objects.Length == 0)
            {
                Debug.LogError("Objects not found");
                return;
            }

            for (int i = 0; i < objects.Length; i++)
            {

                // Define the regex pattern with escaped square brackets
                string pattern = @"^\[\d+\]_";

                // Create a Regex object with the pattern
                Regex regex = new Regex(pattern);

                bool containsSquareBrackets = regex.IsMatch(objects[i].name);
                if (!containsSquareBrackets)
                    objects[i].name = string.Format("[{0}]_", i) + objects[i].name;
                else
                {
                    string extractedText = Regex.Replace(objects[i].name, pattern, "");
                    objects[i].name = string.Format("[{0}]_", i) + extractedText;

                }
            }
        }
    }
}
