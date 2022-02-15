using UnityEngine;
using System.Collections.Generic;

namespace RaindropFX 
{
    public class RaindropGenerator 
    {
        #region parameters

        public bool fadeOut = false;
        public bool fastMode = false;
        public float fadeSpeed = 0.01f;

        public Texture2D raindropTex_alpha;
        public Texture2D cullMask_grayscale;
        public Vector2Int raindropTexSize;
        public Texture2D calcRainTex;
        public Vector2Int calcTexSize;

        public bool forceRainTextureSize = true;
        public float calcTimeStep = 0.1f;
        public int refreshRate = 1;
        public bool generateTrail = true;
        public int maxStaticRaindropNumber = 5000;
        public int maxDynamicRaindropNumber = 10;
        public Vector2 raindropSizeRange = new Vector2(0.1f, 0.25f);
        public float maxRandomDynamicForce = 3.5f;

        public bool useWind = false;
        public bool radialWind = false;
        public float windTurbulence = 0.1f;
        public float windTurbScale = 1.0f;
        public Vector2 wind = new Vector2(0.0f, 0.0f);
        public Vector2 gravity = new Vector2(0.0f, -9.8f);
        public float friction = 0.8f;

        public int edgeSoftness = 1;

        public Vector2 fadeSpeedRange = new Vector2(0.3f, 0.9f);
        public Vector2 shrinkSpeed = new Vector2(0.01f, 0.02f);
        public Vector2 loseWeightRange = new Vector2(0.01f, 0.02f);
        public float killSize = 0.01f;
        
        public Material blur_material;
        public Material normal_material;

        public List<Raindrop_PPV> staticRaindrops;
        public List<Raindrop_PPV> dynamicRaindrops;
        public CachePool_PPV cachePool;

        private Texture2D _mask;
        private bool[][] _rainTexColorFlags;
        private int _staticUpdateCounter = 0;
        private int _genClock = 0;
        #endregion

        public void Init(Texture2D raindropTex_alpha, Vector2Int calcRainTextureSize) 
        {
            cachePool = new CachePool_PPV();
            cachePool.Init();

            if (forceRainTextureSize) calcTexSize = new Vector2Int(calcRainTextureSize.x, calcRainTextureSize.y);
            else calcTexSize = RaindropFX_Tools.GetViewSize();
            this.raindropTex_alpha = raindropTex_alpha;
            raindropTexSize = new Vector2Int(raindropTex_alpha.width, raindropTex_alpha.height);
            generateBaseMap();
            staticRaindrops = new List<Raindrop_PPV>();
            dynamicRaindrops = new List<Raindrop_PPV>();
            PreparRainTexFlags();

            if (blur_material == null)
            {
                blur_material = new Material(Shader.Find("Hidden/Custom/GaussianBlur"));
            }
            if (normal_material == null)
            {
                normal_material = new Material(Shader.Find("Hidden/Custom/HeightToNormal"));
            }
        }

        public void UpdateProps(bool fastMode, float fadeSpeed, bool forceRainTextureSize,
                               float calcTimeStep, int refreshRate, bool generateTrail, int maxStaticRaindropNumber,
                               int maxDynamicRaindropNumber, Vector2 raindropSizeRange, bool useWind, float windTurbulence,
                               float windTurbScale, Vector2 wind, Vector2 gravity, float friction, int edgeSoftness, bool radialWind) 
        {
            this.fastMode = fastMode;
            this.fadeSpeed = fadeSpeed;
            this.forceRainTextureSize = forceRainTextureSize;
            this.calcTimeStep = calcTimeStep;
            this.refreshRate = refreshRate;
            this.generateTrail = generateTrail;
            this.maxStaticRaindropNumber = maxStaticRaindropNumber;
            this.maxDynamicRaindropNumber = maxDynamicRaindropNumber;
            this.raindropSizeRange = raindropSizeRange;
            this.useWind = useWind;
            this.windTurbulence = windTurbulence;
            this.windTurbScale = windTurbScale;
            this.wind = wind;
            this.gravity = gravity;
            this.friction = friction;
            this.edgeSoftness = edgeSoftness;
            this.radialWind = radialWind;
            this.cullMask_grayscale = _mask;
        }

        public void FadeOut(bool fadeOut)
        {
            this.fadeOut = fadeOut;
        }

