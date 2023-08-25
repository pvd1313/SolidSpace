using System;
using System.Diagnostics;
using System.IO;
using SolidSpace.Editor.Utilities;
using SolidSpace.Entities.Health;
using SolidSpace.JobUtilities;
using SolidSpace.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace SolidSpace.Entities.Splitting.Editor
{
    public class ShapeFillWindow : EditorWindow
    {
        private Texture2D _inputTexture;
        private string _outputTexturePath;
        private Stopwatch _stopwatch;
        
        private void OnGUI()
        {
            _stopwatch ??= new Stopwatch();
            
            _inputTexture = (Texture2D) EditorGUILayout.ObjectField("Input Texture", _inputTexture, typeof(Texture2D), false);
            _outputTexturePath = EditorGUILayout.TextField("Output path", _outputTexturePath);

            if (GUILayout.Button("Test Fill"))
            {
                try
                {
                    FillTexture();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void TimerBegin()
        {
            _stopwatch.Restart();
        }

        private void TimerEnd(string operationName)
        {
            _stopwatch.Stop();
            var elapsedMs = _stopwatch.ElapsedTicks / (float) Stopwatch.Frequency * 1000;
            Debug.Log($"{operationName}: {elapsedMs}ms");
        }

        private void FillTexture()
        {
            ConsoleUtil.ClearLog();
            
            TimerBegin();
            var pixels = _inputTexture.GetPixelData<Color32>(0);
            var textureSize = new int2(_inputTexture.width, _inputTexture.height);
            var requiredByteCount = HealthUtil.GetRequiredByteCount(textureSize);
            var frameBits = NativeMemory.CreateTempArray<byte>(requiredByteCount);
            HealthUtil.TextureToHealth(pixels, textureSize, frameBits);
            TimerEnd("Convert to bit array");

            var seedJobConnections = NativeMemory.CreateTempArray<byte2>(256);
            var seedJobBounds = NativeMemory.CreateTempArray<ByteBounds>(256);
            var seedJobMask = NativeMemory.CreateTempArray<byte>(textureSize.x * textureSize.y);
            var seedJobResult = NativeMemory.CreateTempArray<ShapeSeedJobResult>(1);
            
            var seedJob = new ShapeSeedJob
            {
                inFrameBits = frameBits,
                inFrameSize = textureSize,
                outResult = seedJobResult,
                outConnections = seedJobConnections,
                outSeedBounds = seedJobBounds,
                outSeedMask = seedJobMask
            };
            
            TimerBegin();
            seedJob.Run();
            TimerEnd("ShapeFillJob");

            if (seedJob.outResult[0].code != EShapeSeedResult.Success)
            {
                Debug.LogError($"ShapeFillJob ended with '{seedJob.outResult[0].code}'");
                return;
            }
            
            Debug.Log("Seed count: " + seedJob.outResult[0].seedCount);
            Debug.Log("Connection count: " + seedJob.outResult[0].connectionCount);

            var shapeReadJobRootSeeds = NativeMemory.CreateTempArray<byte>(256);
            var shapeReadShapeCount = NativeMemory.CreateTempArray<int>(1);
            
            var shapeReadJob = new ShapeReadJob
            {
                inOutConnections = seedJob.outConnections,
                inOutBounds = seedJob.outSeedBounds,
                inSeedJobResult = seedJobResult,
                outShapeCount = shapeReadShapeCount,
                outShapeRootSeeds = shapeReadJobRootSeeds,
            };
            
            TimerBegin();
            shapeReadJob.Run();
            TimerEnd("ShapeReadJob");

            var shapeCount = shapeReadJob.outShapeCount[0];
            Debug.Log("Shape count: " + shapeCount);
            for (var i = 0; i < shapeCount; i++)
            {
                var bounds = shapeReadJob.inOutBounds[i];
                var pos = bounds.min;
                var width = bounds.max.x - bounds.min.x + 1;
                var height = bounds.max.y - bounds.min.y + 1;
                Debug.Log($"Shape at ({pos.x}, {pos.y}) with size ({width}, {height})");
            }

            var exportTexture = new Texture2D(textureSize.x, textureSize.y, TextureFormat.RGB24, false);
            var exportTextureRaw = exportTexture.GetRawTextureData<ColorRGB24>();
            var colors = new ColorRGB24[]
            {
                new ColorRGB24 {r = 0, g = 0, b = 255},
                new ColorRGB24 {r = 0, g = 255, b = 0},
                new ColorRGB24 {r = 0, g = 255, b = 255},
                new ColorRGB24 {r = 255, g = 0, b = 0},
                new ColorRGB24 {r = 255, g = 0, b = 255},
                new ColorRGB24 {r = 255, g = 255, b = 0},
                new ColorRGB24 {r = 255, g = 255, b = 255},
            };
            
            for (var i = 0; i < textureSize.x * textureSize.y; i++)
            {
                var maskColor = seedJob.outSeedMask[i];
                exportTextureRaw[i] = maskColor == 0 ? default : colors[maskColor % colors.Length];
            }
            exportTexture.Apply();

            File.WriteAllBytes(_outputTexturePath, exportTexture.EncodeToPNG());

            DestroyImmediate(exportTexture);
            seedJobResult.Dispose();
            seedJobConnections.Dispose();
            seedJobBounds.Dispose();
            seedJobMask.Dispose();
            shapeReadJobRootSeeds.Dispose();
            frameBits.Dispose();
            shapeReadShapeCount.Dispose();
        }
    }
}