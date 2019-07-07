using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Post Retro Look Pro")]
public class RetroLookProPost : MonoBehaviour {
    [HideInInspector]
    public bool developmentMode = true;
    [HideInInspector]
    public PresetScriptableObject referenceScr;
    public Preset tempPreset;
    private bool previous;
    float time_ = 0.0f;
    RenderTexture tempPass = null;
    RenderTexture zeroPass = null;
    RenderTexture texPass12 = null;  		//texture inbetween 1st and 2nd pass
    RenderTexture texPass23 = null;  		//texture inbetween 2nd and 3rd pass
    RenderTexture texLast = null; 		//latest frame / previous frame
    RenderTexture texFeedback = null; 	//feedback buffer
    RenderTexture texFeedback2 = null;  //feedback buffer
    RenderTexture myRenderTexture;
    RenderTexture bypassRT;
    RenderTexture texClear = null;  		//texture to clear other textures
    RenderTexture texTape = null;       //tape noise texture
    Texture3D colormapTexture;
    Texture2D colormapPalette;
    int max_curve_length = 50;
    Texture2D texCurves = null; 
    Vector4 curvesOffest = new Vector4(0, 0, 0, 0);  
    float[,] curvesData = new float[50, 3];
    Camera cam;
    Material colorMat;
    int m_tempColorPresIndex;
    [HideInInspector]
    public effectPresets presetss;

    private void CreateMaterials(){
        tempPreset.m_1 = newMat(tempPreset.sh_first, "RetroLook/First_RetroLook");
        tempPreset.m_2 = newMat(tempPreset.sh_second, "RetroLook/Second_RetroLook");
        tempPreset.m_3 = newMat(tempPreset.sh_third, "RetroLook/Third_RetroLook");
        tempPreset.m_4 = newMat(tempPreset.sh_fourth, "RetroLook/Forth_RetroLook");
        tempPreset.m_clear = newMat(tempPreset.sh_clear, "RetroLook/ResetRetroLook");
        tempPreset.m_tape_noise = newMat(tempPreset.sh_tape, "RetroLook/Tape_RetroLook");       
        if (tempPreset.b_Mode == 3) Curves();
	}
    private void Awake()
    {
        if (this.isActiveAndEnabled)
        {
            if (referenceScr)
            {
                if (developmentMode)
                    tempPreset = referenceScr.currPreset;
                else
                {
                    tempPreset = new Preset();
                    tempPreset = referenceScr.currPreset.ShallowCopy();
                }
            }
            tempPreset._bottomNoiseMat = new Material(Shader.Find("RetroLook/BottomNoiseEffect"));
            tempPreset._bottomNoiseMat.SetTexture("_SecondaryTex", tempPreset._VHSNoise);

            if (tempPreset.b_Mode == 3) Curves();
            if (tempPreset._enableTVmode)
            {
                previous = tempPreset._scan;
                tempPreset._VHS_Material = new Material(Shader.Find("RetroLook/VHS_RetroLook"));
                if (tempPreset._scan)
                    tempPreset._VHS_Material.shader = Shader.Find("RetroLook/VHSwithLines_RetroLook");
                tempPreset._VHS_Material.SetFloat("_OffsetPosY", tempPreset._verticalOffset);
                tempPreset._TV_Material = new Material(Shader.Find("RetroLook/TV_RetroLook"));
            }

            presetss.presetsList[tempPreset.colorPresetIndex].preset.changed = false;
            string shaderName = "RetroLook/ColorPalette";
            Shader colorShader = Shader.Find(shaderName);

            if (colorShader == null)
            {
                Debug.LogWarning("Shader '" + shaderName + "' not found. Was it deleted?");
                enabled = false;
            }

            colorMat = new Material(colorShader);
            colorMat.hideFlags = HideFlags.DontSave;

            Texture2D texture = Resources.Load("Noise Textures/blue_noise") as Texture2D;

            if (texture == null)
            {
                Debug.LogWarning("Noise Textures/blue_noise.png not found. Was it moved or deleted?");
            }
            m_tempColorPresIndex = tempPreset.colorPresetIndex;
            colorMat.SetTexture("_BlueNoise", texture);

        }
        cam = GetComponent<Camera>();
        cam.forceIntoRenderTexture = false;
    }
    private void Reset()
    {
        tempPreset.resolutionMode = ResolutionMode.ConstantPixelSize;
        tempPreset.pixelSize = 3;
        tempPreset.opacity = 1;
        tempPreset.dither = 1;
    }
    private void OnEnable()
    {
        ApplyColormapToMaterial();
    }
    private void OnDisable()
    {
        if (colorMat != null)
        {
            DestroyImmediate(colorMat);
        }
    }
    private Material newMat(Shader ref_shader, string shader_path)
    {
        Material temp_mat;
        if (ref_shader)
            temp_mat = new Material(ref_shader);
        else
            temp_mat = new Material(Shader.Find(shader_path)) { hideFlags = HideFlags.DontSave };
        return temp_mat;
    }
    private void CreateTextures(RenderTexture src){

        texClear = CreateNewTexture(texClear, src);
        texClear.Create();
        texPass12=CreateNewTexture(texPass12, src);
        texPass12.Create();
        texPass23=CreateNewTexture(texPass23, src);
        texPass23.Create();
        texFeedback=CreateNewTexture(texFeedback, src, HideFlags.DontSave);
        texFeedback.Create();
        texFeedback2=CreateNewTexture(texFeedback2, src, HideFlags.DontSave);
        texFeedback2.Create();
        texLast=CreateNewTexture(texLast, src, HideFlags.DontSave);
        texLast.Create();
        
        //clear textures
        Graphics.Blit (texClear, texFeedback, tempPreset.m_clear);
		Graphics.Blit (texClear, texFeedback2, tempPreset.m_clear);
		Graphics.Blit (texClear, texLast, tempPreset.m_clear);
	}