        public void PaintToCanvas(Raindrop_PPV raindrop) 
        {
            int widthOfRain = (int)raindrop.realSize.x;
            int heightOfRain = (int)raindrop.realSize.y;
            int left_up_X = (int)(raindrop.position.x - widthOfRain / 2);
            int left_up_Y = (int)(raindrop.position.y - heightOfRain / 2);

            for (int i = left_up_X, r = 0; i < left_up_X + widthOfRain; i++, r++)
            {
                for (int j = left_up_Y, c = 0; j < left_up_Y + heightOfRain; j++, c++)
                {
                    if (i < calcTexSize.x && i >= 0 && j >= 0 && j < calcTexSize.y)
                    {
                        Color newColor = getColorAtPos(r, c, raindrop.size);
                        if (newColor == Color.white) calcRainTex.SetPixel(i, j, newColor);
                    }
                }
            }
        }

        public void EraseFromCanvas(Raindrop_PPV raindrop) 
        {
            int widthOfRain = (int)raindrop.realSize.x;
            int heightOfRain = (int)raindrop.realSize.y;
            int left_up_X = (int)(raindrop.position.x - widthOfRain / 2);
            int left_up_Y = (int)(raindrop.position.y - heightOfRain / 2);

            for (int i = left_up_X; i < left_up_X + widthOfRain; i++)
            {
                for (int j = left_up_Y; j < left_up_Y + heightOfRain; j++)
                {
                    if (i < calcTexSize.x && i >= 0 && j >= 0 && j < calcTexSize.y)
                    {
                        calcRainTex.SetPixel(i, j, Color.black);
                    }
                }
            }
        }

        public bool Shrink(Raindrop_PPV raindrop, float shrinkAmount) 
        {
            if (raindrop.size < killSize) return true;
            EraseFromCanvas(raindrop);
            raindrop.size *= (1.0f - shrinkAmount);
            raindrop.UpdateWeight();
            PaintToCanvas(raindrop);
            return false;
        }

        public bool Fade(Raindrop_PPV raindrop, float fadeAmount) 
        {
            int widthOfRain = (int)raindrop.realSize.x;
            int heightOfRain = (int)raindrop.realSize.y;
            int left_up_X = (int)(raindrop.position.x - widthOfRain / 2);
            int left_up_Y = (int)(raindrop.position.y - heightOfRain / 2);

            int cnt = 0;
            int judger = (int)(widthOfRain * heightOfRain * (fastMode ? 0.7f : 0.9f));
            for (int i = left_up_X; i < left_up_X + widthOfRain; i++)
            {
                for (int j = left_up_Y; j < left_up_Y + heightOfRain; j++)
                {
                    if (i < calcRainTex.width && i >= 0 && j >= 0 && j < calcRainTex.height)
                    {
                        Color newColor = calcRainTex.GetPixel(i, j);
                        if (newColor.r > 0)
                        {
                            if (fastMode && fadeOut && newColor.r < 0.6f) return true;
                            else if (newColor.r < fadeAmount) return true;
                            newColor.b = newColor.g = newColor.r = newColor.r - fadeAmount;
                            calcRainTex.SetPixel(i, j, newColor);
                        }
                        else cnt++;
                    }
                    else cnt++;
                    if (fadeOut && cnt >= judger) return true;
                }
            }

            if (fadeOut)
            {
                raindrop.lifeTime--;
                if (raindrop.lifeTime <= 0) return true;
            }

            return false;
        }

        public void generateBaseMap() 
        {
            calcRainTex = new Texture2D(calcTexSize.x, calcTexSize.y);
            for (int x = 0; x < calcTexSize.x; x++) 
            {
                for (int y = 0; y < calcTexSize.y; y++) 
                {
                    calcRainTex.SetPixel(x, y, Color.black);
                }
            }
            calcRainTex.Apply();
        }

