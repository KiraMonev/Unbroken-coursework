using UnityEngine;
using UnityEditor;

namespace CustomEditorTools
{
    static public class CET
    {
        static public Texture2D MakeEditorBackgroundColor (Color Color)
        {
            Texture2D t = new Texture2D(1, 1);
            t.SetPixel(0, 0, Color);
            t.Apply();
            return t;
        }
        static public Texture2D LoadImageFromFile (string Path)
        {
            return (Texture2D)AssetDatabase.LoadAssetAtPath(Path, typeof(Texture2D));
        }
        
        static public void HorizontalLine ()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }
    }
}