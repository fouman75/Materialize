#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using General;
using Gui;
using Settings;
using UnityEngine;
using Logger = General.Logger;

#endregion

public enum CommandType
{
    Settings,
    Open,
    Save,
    HeightFromDiffuse,
    NormalFromHeight,
    Metallic,
    Smoothness,
    AoFromNormal,
    MaskMap,
    QuickSave,
    FlipNormalMapY,
    FileFormat
}

public struct Command
{
    //public string xmlCommand;
    public CommandType CommandType;
    public string Extension;
    public string FilePath;
    public ProgramEnums.MapType MapType;

    public ProgramSettings ProjectProgramSettings;
}

public class CommandList
{
    public List<Command> Commands;
}

public class CommandListExecutor : MonoBehaviour
{
    private SaveLoadProject _saveLoad;

    public GameObject SaveLoadProjectObject;

    public SettingsGui SettingsGui;

    // Use this for initialization
    private void Awake()
    {
        ProgramManager.Instance.SceneObjects.Add(gameObject);
    }

    private void Start()
    {
        _saveLoad = SaveLoadProjectObject.GetComponent<SaveLoadProject>();

        StartCoroutine(StartCommandString());
    }

    private IEnumerator StartCommandString()
    {
        yield return new WaitForSeconds(0.1f);
        var commandString = ClipboardHelper.ClipBoard;
        if (commandString.Contains(
            "<CommandList xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">")
        ) ProcessCommands(commandString);
    }

    public void SaveTestString()
    {
        var commandList = new CommandList {Commands = new List<Command>()};

        var command = new Command
            {CommandType = CommandType.Settings, ProjectProgramSettings = SettingsGui.ProgramSettings};
        commandList.Commands.Add(command);

        command = new Command
        {
            CommandType = CommandType.Open,
            FilePath = "F:\\Project_Files\\TextureTools5\\Dev\\Output\\test_diffuse.bmp",
            MapType = ProgramEnums.MapType.DiffuseOriginal
        };
        commandList.Commands.Add(command);

        command = new Command
        {
            CommandType = CommandType.Open,
            FilePath = "F:\\Project_Files\\TextureTools5\\Dev\\Output\\test_normal.bmp",
            MapType = ProgramEnums.MapType.Normal
        };
        commandList.Commands.Add(command);

        command = new Command {CommandType = CommandType.FlipNormalMapY};
        commandList.Commands.Add(command);

        command = new Command {CommandType = CommandType.AoFromNormal};
        commandList.Commands.Add(command);

        command = new Command {CommandType = CommandType.MaskMap};
        commandList.Commands.Add(command);

        command = new Command {CommandType = CommandType.FileFormat, Extension = "tga"};
        commandList.Commands.Add(command);

        command = new Command
        {
            CommandType = CommandType.QuickSave,
            FilePath = "F:\\Project_Files\\TextureTools5\\Dev\\Output\\test_property.bmp"
        };
        commandList.Commands.Add(command);


        var sb = new StringBuilder();
        var serializer = new XmlSerializer(typeof(CommandList));
        var stream = new StringWriter(sb);
        serializer.Serialize(stream, commandList);
        ClipboardHelper.ClipBoard = stream.ToString();

        Logger.Log(stream.ToString());
    }

    private void OnApplicationFocus(bool focusStatus)
    {
        if (!focusStatus) return;
        var commandString = ClipboardHelper.ClipBoard;
        if (commandString.Contains(
            "<CommandList xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">")
        ) ProcessCommands(commandString);
    }

    public void ProcessCommands()
    {
        var commandString = ClipboardHelper.ClipBoard;
        if (commandString.Contains(
            "<CommandList xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">")
        ) StartCoroutine(ProcessCommandsCoroutine(commandString));
    }

    public void ProcessCommands(string commandString)
    {
        StartCoroutine(ProcessCommandsCoroutine(commandString));
    }

    private IEnumerator ProcessCommandsCoroutine(string commandString)
    {
        //string commandString = ClipboardHelper.clipBoard;

        var serializer = new XmlSerializer(typeof(CommandList));
        var stream = new StringReader(commandString);

        if (serializer.Deserialize(stream) is CommandList commandList)
            foreach (var thisCommand in commandList.Commands)
            {
                switch (thisCommand.CommandType)
                {
                    case CommandType.Settings:
                        SettingsGui.ProgramSettings = thisCommand.ProjectProgramSettings;
                        SettingsGui.SetSettings();
                        break;
                    case CommandType.Open:
                    {
                        yield return StartCoroutine(_saveLoad.LoadTexture(thisCommand.MapType, thisCommand.FilePath));
                        break;
                    }
                    case CommandType.Save:
                    {
                        switch (thisCommand.MapType)
                        {
                            case ProgramEnums.MapType.Height:
                                StartCoroutine(_saveLoad.SaveTexture(thisCommand.Extension,
                                    TextureManager.Instance.HeightMap,
                                    thisCommand.FilePath));
                                break;
                            case ProgramEnums.MapType.Diffuse:
                                StartCoroutine(_saveLoad.SaveTexture(thisCommand.Extension,
                                    TextureManager.Instance.DiffuseMapOriginal,
                                    thisCommand.FilePath));
                                break;
                            case ProgramEnums.MapType.Metallic:
                                StartCoroutine(_saveLoad.SaveTexture(thisCommand.Extension,
                                    TextureManager.Instance.MetallicMap,
                                    thisCommand.FilePath));
                                break;
                            case ProgramEnums.MapType.Smoothness:
                                StartCoroutine(_saveLoad.SaveTexture(thisCommand.Extension,
                                    TextureManager.Instance.SmoothnessMap,
                                    thisCommand.FilePath));
                                break;
                            case ProgramEnums.MapType.MaskMap:
                                StartCoroutine(_saveLoad.SaveTexture(thisCommand.Extension,
                                    TextureManager.Instance.MaskMap,
                                    thisCommand.FilePath));
                                break;
                            case ProgramEnums.MapType.Ao:
                                StartCoroutine(
                                    _saveLoad.SaveTexture(thisCommand.Extension, TextureManager.Instance.AoMap,
                                        thisCommand.FilePath));
                                break;
                            case ProgramEnums.MapType.Property:
                                MainGui.Instance.ProcessPropertyMap();
                                StartCoroutine(_saveLoad.SaveTexture(thisCommand.Extension,
                                    TextureManager.Instance.PropertyMap,
                                    thisCommand.FilePath));
                                break;
                            case ProgramEnums.MapType.DiffuseOriginal:
                                break;
                            case ProgramEnums.MapType.Normal:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        while (_saveLoad.Busy) yield return new WaitForSeconds(0.1f);
                        break;
                    }
                    case CommandType.FlipNormalMapY:
                        TextureManager.Instance.FlipNormalYCallback();
                        break;
                    case CommandType.FileFormat:
                        MainGui.Instance.SetFormat(thisCommand.Extension);
                        break;
                    case CommandType.HeightFromDiffuse:
                    {
                        MainGui.Instance.CloseWindows();
                        MainGui.Instance.HeightFromDiffuseGuiObject.SetActive(true);
                        yield return new WaitForSeconds(0.1f);
                        MainGui.Instance.HeightFromDiffuseGuiScript.InitializeTextures();
                        yield return new WaitForSeconds(0.1f);
                        yield return StartCoroutine(MainGui.Instance.HeightFromDiffuseGuiScript.ProcessDiffuse());
                        yield return StartCoroutine(MainGui.Instance.HeightFromDiffuseGuiScript.Process());
                        MainGui.Instance.HeightFromDiffuseGuiScript.Close();
                        break;
                    }
                    case CommandType.NormalFromHeight:
                    {
                        MainGui.Instance.CloseWindows();
                        MainGui.Instance.NormalFromHeightGuiObject.SetActive(true);
                        yield return new WaitForSeconds(0.1f);
                        MainGui.Instance.NormalFromHeightGuiScript.InitializeTextures();
                        yield return new WaitForSeconds(0.1f);
                        yield return StartCoroutine(MainGui.Instance.NormalFromHeightGuiScript.ProcessHeight());
                        MainGui.Instance.NormalFromHeightGuiScript.Process();

                        MainGui.Instance.NormalFromHeightGuiScript.Close();
                        break;
                    }
                    case CommandType.Metallic:
                    {
                        MainGui.Instance.CloseWindows();
                        MainGui.Instance.MetallicGuiObject.SetActive(true);
                        yield return new WaitForSeconds(0.1f);
                        MainGui.Instance.MetallicGuiScript.InitializeTextures();
                        yield return new WaitForSeconds(0.1f);
                        yield return StartCoroutine(MainGui.Instance.MetallicGuiScript.ProcessBlur());
                        yield return StartCoroutine(MainGui.Instance.MetallicGuiScript.Process());
                        MainGui.Instance.MetallicGuiScript.Close();
                        break;
                    }
                    case CommandType.Smoothness:
                    {
                        MainGui.Instance.CloseWindows();
                        MainGui.Instance.SmoothnessGuiObject.SetActive(true);
                        yield return new WaitForSeconds(0.1f);
                        MainGui.Instance.SmoothnessGuiScript.InitializeTextures();
                        yield return new WaitForSeconds(0.1f);
                        yield return StartCoroutine(MainGui.Instance.SmoothnessGuiScript.ProcessBlur());
                        MainGui.Instance.SmoothnessGuiScript.Process();
                        MainGui.Instance.SmoothnessGuiScript.Close();
                        break;
                    }
                    case CommandType.AoFromNormal:
                    {
                        MainGui.Instance.CloseWindows();
                        MainGui.Instance.AoFromNormalGuiObject.SetActive(true);
                        yield return new WaitForSeconds(0.1f);
                        MainGui.Instance.AoFromNormalGuiScript.InitializeTextures();
                        yield return new WaitForSeconds(0.1f);
                        yield return StartCoroutine(MainGui.Instance.AoFromNormalGuiScript.ProcessNormalDepth());
                        yield return StartCoroutine(MainGui.Instance.AoFromNormalGuiScript.Process());
                        MainGui.Instance.AoFromNormalGuiScript.Close();
                        break;
                    }
                    case CommandType.MaskMap:
                    {
                        MainGui.Instance.CloseWindows();
                        TextureManager.Instance.MakeMaskMap();
                        break;
                    }
                    case CommandType.QuickSave:

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                yield return new WaitForSeconds(0.1f);

                ClipboardHelper.ClipBoard = "";
            }

        yield return new WaitForSeconds(0.1f);

        MainGui.Instance.CloseWindows();
        TextureManager.Instance.FixSize();
        MainGui.Instance.MaterialGuiObject.SetActive(true);
        MainGui.Instance.MaterialGuiScript.Initialize();
    }
}