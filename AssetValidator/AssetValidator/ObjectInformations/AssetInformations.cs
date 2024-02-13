using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetValidator
{

	public class AssetInformations : IComparable<AssetInformations>
	{
		public GameObject myAsset { get; set; }

		private Mesh mesh;

		public string assetName { get; set; }
		public string meshPath { get; set; }
		public GameObject prefabParent { get; set; }
		public string prefabName { get; set; }
		public string prefabPath { get; set; }
		public int vertexCount { get; set; }
		public int trianglesCount { get; set; }
		public int submeshCount { get; set; }
		public int materialsCount { get; set; }

		public bool meshFound { get; set; }
		public bool rendererFound { get; set; }

		public List<Material> listMaterials { get; set; }
		public List<MaterialInformations> listMaterialInformations { get; set; }

		public AssetInformations(GameObject obj, Mesh _mesh)
		{
			myAsset = obj;
			mesh = _mesh;

			listMaterials = new List<Material>();
			listMaterialInformations = new List<MaterialInformations>();
		}

		public void CheckAsset()
		{
			if (myAsset == null)
				return;

			assetName = myAsset.name;

			vertexCount = mesh.vertexCount;
			trianglesCount = mesh.triangles.Length / 3;
			submeshCount = mesh.subMeshCount;

			meshPath = AssetDatabase.GetAssetPath(mesh);
			meshFound = true;

			if (myAsset.TryGetComponent(out Renderer renderer))
			{
				rendererFound = true;

				Material[] materials = renderer.sharedMaterials;
				if (materials != null)
				{
					materialsCount = materials.Length;

					foreach (Material mat in materials)
					{
						if (mat == null)
							break;

						listMaterials.Add(mat);
					}
				}
			}

			//FindPrefabParentRecursive(myAsset.transform);
			prefabParent = PrefabUtility.GetNearestPrefabInstanceRoot(myAsset.gameObject);
			if (prefabParent != null)
			{
				prefabName = prefabParent.name;
				GameObject originPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefabParent);
				prefabPath = AssetDatabase.GetAssetPath(originPrefab);
			}
		}

		private void FindPrefabParentRecursive(Transform transform)
		{
			if (transform == null)
				return;

			//Transform parent = PrefabUtility.GetCorrespondingObjectFromOriginalSource(transform);
			PrefabInstanceStatus prefabInstance = PrefabUtility.GetPrefabInstanceStatus(transform);
			if (prefabInstance == PrefabInstanceStatus.NotAPrefab)
				FindPrefabParentRecursive(transform.parent);
			else
			{
				prefabParent = transform.gameObject;
				prefabName = prefabParent.name;
				prefabPath = AssetDatabase.GetAssetPath(transform.gameObject);
			}


		}

		public void AddMaterialInformations(MaterialInformations newMatInfos)
		{
			listMaterialInformations.Add(newMatInfos);
		}

		public int CompareTo(AssetInformations assetInformations)
		{
			return assetName.CompareTo(assetInformations.assetName);
		}

	}

}
