using UnityEngine;
using UnityEngine.UI;
using Klak.TestTools;
using MediaPipe.HandPose;
using System.Collections.Generic;

public sealed class HandPhysicsAnimator : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] ImageSource _source = null;
    [Space]
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] bool _useAsyncReadback = true;
    [Space]
    [SerializeField] Mesh _jointMesh = null;
    [SerializeField] Mesh _boneMesh = null;
    [Space]
    [SerializeField] Material _jointMaterial = null;
    [SerializeField] Material _boneMaterial = null;
    [Space]
    [SerializeField] RawImage _monitorUI = null;

    #endregion

    #region Private members

    HandPipeline _pipeline;
    private List<GameObject> _joints, _bones;
    static readonly (int, int)[] BonePairs =
    {
        (0, 1), (1, 2), (1, 2), (2, 3), (3, 4),     // Thumb
        (5, 6), (6, 7), (7, 8),                     // Index finger
        (9, 10), (10, 11), (11, 12),                // Middle finger
        (13, 14), (14, 15), (15, 16),               // Ring finger
        (17, 18), (18, 19), (19, 20),               // Pinky
        (0, 17), (2, 5), (5, 9), (9, 13), (13, 17)  // Palm
    };

    Matrix4x4 CalculateJointXform(Vector3 pos)
      => Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * 0.07f);

    Matrix4x4 CalculateBoneXform(Vector3 p1, Vector3 p2)
    {
        var length = Vector3.Distance(p1, p2) / 2;
        var radius = 0.03f;

        var center = (p1 + p2) / 2;
        var rotation = Quaternion.FromToRotation(Vector3.up, p2 - p1);
        var scale = new Vector3(radius, length, radius);

        return Matrix4x4.TRS(center, rotation, scale);
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        _pipeline = new HandPipeline(_resources);

        _joints = new List<GameObject>();
        _bones = new List<GameObject>();

        GameObject handBaseGO = new GameObject($"Hand");
        GameObject jointsBaseGO = new GameObject($"Joints");
        jointsBaseGO.transform.parent = handBaseGO.transform;
        GameObject bonesBaseGO = new GameObject($"Bones");
        bonesBaseGO.transform.parent = handBaseGO.transform;


        for (var i = 0; i < HandPipeline.KeyPointCount; i++)
        {
            GameObject newKeyPointGO = new GameObject($"Joint {i}");
            newKeyPointGO.transform.parent = jointsBaseGO.transform;

            MeshFilter mf = newKeyPointGO.AddComponent<MeshFilter>();
            mf.mesh = _jointMesh;
            
            MeshRenderer mr = newKeyPointGO.AddComponent<MeshRenderer>();
            mr.material = _jointMaterial;
            
            Rigidbody rb = newKeyPointGO.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            newKeyPointGO.AddComponent<SphereCollider>();

            _joints.Add(newKeyPointGO);
        }

        for (int j = 0; j < BonePairs.Length; j++)
        {
            GameObject newBoneGO = new GameObject($"Bone {j}");
            newBoneGO.transform.parent = bonesBaseGO.transform;

            MeshFilter mf = newBoneGO.AddComponent<MeshFilter>();
            MeshRenderer mr = newBoneGO.AddComponent<MeshRenderer>();
            mf.mesh = _jointMesh;
            mr.material = _boneMaterial;
            _bones.Add(newBoneGO);
        }
    }

    void OnDestroy()
    {
        _pipeline.Dispose();
    }

    void LateUpdate()
    {
        // Feed the input image to the Hand pose pipeline.
        _pipeline.UseAsyncReadback = _useAsyncReadback;
        _pipeline.ProcessImage(_source.Texture);

        for (int i = 0; i < _joints.Count; i++)
        {
            GameObject joint = _joints[i];
            var xform = CalculateJointXform(_pipeline.GetKeyPoint(i));
            joint.transform.position = xform.ExtractPosition();
            joint.transform.rotation = xform.ExtractRotation();
            joint.transform.localScale = xform.ExtractScale();
        }

        for (int j = 0; j < BonePairs.Length; j++)
        {
            (int, int) pair = BonePairs[j];
            var p1 = _pipeline.GetKeyPoint(pair.Item1);
            var p2 = _pipeline.GetKeyPoint(pair.Item2);
            var xform = CalculateBoneXform(p1, p2);
            GameObject bone = _bones[j];
            bone.transform.position = xform.ExtractPosition();
            bone.transform.rotation = xform.ExtractRotation();
            bone.transform.localScale = xform.ExtractScale();
        }

        // UI update
        _monitorUI.texture = _source.Texture;
    }

    #endregion
}