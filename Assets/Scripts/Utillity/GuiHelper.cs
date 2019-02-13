#region

using System;
using UnityEngine;

#endregion

namespace Utility
{
    public static class GuiHelper
    {
        public static bool Slider(Rect rect, int value, out int outValue, int minValue, int maxValue)
        {
            var res = Slider(rect, null, value, out var outValueFloat, minValue, maxValue);
            outValue = outValueFloat;
            return res;
        }

        public static bool Slider(Rect rect, float value, out float outValue, float minValue, float maxValue)
        {
            var res = Slider(rect, null, value, out var outValue2, minValue, maxValue);
            outValue = outValue2;
            return res;
        }

        public static bool Slider(Rect rect, string title, int value, out int outValue, int minValue, int maxValue)
        {
            var res = Slider(rect, title, value, out float outValueFloat, minValue, maxValue);

            outValue = (int) outValueFloat;
            return res;
        }

        public static bool Slider(Rect rect, string title, float value, out float outValue, float minValue,
            float maxValue)
        {
            var offsetX = (int) rect.x;
            var offsetY = (int) rect.y;
            var startValue = value;

            if (!title.IsNullOrEmpty())
            {
                GUI.Label(new Rect(rect.x, rect.y, 250, 30), title);
                offsetY += 20;
            }

            var isChanged = false;

            value = GUI.HorizontalSlider(new Rect(offsetX, offsetY, rect.width - 60, 10), value, minValue, maxValue);

            var handler = Time.time.ToString();
            GUI.SetNextControlName(handler);
            var textValue = value.ToString();
            textValue = GUI.TextField(new Rect(offsetX + rect.width - 50, offsetY - 5, 50, 20), textValue);
            if (Event.current.type == EventType.KeyDown && Event.current.character == '\n' &&
                GUI.GetNameOfFocusedControl() == handler)
            {
                if (textValue.Contains("."))
                {
                    textValue = textValue.Replace(".", ",");
                }

                float.TryParse(textValue, out value);
                value = Mathf.Clamp(value, minValue, maxValue);
            }

            if (Math.Abs(value - startValue) > 0.0001f) isChanged = true;

            outValue = value;

            return isChanged;
        }

        public static bool VerticalSlider(Rect rect, float value, out float outValue, float minValue, float maxValue,
            bool doStuff)
        {
            var isChanged = false;

            var tempValue = value;
            value = GUI.VerticalSlider(rect, value, minValue, maxValue);
            if (Math.Abs(value - tempValue) > 0.0001f || doStuff) isChanged = true;

            outValue = value;

            return isChanged;
        }

        public static bool Toggle(Rect rect, bool value, out bool outValue, string text, bool doStuff)
        {
            var isChanged = false;

            var tempValue = value;
            value = GUI.Toggle(rect, value, text);
            if (value != tempValue || doStuff) isChanged = true;

            outValue = value;

            return isChanged;
        }
    }
}