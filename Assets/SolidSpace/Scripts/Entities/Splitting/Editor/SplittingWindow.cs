using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace SolidSpace.Entities.Splitting.Editor
{
    public class SplittingWindow : EditorWindow
    {
        private int _frame;
        private Stopwatch _watch;

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            _frame = PixelBitButton("5", 5, _frame);
            _frame = PixelBitButton("6", 6, _frame);
            _frame = PixelBitButton("7", 7, _frame);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            _frame = PixelBitButton("3", 3, _frame);
            PixelBitButton(" ", 8, _frame);
            _frame = PixelBitButton("4", 4, _frame);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            _frame = PixelBitButton("0", 0, _frame);
            _frame = PixelBitButton("1", 1, _frame);
            _frame = PixelBitButton("2", 2, _frame);
            GUILayout.EndHorizontal();
            
            _watch ??= new Stopwatch();
            _watch.Restart();
            var connectionMask = SplittingUtil.Bake4NeighbourPixelConnectionMask();
            _watch.Stop();

            EditorGUILayout.IntField("Frame value (dec)", _frame);
            
            var elapsedMs = _watch.ElapsedTicks / (float) Stopwatch.Frequency * 1000;
            EditorGUILayout.FloatField("Connection mask baking (ms)", elapsedMs);
            
            _watch.Restart();
            var borderMask = SplittingUtil.BakeAloneBorderPixelMask();
            _watch.Stop();
            
            elapsedMs = _watch.ElapsedTicks / (float) Stopwatch.Frequency * 1000;
            EditorGUILayout.FloatField("Border mask baking (ms)", elapsedMs);

            var maskConnected = connectionMask.HasBit((byte) _frame);
            var directConnected = SplittingUtil.CheckAll4NeighbourPixelsAreConnected((byte) _frame);
            var isBorderPixel = borderMask.HasBit((byte) _frame);
            
            EditorGUILayout.TextField("Mask", maskConnected ? "TRUE" : "FALSE");
            EditorGUILayout.TextField("Direct", directConnected ? "TRUE" : "FALSE");
            EditorGUILayout.TextField("Border", isBorderPixel ? "TRUE" : "FALSE");
        }
        private int PixelBitButton(string label, int bitIndex, int mask)
        {
            var value = mask & (1 << bitIndex);
            var prevColor = GUI.contentColor;

            GUI.color = value != 0 ? Color.green : prevColor;
            if (GUILayout.Button(label, GUILayout.Width(20)))
            {
                mask ^= 1 << bitIndex;
            }

            GUI.color = prevColor;

            return mask;
        }
    }
}