using System;
using BepInEx.Logging;
using MossLib;

namespace CustomFungamePack;

public static class MapLoader
{
    private static readonly ManualLogSource Logger = Plugin.Logger;

    public static void LoadAndApplyMapFromFungame(Fungame fungame)
    {
        try
        {
            if (fungame?.Map == null)
            {
                Logger.LogError("Fungame 或地图数据为空");
                return;
            }

            var mapData = fungame.Map;
            ValidateAndApplyMap(fungame);
            
            var width = mapData.Blocks.Length;
            var height = mapData.Blocks.Length > 0 ? mapData.Blocks[0].Length : 0;
            Logger.LogInfo($"成功加载地图: 起始坐标({mapData.X}, {mapData.Y}), 尺寸({width}x{height})");
        }
        catch (Exception ex)
        {
            Logger.LogError($"加载地图失败: {ex.Message}");
        }
    }
    
    private static void ValidateAndApplyMap(Fungame fungame)
    {
        var mapData = fungame.Map;
        if (mapData.Blocks == null || mapData.Blocks.Length == 0)
        {
            Logger.LogWarning("地图中没有方块数据");
            return;
        }

        var rowCount = mapData.Blocks.Length;
        var maxColCount = 0;
        
        foreach (var row in mapData.Blocks)
        {
            if (row != null && row.Length > maxColCount)
            {
                maxColCount = row.Length;
            }
        }

        if (maxColCount == 0)
        {
            Logger.LogWarning("地图行数据为空");
            return;
        }

        var isIrregular = false;
        for (int i = 0; i < rowCount; i++)
        {
            if (mapData.Blocks[i] == null || mapData.Blocks[i].Length != maxColCount)
            {
                isIrregular = true;
                break;
            }
        }

        if (isIrregular)
        {
            Logger.LogWarning($"检测到不规则地图形状！正在填充为规则长方形 ({rowCount}x{maxColCount})...");
            NormalizeMapShape(mapData, rowCount, maxColCount);
        }

        Tools.CheckForWorld();
        
        var blockCount = 0;
        var failCount = 0;
        
        for (int row = 0; row < rowCount; row++)
        {
            if (mapData.Blocks[row] == null)
            {
                Logger.LogWarning($"第 {row} 行为空，跳过");
                continue;
            }

            for (int col = 0; col < mapData.Blocks[row].Length; col++)
            {
                var blockType = mapData.Blocks[row][col];
                
                if (blockType < 0)
                {
                    Logger.LogWarning($"无效的方块类型 ({blockType}) 在位置 ({col}, {row})，跳过");
                    continue;
                }
                
                var worldX = mapData.X + col;
                var worldY = mapData.Y - row;
                
                try
                {
                    Tools.SetBlock(worldX, worldY, (ushort)blockType);
                    blockCount++;
                }
                catch (Exception ex)
                {
                    failCount++;
                    if (failCount <= 5)
                    {
                        Logger.LogError($"在 ({worldX}, {worldY}) 放置方块 {blockType} 失败: {ex.Message}");
                    }
                }
            }
        }

        if (failCount > 0)
        {
            Logger.LogWarning($"地图应用完成，成功放置 {blockCount} 个方块，失败 {failCount} 个");
        }
        else
        {
            Logger.LogInfo($"地图应用完成，共放置 {blockCount} 个方块");
        }

        ApplyItemsFromMap(mapData);
        Tools.Tp(fungame.Spawn);
    }

private static void ApplyItemsFromMap(MapData mapData)
    {
        if (mapData.Items == null || mapData.Items.Length == 0)
        {
            Logger.LogInfo("地图中没有物品数据");
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
            Logger.LogWarning("物品行数据为空");
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
            Logger.LogWarning($"检测到不规则物品形状！正在填充为规则长方形 ({rowCount}x{maxColCount})...");
            NormalizeItemShape(mapData, rowCount, maxColCount);
        }

        var itemCount = 0;
        var itemFailCount = 0;
        
        for (int row = 0; row < rowCount; row++)
        {
            if (mapData.Items[row] == null)
            {
                Logger.LogWarning($"物品第 {row} 行为空，跳过");
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
                        Logger.LogError($"在 ({worldX}, {worldY}) 放置物品 {itemName} 失败: {ex.Message}");
                    }
                }
            }
        }

        if (itemFailCount > 0)
        {
            Logger.LogWarning($"物品应用完成，成功放置 {itemCount} 个物品，失败 {itemFailCount} 个");
        }
        else
        {
            Logger.LogInfo($"物品应用完成，共放置 {itemCount} 个物品");
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
        Logger.LogInfo($"物品已规范化为 {rowCount}x{maxColCount} 的长方形，缺失部分已用空字符串填充");
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
        Logger.LogInfo($"地图已规范化为 {rowCount}x{maxColCount} 的长方形，缺失部分已用空气方块填充");
    }}
