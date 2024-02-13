using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetValidator
{

	public class MaterialInformations : IComparable<MaterialInformations>
	{
		public Material myMaterial { get; set; }

		public string materialName { get; set; }
		public string materialPath { get; set; }
		public string materialShader { get; set; }

		public List<AssetInformations> listOfUsers { get; set; }

		public List<Texture> listTextures { get; set; }
		public List<string> listTypesOfTextures { get; set; }
		public List<TextureInformations> listTextureInformations { get; set; }

		public MaterialInformations(Material mat, AssetInformations user)
		{
			myMaterial = mat;
			materialName = mat.name;

			listOfUsers = new List<AssetInformations>();
			listOfUsers.Add(user);

			listTextures = new List<Texture>();
			listTypesOfTextures = new List<string>();
			listTextureInformations = new List<TextureInformations>();
		}

		public void CheckMaterial()
		{
			if (myMaterial == null)
				return;

			materialPath = AssetDatabase.GetAssetPath(myMaterial);

			string[] list = myMaterial.GetTexturePropertyNames();
			foreach (string elem in list)
			{
				Texture texture = myMaterial.GetTexture(elem);
				if (texture != null)
				{
					listTypesOfTextures.Add(elem.Substring(1));
					listTextures.Add(texture);
				}
			}

			materialShader = myMaterial.shader.name;
		}

		public bool AlreadyExist(Material mat, AssetInformations user)
		{
			if (IsEqual(mat) && !listOfUsers.Contains(user))
			{
				NewOccurenceFind(user);
				return true;
			}
			return false;
		}

		public bool IsEqual(Material mat)
		{
			return myMaterial == mat;
		}

		public void NewOccurenceFind(AssetInformations newUser)
		{
			listOfUsers.Add(newUser);
		}

		public void AddTextureInformations(TextureInformations newTextureInfos)
		{
			listTextureInformations.Add(newTextureInfos);
		}

		public int CompareTo(MaterialInformations matInfos)
		{
			return myMaterial.name.CompareTo(matInfos.myMaterial.name);
		}
	}

}