        public void CalcRainTex() 
        {
            if (_genClock < refreshRate) 
            {
                _genClock++;
            } 
            else
            {
                _genClock = 0;
                if (fadeOut)
                {
                    int lenS = staticRaindrops.Count;
                    int lenD = dynamicRaindrops.Count;
                    float fadePst = UnityEngine.Random.Range(fadeSpeedRange.x, fadeSpeedRange.y) * fadeSpeed;

                    if (fastMode) for (int i = lenD - 1; i >= 0; i--) Kill(i, false);
                    else UpdateDyanmicRaindrops();

                    int cnt = 0;
                    for (int i = lenS - 1; i >= 0; i--) 
                    {
                        if (staticRaindrops[i].size <= killSize * (fastMode ? 3.0f : 1.0f))
                        {
                            Kill(i, true);
                            continue;
                        }
                        if (Fade(staticRaindrops[i], fadePst)) Kill(i, true);
                        if (fastMode && cnt++ >= 600) break;
                    }
                    if (lenS == 0 && lenD == 0) RaindropFX_Tools.PrintLog("all droplets killed.");
                    else RaindropFX_Tools.PrintLog("surplus: Static: " + lenS + " Dynamic: " + lenD);
                } 
                else
                {
                    UpdateStaticRaindrops();
                    UpdateDyanmicRaindrops();
                    int genNum = UnityEngine.Random.Range(0, 10);
                    if (genNum >= 5)
                    {
                        if (genNum >= 8)
                        {
                            if (dynamicRaindrops.Count < maxDynamicRaindropNumber)
                                GenDynamicRaindrop(new Vector2(UnityEngine.Random.Range(0, calcTexSize.x),
                                                    UnityEngine.Random.Range(0, calcTexSize.y)),
                                                    UnityEngine.Random.Range(raindropSizeRange.x,
                                                                            raindropSizeRange.y));
                        } 
                        else GenRandomStaticRaindrops();
                    }
                }
                calcRainTex.Apply();
            }
        }               

        public void GenerateTextures(ref RenderTexture resultTex)
        {
            var temp = RenderTexture.GetTemporary(calcTexSize.x, calcTexSize.y, 0);
            var temp2 = RenderTexture.GetTemporary(calcTexSize.x, calcTexSize.y, 0);

            if (resultTex != null) RenderTexture.ReleaseTemporary(resultTex);

            RaindropFX_Tools.Blur(calcRainTex, temp, edgeSoftness, blur_material);

            SetNormalMat(ref temp);
            Graphics.Blit(temp, temp2, normal_material);

            resultTex = temp2;

            RenderTexture.ReleaseTemporary(temp);
            RenderTexture.ReleaseTemporary(temp2);
        }

        public void SetNormalMat(ref RenderTexture temp)
        {
            normal_material.SetVector("_MainTex_TexelSize", new Vector4(calcTexSize.x, calcTexSize.y, 0, 0));
            normal_material.SetTexture("_HeightMap", temp);
        }

        public void Kill(int id, bool isStatic)
        {
            if (isStatic)
            {
                if (id >= staticRaindrops.Count) return;
                EraseFromCanvas(staticRaindrops[id]);
                cachePool.Recycle(staticRaindrops[id]);
                staticRaindrops.RemoveAt(id);
            }
            else
            {
                if (id >= dynamicRaindrops.Count) return;
                EraseFromCanvas(dynamicRaindrops[id]);
                cachePool.Recycle(dynamicRaindrops[id]);
                dynamicRaindrops.RemoveAt(id);
            }
        }

        public void UpdateStaticRaindrops()
        {
            int updateNumber = UnityEngine.Random.Range(0, 5) + 1;
            while (updateNumber > 0)
            {
                if (staticRaindrops.Count == 0) return;
                updateNumber--;
                int randomID = UnityEngine.Random.Range(0, staticRaindrops.Count);
                bool flag = Shrink(staticRaindrops[randomID], UnityEngine.Random.Range(shrinkSpeed.x, shrinkSpeed.y));
                if (flag) Kill(randomID, true);
                if (_staticUpdateCounter >= staticRaindrops.Count) _staticUpdateCounter = 0;
                if (_staticUpdateCounter < staticRaindrops.Count) 
                    flag = Fade(staticRaindrops[_staticUpdateCounter], UnityEngine.Random.Range(fadeSpeedRange.x, fadeSpeedRange.y));
                if (flag) Kill(_staticUpdateCounter, true);
                _staticUpdateCounter++;
            }
        }

