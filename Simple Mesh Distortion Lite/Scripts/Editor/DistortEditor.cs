using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;

namespace MeshDistortLite
{
    /// <summary>
    /// Custom editor to the DistortEditor script
    /// </summary>
    [CustomEditor(typeof(Distort))]
    public class DistortEditor : Editor
    {
        //target with cast
        Distort myTarget;
        //Preview window
        PreviewRenderUtility previewRenderUtility;

        ReorderableList reList;

        public void OnEnable()
        {
            myTarget = (target as Distort);

            previewRenderUtility = new PreviewRenderUtility(false);

            previewRenderUtility.cameraFieldOfView = 30f;

            previewRenderUtility.camera.farClipPlane = 1000;
            previewRenderUtility.camera.nearClipPlane = 0.3f;

            //Set the camera to be a little far away from the model.
            previewRenderUtility.camera.transform.position = Vector3.back * (myTarget.combinedBounds.size.magnitude * 2) ;


            reList = new ReorderableList(myTarget.distort, typeof(DistortData), true, false, false, false);

            reList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Effects order:");
            };

            reList.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) => {
                //var element = reList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                myTarget.distort[index].name = EditorGUI.TextField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    myTarget.distort[index].name);
                /*
                EditorGUI.PropertyField(
                    new Rect(rect.x + 60, rect.y, rect.width - 60 - 30, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("Prefab"), GUIContent.none);
                EditorGUI.PropertyField(
                    new Rect(rect.x + rect.width - 30, rect.y, 30, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("Count"), GUIContent.none);
                    */
            };
        }

        public void OnDisable()
        {
            previewRenderUtility.Cleanup();
            Undo.undoRedoPerformed -= OnUndo;
        }

        public DistortEditor()
        {
            Undo.undoRedoPerformed += OnUndo;
        }


        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("Distorted mesh");
        }

       

        public void OnUndo()
        {
            myTarget = (target as Distort);
            if (myTarget.updateIntEditor)
                myTarget.UpdateDistort();
        }

       


        public override void OnInspectorGUI()
        {

            //Get all the distortions
            System.Collections.Generic.List<DistortData> distort = myTarget.distort;

            Undo.RecordObject(target, "Changing distortion");

            myTarget.calculation = (Distort.Calculate)EditorGUILayout.EnumPopup(new GUIContent("Calculate", "Do calculations in local or global position."), myTarget.calculation);

            GUILayout.Toggle(false, new GUIContent("Calculate in GPU (Ony in the Full Version)", "Only in the full version."));

            if (myTarget.calculateInGPU)
            {
                myTarget.distortShader = EditorGUILayout.ObjectField("Shader", myTarget.distortShader, typeof(ComputeShader), false) as ComputeShader;
            }

            //Debug options
            myTarget.showPreviewWindow = GUILayout.Toggle(myTarget.showPreviewWindow, new GUIContent("Show Preview Window", "Show the preview window in the editor."));
            myTarget.showMeshInEditor = GUILayout.Toggle(myTarget.showMeshInEditor, new GUIContent("Show Mesh in Editor", "Show a mesh preview in the editor window."));
            myTarget.showDebugLines = GUILayout.Toggle(myTarget.showDebugLines, new GUIContent("Show Debug Lines", "Show lines in the editor to help to see the distort effects."));

           
            if (myTarget.showDebugLines)
            {
                myTarget.debugLinesDistance = EditorGUILayout.Slider(new GUIContent("Debug line distance", "Distance from the line between the center and the borders of the mesh"), myTarget.debugLinesDistance, 0, 1);
            }


            //Save the distorted mesh as a file
            if (GUILayout.Button(new GUIContent("Save Mesh as File", "Save a file of the mesh in the asset folder.")))
            {
                SaveMesh();
            }

           

            reList.DoLayoutList();

            //Set the distortion boxes
            if (distort != null)
            {
                for (int i = 0; i < distort.Count; i++)
                {

                    distort[i].showInEditor = EditorGUILayout.Foldout(distort[i].showInEditor, (i + 1) + ": " +distort[i].name, true);


                    if (distort[i].showInEditor)
                    {
                        EditorGUILayout.BeginVertical("box");
                        distort[i].enabled = EditorGUILayout.Toggle("Enabled", distort[i].enabled);
                        distort[i].calculateInWorldSpace = EditorGUILayout.Toggle(new GUIContent("World Space", "Calculate the distorion in world or local space"), distort[i].calculateInWorldSpace);
                        distort[i].isPingPong = EditorGUILayout.Toggle(new GUIContent("Use PingPong", "Repeat the distorion forever in a pingpong manner"), distort[i].isPingPong);

                        distort[i].animationSpeed = EditorGUILayout.FloatField(new GUIContent("Animation Mult.", "Speed up or down the animation speed of this distortion"), distort[i].animationSpeed);
                        distort[i].type = (Distort.Type)EditorGUILayout.EnumPopup("Type", distort[i].type);

                        distort[i].force = EditorGUILayout.FloatField(new GUIContent("Force", "How much force this distortion will have"), distort[i].force);
                        distort[i].movementDisplacement = EditorGUILayout.FloatField(new GUIContent("Force Displacement", "Displace the distortion force, mostly used for animation"), distort[i].movementDisplacement);

                      
                        distort[i].tile = EditorGUILayout.Vector3Field(new GUIContent("Tile: ", "How much times the distortion will repeat between bounds"), distort[i].tile);

                        EditorGUILayout.BeginVertical("box");

                        EditorGUILayout.LabelField(new GUIContent("Displaced Force:", "Calculated WITH the Force Displacement value."));
                        distort[i].displacedForceXY = EditorGUILayout.CurveField(new GUIContent("X by Y", "Displace the X axis by the Y position"), distort[i].displacedForceXY);
                        distort[i].displacedForceXZ = EditorGUILayout.CurveField(new GUIContent("X by Z", "Displace the X axis by the Z position"), distort[i].displacedForceXZ);

                        EditorGUILayout.Space();

                        distort[i].displacedForceYX = EditorGUILayout.CurveField(new GUIContent("Y by X", "Displace the Y axis by the X position"), distort[i].displacedForceYX);
                        distort[i].displacedForceYZ = EditorGUILayout.CurveField(new GUIContent("Y by Z", "Displace the Y axis by the Z position"), distort[i].displacedForceYZ);

                        EditorGUILayout.Space();

                        distort[i].displacedForceZX = EditorGUILayout.CurveField(new GUIContent("Z by X", "Displace the Z axis by the X position"), distort[i].displacedForceZX);
                        distort[i].displacedForceZY = EditorGUILayout.CurveField(new GUIContent("Z by Y", "Displace the Z axis by the Y position"), distort[i].displacedForceZY);

                        
   
                      

                        EditorGUILayout.Space();


                        EditorGUILayout.LabelField(new GUIContent("Static force:", "Calculated WITHOUT the Force Displacement value."));
                        distort[i].staticForceX = EditorGUILayout.CurveField(new GUIContent("X", "Displacement in the X axis"), distort[i].staticForceX);
                        distort[i].staticForceY = EditorGUILayout.CurveField(new GUIContent("Y", "Displacement in the Y axis"), distort[i].staticForceY);
                        distort[i].staticForceZ = EditorGUILayout.CurveField(new GUIContent("Z", "Displacement in the Z axis"), distort[i].staticForceZ);
                        EditorGUILayout.EndVertical();

                        //Button to remove the distortion
                        if (GUILayout.Button("Delete this distortion."))
                        {
                            if (EditorUtility.DisplayDialog("Delete Distortion", "Do you really want to remove this distortion?", "Remove", "Cancel"))
                            {
                                myTarget.RemoveDistort(i);
                            }
                        }

                        EditorGUILayout.EndVertical();
                    }
                }
            }

            EditorGUILayout.Space();

            //Button to add a new distortion
            if (GUILayout.Button("Add New Distortion"))
            {
                if(target != null)
                    myTarget.AddDistortion();
            }




            if (GUI.changed)
            {

                if (myTarget.updateIntEditor)
                {
                    myTarget.EditParameters();
                    myTarget.UpdateDistort();
                }
                EditorUtility.SetDirty(target);
            }

        }

        public override bool HasPreviewGUI()
        {
            return myTarget.showPreviewWindow;
        }

        public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
        {

            var drag = Vector2.zero;

            //Get drag value
            if (Event.current.type == EventType.MouseDrag)
            {
                drag = Event.current.delta;
            }

            previewRenderUtility.BeginPreview(r, background);

            //Set each mesh to be rendered in the preview window
            if(myTarget.meshList != null)
            foreach (MeshDistortData data in myTarget.meshList) {
                if (data.meshTransform == null)
                    continue;
                Matrix4x4 matrix = Matrix4x4.TRS(data.meshTransform.position - myTarget.transform.position, data.meshTransform.rotation, data.meshTransform.lossyScale);
                previewRenderUtility.DrawMesh(data.mesh, matrix, data.originalMaterial, 0);
            }
            var previewCamera = previewRenderUtility.camera;

            //Rotate camera by the drag
            previewCamera.transform.RotateAround(Vector3.zero, Vector3.up, -drag.x);
            previewCamera.transform.RotateAround(Vector3.zero, Vector3.right, -drag.y);

            previewCamera.Render();

            previewRenderUtility.EndAndDrawPreview(r);

            if (drag != Vector2.zero)
                Repaint();

        }

        
        public override void OnPreviewSettings()
        {
            GUIStyle preLabel = new GUIStyle("preLabel");
            GUILayout.Label("Distorted preview", preLabel);
        }


        /// <summary>
        /// Save a distorted mesh as a file
        /// </summary>
        public void SaveMesh()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                             "Save Mesh",
                             myTarget.name,
                             "asset",
                             "Sabe Distort Mesh");
            if (path == "")
                return;

            //Remove the .asset from the name
            path = path.Substring(0, path.Length - 6);

            int count = 1;

            //Generate a file for each mesh in the list
            foreach (MeshDistortData data in myTarget.meshList)
            {
                Mesh tempMesh = (Mesh)UnityEngine.Object.Instantiate(data.mesh);

                AssetDatabase.CreateAsset(tempMesh, path + "_" + count.ToString() + ".asset");
                AssetDatabase.SaveAssets();

                count++;
            }
        }


    }
}