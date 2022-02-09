using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pcx;
using Unity.Mathematics;

public class IFSSettings : MonoBehaviour
{
    [SerializeField] PointCloudData _sourceData = null;
    [SerializeField] ComputeShader _computeShader = null;

    [SerializeField] List<Vector3> centers = new List<Vector3>() { new Vector3(0,0,0),
        new Vector3(1,0,0),
        new Vector3(0.5f, Mathf.Sqrt(3)/2, 0),
        new Vector3(0.5f, Mathf.Sqrt(3)/6, Mathf.Sqrt(2.0f/3.0f) ) };
    [SerializeField] List<float3x3> multipliers = new List<float3x3>() { 0.5f * float3x3.identity,
        0.5f * float3x3.identity,
        0.5f * float3x3.identity,
        0.5f * float3x3.identity };

    ComputeBuffer _pointBuffer;

    public PointCloudData sourceData { get { return _sourceData; } set { _sourceData = value; } }

    void OnDisable()
    {
        if (_pointBuffer != null)
        {
            _pointBuffer.Release();
            _pointBuffer = null;
        }
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

    void Update()
    {
        if (_sourceData == null) return;

        var sourceBuffer = _sourceData.computeBuffer;

        if (_pointBuffer == null || _pointBuffer.count != sourceBuffer.count)
        {
            if (_pointBuffer != null) _pointBuffer.Release();
            _pointBuffer = new ComputeBuffer(sourceBuffer.count, PointCloudData.elementSize);
        }

        int n = GetComponent<CreateSymbolicSequencesMesh>().numSymbols;
        if ( (n > centers.Count) || (n > multipliers.Count) )
        {
            Debug.Log("Error: Number of centers/multipliers is less than the number of symbols in CreateSymbolicSequencesMesh.");
        }

        Vector4[] vector4s = new Vector4[centers.Count];
        for (int i=0; i<centers.Count; i++ )
        {
            vector4s[i] = centers[i];
        }
        Matrix4x4[] matrix4X4s = new Matrix4x4[multipliers.Count];
        for (int i=0; i<multipliers.Count; i++)
        {
            matrix4X4s[i] = Matrix3x3to4x4(multipliers[i]);
        }

        var kernel = _computeShader.FindKernel("CSMain");
        _computeShader.SetVectorArray("centers", vector4s);
        _computeShader.SetMatrixArray("multipliers", matrix4X4s);
        _computeShader.SetInt("length", GetComponent<CreateSymbolicSequencesMesh>().length);
        _computeShader.SetInt("numSymbols", GetComponent<CreateSymbolicSequencesMesh>().numSymbols);
        _computeShader.SetInt("bits", GetComponent<CreateSymbolicSequencesMesh>().bits);
        _computeShader.SetInt("numSymbolsInFloat", GetComponent<CreateSymbolicSequencesMesh>().numSymbolsInFloat);
        _computeShader.SetBuffer(kernel, "SourceBuffer", sourceBuffer);
        _computeShader.SetBuffer(kernel, "OutputBuffer", _pointBuffer);
        _computeShader.Dispatch(kernel, sourceBuffer.count / 128+1, 1, 1);

        GetComponent<PointCloudRenderer>().sourceBuffer = _pointBuffer;
    }
}
