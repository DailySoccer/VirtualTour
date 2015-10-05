using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UMA.PowerTools
{
	/// <summary>
	/// The Bone Baking mesh combiner from the UMA Power Tools third party package.
	/// </summary>
    public class UMABoneBakingMeshCombiner : UMAMeshCombiner
    {
        protected List<Material> combinedMaterialList;
		UMAImprovedSkeleton umaSkeleton;
		Matrix4x4[] inverseResolvedBoneMatrixes;
		UMAMeshData umaMesh;

        UMAData umaData;
        int atlasResolution;
		int animatedBonesCount;

		protected void EnsureUMADataSetup(bool updatedAtlas)
		{
			if (umaData.umaRoot == null)
			{
				GameObject newRoot = new GameObject("Root");
				newRoot.transform.parent = umaData.transform;
				newRoot.transform.localPosition = Vector3.zero;
				newRoot.transform.localRotation = Quaternion.Euler(270f, 0, 0f);
				umaData.umaRoot = newRoot;

				GameObject newGlobal = new GameObject("Global");
				newGlobal.transform.parent = newRoot.transform;
				newGlobal.transform.localPosition = Vector3.zero;
				newGlobal.transform.localRotation = Quaternion.Euler(90f, 90f, 0f);

				umaSkeleton = new UMAImprovedSkeleton(newGlobal.transform);
				umaData.skeleton = umaSkeleton;

				var newRenderer = umaData.umaRoot.AddComponent<SkinnedMeshRenderer>();
				newRenderer.rootBone = newGlobal.transform;
				umaData.myRenderer = newRenderer;
				umaData.myRenderer.enabled = false;
				umaData.myRenderer.sharedMesh = new Mesh();
			}
			else
			{
				if (updatedAtlas)
				{
					umaData.CleanMesh(false);
				}
				umaSkeleton = umaData.skeleton as UMAImprovedSkeleton;
			}
		}


		public override void Preprocess(UMAData umaData)
		{
			umaData.isMeshDirty |= umaData.isShapeDirty;
		}

		/// <summary>
		/// Updates the UMA mesh and skeleton to match current slots.
		/// </summary>
		/// <param name="updatedAtlas">If set to <c>true</c> atlas has changed.</param>
		/// <param name="umaData">UMA data.</param>
		/// <param name="atlasResolution">Atlas resolution.</param>
        public override void UpdateUMAMesh(bool updatedAtlas, UMAData umaData, int atlasResolution)
        {
			this.umaData = umaData;
            this.atlasResolution = atlasResolution;

            combinedMaterialList = new List<Material>();

			umaData.ResetAnimatedBones();
            var combinedMeshArray = BuildCombineInstances();

			EnsureUMADataSetup(updatedAtlas);
			umaData.skeleton.BeginSkeletonUpdate();

			umaMesh = new UMAMeshData();
			umaMesh.ClaimSharedBuffers();

			PopulateSkeleton(combinedMeshArray);

			umaData.umaRecipe.ClearDNAConverters();
			for (int i = 0; i < umaData.umaRecipe.slotDataList.Length; i++)
			{
				SlotData slotData = umaData.umaRecipe.slotDataList[i];
				if (slotData != null)
				{
					umaData.umaRecipe.AddDNAUpdater(slotData.asset.slotDNA);
				}
			}
			umaData.skeleton.ResetAll();
			AddHumanoidBones();
			MarkAnimatedBones();
			umaData.ApplyDNA();
			umaData.FireDNAAppliedEvents();

			MergeSkeletons(combinedMeshArray);
			PopulateMatrix(combinedMeshArray);

			SkinnedMeshCombinerRetargeting.CombineMeshes(umaMesh, combinedMeshArray, inverseResolvedBoneMatrixes);

			RecalculateUV();

			umaMesh.ApplyDataToUnityMesh(umaData.myRenderer, umaData.skeleton);
			umaMesh.ReleaseSharedBuffers();
			umaData.skeleton.EndSkeletonUpdate();

            umaData.myRenderer.quality = SkinQuality.Bone4;
            //umaData.myRenderer.useLightProbes = true;
			if (updatedAtlas)
			{
				var materials = combinedMaterialList.ToArray();
				umaData.myRenderer.sharedMaterials = materials;
			}
            //umaData.myRenderer.sharedMesh.RecalculateBounds();
            umaData.myRenderer.sharedMesh.name = "UMAMesh";

			umaData.isShapeDirty = false;
            umaData.firstBake = false;

			umaData.umaGenerator.UpdateAvatar(umaData);
			//FireSlotAtlasNotification(umaData, materials);
        }

		private void AddHumanoidBones()
		{
			var tpose = umaData.umaRecipe.raceData.TPose;
			if (tpose != null)
			{
				tpose.DeSerialize();
				for (int i = 0; i < tpose.humanInfo.Length; i++)
				{
					var bone = tpose.humanInfo[i];
					var hash = UMAUtils.StringToHash(bone.boneName);
					umaData.RegisterAnimatedBone(hash);
				}
			}
		}

		private void MarkAnimatedBones()
		{
			var animatedBones = umaData.GetAnimatedBones();
			animatedBonesCount = animatedBones.Length;
			foreach (var animatedBone in animatedBones)
			{
				umaSkeleton.SetAnimatedBone(animatedBone);
			}
		}

		private void MergeSkeletons(SkinnedMeshCombinerRetargeting.CombineInstance[] combinedInstances)
		{
			var mergedBones = new Dictionary<int, int>(animatedBonesCount);
			foreach (var combineInstance in combinedInstances)
			{
				var meshData = combineInstance.meshData;
				combineInstance.targetBoneIndices = new int[meshData.boneNameHashes.Length];
				for (int i = 0; i < meshData.boneNameHashes.Length; i++)
				{
					var targetHash = umaSkeleton.ResolvePreservedHash(meshData.boneNameHashes[i]);
					int targetIndex;
					if (!mergedBones.TryGetValue(targetHash, out targetIndex))
					{
						targetIndex = mergedBones.Count;
						mergedBones.Add(targetHash, targetIndex);
					}
					combineInstance.targetBoneIndices[i] = targetIndex;
					if (!mergedBones.ContainsKey(targetHash))
					{
						mergedBones.Add(targetHash, mergedBones.Count);
					}
				}				
			}
			umaMesh.boneNameHashes = new int[mergedBones.Count];
			foreach (var entry in mergedBones)
			{
				umaMesh.boneNameHashes[entry.Value] = entry.Key;
			}
		}


		private void PopulateMatrix(SkinnedMeshCombinerRetargeting.CombineInstance[] combinedInstances)
		{
			foreach (var combineInstance in combinedInstances)
			{
				var meshData = combineInstance.meshData;
				combineInstance.resolvedBoneMatrixes = new Matrix4x4[meshData.boneNameHashes.Length];
				for(int i = 0; i < meshData.boneNameHashes.Length; i++)
				{
					var boneNameHash = meshData.boneNameHashes[i];
					var boneMatrix = umaSkeleton.GetLocalToWorldMatrix(boneNameHash);
					combineInstance.resolvedBoneMatrixes[i] = boneMatrix * meshData.bindPoses[i];
				}
			}

			inverseResolvedBoneMatrixes = new Matrix4x4[umaMesh.boneNameHashes.Length];
			umaMesh.bindPoses = new Matrix4x4[umaMesh.boneNameHashes.Length];
			var rootMatrix = umaSkeleton.GetLocalToWorldMatrix(umaSkeleton.rootBoneHash);
			for (int i = 0; i < umaMesh.boneNameHashes.Length; i++)
			{
				var boneMatrix = umaSkeleton.GetLocalToWorldMatrix(umaMesh.boneNameHashes[i]);
				umaMesh.bindPoses[i] = boneMatrix.inverse * rootMatrix;
				inverseResolvedBoneMatrixes[i] = (boneMatrix * umaMesh.bindPoses[i]).inverse;
			}
		}

		private void PopulateSkeleton(SkinnedMeshCombinerRetargeting.CombineInstance[] combinedInstances)
		{
			foreach (var combineInstance in combinedInstances)
			{
				var meshData = combineInstance.meshData;
				for (int i = 0; i < meshData.umaBoneCount; i++)
				{
					var umaBone = meshData.umaBones[i];
					if (!umaSkeleton.HasBone(umaBone.hash))
					{
						umaSkeleton.AddBone(umaBone);
					}
				}
			}
		}


		//private void FireSlotAtlasNotification(UMAData umaData, Material[] materials)
		//{
		//    for (int atlasIndex = 0; atlasIndex < umaData.atlasList.atlas.Count; atlasIndex++)
		//    {
		//        for (int materialDefinitionIndex = 0; materialDefinitionIndex < umaData.atlasList.atlas[atlasIndex].atlasMaterialDefinitions.Count; materialDefinitionIndex++)
		//        {
		//            var materialDefinition = umaData.atlasList.atlas[atlasIndex].atlasMaterialDefinitions[materialDefinitionIndex];
		//            var slotData = materialDefinition.source.slotData;
		//            if (slotData.SlotAtlassed != null)
		//            {
		//                slotData.SlotAtlassed.Invoke(umaData, slotData, materials[atlasIndex], materialDefinition.atlasRegion);
		//            }
		//        }
		//    }
		//    SlotData[] slots = umaData.umaRecipe.slotDataList;
		//    for (int slotIndex = 0; slotIndex < slots.Length; slotIndex++)
		//    {
		//        var slotData = slots[slotIndex];
		//        if (slotData == null) continue;
		//        if (slotData.textureNameList.Length == 1 && string.IsNullOrEmpty(slotData.textureNameList[0]))
		//        {
		//            if (slotData.SlotAtlassed != null)
		//            {
		//                slotData.SlotAtlassed.Invoke(umaData, slotData, materials[atlasIndex], materialDefinition.atlasRegion);
		//            }
		//        }
		//    }
		//}

        protected SkinnedMeshCombinerRetargeting.CombineInstance[] BuildCombineInstances()
        {
			var combinedMeshList = new List<SkinnedMeshCombinerRetargeting.CombineInstance>();

			SkinnedMeshCombinerRetargeting.CombineInstance combineInstance;

            for (int materialIndex = 0; materialIndex < umaData.generatedMaterials.materials.Count; materialIndex++)
            {
				var generatedMaterial = umaData.generatedMaterials.materials[materialIndex];
				combinedMaterialList.Add(generatedMaterial.material);

				for (int materialDefinitionIndex = 0; materialDefinitionIndex < generatedMaterial.materialFragments.Count; materialDefinitionIndex++)
                {
					var materialDefinition = generatedMaterial.materialFragments[materialDefinitionIndex];
					var slotData = materialDefinition.slotData;
					combineInstance = new SkinnedMeshCombinerRetargeting.CombineInstance();
					combineInstance.meshData = slotData.asset.meshData;
					foreach(var boneHash in slotData.asset.animatedBoneHashes)
					{
						umaData.RegisterAnimatedBone(boneHash);
					}
					combineInstance.targetSubmeshIndices = new int[combineInstance.meshData.subMeshCount];
					for (int i = 0; i < combineInstance.meshData.subMeshCount; i++)
					{
						combineInstance.targetSubmeshIndices[i] = -1;
					}
					combineInstance.targetSubmeshIndices[slotData.asset.subMeshIndex] = materialIndex;
                    combinedMeshList.Add(combineInstance);

					if (slotData.asset.SlotAtlassed != null)
					{
						slotData.asset.SlotAtlassed.Invoke(umaData, slotData, generatedMaterial.material, materialDefinition.atlasRegion);
					}
                }
            }
			return combinedMeshList.ToArray();
        }

		protected void RecalculateUV()
        {
            int idx = 0;
            //Handle Atlassed Verts
            for (int materialIndex = 0; materialIndex < umaData.generatedMaterials.materials.Count; materialIndex++)
            {
				var generatedMaterial = umaData.generatedMaterials.materials[materialIndex];
				if (generatedMaterial.umaMaterial.materialType != UMAMaterial.MaterialType.Atlas) continue;

				for (int materialDefinitionIndex = 0; materialDefinitionIndex < generatedMaterial.materialFragments.Count; materialDefinitionIndex++)
                {
					var fragment = generatedMaterial.materialFragments[materialDefinitionIndex];
					var tempAtlasRect = fragment.atlasRegion;
					int vertexCount = fragment.slotData.asset.meshData.vertices.Length;
					float atlasXMin = tempAtlasRect.xMin / atlasResolution;
					float atlasXMax = tempAtlasRect.xMax / atlasResolution;
					float atlasXRange = atlasXMax - atlasXMin;
					float atlasYMin = tempAtlasRect.yMin / atlasResolution;
					float atlasYMax = tempAtlasRect.yMax / atlasResolution;
					float atlasYRange = atlasYMax - atlasYMin;
					while (vertexCount-- > 0)
                    {
						umaMesh.uv[idx].x = atlasXMin + atlasXRange * umaMesh.uv[idx].x;
						umaMesh.uv[idx].y = atlasYMin + atlasYRange * umaMesh.uv[idx].y;
						idx++;
                    }

                }
            }
        }
	}
}
