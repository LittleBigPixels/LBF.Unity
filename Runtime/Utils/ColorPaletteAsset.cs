﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace LBF
{
    [CreateAssetMenu(menuName = "LBF/Colors/ColorPaletteAsset")]
    public class ColorPaletteAsset : SerializedScriptableObject
    {
        public struct ColorEntry
        {
            public String Name;
            public Color Color;
        
            public ColorEntry( string name, Color color ) {
                Name = name;
                Color = color;
            }
        }

        [TitleGroup("Code Generation")]
        public String Namespace;
        public String Class;
        [FolderPath]
        public String Folder;

        [TitleGroup("Actions")]
        [Button]
        public void Generate() => DoGenerate();

        [TitleGroup("Colors")]
        [TableList(AlwaysExpanded = true)]
        public List<ColorEntry> Colors = new List<ColorEntry>();

        private void DoGenerate()
        {
        #if UNITY_EDITOR
            if (Class == "") return;
            if (Folder == "") return;
            if (Colors == null) return;

            var sb = new StringBuilder();
            
            //Usings
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();

            //Namespace Open
            if (Namespace != "") sb.AppendLine("namespace " + Namespace);
            if (Namespace != "") sb.AppendLine("{");

            //Class Declaration
            sb.AppendLine("public static class " + SafeName(Class));
            sb.AppendLine("{");

            //Fields
            foreach (var color in Colors)
            {
                sb.AppendLine("\tpublic static Color " + SafeName(color.Name) + ";");
            }
            sb.AppendLine();

            //Constructor
            sb.AppendLine("static " + SafeName(Class) + "()");
            sb.AppendLine("{");
            foreach (var color in Colors)
            {
                sb.AppendLine("\tColorUtility.TryParseHtmlString(\"#" + ColorUtility.ToHtmlStringRGB(color.Color) + "\", out " + SafeName(color.Name) + ");");
            }
            sb.AppendLine("}");

            //Class Close
            sb.AppendLine("}");

            //Namespace Close
            if (Namespace != "") sb.AppendLine("}");

            var fileContent = sb.ToString();

            var path = Path.Combine(Folder, Class + ".cs");
            using (FileStream fs = File.Open(path, FileMode.Create))
            {
                var writer = new StreamWriter(fs);
                writer.Write(fileContent);
                writer.Flush();
            }
            
            AssetDatabase.Refresh();
  #endif
        }

        private string SafeName(string value)
        {
            string className = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
            bool isValid = true;//Microsoft.CSharp.CSharpCodeProvider.CreateProvider("C#").IsValidIdentifier(className);

            if (isValid)
                return className.Replace( " ", String.Empty );
            
            // File name contains invalid chars, remove them
            Regex regex = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");
            className = regex.Replace(className, "");

            // Class name doesn't begin with a letter, insert an underscore
            if (!Char.IsLetter(className, 0))
            {
                className = className.Insert(0, "_");
            }

            return className.Replace(" ", String.Empty); 
        }
    }
}