    void OnPreRender()
    {
        if(developmentMode)
        tempPreset = referenceScr.currPreset;
        myRenderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 16);
        cam.targetTexture = myRenderTexture;
    }

    void OnPostRender()
    {
            cam.targetTexture = null; //null means framebuffer

        if (tempPreset.m_1 == null)
            {
                CreateMaterials();
            }
            if (texPass12 == null || (myRenderTexture.width != texPass12.width || myRenderTexture.height != texPass12.height))
            {
                CreateTextures(myRenderTexture);
            }
            //bottom noise
        if (tempPreset._bottomNoiseMat == null)
        {
            tempPreset._bottomNoiseMat = new Material(Shader.Find("RetroLook/BottomNoiseEffect"));
            tempPreset._bottomNoiseMat.SetTexture("_SecondaryTex", tempPreset._VHSNoise);
        }
        if (zeroPass == null)
        {
            zeroPass = new RenderTexture(myRenderTexture.width, myRenderTexture.height, myRenderTexture.depth);
        }
        // color palette
        if (tempPreset.enableColorPalette)
        {
            if (presetss != null && intHasChanged(tempPreset.colorPresetIndex, m_tempColorPresIndex))
            {
                ApplyColormapToMaterial();
            }

            ApplyMaterialVariables();

            RenderTexture scaled = RenderTexture.GetTemporary(tempPreset.resolution.x, tempPreset.resolution.y);
            scaled.filterMode = FilterMode.Point;

            if (presetss == null)
            {
                Graphics.Blit(myRenderTexture, scaled);
            }
            else
            {
                Graphics.Blit(myRenderTexture, scaled, colorMat);
            }

            Graphics.Blit(scaled, myRenderTexture);
            RenderTexture.ReleaseTemporary(scaled);
        }
        //

        tempPreset._bottomNoiseMat.SetFloat("_OffsetNoiseX", Random.Range(0f, 1.0f));
        float offsetNoise1 = tempPreset._bottomNoiseMat.GetFloat("_OffsetNoiseY");
        tempPreset._bottomNoiseMat.SetFloat("_OffsetNoiseY", offsetNoise1 + Random.Range(-0.05f, 0.05f));

        if (tempPreset.useBottomNoise) tempPreset._bottomNoiseMat.EnableKeyword("NOISE_BOTTOM_ON");
        else tempPreset._bottomNoiseMat.DisableKeyword("NOISE_BOTTOM_ON");

        if (tempPreset.useBottomStretch) tempPreset._bottomNoiseMat.EnableKeyword("BOTTOM_STRETCH_ON");
        else tempPreset._bottomNoiseMat.DisableKeyword("BOTTOM_STRETCH_ON");

        tempPreset._bottomNoiseMat.SetFloat("_NoiseBottomHeight", tempPreset.bottomHeight);
        tempPreset._bottomNoiseMat.SetFloat("_NoiseBottomIntensity", tempPreset.bottomIntensity);

        if (tempPreset._enableTVmode)
        Graphics.Blit(myRenderTexture, zeroPass, tempPreset._bottomNoiseMat);
        else
        {
            Graphics.Blit(myRenderTexture, zeroPass, tempPreset._bottomNoiseMat);
            Graphics.Blit(zeroPass, myRenderTexture, tempPreset._bottomNoiseMat);
        }        
        //
        // TV mode
        if (tempPreset._enableTVmode)
        {
            if (tempPreset._VHS_Material == null)
            {
                tempPreset._VHS_Material = new Material(Shader.Find("RetroLook/VHS_RetroLook"));
                tempPreset._TV_Material = new Material(Shader.Find("RetroLook/TV_RetroLook"));
            }
            if (previous != tempPreset._scan)
            {
                previous = tempPreset._scan;
                if (tempPreset._scan)
                    tempPreset._VHS_Material.shader = Shader.Find("RetroLook/VHSwithLines_RetroLook");
                else
                    tempPreset._VHS_Material.shader = Shader.Find("RetroLook/VHS_RetroLook");
            }
            if (tempPreset._scan)
            {
                tempPreset._VHS_Material.SetColor("_ScanLinesColor", tempPreset._scanLinesColor);
                tempPreset._VHS_Material.SetFloat("_ScanLines", tempPreset._adjustLines);
            }
            else
            {
                tempPreset._VHS_Material.shader = Shader.Find("RetroLook/VHS_RetroLook");
            }

            if (Random.Range(0, 100 - tempPreset._VerticalOffsetFrequency) <= 5)
            {
                if (tempPreset._verticalOffset == 0.0f)
                {
                    tempPreset._VHS_Material.SetFloat("_OffsetPosY", tempPreset._verticalOffset);
                }
                if (tempPreset._verticalOffset > 0.0f)
                {
                    tempPreset._VHS_Material.SetFloat("_OffsetPosY", tempPreset._verticalOffset - Random.Range(0f, tempPreset._verticalOffset));
                }
                else if (tempPreset._verticalOffset < 0.0f)
                {
                    tempPreset._VHS_Material.SetFloat("_OffsetPosY", tempPreset._verticalOffset + Random.Range(0f, -tempPreset._verticalOffset));
                }
            }
            tempPreset._VHS_Material.SetFloat("_OffsetDistortion", tempPreset._OffsetDistortion);
            tempPreset._VHS_Material.SetFloat("_OffsetColor", tempPreset._offsetColor);
            tempPreset._VHS_Material.SetFloat("_OffsetNoiseX", Random.Range(0f, 0.6f));
            tempPreset._VHS_Material.SetTexture("_SecondaryTex", tempPreset._VHSNoise);
            float offsetNoise = tempPreset._VHS_Material.GetFloat("_OffsetNoiseY");
            tempPreset._VHS_Material.SetFloat("_OffsetNoiseY", offsetNoise + Random.Range(-0.03f, 0.03f));
            tempPreset._VHS_Material.SetFloat("_Intensity", tempPreset._textureIntensity);
            tempPreset._offsetColor = tempPreset._VHS_Material.GetFloat("_OffsetColor");
            tempPreset._TV_Material.SetFloat("hardScan", tempPreset._hardScan);
            tempPreset._TV_Material.SetFloat("resScale", tempPreset._resolution);
            tempPreset._TV_Material.SetFloat("maskDark", tempPreset.maskDark);
            tempPreset._TV_Material.SetFloat("maskLight", tempPreset.maskLight);
            tempPreset._TV_Material.SetVector("warp", tempPreset.warp);
            if (tempPass == null)
            {
                tempPass = new RenderTexture(myRenderTexture.width, myRenderTexture.height, myRenderTexture.depth);
            }
        Graphics.Blit(zeroPass, tempPass, tempPreset._VHS_Material);
        Graphics.Blit(tempPass, myRenderTexture, tempPreset._TV_Material);
    }
    //

    float screenLinesNum_ = tempPreset.b_ScreenLinesNum;
            if (screenLinesNum_ <= 0) screenLinesNum_ = myRenderTexture.height;
            if (tempPreset.f_TapeNoise || tempPreset.f_Granularity || tempPreset.f_LineNoise)
                if (texTape == null || (texTape.height != Mathf.Min(tempPreset.n_NoiseLinesAmountY, screenLinesNum_)))
                {
                    int texHeight = (int)Mathf.Min(tempPreset.n_NoiseLinesAmountY, screenLinesNum_);
                    int texWidth = (int)(
                          (float)texHeight * (float)myRenderTexture.width / (float)myRenderTexture.height);
                    DestroyImmediate(texTape); 
                    texTape = new RenderTexture(texWidth, texHeight, 0);
                    texTape.hideFlags = HideFlags.HideAndDontSave;
                    texTape.filterMode = FilterMode.Point;
                    texTape.Create();
                    Graphics.Blit(texClear, texTape, tempPreset.m_tape_noise); 
                }            
            if (tempPreset.independentTimeOn) { time_ = Time.unscaledTime; }
            else { time_ = Time.time; }
            tempPreset.m_1.SetFloat("time_", time_);

        tempPreset.m_1.SetFloat("screenLinesNum", screenLinesNum_);
            tempPreset.m_1.SetFloat("noiseLinesNum", tempPreset.n_NoiseLinesAmountY);
            tempPreset.m_1.SetFloat("noiseQuantizeX", tempPreset.n_NoiseSignalProcessing);
            ParamSwitch(tempPreset.m_1, tempPreset.f_Granularity, "VHS_FILMGRAIN_ON");
            ParamSwitch(tempPreset.m_1, tempPreset.f_TapeNoise, "VHS_TAPENOISE_ON");
            ParamSwitch(tempPreset.m_1, tempPreset.f_LineNoise, "VHS_LINENOISE_ON");
            ParamSwitch(tempPreset.m_1, tempPreset.j_JitterHorizontal, "VHS_JITTER_H_ON");
            tempPreset.m_1.SetFloat("jitterHAmount", tempPreset.j_JitterHorizAmount);
            ParamSwitch(tempPreset.m_1, tempPreset.j_JitterVertical, "VHS_JITTER_V_ON");
            tempPreset.m_1.SetFloat("jitterVAmount", tempPreset.j_VertAmount);
            tempPreset.m_1.SetFloat("jitterVSpeed", tempPreset.j_VertSpeed);
            ParamSwitch(tempPreset.m_1, tempPreset.j_LinesFloat, "VHS_LINESFLOAT_ON");
            tempPreset.m_1.SetFloat("linesFloatSpeed", tempPreset.j_LinesSpeed);
            ParamSwitch(tempPreset.m_1, tempPreset.j_TwitchHorizontal, "VHS_TWITCH_H_ON");
            tempPreset.m_1.SetFloat("twitchHFreq", tempPreset.j_TwitchHorizFreq);
            ParamSwitch(tempPreset.m_1, tempPreset.j_TwitchVertical, "VHS_TWITCH_V_ON");
            tempPreset.m_1.SetFloat("twitchVFreq", tempPreset.j_TwitchVertFreq);
            ParamSwitch(tempPreset.m_1, tempPreset.j_ScanLines, "VHS_SCANLINES_ON");
            tempPreset.m_1.SetFloat("scanLineWidth", tempPreset.j_ScanLinesWidth);
            ParamSwitch(tempPreset.m_1, tempPreset.f_SignalNoise, "VHS_YIQNOISE_ON");
            tempPreset.m_1.SetFloat("signalNoisePower", tempPreset.f_SignalNoisePower);
            tempPreset.m_1.SetFloat("signalNoiseAmount", tempPreset.f_SignalNoiseAmount);
            ParamSwitch(tempPreset.m_1, tempPreset.j_Stretch, "VHS_STRETCH_ON");
            ParamSwitch(tempPreset.m_1, tempPreset.f_Fisheye, "VHS_FISHEYE_ON");
            tempPreset.m_1.SetFloat("cutoffX", tempPreset.f_CutoffX);
            tempPreset.m_1.SetFloat("cutoffY", tempPreset.f_CutoffY);
            tempPreset.m_1.SetFloat("cutoffFadeX", tempPreset.f_FadeX);
            tempPreset.m_1.SetFloat("cutoffFadeY", tempPreset.f_FadeY);
            tempPreset.m_2.SetFloat("time_", time_);
            tempPreset.m_2.SetFloat("screenLinesNum", screenLinesNum_);
            ParamSwitch(tempPreset.m_2, tempPreset.b_Bleed, "VHS_BLEED_ON");
            tempPreset.m_2.DisableKeyword("VHS_OLD_THREE_PHASE");
            tempPreset.m_2.DisableKeyword("VHS_THREE_PHASE");
            tempPreset.m_2.DisableKeyword("VHS_TWO_PHASE");
            if (tempPreset.b_Mode == 0) { tempPreset.m_2.EnableKeyword("VHS_OLD_THREE_PHASE"); }
            else if (tempPreset.b_Mode == 1) { tempPreset.m_2.EnableKeyword("VHS_THREE_PHASE"); }
            else if (tempPreset.b_Mode == 2) { tempPreset.m_2.EnableKeyword("VHS_TWO_PHASE"); }
            else if (tempPreset.b_Mode == 3) { if (tempPreset.b_BleedCurveEditMode) Curves(); }
            tempPreset.m_2.SetTexture("_CurvesTex", texCurves);
            tempPreset.m_2.SetVector("curvesOffest", curvesOffest);
            tempPreset.m_2.SetInt("bleedLength", tempPreset.b_BleedLength);
            ParamSwitch(tempPreset.m_2, (tempPreset.b_Mode == 3), "VHS_CUSTOM_BLEED_ON");
            ParamSwitch(tempPreset.m_2, tempPreset.b_BleedDebug, "VHS_DEBUG_BLEEDING_ON");
            tempPreset.m_2.SetFloat("bleedAmount", tempPreset.b_BleedAmount);
            ParamSwitch(tempPreset.m_2, tempPreset.f_Fisheye, "VHS_FISHEYE_ON");
            ParamSwitch(tempPreset.m_2, tempPreset.f_FisheyeType == 1, "VHS_FISHEYE_HYPERSPACE");
            tempPreset.m_2.SetFloat("fisheyeBend", tempPreset.f_FisheyeBend);
            tempPreset.m_2.SetFloat("fisheyeSize", tempPreset.f_FisheyeSize);

            ParamSwitch(tempPreset.m_2, tempPreset.v_Vignette, "VHS_VIGNETTE_ON");
            tempPreset.m_2.SetFloat("vignetteAmount", tempPreset.v_VignetteAmount);
            tempPreset.m_2.SetFloat("vignetteSpeed", tempPreset.v_VignetteSpeed);
            ParamSwitch(tempPreset.m_2, tempPreset.p_PictureCorrection, "VHS_SIGNAL_TWEAK_ON");
            tempPreset.m_2.SetFloat("signalAdjustY", tempPreset.p_PictureCorr1);
            tempPreset.m_2.SetFloat("signalAdjustI", tempPreset.p_PictureCorr2);
            tempPreset.m_2.SetFloat("signalAdjustQ", tempPreset.p_PictureCorr3);
            tempPreset.m_2.SetFloat("signalShiftY", tempPreset.p_PictureShift1);
            tempPreset.m_2.SetFloat("signalShiftI", tempPreset.p_PictureShift2);
            tempPreset.m_2.SetFloat("signalShiftQ", tempPreset.p_PictureShift3);
            tempPreset.m_2.SetFloat("gammaCorection", tempPreset.p_Gamma);                        
            if (tempPreset.f_TapeNoise || tempPreset.f_Granularity || tempPreset.f_LineNoise)
            {
                tempPreset.m_tape_noise.SetFloat("time_", time_);
                ParamSwitch(tempPreset.m_tape_noise, tempPreset.f_Granularity, "VHS_FILMGRAIN_ON");
                tempPreset.m_tape_noise.SetFloat("filmGrainAmount", tempPreset.f_GranularityAmount);
                ParamSwitch(tempPreset.m_tape_noise, tempPreset.f_TapeNoise, "VHS_TAPENOISE_ON");
                tempPreset.m_tape_noise.SetFloat("tapeNoiseTH", tempPreset.f_TapeNoiseTH);
                tempPreset.m_tape_noise.SetFloat("tapeNoiseAmount", tempPreset.f_TapeNoiseAmount);
                tempPreset.m_tape_noise.SetFloat("tapeNoiseSpeed", tempPreset.f_TapeNoiseSpeed);
                ParamSwitch(tempPreset.m_tape_noise, tempPreset.f_LineNoise, "VHS_LINENOISE_ON");
                tempPreset.m_tape_noise.SetFloat("lineNoiseAmount", tempPreset.f_LineNoiseAmount);
                tempPreset.m_tape_noise.SetFloat("lineNoiseSpeed", tempPreset.f_LineNoiseSpeed);
                Graphics.Blit(texTape, texTape, tempPreset.m_tape_noise);
                tempPreset.m_1.SetTexture("_TapeTex", texTape);
                tempPreset.m_1.SetFloat("tapeNoiseAmount", tempPreset.f_TapeNoiseAmount);
            }
            Graphics.Blit(myRenderTexture, texPass12, tempPreset.m_1);

        if (!tempPreset.a_Artefacts)
            {
                Graphics.Blit(texPass12, myRenderTexture, tempPreset.m_2);
            }
            else
            {
                Graphics.Blit(texPass12, texPass23, tempPreset.m_2);
                tempPreset.m_3.SetTexture("_LastTex", texLast);
                tempPreset.m_3.SetTexture("_FeedbackTex", texFeedback);
                tempPreset.m_3.SetFloat("feedbackThresh", tempPreset.a_ArtefactsThreshold);
                tempPreset.m_3.SetFloat("feedbackAmount", tempPreset.a_ArtefactsAmount);
                tempPreset.m_3.SetFloat("feedbackFade", tempPreset.a_ArtefactsFadeAmount);
                tempPreset.m_3.SetColor("feedbackColor", tempPreset.a_ArtefactsColor);
                Graphics.Blit(texPass23, texFeedback2, tempPreset.m_3);
                Graphics.Blit(texFeedback2, texFeedback); 
                tempPreset.m_4.SetFloat("feedbackAmp", 1.0f);
                tempPreset.m_4.SetTexture("_FeedbackTex", texFeedback);
                Graphics.Blit(texPass23, texLast, tempPreset.m_4);

                if (!tempPreset.a_ArtefactsDebug)
                    Graphics.Blit(texLast, myRenderTexture);
                else
                    Graphics.Blit(texFeedback, myRenderTexture);
            }

        Graphics.Blit(myRenderTexture, null as RenderTexture);
        RenderTexture.ReleaseTemporary(myRenderTexture);
    }
	private void ParamSwitch(Material mat, bool paramValue, string paramName){
		if(paramValue) 	mat.EnableKeyword(paramName);
		else  			mat.DisableKeyword(paramName);
	}
	private void Curves()
    { 
		if(texCurves==null) texCurves  = new Texture2D(max_curve_length, 1, TextureFormat.RGBA32, false);
		curvesOffest[0] = 0.0f;
		curvesOffest[1] = 0.0f;
		curvesOffest[2] = 0.0f;
		float t = 0.0f;
		for(int i=0; i< tempPreset.b_BleedLength; i++){

			t =  ((float)i)/((float)tempPreset.b_BleedLength);
			curvesData[i,0] = tempPreset.b_BleedCurve1.Evaluate( t );
			curvesData[i,1] = tempPreset.b_BleedCurve2.Evaluate( t );
			curvesData[i,2] = tempPreset.b_BleedCurve3.Evaluate( t );
			if(tempPreset.b_BleedCurveSync) curvesData[i,2] = curvesData[i,1]; 	

			if(curvesOffest[0]>curvesData[i,0]) curvesOffest[0] = curvesData[i,0];			
			if(curvesOffest[1]>curvesData[i,1]) curvesOffest[1] = curvesData[i,1];			
			if(curvesOffest[2]>curvesData[i,2]) curvesOffest[2] = curvesData[i,2];			
		};
		curvesOffest[0] = Mathf.Abs(curvesOffest[0]); 		
		curvesOffest[1] = Mathf.Abs(curvesOffest[1]); 		
		curvesOffest[2] = Mathf.Abs(curvesOffest[2]);		
		
		for(int i=0; i< tempPreset.b_BleedLength; i++){
			curvesData[i,0] += curvesOffest[0]; 		
			curvesData[i,1] += curvesOffest[1]; 		
			curvesData[i,2] += curvesOffest[2]; 				
			texCurves.SetPixel(-2+ tempPreset.b_BleedLength - i, 0, new Color(curvesData[i,0],curvesData[i,1],curvesData[i,2]));
        };

		texCurves.Apply();			

	}
    private RenderTexture CreateNewTexture(RenderTexture texture, RenderTexture scrT, HideFlags flags)
    {
        DestroyImmediate(texture);
        texture = new RenderTexture(scrT.width, scrT.height, 0) { hideFlags = flags, filterMode = FilterMode.Point };
        return texture;
    }
    private RenderTexture CreateNewTexture(RenderTexture texture, RenderTexture scrT)
    {
        DestroyImmediate(texture);
        texture = new RenderTexture(scrT.width, scrT.height, scrT.depth) { filterMode = FilterMode.Point };
        return texture;
    }
    public void ApplyMaterialVariables()
    {
        switch (tempPreset.resolutionModeIndex)
        {
            case 0:
                tempPreset.resolutionMode = ResolutionMode.ConstantResolution;
                break;
            case 1:
                tempPreset.resolutionMode = ResolutionMode.ConstantPixelSize;
                break;
            default:
                break;
        }
        tempPreset.pixelSize = (int)Mathf.Clamp(tempPreset.pixelSize, 1, float.MaxValue);

        if (tempPreset.resolutionMode == ResolutionMode.ConstantPixelSize)
        {
            tempPreset.resolution.x = Screen.width / tempPreset.pixelSize;
            tempPreset.resolution.y = Screen.height / tempPreset.pixelSize;
        }

        tempPreset.resolution.x = Mathf.Clamp(tempPreset.resolution.x, 1, 16384);
        tempPreset.resolution.y = Mathf.Clamp(tempPreset.resolution.y, 1, 16384);

        tempPreset.opacity = Mathf.Clamp01(tempPreset.opacity);
        tempPreset.dither = Mathf.Clamp01(tempPreset.dither);

        colorMat.SetFloat("_Opacity", tempPreset.opacity);
        colorMat.SetFloat("_Dither", tempPreset.dither);
    }
    public void ApplyColormapToMaterial()
    {
        if (presetss != null)
        {
            ApplyPalette();
            ApplyMap();
        }
    }
    void ApplyPalette()
    {
        colormapPalette = new Texture2D(256, 1, TextureFormat.RGB24, false);
        colormapPalette.filterMode = FilterMode.Point;
        colormapPalette.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < presetss.presetsList[tempPreset.colorPresetIndex].preset.numberOfColors; ++i)
        {
            colormapPalette.SetPixel(i, 0, presetss.presetsList[tempPreset.colorPresetIndex].preset.palette[i]);
        }

        colormapPalette.Apply();

        colorMat.SetTexture("_Palette", colormapPalette);
    }
    public void ApplyMap()
    {
        int colorsteps = 64;
        colormapTexture = new Texture3D(colorsteps, colorsteps, colorsteps, TextureFormat.RGB24, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        colormapTexture.SetPixels32(presetss.presetsList[tempPreset.colorPresetIndex].preset.pixels);
        colormapTexture.Apply();
        colorMat.SetTexture("_Colormap", colormapTexture);
    }
    public bool intHasChanged(int A, int B)
    {
        bool result = false;
        if (B != A)
        {
            A = B;
            result = true;
        }
        return result;
    }
}