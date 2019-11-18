
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace UnityTools.EditorTools {

    public static class GUITools
    {

        public static readonly Color32 black = new Color32(0,0,0,255);
        public static readonly Color32 white = new Color32(255, 255, 255, 255);

        public static readonly Color32 blue = new Color32(114, 160, 245, 255);
        public static readonly Color32 red = new Color32(211, 88, 88, 255);
        public static readonly Color32 green = new Color32(77, 173, 89, 255);

        public static readonly Color32 darkGray = new Color32(75, 75, 75, 255);
        public static readonly Color32 gray = new Color32 (115, 115, 115, 255);
        public static readonly Color32 liteGray = new Color32 (210, 210, 210, 255);


        public static readonly Color32 shade = new Color32 (0, 0, 0, 25);
                            




        static Rect _rect = new Rect(0,0,0,0);
        public static void Space (int spacing=1) {
            for (int i = 0; i < spacing; i++) EditorGUILayout.Space();
        }

        static GUIContent _noContent;
        public static GUIContent noContent {
            get {
                if (_noContent == null) _noContent = GUIContent.none;
                return _noContent;
            }
        }
        
        static GUIStyle _miniButton;
        public static GUIStyle miniButton {
            get {
                if (_miniButton == null) _miniButton = new GUIStyle(EditorStyles.miniButton);
                return _miniButton;
            }
        }
        static GUIStyle _toolbarButton;
        public static GUIStyle toolbarButton {
            get {
                if (_toolbarButton == null) _toolbarButton = new GUIStyle(EditorStyles.toolbarButton);
                return _toolbarButton;
            }
        }
        static GUIStyle _toolbar;
        public static GUIStyle toolbar {
            get {
                if (_toolbar == null) _toolbar = new GUIStyle(EditorStyles.toolbar);
                return _toolbar;
            }
        }
        static GUIStyle _toolbarDropDown;
        public static GUIStyle toolbarDropDown {
            get {
                if (_toolbarDropDown == null) _toolbarDropDown = new GUIStyle(EditorStyles.toolbarDropDown);
                return _toolbarDropDown;
            }
        }

        static GUIStyle _label;
        public static GUIStyle label {
            get {
                if (_label == null) _label = new GUIStyle(EditorStyles.label);
                return _label;
            }
        }
        static GUIStyle _boldLabel;
        public static GUIStyle boldLabel {
            get {
                if (_boldLabel == null) _boldLabel = new GUIStyle(EditorStyles.boldLabel);
                return _boldLabel;
            }
        }
        static GUIStyle _button;
        public static GUIStyle button {
            get {
                if (_button == null) _button = new GUIStyle(GUI.skin.button);
                return _button;
            }
        }

        static GUIStyle _popup;

        public static GUIStyle popup {
            get {
                if (_popup == null) {
                    _popup = new GUIStyle(EditorStyles.popup);
                    _popup.normal.background = (Texture2D)BuiltInIcons.GetIcon("mini popup@2x", "").image;
                    _popup.focused.background = (Texture2D)BuiltInIcons.GetIcon("mini popup focus@2x", "").image;
                    _popup.normal.textColor = liteGray;
                    _popup.focused.textColor = liteGray;

                }
                return _popup;
            }
        }
        

        static GUILayoutOption[] _littleButtonOptions;
        static GUILayoutOption[] littleButtonOptions {
            get {
                if (_littleButtonOptions == null || _littleButtonOptions.Length != 2) {
                    _littleButtonOptions = new GUILayoutOption[] { 
                        GUILayout.Width(littleButtonSize), GUILayout.Height(littleButtonSize)
                    };
                }
                return _littleButtonOptions;
            }
        }

        static GUILayoutOption _iconButtonOptions;
        static GUILayoutOption iconButtonOptions {
            get {
                if (_iconButtonOptions == null) _iconButtonOptions = GUILayout.Width(iconButtonWidth);
                return _iconButtonOptions;
            }
        }
        public const int iconButtonWidth = 24;
        public const int littleButtonSize = 12;

        public static bool DrawToggleButton (bool value, GUIContent content, float x, float y, Color32 onColor, Color32 offColor) {
            if (IconButton(x, y, content, value ? onColor : offColor)) {
                value = !value;
            }
            return value;
        }
        public static bool DrawToggleButton (SerializedProperty prop, GUIContent content, float x, float y, Color32 onColor, Color32 offColor) {
            prop.boolValue = DrawToggleButton (prop.boolValue, content, x, y, onColor, offColor);
            return prop.boolValue;
        }
            
        public static void DrawIconPrefixedField (float x, float y, float w, float h, SerializedProperty prop, GUIContent content, Color32 color) {
            IconButton(x, y, content, color);
            float offset = iconButtonWidth + toolbarDividerSize;
            SetRect(x + offset, y, w - offset, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(_rect, prop, noContent, true);
        }

        public static float singleLineHeight { get { return EditorGUIUtility.singleLineHeight * 1.1f; } }
            

        public static void Label (Rect rect, GUIContent content, Color32 color, GUIStyle style) {
            Color32 textCol = style.normal.textColor;
            style.normal.textColor = color;
            
            GUI.Label(rect, content, style);
            
            style.normal.textColor = textCol;
            
        }


        public static void Box (Rect rect, Color32 color) {
            GUI.backgroundColor = color;
            // GUI.Box( rect, string.Empty );
            
            GUI.BeginGroup(rect, GUI.skin.box);
            GUI.EndGroup();
            
            GUI.backgroundColor = white;
        }

        public static bool LittleButton (float x, float y, GUIContent content, Color32 color) {
            return Button(x, y, littleButtonSize, littleButtonSize, content, color, miniButton, black);
        }
        public static bool LittleButton (GUIContent content, Color32 color) {
            return Button(content, color, miniButton, black, littleButtonOptions);
        }
        public static bool IconButton (float x, float y, GUIContent content, Color32 color) {            
            return Button(x, y, iconButtonWidth, EditorGUIUtility.singleLineHeight, content, color, toolbarButton, black);
        }
        public static bool IconButton (GUIContent content, Color32 color) {
            return Button(content, color, toolbarButton, black, iconButtonOptions);
        }

        public static bool Button (float x, float y, float w, float h, GUIContent content, Color32 color, GUIStyle style, Color32 textColor) {
            SetRect(x, y, w, h);
            Color32 textCol = style.normal.textColor;
            style.normal.textColor = textColor;
            GUI.backgroundColor = color;
            bool pressed = GUI.Button(_rect, content, style);
            GUI.backgroundColor = white;

            style.normal.textColor = textCol;
            return pressed;
        }
        public static bool Button (GUIContent content, Color32 color, GUIStyle style, Color32 textColor, params GUILayoutOption[] layoutOptions) {
            Color32 textCol = style.normal.textColor;
            style.normal.textColor = textColor;
            
            GUI.backgroundColor = color;
            bool pressed = GUILayout.Button(content, style, layoutOptions);
            
            GUI.backgroundColor = white;
            style.normal.textColor = textCol;
            return pressed;
        }

        public const float toolbarDividerSize = 5;

        public static void DrawToolbarDivider (float x, float y) {
            SetRect(x, y, toolbarDividerSize, EditorGUIUtility.singleLineHeight);
            GUI.backgroundColor = GUITools.darkGray;
            GUI.BeginGroup(_rect, toolbar);
            GUI.EndGroup();
            GUI.backgroundColor = GUITools.white;
        }
        static void SetSize (float w, float h) {
            _rect.width = w;
            _rect.height = h;
        }
        static void SetPosition (float x, float y) {
            _rect.x = x;
            _rect.y = y;
        }
        static void SetSize (Vector2 size) {
            SetSize(size.x, size.y);
        }
        static void SetPosition (Vector2 position) {
            SetPosition(position.x, position.y);
        }

        static void SetRect (float x, float y, float w, float h) {
            SetPosition(x, y);
            SetSize(w, h);
        }

        public static void StringFieldWithDefault (float x, float y, float w, float h, SerializedProperty prop, string defaultString) {
            string s = prop.stringValue;
            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s)) prop.stringValue = defaultString;
            SetRect(x, y, w, h);
            EditorGUI.PropertyField(_rect, prop, GUITools.noContent);
        }
        public static string StringFieldWithDefault (float x, float y, float w, float h, string value, string defaultString) {
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value)) value = defaultString;
            SetRect(x, y, w, h);
            return EditorGUI.TextField(_rect, GUITools.noContent, value);
        }
    }
}


#endif
