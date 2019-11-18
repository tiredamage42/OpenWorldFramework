
using System.Collections.Generic;
using UnityEngine;

using UnityTools.EditorTools;
using UnityTools.Internal;
using UnityEditor;
namespace UnityTools {
    
    [System.Serializable] public class Messages : NeatArrayWrapper<Message> { 
        
        public void Invoke (Messages messages, GameObject subject, GameObject target) {
            if (messages == null) 
                return;

            for (int i = 0; i < messages.Length; i++)
                messages[i].Invoke( subject, target );
        }
    }

}
namespace UnityTools.Internal {
    
    [System.Serializable] public class Message {

        public RunTarget runTarget;
        public GameObject referenceTarget;
        public string callMethod;
        public MessageParameters parameters;
        public bool showParameters;


        public void Invoke (GameObject subject, GameObject target) {
            
            object[] suppliedParameters;
            GameObject obj;

            if (!Messaging.PrepareForMessageSend(callMethod, runTarget, subject, target, referenceTarget, parameters, out obj, out suppliedParameters))
                return;
        
            if (runTarget == RunTarget.Static) {
                Messaging.CallStaticMethod(callMethod, suppliedParameters);
            }
            else {
                obj.CallMethod ( callMethod, suppliedParameters);
            }   
        }
    }

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Messages))] class MessagesDrawer : NeatArrayWithMessageParametersInElements { }

    /*
        DRAW A SINGLE CONDITION:
    */
    [CustomPropertyDrawer(typeof(Message))] 
    class MessageDrawer : FieldWithMessageDrawer {
        
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);

            float origX = pos.x;
            float origWidth = pos.width;

            SerializedProperty runTargetProp = DrawRunTargetAndCallMethod (ref pos, prop, origWidth - (65 + GUITools.toolbarDividerSize + GUITools.iconButtonWidth));

            GUITools.DrawToolbarDivider(pos.x, pos.y);
            pos.x += GUITools.toolbarDividerSize;
            
            DrawEnd (ref pos, prop, origX, origWidth, runTargetProp);
            
            EditorGUI.EndProperty();
        }
    }

    #endif






}
