using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MeshDistortLite
{
    /// <summary>
    /// Hold the information of a mesh and the data used to generated distortions for it.
    /// </summary>
    [System.Serializable]
    public class MeshDistortData
    {
        /// <summary>
        /// The mesh that will be applied distortios to
        /// </summary>
        public Mesh mesh;



        /// <summary>
        /// MeshFilter of the gameObject
        /// </summary>
        public MeshFilter filter;

        /// <summary>
        /// Original Material used by the GameObject
        /// </summary>
        public Material originalMaterial;
        
        /// <summary>
        /// The transform from the mesh GameObject
        /// </summary>
        public Transform meshTransform;

        public Matrix4x4 localToWorldMatrix
        {
            get
            {
                if (skin == null)
                {
                    return meshTransform.localToWorldMatrix;
                }
                else
                {
                    return skinLocalToWorldMatrix;
                }
            }
        }
        public Matrix4x4 worldToLocalMatrix
        {
            get
            {
                if (skin == null)
                {
                    return meshTransform.worldToLocalMatrix;
                }
                else
                {
                    return skinWorldToLocalMatrix;
                }
            }
        }

        protected Matrix4x4 skinLocalToWorldMatrix;
        protected Matrix4x4 skinWorldToLocalMatrix;


        private Mesh bakedMesh = new Mesh();
        /// <summary>
        /// Hold the values for the vertices without any distortion applied
        /// </summary>
        public Vector3[] skinVertices
        {
            get
            {
                if (skin == null)
                {
                    return originalVertices;
                }
                else
                {
                    mesh.vertices = originalVertices;
                    Transform parent = skin.transform.parent;
                    Vector3 scale = skin.transform.localScale;

                    skin.transform.parent = null;
                    skin.transform.localScale = Vector3.one;
                    skin.BakeMesh(bakedMesh);

                    skinLocalToWorldMatrix = skin.transform.localToWorldMatrix;
                    skinWorldToLocalMatrix = skin.transform.worldToLocalMatrix;

                    skin.transform.parent = parent;
                    skin.transform.localScale = scale;

                    return bakedMesh.vertices;

                }
            }
        }

        /// <summary>
        /// Hold the values for the vertices without any distortion applied
        /// </summary>
        public Vector3[] originalVertices;

        public SkinnedMeshRenderer skin; 


        public ComputeBuffer verticeBuffer;
        public ComputeBuffer matrixBuffer;

        public MeshDistortData(Transform transform, Material material, MeshFilter filter)
        {
            
            this.filter = filter;
            this.originalMaterial = material;

            meshTransform = transform;
            UpdateMesh();
            originalVertices = mesh.vertices;

        }

        public Transform[] bones;
        public Transform root;
        public MeshDistortData(Transform transform, Material material, SkinnedMeshRenderer skin)
        {
            this.skin = skin;
            this.originalMaterial = material;

            meshTransform = transform;
            UpdateMesh();
            originalVertices = mesh.vertices;


            if (Application.isPlaying) {
                bones = skin.bones;
                root = skin.rootBone;
            }

        }

        public void CreateBuffers()
        {
            ReleaseBuffers();
            verticeBuffer = new ComputeBuffer(originalVertices.Length, 12);
            matrixBuffer = new ComputeBuffer(2, 16 * 4);
        }
        public void ReleaseBuffers()
        {
            if (verticeBuffer != null)
            {
                verticeBuffer.Dispose();
                verticeBuffer = null;
            }
            if (matrixBuffer != null)
            {
                matrixBuffer.Dispose();
                matrixBuffer = null;
            }
        }

       
        public void BufferSet(ComputeShader shader, int kernel)
        {
            shader.SetBuffer(kernel, "vertices", verticeBuffer);
            shader.SetBuffer(kernel, "matrixList", matrixBuffer);
        }

        

        /// <summary>
        /// Update the mesh to apply the distortions later on.
        /// </summary>
        public void UpdateMesh()
        {
            //If the game is playing, get the real mesh to apply the distortions
            if (Application.isPlaying)
            {
                if (skin != null)
                {
                    skin.sharedMesh = GameObject.Instantiate<Mesh>(skin.sharedMesh);
                    mesh = skin.sharedMesh;
                }
                else
                {
                    mesh =  filter.mesh;
                }
            }
            else
            {
                //If it is not playing, create a clone of the mesh to show the user the distortions without the need to play the game.

                mesh = GameObject.Instantiate<Mesh>(filter != null ? filter.sharedMesh : skin.sharedMesh);
                mesh.hideFlags = HideFlags.HideAndDontSave;
            }

        }

        /// <summary>
        /// Reset the mesh to its default values.
        /// </summary>
        public void ResetMesh()
        {
            mesh.vertices = originalVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
        
    }

}