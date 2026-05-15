using System;
using System.Linq;
using BepInEx.Logging;
using MossLib;

namespace CustomFungamePack;

public static class MapLoader
{
    private const string LogPrefix = "log.";
    private static readonly ManualLogSource Logger = Plugin.Logger;

    public static void LoadAndApplyMapFromFungame(Fungame fungame)
    {
        try
        {
            if (fungame?.Map == null)
            {
                Error("map_loader.load_error");
                return;
            }

            var mapData = fungame.Map;
            ValidateAndApplyMap(fungame);
            ValidateAndApplyItems(fungame);
            
            var width = mapData.Blocks.Length;
            var height = mapData.Blocks.Length > 0 ? mapData.Blocks[0].Length : 0;
            Info("map_loader.load_success", mapData.X, mapData.Y, width, height);
        }
        catch (Exception ex)
        {
            Error("map_loader.load_failed", ex.Message);
        }
    }
    
    private static void ValidateAndApplyMap(Fungame fungame)
    {
        var mapData = fungame.Map;
        if (mapData.Blocks == null || mapData.Blocks.Length == 0)
        {
            Warning("validation.no_data", ModLocale.GetFormat("log.common.map"), ModLocale.GetFormat("log.common.block"));
            return;
        }

        var maxColCount = 0;
        foreach (var row in mapData.Blocks)
        {
            if (row != null && row.Length > maxColCount)
            {
                maxColCount = row.Length;
            }
        }

        var rowCount = mapData.Blocks.Length;
        if (maxColCount == 0)
        {
            Warning("validation.row_data_empty", ModLocale.GetFormat("log.common.map"));
            return;
        }

        var isIrregular = mapData.Blocks.Any(row => row == null || row.Length != maxColCount);
        if (isIrregular)
        {
            Warning("map_loader.irregular_shape", ModLocale.GetFormat("log.common.map"), rowCount, maxColCount);
            NormalizeMapShape(mapData, rowCount, maxColCount);
        }

        var worldX = mapData.X;
        var worldY = mapData.Y;
        var blockCount = 0;
        var failCount = 0;

        for (int row = 0; row < rowCount; row++)
        {
            if (mapData.Blocks[row] == null || mapData.Blocks[row].Length == 0)
            {
                Warning("map_loader.row_empty_skip", ModLocale.GetFormat("log.common.map"), row);
                continue;
            }

            for (int col = 0; col < mapData.Blocks[row].Length; col++)
            {
                var blockType = mapData.Blocks[row][col];
                
                if (blockType <= 0)
                {
                    worldX++;
                    continue;
                }

                try
                {
                    Tools.SetBlock(worldX, worldY, (ushort)blockType);
                    blockCount++;
                }
                catch (Exception ex)
                {
                    Error("map_loader.place_failed", worldX, worldY, ModLocale.GetFormat("log.common.block"), blockType, ex.Message);
                    failCount++;
                }

                worldX++;
            }

            worldX = mapData.X;
            worldY--;
        }

        if (failCount > 0)
        {
            Warning("map_loader.apply_complete_fail", ModLocale.GetFormat("log.common.map"), blockCount, ModLocale.GetFormat("log.common.block"), failCount);
        }
        else
        {
            Info("map_loader.apply_complete", ModLocale.GetFormat("log.common.map"), blockCount, ModLocale.GetFormat("log.common.block"));
        }
    }

