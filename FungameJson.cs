using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomFungamePack;

public static class FungameJson
{
    private static readonly ManualLogSource Logger =  Plugin.Logger;
    private static readonly string FungamesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fungames");
    public static List<string> FungameJsons = new();
        
    public static void Initialize()
    {
        CheckFungameDirectory();
    }
    
    public static void LoadFungameJsons()
    { 
        FungameJsons = [];
        foreach (var file in Directory.GetFiles(FungamesDirectory, "*.json"))
        {
            FungameJsons.Add(file);
        }
        Logger.LogInfo($"已读取 {FungameJsons.Count} 个有效的Fungame文件");
    }
    
    private static bool ValidateAndLoadFungame(string filePath)
    {
        try
        {
            var jsonContent = File.ReadAllText(filePath);
            var jsonObject = JObject.Parse(jsonContent);
            var errors = new List<string>();
            
            ValidateRequiredField(jsonObject, "Name", errors);
            ValidateRequiredField(jsonObject, "Id", errors);
            ValidateRequiredField(jsonObject, "Version", errors);
            ValidateRequiredField(jsonObject, "Author", errors);
            ValidateRequiredField(jsonObject, "Description", errors);
            
            if (!errors.Contains("Author") && jsonObject["Author"] != null)
            {
                if (jsonObject["Author"].Type != JTokenType.Array)
                {
                    errors.Add("Author 字段必须是字符串数组");
                }
                else
                {
                    var authorArray = jsonObject["Author"] as JArray;
                    if (authorArray == null || authorArray.Count == 0)
                    {
                        errors.Add("Author 数组不能为空");
                    }
                }
            }
            
            if (errors.Count > 0)
            {
                Logger.LogWarning($"Fungame文件验证失败: {Path.GetFileName(filePath)}");
                foreach (var error in errors)
                {
                    Logger.LogWarning($"  - {error}");
                }
                return false;
            }
            
            var fungame = JsonConvert.DeserializeObject<Fungame>(jsonContent);
            
            if (string.IsNullOrWhiteSpace(fungame.Id))
            {
                Logger.LogWarning($"Fungame文件的Id字段不能为空: {Path.GetFileName(filePath)}");
                return false;
            }
            
            if (!IsValidVersion(fungame.Version))
            {
                Logger.LogWarning($"Fungame文件的Version格式不正确: {Path.GetFileName(filePath)}, Version: {fungame.Version}");
                return false;
            }
            
            Logger.LogInfo($"成功加载Fungame: {fungame.Name} (ID: {fungame.Id}, Version: {fungame.Version})");
            return true;
        }
        catch (JsonException ex)
        {
            Logger.LogError($"Fungame文件JSON格式错误: {Path.GetFileName(filePath)}, 详情: {ex.Message}");
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogError($"Fungame文件读取权限不足: {Path.GetFileName(filePath)}, 详情: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Fungame文件处理失败: {Path.GetFileName(filePath)}, 详情: {ex.Message}");
            return false;
        }
    }
    
    private static void ValidateRequiredField(JObject jsonObject, string fieldName, List<string> errors)
    {
        if (!jsonObject.ContainsKey(fieldName))
        {
            errors.Add($"缺少必需字段: {fieldName}");
        }
        else if (jsonObject[fieldName] == null || jsonObject[fieldName].Type == JTokenType.Null)
        {
            errors.Add($"字段不能为空: {fieldName}");
        }
        else if (jsonObject[fieldName].Type == JTokenType.String && string.IsNullOrWhiteSpace(jsonObject[fieldName].ToString()))
        {
            errors.Add($"字段不能为空字符串: {fieldName}");
        }
    }
    
    private static bool IsValidVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return false;
        
        var parts = version.Split('.');
        if (parts.Length < 2 || parts.Length > 4)
            return false;
        
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out _))
                return false;
        }
        
        return true;
    }
    
    private static void CheckFungameFile(string file)
    {
        var error = $"Fungame文件识别失败（权限不足）：{file}，详情：";
    }
    
    private static void CheckFungameDirectory()
    {
        var error = $"Fungames目录创建失败（权限不足）：{FungamesDirectory}，详情：";
        try
        {
            Directory.CreateDirectory(FungamesDirectory);
            Logger.LogInfo($"Fungames目录确保存在：{FungamesDirectory}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogError(error + ex);
        }
        catch (IOException ex)
        {
            Logger.LogError(error + ex);
        }
        catch (Exception ex)
        {
            Logger.LogError(error + ex);
        }
    }
}