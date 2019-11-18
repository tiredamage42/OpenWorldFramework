using UnityEngine;
using UnityEditor;

namespace UnityTools.EditorTools.Editor {
    public class EditorColorPicker : EditorWindow
    {
        [MenuItem(ProjectTools.defaultMenuItemSection + "Unity Tools/Editor Tools/Colors", false, ProjectTools.defaultMenuItemPriority)]
		public static void ShowWindow() {
			EditorWindowTools.SetSize(EditorWindow.GetWindow<EditorColorPicker>("Icons Menu"), 512, 512).CenterWindow();
		}

        Color32[] colors;
        GUIContent[] icons;
        GUIContent[] contents = new GUIContent[]  {
            new GUIContent("Blue"),
            new GUIContent("Red"), 
            new GUIContent("Green"),
            new GUIContent("Dark Gray"),
            new GUIContent("Gray"), 
            new GUIContent("Lite Gray"),
        };


        void OnGUI () {
            for (int i = 0; i < colors.Length; i++) colors[i] = EditorGUILayout.ColorField( contents[i], colors[i] );
             
            if (GUILayout.Button("Copy Values")) {
                ProjectTools.CopyStringToClipBoard(
                    string.Format(@"
                        public static readonly Color32 blue = new Color32({0}, {1}, {2}, 255);
                        public static readonly Color32 red = new Color32({3}, {4}, {5}, 255);
                        public static readonly Color32 green = new Color32({6}, {7}, {8}, 255);
                        public static readonly Color32 darkGray = new Color32({9}, {10}, {11}, 255);
                        public static readonly Color32 gray = new Color32 ({12}, {13}, {14}, 255);
                        public static readonly Color32 liteGray = new Color32 ({15}, {16}, {17}, 255);
                    ", 
                        colors[0].r, colors[0].g, colors[0].b,
                        colors[1].r, colors[1].g, colors[1].b,
                        colors[2].r, colors[2].g, colors[2].b,
                        colors[3].r, colors[3].g, colors[3].b,
                        colors[4].r, colors[4].g, colors[4].b,
                        colors[5].r, colors[5].g, colors[5].b                        
                    )
                );
            }

            InitializeIcons(false);

            DrawButtonsPreview(GUITools.black);

            GUITools.Space(4);

            DrawButtonsPreview(colors[5]);

            if (GUILayout.Button("Refresh Icons")) {
                InitializeIcons(true);
            }
        }
        void DrawButtonsPreview (Color32 textColor) {
            for (int i = 0; i < colors.Length; i++) {
                EditorGUILayout.BeginHorizontal();
                GUITools.IconButton(icons[i], colors[i]);
                GUITools.Button(contents[i], colors[i], GUITools.toolbarButton, textColor);
                EditorGUILayout.EndHorizontal();
            }
        }

        void InitializeIcons (bool manual) {
            if (icons == null || manual) {
                icons = new GUIContent[colors.Length];

                for (int i = 0; i < colors.Length; i++) icons[i] = BuiltInIcons.GetIcon(BuiltInIcons.allIcons[Random.Range(0, BuiltInIcons.iconsCount)], "");
                
                colors = new Color32[6] {
                    GUITools.blue,
                    GUITools.red,
                    GUITools.green,
                    GUITools.darkGray,
                    GUITools.gray,
                    GUITools.liteGray,
                };
            }
        }
    }
}
