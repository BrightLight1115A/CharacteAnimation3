using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace MeshDistortLite
{
    /// <summary>
    /// Generate the distortions applied to the mesh.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class Distort : MonoBehaviour
    {
        /// <summary>
        /// Possible types of distortion
        /// </summary>
        public enum Type
        {
            Stretch
        }

        public enum Calculate
        {
            global,
            local

        }

        /// <summary>
        /// Do the calculations locally or globally?
        /// </summary>
        public Calculate calculation;

        /// <summary>
        /// Update the mesh while in EditorMode
        /// </summary>
        [HideInInspector]
        public bool updateIntEditor = true;

        /// <summary>
        /// List of all the distortions of this mesh
        /// </summary>
        public List<DistortData> distort = new List<DistortData>();


        /// <summary>
        /// List of all meshes to apply the distortion
        /// </summary>
        [System.NonSerialized]
        public List<MeshDistortData> meshList;

        /// <summary>
        /// All bounds of the meshList combined into one
        /// </summary>
        public Bounds combinedBounds;

        /// <summary>
        /// Show the debug lines in the editor
        /// </summary>
        public bool showDebugLines = false;
        /// <summary>
        /// Distance from the center of the model of the debug lines
        /// </summary>
        public float debugLinesDistance = 1;
        /// <summary>
        /// All the points of the debug lines
        /// </summary>
        public Vector3[,] debugLines;

        /// <summary>
        /// Show the distort mesh in the editor
        /// </summary>
        public bool showMeshInEditor = true;

        /// <summary>
        /// Show the distort mesh in the editor
        /// </summary>
        public bool showPreviewWindow = true;

        public bool calculateInGPU = false;
        public ComputeShader distortShader;
        protected int dirtortKernel;

        private bool hasSkinnedMesh = false;
        private AnimatedDistort animDistort;

        void Awake()
        {
            //Set up and distort the mesh on the start
            //SetVertices();
            //UpdateInCPU();

            animDistort = GetComponent<AnimatedDistort>();

        }


        void Reset()
        {
            /*
            //If there is no vertices, create one before updating the distortion
            if (meshList == null)
                SetVertices();

            //Update the distortions
            ResetVertices();
            */

        }

        void OnEnable()
        {
            //If there is no vertices, create one before updating the distortion
            if (meshList == null)
                SetVertices();

            if (Application.isPlaying)
            {
                foreach (MeshDistortData data in meshList)
                {
                    if (data.skin != null)
                    {
                        data.skin.enabled = false;
                        hasSkinnedMesh = true;
                    }
                }
            }


          

            UpdateDistort();
        }

        

        /// <summary>
        /// Get all the distorted meshes as a array
        /// </summary>
        /// <returns></returns>
        public Object[] GetAllMeshes()
        {
            List<Object> obj = new List<Object>();

            foreach (MeshDistortData data in meshList)
            {
                obj.Add(data.mesh);
            }

            return obj.ToArray();
        }

        /// <summary>
        /// Set the debug lines to show in the editor
        /// </summary>
        void SetDebugLines()
        {
            //Qtd of points that each line will have
            int lineQtd = 10;

            //Each axis have 4 lines, so (3 axis * 4 lines) have X points
            debugLines = new Vector3[3 * 4, lineQtd];

            //Make the line have a minimum width, so the line don't disapear if a axis have a bound of 0
            float minLineWidth = 0.2f;

            //Same as combinedBounds, but set a minimum value for each extend.
            Bounds myBound = combinedBounds;

            Vector3 extend = myBound.extents;
            if (extend.x < minLineWidth)
                extend.x = minLineWidth;
            if (extend.y < minLineWidth)
                extend.y = minLineWidth;
            if (extend.z < minLineWidth)
                extend.z = minLineWidth;
            myBound.extents = extend;

            //Extend is the border of the model, it multiply with the debugLinesDistance to make the lines render between 0 and distance.
            extend *= debugLinesDistance;

            //Generate the points of the debug line
            for (int i = 0; i < lineQtd; i++)
            {
                //Difference between the bound min and max
                float diffX = myBound.max.x - myBound.min.x;
                float diffY = myBound.max.y - myBound.min.y;
                float diffZ = myBound.max.z - myBound.min.z;

                //Position of the line between the Bound min and Bound Max
                float x = myBound.min.x + (diffX * (i / (float)lineQtd));
                float y = myBound.min.y + (diffY * (i / (float)lineQtd));
                float z = myBound.min.z + (diffZ * (i / (float)lineQtd));

                //Position of the 4 lines in the X axis
                debugLines[0, i] = new Vector3(x, myBound.center.y + extend.y, myBound.center.z);
                debugLines[1, i] = new Vector3(x, myBound.center.y - extend.y, myBound.center.z);
                debugLines[2, i] = new Vector3(x, myBound.center.y, myBound.center.z + extend.z);
                debugLines[3, i] = new Vector3(x, myBound.center.y, myBound.center.z - extend.z);

                //Position of the 4 lines in the Y axis
                debugLines[4, i] = new Vector3(myBound.center.x + extend.x, y, myBound.center.z);
                debugLines[5, i] = new Vector3(myBound.center.x - extend.x, y, myBound.center.z);
                debugLines[6, i] = new Vector3(myBound.center.x, y, myBound.center.z + extend.z);
                debugLines[7, i] = new Vector3(myBound.center.x, y, myBound.center.z - extend.z);

                //Position of the 4 lines in the Z axis
                debugLines[8, i] = new Vector3(myBound.center.x + extend.x, myBound.center.y, z);
                debugLines[9, i] = new Vector3(myBound.center.x - extend.x, myBound.center.y, z);
                debugLines[10, i] = new Vector3(myBound.center.x, myBound.center.y + extend.y, z);
                debugLines[11, i] = new Vector3(myBound.center.x, myBound.center.y - extend.y, z);

            }
        }

        /// <summary>
        /// Get all the meshes of children and set up the data and bounds
        /// </summary>
        void SetVertices()
        {
            meshList = new List<MeshDistortData>();

            foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                //Generate data for this mesh
                MeshDistortData data = new MeshDistortData(meshFilter.transform,meshFilter.GetComponent<Renderer>().sharedMaterial, meshFilter);
                meshList.Add(data);
            }

            foreach (SkinnedMeshRenderer rendered in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                //Generate data for this mesh
                MeshDistortData data = new MeshDistortData(rendered.transform, rendered.sharedMaterial, rendered);
                meshList.Add(data);
            }

            //Update the bounds
            SetBounds();
        }

        /// <summary>
        /// Generate a bound encapsulating all the bounds of meshes to be distorted.
        /// </summary>
        void SetBounds()
        {
            combinedBounds = new Bounds();

            foreach (MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                Bounds b = meshFilter.GetComponent<Renderer>().bounds;

                //If first bound, use it, else encapsulate bound.
                if (combinedBounds.size.magnitude == 0)
                    combinedBounds = b;
                else
                    combinedBounds.Encapsulate(b);
            }

            foreach (SkinnedMeshRenderer renderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Bounds b = renderer.bounds;

                //If first bound, use it, else encapsulate bound.
                if (combinedBounds.size.magnitude == 0)
                    combinedBounds = b;
                else
                    combinedBounds.Encapsulate(b);
            }


        }


        /// <summary>
        /// Reset the vertices position to the start position.
        /// </summary>
        void ResetVertices()
        {
            foreach (MeshDistortData data in meshList)
            {
                data.ResetMesh();
            }
        }

        public void EditParameters()
        {
            
        }

        public void LateUpdate()
        {
            if (transform.hasChanged)
            {
                UpdateDistort();
                transform.hasChanged = false;
            }
            else if(hasSkinnedMesh && (animDistort == null || !animDistort.enabled || !animDistort.isPlaying))
            {
                UpdateDistort();
            }


        }


        public void UpdateDistort()
        {
            if (!isActiveAndEnabled)
                return;

           UpdateInCPU();
            
        }
        

       

        /// <summary>
        /// Generate the distortions to the mesh
        /// </summary>
        public void UpdateInCPU()
        {
            Vector3 pos = transform.position;
            Vector3 scale = transform.localScale;
            Quaternion rotation = transform.rotation;

            if (calculation == Calculate.local)
            {
                transform.position = Vector3.zero;
                transform.localScale = Vector3.one;
                transform.rotation = Quaternion.identity;
            }

            //Update bounds
            SetBounds();

            //Vertices to be distorted
            Vector3[] vertices;


            //Matrix to set vertices from local to global
            Matrix4x4 localToGlobal;
            //Matrix to set vertices from global to local
            Matrix4x4 worldToLocal;
            //Qtd of vertices to update
            int verLength;

    
            if (GetComponent<Animator>() != null)
            {
                //avatar = GetComponent<Animator>().avatar;
                //GetComponent<Animator>().avatar = null;
            }


            foreach (MeshDistortData data in meshList)
            {
                if (data.meshTransform == null)
                    continue;

                

                //If there is no mesh to update, try to find it.
                if (data.mesh == null)
                    data.UpdateMesh();


                //Create vertice data using the original vertive data
                vertices = data.skinVertices.Clone() as Vector3[];

                verLength = vertices.Length;
                localToGlobal = data.localToWorldMatrix;
                worldToLocal = data.worldToLocalMatrix;

                //Set the vertices position from local to global.
                for (int i = 0; i < verLength; i++)
                {
                   vertices[i] = localToGlobal.MultiplyPoint3x4(vertices[i]);
                    
                }

                if(enabled)
                //Get distortions to be applied
                foreach (DistortData dist in distort)
                {
                    //Check if distortion is enabled
                    if (dist.enabled)
                    {
                        //Set the bounds for the distortion to use
                        dist.SetBounds(combinedBounds);

                        //Update each vertice with distortion
                        for (int i = 0; i < verLength; i++)
                        {
                            dist.DistortVertice(ref vertices[i]);
                        }
                    }
                }

                //Set the vertices from global position to local
                for (int i = 0; i < verLength; i++)
                {
                   vertices[i] = worldToLocal.MultiplyPoint3x4(vertices[i]);
                }

                //Update vertices data in mesh
                data.mesh.vertices = vertices;
                //Recalculate normals
                data.mesh.RecalculateNormals();

                if (Application.isPlaying && data.skin != null && calculation == Calculate.global)
                {
                    Matrix4x4 matrix = Matrix4x4.TRS(data.meshTransform.position, data.meshTransform.rotation, Vector3.one);

                    Graphics.DrawMesh(data.mesh, matrix, data.originalMaterial, 0);
                }

            }

#if UNITY_EDITOR
            //Update debug lines if they are enabled and the project is in editor mode.
            if (showDebugLines)
                UpdateDebugLines();
#endif

            if (calculation == Calculate.local)
            {

                transform.position = pos;
                transform.localScale = scale;
                transform.rotation = rotation;


                foreach (MeshDistortData data in meshList)
                {
                    if (Application.isPlaying && data.skin != null)
                    {
                        Matrix4x4 matrix = Matrix4x4.TRS(data.meshTransform.position, data.meshTransform.rotation, data.meshTransform.lossyScale);
                        Graphics.DrawMesh(data.mesh, matrix, data.originalMaterial, 0);
                    }
                }

            }

        }

        /// <summary>
        /// Update the positions of the debug lines.
        /// </summary>
        public void UpdateDebugLines()
        {
            //Set up the debug lines
            SetDebugLines();

            //Update debug points using the distortions
            for (int line = 0; line < debugLines.GetLength(0); line++)
            {
                for (int point = 0; point < debugLines.GetLength(1); point++)
                {
                    Vector3 pos = debugLines[line, point];

                    for (int d = 0; d < distort.Count; d++)
                    {
                        if (distort[d].enabled)
                        {
                            distort[d].DistortVertice(ref pos);
                        }
                    }

                    debugLines[line, point] = pos;
                }
            }
        }


        /// <summary>
        /// Mark all meshes as dynamic.
        /// </summary>
        public void MakeDynamic()
        {
            foreach (MeshDistortData data in meshList)
            {
                data.mesh.MarkDynamic();
            }
        }


        /// <summary>
        /// Add a new distortion to the mesh
        /// </summary>
        public void AddDistortion()
        {
            DistortData data = new DistortData();

            ///If there is no distortion, create a new list
            if (distort == null)
                distort = new List<DistortData>();

            distort.Add(data);
        }

        /// <summary>
        /// Remove a distortion from the mesh
        /// </summary>
        /// <param name="index">Index to remove from the list</param>
        public void RemoveDistort(int index)
        {
            distort.RemoveAt(index);
        }


        void OnDrawGizmos()
        {
            //Show mesh in the editor as a gizmo.
            if (showMeshInEditor && !Application.isPlaying && isActiveAndEnabled)
            {
                foreach (MeshDistortData data in meshList)
                {
                    if (data.meshTransform != null)
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawMesh(data.mesh, data.meshTransform.position, data.meshTransform.rotation, data.meshTransform.lossyScale);
                    }
                }
            }

            //Show debug lines in editor
            if (showDebugLines)
            {
                if (debugLines == null)
                    SetDebugLines();

                Vector3 lastPos = Vector3.zero;

                for (int line = 0; line < debugLines.GetLength(0); line++)
                {
                    //If is the first 4 lines (X axis) paint red
                    if (line <= 3)
                        Gizmos.color = Color.red;
                    //If is (Y axis) paint green
                    else if (line <= 7)
                        Gizmos.color = Color.green;
                    //If is (Z axis) paint blue
                    else
                        Gizmos.color = Color.blue;

                    for (int point = 1; point < debugLines.GetLength(1); point++)
                    {
                        Gizmos.DrawLine(debugLines[line, point-1], debugLines[line, point]);
                    }
                }
            }
        }
    }
}