#region

using System.ComponentModel;
using System.Threading.Tasks;
using UnityEditor.Profiling.Memory.Experimental;
using UnityEngine;

#endregion

public class MaterialSettings
{
    [DefaultValue(1.0f)] public float DisplacementAmplitude;

    [DefaultValue("1")] public string DisplacementAmplitudeText;

    [DefaultValue(1.0f)] public float LightB;

    [DefaultValue(1.0f)] public float LightG;

    [DefaultValue(1.0f)] public float LightIntensity;

    [DefaultValue(1.0f)] public float LightR;

    [DefaultValue(1.0f)] public float Metallic;
    [DefaultValue("1")] public string MetallicText;

    [DefaultValue(0)] public float AoRemapMin;
    [DefaultValue("0")] public string AoRemapMinText;

    [DefaultValue(1)] public float AoRemapMax;
    [DefaultValue("1")] public string AoRemapMaxText;

    [DefaultValue(0)] public float SmoothnessRemapMin;
    [DefaultValue("0")] public string SmoothnessRemapMinText;

    [DefaultValue(1)] public float SmoothnessRemapMax;
    [DefaultValue("1")] public string SmoothnessRemapMaxText;

    [DefaultValue(1)] public float TexTilingX;
    [DefaultValue("1")] public string TexTilingXText;

    [DefaultValue(1)] public float TexTilingY;
    [DefaultValue("1")] public string TexTilingYText;


    [DefaultValue(0)] public float TexOffsetX;
    [DefaultValue("0")] public string TexOffsetXText;

    [DefaultValue(0)] public float TexOffsetY;
    [DefaultValue("0")] public string TexOffsetYText;

    public MaterialSettings()
    {
        Metallic = 0.4f;
        DisplacementAmplitude = 5.0f;
        AoRemapMax = 1.0f;
        SmoothnessRemapMax = 1.0f;

        MetallicText = "0.4";
        DisplacementAmplitudeText = "5";
        AoRemapMaxText = "1";
        SmoothnessRemapMaxText = "1";

        LightR = 1.0f;
        LightG = 1.0f;
        LightB = 1.0f;
        LightIntensity = 1.0f;

        TexTilingX = 1;
        TexTilingY = 1;
        TexTilingXText = TexTilingX.ToString();
        TexTilingYText = TexTilingY.ToString();

        TexOffsetX = 0;
        TexOffsetY = 0;
        TexOffsetXText = TexOffsetX.ToString();
        TexOffsetYText = TexOffsetY.ToString();
    }
}

public class MaterialGui : MonoBehaviour
{
    private static readonly int MetallicId = Shader.PropertyToID("_Metallic");
    private static readonly int AoRemapMinId = Shader.PropertyToID("_AORemapMin");
    private static readonly int AoRemapMaxId = Shader.PropertyToID("_AORemapMax");
    private static readonly int SmoothnessRemapMinId = Shader.PropertyToID("_SmoothnessRemapMin");
    private static readonly int SmoothnessRemapMaxId = Shader.PropertyToID("_SmoothnessRemapMax");
    private static readonly int DisplacementAmplitudeId = Shader.PropertyToID("_HeightTessAmplitude");
    private static readonly int DisplacementOffsetId = Shader.PropertyToID("_HeightOffset");
    private static readonly int HeightMapId = Shader.PropertyToID("_HeightMap");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColorMap");
    private static readonly int NormalMapId = Shader.PropertyToID("_NormalMap");
    private Texture2D _aoMap;
    private bool _cubeShown;
    private bool _cylinderShown;
    private Texture2D _diffuseMap;
    private float _dispOffset = 0.5f;

    private Texture2D _heightMap;
    private Light _light;

    private MainGui _mainGuiScript;

    private MaterialSettings _materialSettings;
    private Texture2D _metallicMap;

    private Texture2D _myColorTexture;
    private Texture2D _normalMap;

    private bool _planeShown = true;

    private bool _settingsInitialized;
    private Texture2D _smoothnessMap;
    private bool _sphereShown;

    private Material _thisMaterial;

    private Rect _windowRect = new Rect(30, 300, 300, 530);

    public GameObject LightObject;
    public GameObject TestObject;
    public GameObject TestObjectCube;
    public GameObject TestObjectCylinder;

    public GameObject TestObjectParent;
    public GameObject TestObjectSphere;
    public ObjRotator TestRotator;
    private static readonly int MaskMapId = Shader.PropertyToID("_MaskMap");
    private Texture2D _maskMap;

    private void OnDisable()
    {
        if (!_mainGuiScript.IsGuiHidden || TestObjectParent == null) return;
        if (!TestObjectParent.activeSelf) TestRotator.Reset();

        TestObjectParent.SetActive(true);
        TestObjectCube.SetActive(false);
        TestObjectCylinder.SetActive(false);
        TestObjectSphere.SetActive(false);
    }

    private void Start()
    {
        _light = LightObject.GetComponent<Light>();
        InitializeSettings();
    }

