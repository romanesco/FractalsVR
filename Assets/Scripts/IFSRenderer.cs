using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pcx;
using Unity.Mathematics;

public class IFSRenderer : MonoBehaviour
{
    [SerializeField] List<Vector3> centers = new List<Vector3>() { new Vector3(0,0,0),
        new Vector3(1,0,0),
        new Vector3(0.5f, Mathf.Sqrt(3)/2, 0),
        new Vector3(0.5f, Mathf.Sqrt(3)/6, Mathf.Sqrt(2.0f/3.0f) ) };

    [SerializeField] List<float3x3> multipliers = new List<float3x3>() { 0.5f * float3x3.identity,
        0.5f * float3x3.identity,
        0.5f * float3x3.identity,
        0.5f * float3x3.identity };

    [SerializeField] bool animate = false;

    Vector3 animateVector3(float t, float a1, float a2, float r1, float b)
    {
        return (-Mathf.Cos(t * b) * r1 + r1) *
            new Vector3(Mathf.Sin(t * a1) * Mathf.Cos(t * a2), Mathf.Sin(t * a1) * Mathf.Sin(t * a2), Mathf.Cos(t * a1));
    }

    float3x3 animateMatrix3x3(float t, float a1, float a2, float a3, float r1, float d1, float b)
    {
        float s1 = Mathf.Sin(t * a1), s2 = Mathf.Sin(t * a2), s3 = Mathf.Sin(t * a3),
            c1 = Mathf.Cos(t * a1), c2 = Mathf.Cos(t * a2), c3 = Mathf.Cos(t * a3);
        Quaternion q = new Quaternion(s1 * c2, s1 * s2 * c3, s1 * s2 * s3, c1); // w is the real part
        return (Mathf.Sin(t*b)*d1+ r1) * new float3x3(q);
    }

    void Animate()
    {
        float time = Time.time*5;

        centers[0] = animateVector3(time, 0.2f, 0.3f, 0.1f, 0.1f);
        centers[1] = animateVector3(time, 0.3f, -0.15f, 0.1f, 0.1f)
            + new Vector3(1, 0, 0);
        centers[2] = animateVector3(time, -0.1f, 0.17f, 0.1f, 0.15f)            
            + new Vector3(0.5f, Mathf.Sqrt(3) / 2, 0);
        centers[3] = animateVector3(time, -0.13f, -0.24f, 0.1f, 0.06f)          
            + new Vector3(0.5f, Mathf.Sqrt(3) / 6, Mathf.Sqrt(2.0f / 3.0f));

        multipliers[0] = animateMatrix3x3(time, 0.1f, 0.1f, 0.07f, 0.5f, 0.1f, 0.05f);
        multipliers[1] = animateMatrix3x3(time, 0.1f, -0.05f, 0.02f, 0.5f, 0.08f, 0.03f);
        multipliers[2] = animateMatrix3x3(time, 0.05f, 0.08f, -0.03f, 0.5f, 0.12f, 0.07f);
        multipliers[3] = animateMatrix3x3(time, 0.07f, 0.13f, 0.07f, 0.5f, 0.07f, 0.06f);

    }

    Matrix4x4 Matrix3x3to4x4(float3x3 m)
    {
        Matrix4x4 a = Matrix4x4.zero;
        for (int i=0; i<3; i++)
        {
            for (int j=0; j<3; j++)
            {
                a[i,j] = m[i][j];
            }
        }
        return a;
    }

    Vector4[] vector4s = new Vector4[20];
    Matrix4x4[] matrix4X4s = new Matrix4x4[20];

    private void Start()
    {
        SetArrays();
    }

    private void OnValidate()
    {
        SetArrays();
    }

    void SetArrays()
    {
        //vector4s = new Vector4[centers.Count];
        for (int i = 0; i < centers.Count; i++)
        {
            vector4s[i] = centers[i];
        }

        //matrix4X4s = new Matrix4x4[multipliers.Count];
        for (int i = 0; i < multipliers.Count; i++)
        {
            matrix4X4s[i] = Matrix3x3to4x4(multipliers[i]);
        }
    }

    void Update()
    {
        if (animate)
        {
            Animate();
            SetArrays();
        }

        int n = GetComponent<CreateSymbolicSequencesMesh>().numSymbols;
        if ( (n > centers.Count) || (n > multipliers.Count) )
        {
            Debug.Log("Error: Number of centers/multipliers is less than the number of symbols in CreateSymbolicSequencesMesh.");
        }

        Material _material = GetComponent<Renderer>().sharedMaterial;

        _material.SetVectorArray("centers", vector4s);
        _material.SetMatrixArray("multipliers", matrix4X4s);
        _material.SetInt("length", GetComponent<CreateSymbolicSequencesMesh>().length);
        _material.SetInt("numSymbols", GetComponent<CreateSymbolicSequencesMesh>().numSymbols);
        _material.SetInt("bits", GetComponent<CreateSymbolicSequencesMesh>().bits);
        _material.SetInt("numSymbolsInFloat", GetComponent<CreateSymbolicSequencesMesh>().numSymbolsInFloat);
    }
}
