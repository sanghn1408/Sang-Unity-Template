using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CutoutMaskRawImage : RawImage
{
    private Material _maskMaterial;

    public override Material materialForRendering
    {
        get
        {
            if (_maskMaterial == null)
            {
                var baseMat = base.materialForRendering;
                _maskMaterial = new Material(baseMat)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                _maskMaterial.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
            }
            return _maskMaterial;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_maskMaterial != null)
        {
            DestroyImmediate(_maskMaterial);
            _maskMaterial = null;
        }
    }
}