    public void GetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        projectObject.MaterialSettings = _materialSettings;
    }

    public void SetValues(ProjectObject projectObject)
    {
        InitializeSettings();
        if (projectObject.MaterialSettings != null)
        {
            _materialSettings = projectObject.MaterialSettings;
        }
        else
        {
            _settingsInitialized = false;
            InitializeSettings();
        }
    }

    private void InitializeSettings()
    {
        if (_settingsInitialized) return;
        Debug.Log("Initializing MaterialSettings");
        _materialSettings = new MaterialSettings();
        _myColorTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        _settingsInitialized = true;
    }


    // Update is called once per frame
    private void Update()
    {
        _thisMaterial.SetFloat(MetallicId, _materialSettings.Metallic);
        _thisMaterial.SetFloat(AoRemapMinId, _materialSettings.AoRemapMin);
        _thisMaterial.SetFloat(AoRemapMaxId, _materialSettings.AoRemapMax);
        _thisMaterial.SetFloat(SmoothnessRemapMinId, _materialSettings.SmoothnessRemapMin);
        _thisMaterial.SetFloat(SmoothnessRemapMaxId, _materialSettings.SmoothnessRemapMax);
        _thisMaterial.SetFloat(DisplacementAmplitudeId, _materialSettings.DisplacementAmplitude);

        _light.color = new Color(_materialSettings.LightR, _materialSettings.LightG, _materialSettings.LightB);
        _light.intensity = _materialSettings.LightIntensity;

        TestObjectParent.SetActive(_planeShown);
        TestObjectCube.SetActive(_cubeShown);
        TestObjectCylinder.SetActive(_cylinderShown);
        TestObjectSphere.SetActive(_sphereShown);
        _thisMaterial.SetFloat(DisplacementOffsetId, _dispOffset);
        TestObject.GetComponent<Renderer>().enabled = false;
        TestObject.GetComponent<Renderer>().enabled = true;
        
    }

    private void ChooseLightColor(int posX, int posY)
    {
        _materialSettings.LightR =
            GUI.VerticalSlider(new Rect(posX + 10, posY + 5, 30, 100), _materialSettings.LightR, 1.0f, 0.0f);
        _materialSettings.LightG =
            GUI.VerticalSlider(new Rect(posX + 40, posY + 5, 30, 100), _materialSettings.LightG, 1.0f, 0.0f);
        _materialSettings.LightB =
            GUI.VerticalSlider(new Rect(posX + 70, posY + 5, 30, 100), _materialSettings.LightB, 1.0f, 0.0f);
        _materialSettings.LightIntensity =
            GUI.VerticalSlider(new Rect(posX + 120, posY + 5, 30, 100), _materialSettings.LightIntensity, 3.0f, 0.0f);

        GUI.Label(new Rect(posX + 10, posY + 110, 30, 30), "R");
        GUI.Label(new Rect(posX + 40, posY + 110, 30, 30), "G");
        GUI.Label(new Rect(posX + 70, posY + 110, 30, 30), "B");
        GUI.Label(new Rect(posX + 100, posY + 110, 100, 30), "Intensity");

        SetColorTexture();

        GUI.DrawTexture(new Rect(posX + 170, posY + 5, 100, 100), _myColorTexture);
    }

    private void SetColorTexture()
    {
        var colorArray = new Color[1];
        colorArray[0] = new Color(_materialSettings.LightR, _materialSettings.LightG, _materialSettings.LightB, 1.0f);

        _myColorTexture.SetPixels(colorArray);
        _myColorTexture.Apply();
    }

    private void DoMyWindow(int windowId)
    {
        const int offsetX = 10;
        var offsetY = 30;


        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Metallic Multiplier", _materialSettings.Metallic,
            _materialSettings.MetallicText,
            out _materialSettings.Metallic, out _materialSettings.MetallicText, 0.0f, 1.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Ambient Occlusion Remap Min",
            _materialSettings.AoRemapMin,
            _materialSettings.AoRemapMinText,
            out _materialSettings.AoRemapMin, out _materialSettings.AoRemapMinText, 0.0f, _materialSettings.AoRemapMax);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Ambient Occlusion Remap Max",
            _materialSettings.AoRemapMax,
            _materialSettings.AoRemapMaxText,
            out _materialSettings.AoRemapMax, out _materialSettings.AoRemapMaxText, _materialSettings.AoRemapMin, 1.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Smoothness Remap Min",
            _materialSettings.SmoothnessRemapMin,
            _materialSettings.SmoothnessRemapMinText,
            out _materialSettings.SmoothnessRemapMin, out _materialSettings.SmoothnessRemapMinText, 0.0f,
            _materialSettings.SmoothnessRemapMax);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Smoothness Remap Max",
            _materialSettings.SmoothnessRemapMax,
            _materialSettings.SmoothnessRemapMaxText,
            out _materialSettings.SmoothnessRemapMax, out _materialSettings.SmoothnessRemapMaxText,
            _materialSettings.SmoothnessRemapMin, 1.0f);
        offsetY += 40;


        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Displacement Amplitude",
            _materialSettings.DisplacementAmplitude,
            _materialSettings.DisplacementAmplitudeText,
            out _materialSettings.DisplacementAmplitude, out _materialSettings.DisplacementAmplitudeText, 0.0f, 100.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Tiling X", _materialSettings.TexTilingX,
            _materialSettings.TexTilingXText,
            out _materialSettings.TexTilingX, out _materialSettings.TexTilingXText, 0.1f, 5.0f);
        offsetY += 30;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Tiling Y", _materialSettings.TexTilingY,
            _materialSettings.TexTilingYText,
            out _materialSettings.TexTilingY, out _materialSettings.TexTilingYText, 0.1f, 5.0f);
        offsetY += 40;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Offset X", _materialSettings.TexOffsetX,
            _materialSettings.TexOffsetXText,
            out _materialSettings.TexOffsetX, out _materialSettings.TexOffsetXText, -1.0f, 1.0f);
        offsetY += 30;

        GuiHelper.Slider(new Rect(offsetX, offsetY, 280, 50), "Texture Offset Y", _materialSettings.TexOffsetY,
            _materialSettings.TexOffsetYText,
            out _materialSettings.TexOffsetY, out _materialSettings.TexOffsetYText, -1.0f, 1.0f);
        offsetY += 40;

        GUI.Label(new Rect(offsetX, offsetY, 250, 30), "Light Color");
        ChooseLightColor(offsetX, offsetY + 20);
        offsetY += 160;

        if (GUI.Button(new Rect(offsetX, offsetY, 60, 30), "Plane"))
        {
            _planeShown = true;
            _cubeShown = false;
            _cylinderShown = false;
            _sphereShown = false;
            _dispOffset = 1.0f;
            Shader.DisableKeyword("TOP_PROJECTION");
        }

        if (GUI.Button(new Rect(offsetX + 70, offsetY, 60, 30), "Cube"))
        {
            _planeShown = false;
            _cubeShown = true;
            _cylinderShown = false;
            _sphereShown = false;
            _dispOffset = 0.25f;
            Shader.EnableKeyword("TOP_PROJECTION");
        }

        if (GUI.Button(new Rect(offsetX + 140, offsetY, 70, 30), "Cylinder"))
        {
            _planeShown = false;
            _cubeShown = false;
            _cylinderShown = true;
            _sphereShown = false;
            _dispOffset = 0.25f;
            Shader.EnableKeyword("TOP_PROJECTION");
        }

        if (GUI.Button(new Rect(offsetX + 220, offsetY, 60, 30), "Sphere"))
        {
            _planeShown = false;
            _cubeShown = false;
            _cylinderShown = false;
            _sphereShown = true;
            _dispOffset = 0.25f;
            Shader.EnableKeyword("TOP_PROJECTION");
        }

        GUI.DragWindow();
    }

    private void OnGUI()
    {
        _windowRect.width = 300;
        _windowRect.height = 620;

        _windowRect = GUI.Window(14, _windowRect, DoMyWindow, "Full Material");
    }

    public void Initialize()
    {
        InitializeSettings();

        _mainGuiScript = MainGui.Instance;
        _thisMaterial = _mainGuiScript.FullMaterialCopy;

        _heightMap = _mainGuiScript.HeightMap;

        _diffuseMap = _mainGuiScript.DiffuseMap != null ? _mainGuiScript.DiffuseMap : _mainGuiScript.DiffuseMapOriginal;
        _normalMap = _mainGuiScript.NormalMap;
        _metallicMap = _mainGuiScript.MetallicMap;
        _smoothnessMap = _mainGuiScript.SmoothnessMap;
        _aoMap = _mainGuiScript.AoMap;
        _maskMap = _mainGuiScript.MaskMap;

        if (_heightMap != null)
        {
            _thisMaterial.EnableKeyword("_TESSELLATION_DISPLACEMENT");
            _thisMaterial.EnableKeyword("_HEIGHTMAP");
            _thisMaterial.SetTexture(HeightMapId, _heightMap);
        }

        if (_diffuseMap != null)
        {
            _thisMaterial.SetTexture(BaseColorId, _diffuseMap);
        }

        if (_normalMap != null)
        {
            // ReSharper disable once StringLiteralTypo
            _thisMaterial.EnableKeyword("_NORMALMAP");
            _thisMaterial.SetTexture(NormalMapId, _normalMap);
        }

        if (_maskMap != null)
        {
            // ReSharper disable once StringLiteralTypo
            _thisMaterial.EnableKeyword("_MASKMAP");
            _thisMaterial.SetTexture(MaskMapId, _maskMap);
        }


        TestObject.GetComponent<Renderer>().material = _thisMaterial;
        TestObjectCube.GetComponent<Renderer>().material = _thisMaterial;
        TestObjectCylinder.GetComponent<Renderer>().material = _thisMaterial;
        TestObjectSphere.GetComponent<Renderer>().material = _thisMaterial;
    }

}