        public void UpdateDyanmicRaindrops()
        {
            if (dynamicRaindrops.Count == 0) return;
            int len = dynamicRaindrops.Count;
            for (int i = len - 1; i >= 0; i--)
            {
                EraseFromCanvas(dynamicRaindrops[i]);
                dynamicRaindrops[i].UpdateProp(raindropTexSize, friction);

                if (generateTrail)
                {
                    Vector2 oldPos = dynamicRaindrops[i].position;
                    float lose = UnityEngine.Random.Range(loseWeightRange.x, loseWeightRange.y);
                    dynamicRaindrops[i].LoseWeight(lose);
                    GenTrailDrop(oldPos, dynamicRaindrops[i].size * lose * 50.0f);
                }

                Vector2 newlyAddedForce = Vector2.zero;
                newlyAddedForce += gravity;
                if (useWind)
                {
                    if (radialWind)
                    {
                        Vector2 wDir = dynamicRaindrops[i].position - 
                                        new Vector2(calcTexSize.x, calcTexSize.y) / 2;
                        if (wDir.magnitude == 0) wDir = Vector2.down;
                        newlyAddedForce += wDir.normalized * wind.magnitude;
                    }
                    else newlyAddedForce += wind;

                    if (windTurbulence > 0)
                    {
                        float turbulenceForce = RaindropFX_Tools.PerlinNoiseSampler(dynamicRaindrops[i].position,
                                                                                    windTurbScale);
                        Vector2 RTDir = RaindropFX_Tools.RotateAround(dynamicRaindrops[i].velocity, Vector2.zero, 90.0f);
                        newlyAddedForce += RTDir * turbulenceForce * windTurbulence;
                    }
                }
                dynamicRaindrops[i].updateForce(newlyAddedForce);
                int randomForceChecker = UnityEngine.Random.Range(0, 10);
                if (randomForceChecker < 3 && !radialWind) dynamicRaindrops[i].ApplyRandomForce(maxRandomDynamicForce);
                dynamicRaindrops[i].ApplyFriction();
                dynamicRaindrops[i].CalcNewPos(calcTimeStep);
                if (dynamicRaindrops[i].position.x >= 0 && dynamicRaindrops[i].position.x < calcTexSize.x && dynamicRaindrops[i].position.y >= 0 && dynamicRaindrops[i].position.y < calcTexSize.y)
                {
                    dynamicRaindrops[i].updateVelocity(calcTimeStep);
                    PaintToCanvas(dynamicRaindrops[i]);
                }
                else Kill(i, false);
            }
        }

        public void GenTrailDrop(Vector2 centerPos, float size)
        {
            if (maxStaticRaindropNumber == 0) return;
            if (staticRaindrops.Count >= maxStaticRaindropNumber) Kill(0, true);
            Raindrop_PPV newRaindrop = cachePool.GetRaindrop();
            newRaindrop.Reuse(true, centerPos, size, (int)((fastMode ? 0.8f : 1.0f) / fadeSpeedRange.x / fadeSpeed), raindropTexSize, friction);
            PaintToCanvas(newRaindrop);
            staticRaindrops.Add(newRaindrop);
        }

        public void GenRandomStaticRaindrops()
        {
            int createNumber = UnityEngine.Random.Range(0, 5) + 1;
            while (createNumber > 0)
            {
                createNumber--;
                if (staticRaindrops.Count >= maxStaticRaindropNumber) return;
                Raindrop_PPV newRaindrop = cachePool.GetRaindrop();
                newRaindrop.Reuse(true, new Vector2(UnityEngine.Random.Range(0, calcTexSize.x),
                                                    UnityEngine.Random.Range(0, calcTexSize.y)),
                                                    UnityEngine.Random.Range(raindropSizeRange.x, raindropSizeRange.y) * 0.5f,
                                                    (int)((fastMode ? 0.8f : 1.0f) / fadeSpeedRange.x / fadeSpeed),
                                                    raindropTexSize, friction);
                staticRaindrops.Add(newRaindrop);
            }
        }

        public void GenDynamicRaindrop(Vector2 centerPos, float size)
        {
            if (dynamicRaindrops.Count >= maxDynamicRaindropNumber) return;
            Raindrop_PPV newRaindrop = cachePool.GetRaindrop();
            newRaindrop.Reuse(false, centerPos, size, -1, raindropTexSize, friction);
            dynamicRaindrops.Add(newRaindrop);
        }

        public Color getColorAtPos(int x, int y, float scaleFactor)
        {
            float scale = 1.0f / scaleFactor;
            int xx = (int)(scale * x);
            int yy = (int)(scale * y);
            return _rainTexColorFlags[xx][yy] ? Color.white : Color.black;
        }

        public void PreparRainTexFlags()
        {
            int widthOfRain = raindropTex_alpha.width;
            int heightOfRain = raindropTex_alpha.height;

            _rainTexColorFlags = new bool[widthOfRain][];
            for (int i = 0; i < widthOfRain; i++)
            {
                _rainTexColorFlags[i] = new bool[heightOfRain];
                for (int j = 0; j < heightOfRain; j++)
                {
                    _rainTexColorFlags[i][j] = (raindropTex_alpha.GetPixel(i, j).a > 0) ? true : false;
                }
            }

            _mask = new Texture2D(1, 1);
            _mask.SetPixel(1, 1, Color.white);
        }
    }
}