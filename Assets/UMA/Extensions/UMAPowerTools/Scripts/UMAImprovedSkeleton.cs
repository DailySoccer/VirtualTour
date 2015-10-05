using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UMA.PowerTools
{
	/// <summary>
	/// Class links UMA's bone data with Unity's transform hierarchy.
	/// </summary>
	public class UMAImprovedSkeleton : UMASkeleton
	{
		/// <summary>
		/// Internal class for storing bone and transform information.
		/// </summary>
		public class BoneDataBoneBaking : BoneData
		{
			public Quaternion rotation;
			public Vector3 position;
			public Vector3 scale;
			public int matrixFrame;
			public Quaternion grotation;
			public Vector3 gposition;
			public Vector3 gscale;
			public Matrix4x4 localToWorld;
			public bool preserved;
			
			internal void ReadUMATransform()
			{
				rotation = umaTransform.rotation;
				position = umaTransform.position;
				scale = umaTransform.scale;
			}
			internal void RestoreUMATransform()
			{
				umaTransform.rotation = rotation;
				umaTransform.position = position;
				umaTransform.scale = scale;
			}
		}

		/// <value>The bone count.</value>
		public override int boneCount { get { return boneHashData.Count; } }

		Dictionary<int, BoneDataBoneBaking> boneHashData;

		public override void EndSkeletonUpdate()
		{
			updating = false;
			foreach (var entry in boneHashData)
			{
				if (!entry.Value.preserved && entry.Value.boneTransform != null)
				{
					if( entry.Value.boneTransform.childCount > 0 )
					{
						var parent = FindPreservedParent(entry.Value.parentBoneNameHash);
						for (int i = 0; i < entry.Value.boneTransform.childCount; i++)
						{
							entry.Value.boneTransform.GetChild(i).parent = parent.boneTransform;
						}
					}
					UnityEngine.Object.Destroy(entry.Value.boneTransform.gameObject);
					entry.Value.boneTransform = null;
				}
			}

		}

		/// <summary>
		/// Initializes a new UMASkeleton from a transform hierarchy.
		/// </summary>
		/// <param name="rootBone">Root transform.</param>
		public UMAImprovedSkeleton(Transform rootBone)
		{
			rootBoneHash = UMAUtils.StringToHash(rootBone.name);
			this.boneHashData = new Dictionary<int, BoneDataBoneBaking>();
			AddBonesRecursive(rootBone);
		}

		private void AddBonesRecursive(Transform transform)
		{
			var hash = UMAUtils.StringToHash(transform.name);
			var parentHash = transform.parent != null ? UMAUtils.StringToHash(transform.parent.name) : 0;
			var umaTransform = new UMATransform(transform, hash, parentHash);
			AddBone(parentHash, hash, transform);
			for (int i = 0; i < transform.childCount; i++)
			{
				var child = transform.GetChild(i);
				AddBonesRecursive(child);
			}
		}

		/// <summary>
		/// Does this skeleton contains bone with specified name hash?
		/// </summary>
		/// <returns><c>true</c> if this instance has bone the specified name hash; otherwise, <c>false</c>.</returns>
		/// <param name="nameHash">Name hash.</param>
		public override bool HasBone(int nameHash)
		{
			return boneHashData.ContainsKey(nameHash);
		}

		/// <summary>
		/// Adds the transform into the skeleton.
		/// </summary>
		/// <param name="parentHash">Hash of parent transform name.</param>
		/// <param name="hash">Hash of transform name.</param>
		/// <param name="transform">Transform.</param>
		public override void AddBone(int parentHash, int hash, Transform transform)
		{
			var umaTransform = new UMATransform(transform, hash, parentHash);
			var newBone = new BoneDataBoneBaking()
			{
				parentBoneNameHash = parentHash,
				boneNameHash = hash,
				accessedFrame = frame,
				boneTransform = transform,
				umaTransform = umaTransform.Duplicate(),
			};
			newBone.ReadUMATransform();
			boneHashData.Add(hash, newBone);
		}

		/// <summary>
		/// Adds the transform into the skeleton.
		/// </summary>
		/// <param name="umaTransform">Transform.</param>
		public override void AddBone(UMATransform umaTransform)
		{
			var newBone = new BoneDataBoneBaking()
			{
				accessedFrame = -1,
				parentBoneNameHash = umaTransform.parent,
				boneNameHash = umaTransform.hash,
				umaTransform = umaTransform.Duplicate(),
			};
			newBone.ReadUMATransform();
			boneHashData.Add(umaTransform.hash, newBone);
		}

		/// <summary>
		/// Removes the bone with the given name hash.
		/// </summary>
		/// <param name="nameHash">Name hash.</param>
		public override void RemoveBone(int nameHash)
		{
			boneHashData.Remove(nameHash);
		}

		/// <summary>
		/// Tries to find bone transform in skeleton.
		/// </summary>
		/// <returns><c>true</c>, if transform was found, <c>false</c> otherwise.</returns>
		/// <param name="nameHash">Name hash.</param>
		/// <param name="boneTransform">Bone transform.</param>
		/// <param name="transformDirty">Transform is dirty.</param>
		/// <param name="parentBoneNameHash">Name hash of parent bone.</param>
		public override bool TryGetBoneTransform(int nameHash, out Transform boneTransform, out bool transformDirty, out int parentBoneNameHash)
		{
			BoneDataBoneBaking res;
			if (boneHashData.TryGetValue(nameHash, out res))
			{
				transformDirty = res.accessedFrame != frame;
				res.accessedFrame = frame;
				boneTransform = res.boneTransform;
				parentBoneNameHash = res.parentBoneNameHash;
				return true;
			}
			transformDirty = false;
			boneTransform = null;
			parentBoneNameHash = 0;
			return false;
		}

		/// <summary>
		/// Gets the game object for a transform in the skeleton.
		/// </summary>
		/// <returns>The game object or null, if not found.</returns>
		/// <param name="nameHash">Name hash.</param>
		public override GameObject GetBoneGameObject(int nameHash)
		{
			BoneDataBoneBaking res;
			if (boneHashData.TryGetValue(nameHash, out res))
			{
				res.accessedFrame = frame;

				return res.boneTransform == null ? null : res.boneTransform.gameObject;
			}
			return null;
		}

		protected override IEnumerable<int> GetBoneHashes()
		{
			foreach (int hash in boneHashData.Keys)
			{
				yield return hash;
			}
		}

		public override void Set(int nameHash, Vector3 position, Vector3 scale, Quaternion rotation)
		{
			BoneDataBoneBaking db;
			if (boneHashData.TryGetValue(nameHash, out db))
			{
				if (updating)
				{
					db.accessedFrame = frame;
					db.position = position;
					db.rotation = rotation;
					db.scale = scale;
				}
				else
				{
					if (db.boneTransform != null)
					{
						db.boneTransform.localPosition = position;
						db.boneTransform.localRotation = rotation;
						db.boneTransform.localScale = scale;
					}
					else
					{
						throw new Exception("Attempting to Set on non animated bone: " + db.umaTransform.name);
					}
				}
			}
			else
			{
				throw new Exception("Bone not found.");
			}
		}

		/// <summary>
		/// Sets the position of a bone.
		/// This method silently fails if the bone doesn't exist! (Desired behaviour in DNA converters due to LOD/Occlusion)
		/// </summary>
		/// <param name="nameHash">Name hash.</param>
		/// <param name="position">Position.</param>
		public override void SetPosition(int nameHash, Vector3 position)
		{
			BoneDataBoneBaking db;
			if (boneHashData.TryGetValue(nameHash, out db))
			{
				if (updating)
				{
					db.accessedFrame = frame;
					db.position = position;
				}
				else
				{
					if (db.boneTransform != null)
					{
						db.boneTransform.localPosition = position;
					}
					else
					{
						throw new Exception("Attempting to SetPosition on non animated bone: " + db.umaTransform.name);
					}
				}
			}
		}

		/// <summary>
		/// Sets the position of a bone relative to it's old position.
		/// This method silently fails if the bone doesn't exist! (Desired behaviour in DNA converters due to LOD/Occlusion)
		/// </summary>
		/// <param name="nameHash">Name hash.</param>
		/// <param name="delta">Position delta.</param>
		public override void SetPositionRelative(int nameHash, Vector3 delta)
		{
			BoneDataBoneBaking db;
			if (boneHashData.TryGetValue(nameHash, out db))
			{
				if (updating)
				{
					db.accessedFrame = frame;
					db.position = db.position + delta;
				}
				else
				{
					if (db.boneTransform != null)
					{
						db.boneTransform.localPosition = db.boneTransform.localPosition + delta;
					}
					else
					{
						throw new Exception("Attempting to SetPositionRelative on non animated bone: " + db.umaTransform.name);
					}
				}

			}
		}

		/// <summary>
		/// Sets the scale of a bone.
		/// This method silently fails if the bone doesn't exist! (Desired behaviour in DNA converters due to LOD/Occlusion)
		/// </summary>
		/// <param name="nameHash">Name hash.</param>
		/// <param name="scale">Scale.</param>
		public override void SetScale(int nameHash, Vector3 scale)
		{
			BoneDataBoneBaking db;
			if (boneHashData.TryGetValue(nameHash, out db))
			{
				if (updating)
				{
					db.accessedFrame = frame;
					db.scale = scale;
				}
				else
				{
					if (db.boneTransform != null)
					{
						db.boneTransform.localScale = scale;
					}
					else
					{
						throw new Exception("Attempting to SetScale on non animated bone: " + db.umaTransform.name);
					}
				}
			}
		}

		/// <summary>
		/// Sets the rotation of a bone.
		/// This method silently fails if the bone doesn't exist! (Desired behaviour in DNA converters due to LOD/Occlusion)
		/// </summary>
		/// <param name="nameHash">Name hash.</param>
		/// <param name="rotation">Rotation.</param>
		public override void SetRotation(int nameHash, Quaternion rotation)
		{
			BoneDataBoneBaking db;
			if (boneHashData.TryGetValue(nameHash, out db))
			{
				if (updating)
				{

					db.accessedFrame = frame;
					db.rotation = rotation;
				}
				else
				{
					if (db.boneTransform != null)
					{
						db.boneTransform.localRotation = rotation;
					}
					else
					{
						throw new Exception("Attempting to SetRotation on non animated bone: " + db.umaTransform.name);
					}
				}
			}
		}

		/// <summary>
		/// Lerp the specified bone toward a new position, rotation, and scale.
		/// This method silently fails if the bone doesn't exist! (Desired behaviour in DNA converters due to LOD/Occlusion)
		/// </summary>
		/// <param name="nameHash">Name hash.</param>
		/// <param name="position">Position.</param>
		/// <param name="scale">Scale.</param>
		/// <param name="rotation">Rotation.</param>
		/// <param name="weight">Weight.</param>
		public override void Lerp(int nameHash, Vector3 position, Vector3 scale, Quaternion rotation, float weight)
		{
			BoneDataBoneBaking db;
			if (boneHashData.TryGetValue(nameHash, out db))
			{
				if (updating)
				{
					db.accessedFrame = frame;
					db.position += position * weight;
					Quaternion fullRotation = db.rotation * rotation;
					db.rotation = Quaternion.Slerp(db.rotation, fullRotation, weight);
					db.scale = Vector3.Lerp(db.scale, scale, weight);
				}
				else
				{
					if (db.boneTransform != null)
					{
						db.boneTransform.localPosition += position * weight;
						Quaternion fullRotation = db.boneTransform.localRotation * rotation;
						db.boneTransform.localRotation = Quaternion.Slerp(db.boneTransform.localRotation, fullRotation, weight);
						db.boneTransform.localScale = Vector3.Lerp(db.boneTransform.localScale, scale, weight);
					}
					else
					{
						throw new Exception("Attempting to Lerp on non animated bone: " + db.umaTransform.name);
					}
				}
			}
		}

		/// <summary>
		/// Reset the specified transform to the default state.
		/// </summary>
		/// <param name="nameHash">Name hash.</param>
		public override bool Reset(int nameHash)
		{
			BoneDataBoneBaking db;
			if (boneHashData.TryGetValue(nameHash, out db) && (db.boneTransform != null))
			{
				if (updating)
				{
					db.accessedFrame = frame;
					db.RestoreUMATransform();
				}
				else
				{
					if (db.boneTransform != null)
					{
						db.boneTransform.localPosition = db.position;
						db.boneTransform.localRotation = db.rotation;
						db.boneTransform.localScale = db.scale;
					}
					else
					{
						throw new Exception("Attempting to Reset non animated bone: " + db.umaTransform.name);
					}
				}
				return true;
			}

			return false;
		}

		/// <summary>
		/// Reset all transforms to the default state.
		/// </summary>
		public override void ResetAll()
		{
			foreach (BoneDataBoneBaking db in boneHashData.Values)
			{
				if (updating)
				{
					db.preserved = false;
					db.accessedFrame = frame;
					db.RestoreUMATransform();
				}
				else
				{
					if (db.boneTransform != null)
					{
						db.boneTransform.localPosition = db.position;
						db.boneTransform.localRotation = db.rotation;
						db.boneTransform.localScale = db.scale;
					}
				}
			}
			if( updating )
			{
				boneHashData[rootBoneHash].preserved = true;
			}
		}

		/// <summary>
		/// Gets the position of a bone.
		/// </summary>
		/// <returns>The position.</returns>
		/// <param name="nameHash">Name hash.</param>
		public override Vector3 GetPosition(int nameHash)
		{
			BoneDataBoneBaking db;
			if (boneHashData.TryGetValue(nameHash, out db))
			{
				db.accessedFrame = frame;
				return db.position;
			}
			else
			{
				throw new Exception("Bone not found.");
			}
		}

		/// <summary>
		/// Gets the scale of a bone.
		/// </summary>
		/// <returns>The scale.</returns>
		/// <param name="nameHash">Name hash.</param>
		public override Vector3 GetScale(int nameHash)
		{
			BoneDataBoneBaking db;
			if (boneHashData.TryGetValue(nameHash, out db))
			{
				db.accessedFrame = frame;
				return db.scale;
			}
			else
			{
				throw new Exception("Bone not found.");
			}
		}

		/// <summary>
		/// Gets the rotation of a bone.
		/// </summary>
		/// <returns>The rotation.</returns>
		/// <param name="nameHash">Name hash.</param>
		public override Quaternion GetRotation(int nameHash)
		{
			BoneDataBoneBaking db;
			if (boneHashData.TryGetValue(nameHash, out db))
			{
				db.accessedFrame = frame;
				return db.rotation;
			}
			else
			{
				throw new Exception("Bone not found.");
			}
		}

		public override Transform[] HashesToTransforms(int[] boneNameHashes)
		{
			Transform[] res = new Transform[boneNameHashes.Length];
			for (int i = 0; i < boneNameHashes.Length; i++)
			{
				res[i] = FindPreservedParent(boneNameHashes[i]).boneTransform;
			}
			return res;
		}

		/// <summary>
		/// Ensures the bone exists in the skeleton.
		/// </summary>
		/// <param name="umaTransform">UMA transform.</param>
		public override void EnsureBone(UMATransform umaTransform)
		{
			Debug.Log("Ensuring Bone: " + umaTransform.name);
			BoneDataBoneBaking res;
			if (!boneHashData.TryGetValue(umaTransform.hash, out res))
			{
				AddBone(umaTransform);
				res = boneHashData[umaTransform.hash];
			}
			else
			{
				res.umaTransform.Assign(umaTransform);
			}
			res.ReadUMATransform();
		}

		/// <summary>
		/// Ensures all bones are properly initialized and parented.
		/// </summary>
		public override void EnsureBoneHierarchy()
		{
			foreach (var entry in boneHashData.Values)
			{
				if (entry.preserved)
				{
					if (entry.boneTransform == null)
					{
						var go = new GameObject(entry.umaTransform.name);
						entry.boneTransform = go.transform;
					}
				}
				entry.ReadUMATransform();
			}

			foreach (var entry in boneHashData.Values)
			{
				if (entry.preserved)
				{
					UpdateMatrix(entry);
					var parent = FindPreservedParent(entry.parentBoneNameHash);
					entry.boneTransform.parent = parent.boneTransform;
					entry.boneTransform.localRotation = Quaternion.Inverse(parent.grotation) * entry.grotation;
					entry.boneTransform.localScale = new Vector3(entry.gscale.x / parent.gscale.x, entry.gscale.y / parent.gscale.y, entry.gscale.z / parent.gscale.z);
					entry.boneTransform.localPosition = parent.localToWorld.inverse.MultiplyPoint(entry.gposition);
				}
			}
		}

		public int ResolvePreservedHash(int hash)
		{
			return FindPreservedParent(hash).boneNameHash;
		}

		private BoneDataBoneBaking FindPreservedParent(int parentHash)
		{
			BoneDataBoneBaking boneData;
			if (!boneHashData.TryGetValue(parentHash, out boneData))
			{
				// sometimes the root bone have strange names, if parent bone name is unknown it must be root.
				return boneHashData[rootBoneHash];
			}

			while (!boneData.preserved)
			{
				if (!boneHashData.TryGetValue(boneData.parentBoneNameHash, out boneData))
				{
					// sometimes the root bone have strange names, if parent bone name is unknown it must be root.
					return boneHashData[rootBoneHash];
				}
			}
			return boneData;
		}

		public virtual Matrix4x4 GetLocalToWorldMatrix(int nameHash)
		{
			BoneDataBoneBaking db;
			if (boneHashData.TryGetValue(nameHash, out db))
			{
				UpdateMatrix(db);
				return db.localToWorld;
			}
			else
			{
				throw new Exception("Bone not found.");
			}
		}

		private void UpdateMatrix(BoneDataBoneBaking db)
		{
			if (db.matrixFrame == frame) return;
			if (db.boneNameHash == rootBoneHash)
			{
				db.gposition = db.position;
				db.grotation = db.rotation;
				db.grotation = Quaternion.Euler(-90, 0, 0) * Quaternion.Euler(90, 90, 0);
				db.gscale	 = db.scale;
			}
			else
			{
				var parentdb = boneHashData[db.parentBoneNameHash];
				UpdateMatrix(parentdb);
				db.gposition = parentdb.localToWorld.MultiplyPoint(db.position);
				db.gscale = Vector3.Scale(parentdb.gscale, db.scale);
				db.grotation = parentdb.grotation * db.rotation;
			}
			db.localToWorld = Matrix4x4.TRS(db.gposition, db.grotation, db.gscale);
			db.matrixFrame = frame;
		}

		public override void SetAnimatedBone(int nameHash)
		{
			BoneDataBoneBaking db;
			if (boneHashData.TryGetValue(nameHash, out db))
			{
				db.preserved = true;
			}
			else
			{
				Debug.LogError("Bone not found! Hash: " + nameHash);
			}
		}

		public override void SetAnimatedBoneHierachy(int nameHash)
		{
			BoneDataBoneBaking db;
			if (boneHashData.TryGetValue(nameHash, out db))
			{
				if (!db.preserved)
				{
					db.preserved = true;
					SetAnimatedBoneHierachy(db.parentBoneNameHash);
				}
			}
			else
			{
				Debug.LogError("Bone not found! Hash: " + nameHash);
			}
		}

		public override Quaternion GetTPoseCorrectedRotation(int nameHash, Quaternion tPoseRotation)
		{
			BoneDataBoneBaking db;
			if (boneHashData.TryGetValue(nameHash, out db))
			{
				//bool debug = db.umaTransform.name == "Spine";
				var parentHash = ResolvePreservedHash(db.parentBoneNameHash);
				if (parentHash != db.parentBoneNameHash)
				{
					Quaternion parentOffset = Quaternion.identity;
					BoneDataBoneBaking immediateParent;
					if (boneHashData.TryGetValue(db.parentBoneNameHash, out immediateParent))
					{
						BoneDataBoneBaking parent = boneHashData[parentHash];
						parentOffset = Quaternion.Inverse(parent.grotation) * db.grotation;
						var normalRotationToTPoseRotation = Quaternion.Inverse(db.rotation) * tPoseRotation;
						var resultingRotation = normalRotationToTPoseRotation * parentOffset;
						return resultingRotation;
					}
				}
				return tPoseRotation;
			}
			else
			{
				Debug.LogError("Bone not found! Hash: " + nameHash);
				return tPoseRotation;
			}
		}

	}
}
