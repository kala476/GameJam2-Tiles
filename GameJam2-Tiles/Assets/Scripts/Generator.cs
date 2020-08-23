using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Generator : MonoBehaviour
{
    public SolidCollection solidCollection;
    public Material tileMaterial;
    public bool generate;

    private void OnValidate()
    {
        if (solidCollection != null && generate)
        {
            generate = false;
            Transform collection = solidCollection.Generate(tileMaterial).transform;
            collection.parent = transform;
            collection.localPosition = Vector3.zero;
        }
    }
}
