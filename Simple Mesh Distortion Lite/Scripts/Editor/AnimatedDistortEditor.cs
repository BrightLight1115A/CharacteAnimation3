using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MeshDistortLite
{
    /// <summary>
    /// Custom Editor to the AnimatedDistort script
    /// </summary>
    [CustomEditor(typeof(AnimatedDistort))]
    public class AnimatedDistortEditor : Editor
    {

        public override void OnInspectorGUI()
        {

            //Casting target
            AnimatedDistort myTarget = (AnimatedDistort)target;

            Undo.RecordObject(target, "Update Animation");
            EditorGUI.BeginChangeCheck();

            //Option to animate, dynamic or one of the saved animations
            List<string> options = new List<string>();
            options.Add("Dynamic (Calculate in realtime)");

            myTarget.isPlaying = EditorGUILayout.Toggle("Play on start", myTarget.isPlaying);

            //Type of animation to play
            myTarget.playAnimationIndex = EditorGUILayout.Popup("Play", myTarget.playAnimationIndex, options.ToArray());

            //Animations values
            myTarget.animate = (AnimatedDistort.Animate)EditorGUILayout.EnumPopup(new GUIContent("Animate", "Property to be animated"), myTarget.animate);


            EditorGUILayout.BeginVertical("box");
            myTarget.type = (AnimatedDistort.Type)EditorGUILayout.EnumPopup(new GUIContent("Type of animation", "Type that the value will be modified"), myTarget.type);

            
            if (myTarget.type == AnimatedDistort.Type.constant)
            {
                myTarget.constantSpeed = EditorGUILayout.FloatField("Anim. Speed", myTarget.constantSpeed);
            }
            else if (myTarget.type == AnimatedDistort.Type.curve)
            {
                myTarget.constantSpeed = EditorGUILayout.FloatField("Anim. Speed", myTarget.constantSpeed);
                myTarget.curveType = EditorGUILayout.CurveField("Curve", myTarget.curveType);
            }
            else
            {
                myTarget.constantSpeed = EditorGUILayout.FloatField("Anim. Speed", myTarget.constantSpeed);
                myTarget.minValue = EditorGUILayout.FloatField("Min. Value", myTarget.minValue);
                myTarget.maxValue = EditorGUILayout.FloatField("Max. Value", myTarget.maxValue);
            }

            EditorGUILayout.EndVertical();

            //Save animations still in development


            EditorGUILayout.Separator();

            

            //Saved animations data
            EditorGUILayout.LabelField("Animations:", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.HelpBox("Animations only possible in the Complete version.", MessageType.Warning);

           
            EditorGUILayout.EndVertical();
            
        }

    }
}