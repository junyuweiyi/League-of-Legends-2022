using RaindropFX;
using System;

namespace UnityEngine.Rendering.Universal
{
    [System.Serializable, VolumeComponentMenu("CustomVolume/RainDrop")]
    public sealed class RainDrop : VolumeComponent, IPostProcessComponent
    {
        public MaterialParameter material = new MaterialParameter(null, true);
        public Texture2DParameter rainDropTex = new Texture2DParameter(null, true);
        public Vector2IntParameter texSize = new Vector2IntParameter(new Vector2Int(800, 450), true);
        public Vector2IntParameter rainDropCount = new Vector2IntParameter(new Vector2Int(200, 1), true);
        public Vector2Parameter rainDropSize = new Vector2Parameter(new Vector2(0.1f, 0.9f), true);
        public Vector2Parameter windForce = new Vector2Parameter(new Vector2(-5, 0), true);

        private RaindropGenerator _generator = new RaindropGenerator();
        private bool _initFlag = false;
        private RenderTexture _calcRainTex;
        public bool IsActive() => material.value != null && active;

        public bool IsTileCompatible()
        {
            return false;
        }

        public override void Override(VolumeComponent state, float interpFactor)
        {
            base.Override(state, interpFactor);
        }

        public RenderTexture GenerateTexture()
        { 
            if (!_initFlag)
            {
                _initFlag = true;
                Init();
            }
            _generator.CalcRainTex();
            _generator.GenerateTextures(ref _calcRainTex);
            return _calcRainTex;
        }

        void Init()
        {
            _generator.Init(rainDropTex.value, texSize.value);
            _generator.UpdateProps(false, 0.05f, true, 0.1f, 5, true, 
                rainDropCount.value.x, rainDropCount.value.y, rainDropSize.value,
                true, 0.4f, 1, windForce.value, new Vector2(0, -9.8f), 9.7f, 1, false);
        }

        public void StartRain()
        {
            _generator.FadeOut(false);
        }

        public void StopRain()
        {
            _generator.FadeOut(true);
        }

    }

    [Serializable]
    public sealed class MaterialParameter : VolumeParameter<Material>
    {
        public MaterialParameter(Material value, bool overrideState = false)
            : base(value, overrideState) { }
    }

    [Serializable]
    public sealed class Texture2DParameter : VolumeParameter<Texture2D>
    {
        public Texture2DParameter(Texture2D value, bool overrideState = false)
            : base(value, overrideState) { }
    }

    [Serializable]
    public sealed class Vector2IntParameter : VolumeParameter<Vector2Int>
    {
        public Vector2IntParameter(Vector2Int value, bool overrideState = false)
            : base(value, overrideState) { }
    }
}