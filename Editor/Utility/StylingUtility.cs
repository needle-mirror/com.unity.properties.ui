using Unity.Properties.UI.Internal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI
{
    /// <summary>
    /// Provides utility methods around styling.
    /// </summary>
    public static class StylingUtility
    {
        const float k_LabelRatio = 0.42f;
        const float k_Indent = 19.0f;
        
        /// <summary>
        /// Dynamically computes and sets the width of <see cref="Label"/> elements so that they stay properly aligned
        /// when indented with <see cref="Foldout"/> elements.  
        /// </summary>
        /// <remarks>
        /// This will effectively inline the <see cref="IStyle.width"/> and the <see cref="IStyle.minWidth"/> value of
        /// every <see cref="VisualElement"/> under the provided root.  
        /// </remarks>
        /// <param name="root">The target element</param>
        public static void AlignInspectorLabelWidth(VisualElement root)
        {
            var width = root.localBound.width * k_LabelRatio;
            AlignInspectorLabelWidth (root, width, 0);
        }

        static void AlignInspectorLabelWidth (VisualElement root, float topLevelLabelWidth, int indentLevel)
        {
            if (root.ClassListContains(UssClasses.Unity.Label))
            {
                root.style.width = Mathf.Max(topLevelLabelWidth - indentLevel * k_Indent, 0.0f);
                root.style.minWidth = 0;
            }

            if (root is Foldout)
            {
                var label = root.Q<Toggle>().Q(className:UssClasses.ListElement.ToggleInput);
                if (null != label)
                {
                    label.style.width = Mathf.Max(topLevelLabelWidth - indentLevel * k_Indent + 16.0f, 0.0f);
                    label.style.minWidth = 0;
                }

                ++indentLevel;
            }

            if (root is IReloadableElement && root.ClassListContains(UssClasses.ListElement.Item))
                --indentLevel;

            foreach (var child in root.Children())
            {
                AlignInspectorLabelWidth (child, topLevelLabelWidth, indentLevel);
            }
        }
    }
}