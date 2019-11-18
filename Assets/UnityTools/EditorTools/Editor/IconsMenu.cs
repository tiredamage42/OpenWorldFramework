using UnityEngine;
using UnityEditor;

namespace UnityTools.EditorTools.Editor
{
	public class IconsMenu : EditorWindow
	{
		[MenuItem(ProjectTools.defaultMenuItemSection + "Unity Tools/Editor Tools/Icons Menu", false, ProjectTools.defaultMenuItemPriority)]
		public static void ShowWindow() {
			EditorWindowTools.SetSize(EditorWindow.GetWindow<IconsMenu>("Icons Menu"), 512, 512).CenterWindow();
		}

		Vector2 scroll_pos;
		GUIContent[] icons;
		GUILayoutOption[] layoutOptions;

		GUIStyle blackButtonStyle;
		
		void InitializeGUI () {
			if (icons == null) {
				icons = new GUIContent[BuiltInIcons.iconsCount];
				for (int i = 0; i < BuiltInIcons.iconsCount; i++) icons[i] = BuiltInIcons.GetIcon(BuiltInIcons.allIcons[i], BuiltInIcons.allIcons[i]);
			}
			if (layoutOptions == null)
				layoutOptions = new GUILayoutOption[] { GUILayout.Width(64) };

			if (blackButtonStyle == null) {
				blackButtonStyle = new GUIStyle(GUITools.toolbarButton);
				blackButtonStyle.normal.background = null;
			}
		}

		void DrawButtonRow (GUIStyle style, Color32 color, int i) {
			GUI.color = color;
			EditorGUILayout.BeginHorizontal(GUITools.toolbar);
			GUI.color = GUITools.white;
			for (int x = 0; x < columns; x++) {
				if (i + x < icons.Length && GUILayout.Button(icons[i + x], style, layoutOptions)) {
					ProjectTools.CopyStringToClipBoard (icons[i + x].tooltip);
					Debug.Log("Copied to clipboard: " + icons[i + x].tooltip);
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		int columns = 7;
			
		void OnGUI()
		{
			InitializeGUI();
			scroll_pos = EditorGUILayout.BeginScrollView(scroll_pos);
			for (int i = 0; i < icons.Length; i += columns) {	
				DrawButtonRow (GUITools.toolbarButton, GUITools.white, i);
				DrawButtonRow (blackButtonStyle, GUITools.black, i);
				GUITools.Space(2);
			}
			EditorGUILayout.EndScrollView();
		}
	}
}

