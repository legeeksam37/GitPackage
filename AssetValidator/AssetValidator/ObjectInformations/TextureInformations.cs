using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AssetValidator
{

    public class TextureInformations : IComparable<TextureInformations>
    {
        public Texture myTexture { get; set; }

        public string textureName { get; set; }
        public string textureType { get; set; }
        public string texturePath { get; set; }
        public int maxTextureSize { get; set; }
        public int useCrunchCompression { get; set; }
        public int textureWidth { get; set; }
        public int textureHeight { get; set; }
        public int textureRealWidth { get; set; }
        public int textureRealHeight { get; set; }
        public string textureExtension { get; set; }
        public float textureLength { get; set; }

        public List<MaterialInformations> listOfUsers { get; set; }

        public TextureInformations(Texture texture, MaterialInformations user, string _textureType)
        {
            myTexture = texture;
            textureName = texture.name;
            textureType = _textureType;

            listOfUsers = new List<MaterialInformations>();
            listOfUsers.Add(user);
        }

        public void CheckTexture()
        {
            if (myTexture == null)
                return;

            texturePath = AssetDatabase.GetAssetPath(myTexture);

            TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (textureImporter != null)
            {
                maxTextureSize = textureImporter.maxTextureSize;
                useCrunchCompression = textureImporter.crunchedCompression ? 1 : 0;

                textureRealWidth = 0;
                textureRealHeight = 0;
                object[] args = new object[2] { 0, 0 };
                MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
                mi.Invoke(textureImporter, args);
                textureRealWidth = (int)args[0];
                textureRealHeight = (int)args[1];

                textureExtension = Path.GetExtension(texturePath);
            }
            else
            {
                // Default values
                maxTextureSize = 0;
                useCrunchCompression = -1; // -1 to not use (its a bool)
            }

            textureWidth = myTexture.width;
            textureHeight = myTexture.height;

            FileInfo fileInfo = new System.IO.FileInfo(texturePath);

            textureLength = (float)(fileInfo.Length / 1000000f); // pass octets to Mo
            textureLength = Mathf.Round(textureLength * 100f) / 100f; // 2 number after the comma
        }

        public bool AlreadyExist(Texture texture, MaterialInformations user)
        {
            if (IsEqual(texture) && !listOfUsers.Contains(user))
            {
                NewOccurenceFind(user);
                return true;
            }
            return false;
        }

        public bool IsEqual(Texture texture)
        {
            return myTexture == texture;
        }

        public void NewOccurenceFind(MaterialInformations newUser)
        {
            listOfUsers.Add(newUser);
        }

        public int CompareTo(TextureInformations textureInfos)
        {
            return myTexture.name.CompareTo(textureInfos.myTexture.name);
        }

    }

}
