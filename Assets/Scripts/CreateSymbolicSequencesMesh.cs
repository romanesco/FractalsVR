using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using Pcx;

public class CreateSymbolicSequencesMesh : MonoBehaviour
{
    public int length = 6;
    public int numSymbols = 3;

    int _bits;
    int _numSymbolsInFloat;

    public int bits {
        get { return _bits;  }
        set { _bits = value; }
    }

    public int numSymbolsInFloat
    {
        get { return _numSymbolsInFloat; }
        set { _numSymbolsInFloat = value; }
    }

    PointCloudData _pointCloudData;

    // Start is called before the first frame update
    void Start()
    {
        ComputeBits();
        SetMesh();
    }

    /*
    void SetData()
    {
        _pointCloudData = CreateData();
        GetComponent<IFSSettings>().sourceData = _pointCloudData;
    }
    */


    // integer power
    // https://stackoverflow.com/questions/383587/how-do-you-do-integer-exponentiation-in-c    
    int IntPow(int x, int pow)
    {
        int ret = 1;
        while (pow != 0)
        {
            if ((pow & 1) == 1)
                ret *= x;
            x *= x;
            pow >>= 1;
        }
        return ret;
    }

    Color GetColorFromSeq(uint[] seq)
    {
        float t = 0;
        for (int i=length-1; i>=0; i--)
        {
            t += seq[i];
            t /= numSymbols;
        }

        Color c = Color.HSVToRGB(t,1,1);
        return c;
    }

    void ComputeBits()
    {
        _bits = 0;

        if (numSymbols < 2)
        {
            numSymbols = 2;
        }

        // how many bits are needed for each symbol
        int l = numSymbols - 1;
        while (l > 0)
        {
            l >>= 1;
            _bits++;
        }

        // how many symbols can be stored in float
        // _numSymbolsInFloat = sizeof(float) * 8 / _bits;
        _numSymbolsInFloat = 16 / _bits;
        Debug.LogFormat("Bits: {0}, NumSymbolsInFloat: {1}", _bits, _numSymbolsInFloat);
    }

    void SetMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter)
        {
            var oldmesh = meshFilter.sharedMesh;
            Mesh mesh = CreateMesh();
            GetComponent<MeshFilter>().mesh = mesh;
            Destroy(oldmesh);
        }
    }

    Mesh CreateMesh()
    //PointCloudData CreateData()
    {

        List<Vector3> vs = new List<Vector3>();
        //List<Vector2> uvs = new List<Vector2>();
        List<Color32> cols = new List<Color32>();

        int size = IntPow(numSymbols, length);

        uint[] seq = new uint[length];

        for (int i = 0; i < size; i++)
        {
            // create symbol sequence
            int k = i;
            int j = 0;
            for (j=length-1; j >= 0; j--)
            {
                seq[j] = (uint) (k % numSymbols);
                k /= numSymbols;
            }

            Vector3 v = new Vector3();

            // convert symbol sequence to float3
            j = 0;
            for (k = 0; k < 3; k++)
            {
                uint n = 0;
                if (j < length)
                {
                    n = seq[j++];
                    for (int l = 1; l < _numSymbolsInFloat; l++)
                    {
                        n <<= _bits;
                        if (j < length)
                        {
                            n += seq[j++];
                        }
                    }
                }                
                v[k] = math.asfloat(n);
            }

            vs.Add(v);
            // corresponding color
            cols.Add(GetColorFromSeq(seq));
        }


        /*
        var data = ScriptableObject.CreateInstance<PointCloudData>();
        data.Initialize(vs, cols);

        return data;
        */

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vs);
        //mesh.SetUVs(1, uvs);
        mesh.SetColors(cols);
        mesh.SetIndices(
                Enumerable.Range(0, vs.Count).ToArray(),
                MeshTopology.Points, 0
            );

        return mesh;
    }

    void OnValidate()
    {
        ComputeBits();

        if (Application.isPlaying)
        {
            SetMesh();
        }
        else
        {
            // reduce the size of the scene
            if (GetComponent<MeshFilter>())
            {
                GetComponent<MeshFilter>().mesh = null;
            }
        }
    }
}