    private static void ValidateAndApplyItems(Fungame fungame)
    {
        var mapData = fungame.Map;
        if (mapData.Items == null || mapData.Items.Length == 0)
        {
            Warning("validation.no_data", ModLocale.GetFormat("log.common.map"), ModLocale.GetFormat("log.common.item"));
            return;
        }

        var rowCount = mapData.Items.Length;
        var maxColCount = 0;
        
        foreach (var row in mapData.Items)
        {
            if (row != null && row.Length > maxColCount)
            {
                maxColCount = row.Length;
            }
        }

        if (maxColCount == 0)
        {
            Warning("validation.row_data_empty", ModLocale.GetFormat("log.common.map"));
            return;
        }

        var isIrregular = false;
        for (int i = 0; i < rowCount; i++)
        {
            if (mapData.Items[i] == null || mapData.Items[i].Length != maxColCount)
            {
                isIrregular = true;
                break;
            }
        }

        if (isIrregular)
        {
            Warning("map_loader.irregular_shape", ModLocale.GetFormat("log.common.map"), rowCount, maxColCount);
            NormalizeItemShape(mapData, rowCount, maxColCount);
        }

        var itemCount = 0;
        var itemFailCount = 0;
        
        for (int row = 0; row < rowCount; row++)
        {
            if (mapData.Items[row] == null)
            {
                Warning("map_loader.row_empty_skip", ModLocale.GetFormat("log.common.item"), row);
                continue;
            }

            for (int col = 0; col < mapData.Items[row].Length; col++)
            {
                var itemName = mapData.Items[row][col];
                
                if (string.IsNullOrEmpty(itemName))
                {
                    continue;
                }
                
                var worldX = mapData.X + col;
                var worldY = mapData.Y - row;
                
                try
                {
                    Tools.SetItem(worldX, worldY, itemName);
                    itemCount++;
                }
                catch (Exception ex)
                {
                    itemFailCount++;
                    if (itemFailCount <= 5)
                    {
                        Error("map_loader.place_failed", worldX, worldY, ModLocale.GetFormat("log.common.item"), itemName, ex.Message);
                    }
                }
            }
        }

        if (itemFailCount > 0)
        {
            Warning("map_loader.apply_complete_fail", ModLocale.GetFormat("log.common.item"), itemCount, ModLocale.GetFormat("log.common.item"), itemFailCount);
        }
        else
        {
            Info("map_loader.apply_complete", ModLocale.GetFormat("log.common.item"), itemCount, ModLocale.GetFormat("log.common.item"));
        }
    }

    private static void NormalizeItemShape(MapData mapData, int rowCount, int maxColCount)
    {
        var normalizedItems = new string[rowCount][];
        
        for (int row = 0; row < rowCount; row++)
        {
            normalizedItems[row] = new string[maxColCount];
            
            if (mapData.Items[row] != null)
            {
                var sourceLength = mapData.Items[row].Length;
                for (int col = 0; col < maxColCount; col++)
                {
                    if (col < sourceLength)
                    {
                        normalizedItems[row][col] = mapData.Items[row][col];
                    }
                    else
                    {
                        normalizedItems[row][col] = "";
                    }
                }
            }
            else
            {
                for (int col = 0; col < maxColCount; col++)
                {
                    normalizedItems[row][col] = "";
                }
            }
        }
        
        mapData.Items = normalizedItems;
        Info("map_loader.normalized", ModLocale.GetFormat("log.common.item"), rowCount, maxColCount, ModLocale.GetFormat("log.common.empty_string"));
    }
    
    private static void NormalizeMapShape(MapData mapData, int rowCount, int maxColCount)
    {
        var normalizedBlocks = new int[rowCount][];
        
        for (int row = 0; row < rowCount; row++)
        {
            normalizedBlocks[row] = new int[maxColCount];
            
            if (mapData.Blocks[row] != null)
            {
                var sourceLength = mapData.Blocks[row].Length;
                for (int col = 0; col < maxColCount; col++)
                {
                    if (col < sourceLength)
                    {
                        normalizedBlocks[row][col] = mapData.Blocks[row][col];
                    }
                    else
                    {
                        normalizedBlocks[row][col] = 0;
                    }
                }
            }
            else
            {
                for (int col = 0; col < maxColCount; col++)
                {
                    normalizedBlocks[row][col] = 0;
                }
            }
        }
        
        mapData.Blocks = normalizedBlocks;
        Info("map_loader.normalized", ModLocale.GetFormat("log.common.map"), rowCount, maxColCount, ModLocale.GetFormat("log.common.air_block"));
    }

    private static void Info(string key, params object[] args)
    {
        var message = ModLocale.GetFormat($"{LogPrefix}{key}", args);
        Tools.LogInfo(message, Logger);
    }

    private static void Error(string key, params object[] args)
    {
        var message = ModLocale.GetFormat($"{LogPrefix}{key}", args);
        Tools.LogError(message, Logger);
    }

    private static void Warning(string key, params object[] args)
    {
        var message = ModLocale.GetFormat($"{LogPrefix}{key}", args);
        Tools.LogWarning(message, Logger);
    }
}