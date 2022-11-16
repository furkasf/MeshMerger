using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

/// <summary>
/// that tool created by furkan tural [github : @furkasf]
/// I was made tool after play with meshes ,after looking that play goes to far i decide made mesh utality merging tool , you are feel free use that tool in your any project and any optimization
/// and improvment idea's wellcome
/// </summary>

public class MeshBachingAndBuilderManagar : MonoBehaviour
{
    [SerializeField] byte _max = 40;

    /// <summary>
    /// Warning => this script execpt your all mergable objects has a meshfilter , mesh renderer and material(s) associate with it
    /// limitation => that script just work on direct children of game object if you has a very complex prefabed model you will need to convert all thils 
    /// paret to this object
    /// </summary> 

   

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Renderer _renderer;

    //reak part
    [SerializeField] private List<MeshFilter> childMeshFilters = new List<MeshFilter>();
    [SerializeField] private List<Material> childMaterials = new List<Material>();
    private List<CombineInstance> combines = new List<CombineInstance>();
    [SerializeField] private List<GameObject> Prefabs;

    private void Awake()
    {
        ResetObjectCompanents();
    }

    private void AdvanceMeshMerging()
    {

        ushort materialIndex = 0;
        List<MeshFilter> subMeshFilters= new List<MeshFilter> ();
        List<CombineInstance> combines = new List<CombineInstance>();

        foreach (Component component in GetComponentsInChildren<Component>(false))
        {



            if ((component is MeshFilter))
            {
                MeshRenderer targetRenderer;
                MeshFilter targetFilter = (MeshFilter)component;
                targetFilter.TryGetComponent<MeshRenderer>(out targetRenderer);

                //create conbine intance to create mesh filter for submeshes
                CombineInstance subMeshcombine = new CombineInstance();
                subMeshcombine.mesh = targetFilter.sharedMesh;
                subMeshcombine.subMeshIndex = materialIndex;
                subMeshcombine.transform = Matrix4x4.identity;

                childMeshFilters.Add((MeshFilter)component);
                combines.Add(subMeshcombine);

                component.gameObject.SetActive(false);
            }

           
        }

        //// Flatten into a single mesh.
        //Mesh mesh = new Mesh();
        //mesh.CombineMeshes(combiners.ToArray(), true);
        //submeshes.Add(mesh);


        _meshFilter.mesh = new Mesh();
        _meshFilter.mesh.CombineMeshes(combines.ToArray());
    }

    /// <summary>
    /// Get all childs Meshfilter and create combine instance from that and store in list
    /// create mesh fileter from collective meshinstance data we fetch mesh filters of childs
    /// </summary>
    [ContextMenu(nameof(MergeMeshBasic))]
    private void MergeMeshBasic()
    {
        //optimized way to fetch all child mesh fileter and creat combine instance
        //in one iteration just in case you may want use this script in run time
        foreach (Component component in GetComponentsInChildren<Component>(false))
        {
            if ((component is MeshFilter))
            {
                MeshFilter targetFilter = (MeshFilter)component;

                CombineInstance combine = new CombineInstance();
                combine.mesh = targetFilter.sharedMesh;
                combine.transform = targetFilter.transform.localToWorldMatrix;

                childMeshFilters.Add((MeshFilter)component);
                combines.Add(combine);

                component.gameObject.SetActive(false);
            }
        }

        _meshFilter.mesh = new Mesh();
        _meshFilter.mesh.CombineMeshes(combines.ToArray());
    }


    /// <summary>
    /// a funtion use to merge meshes and return new mesh by give
    /// </summary>
    /// <param name="companents">list of companents of parent objects and child</param>
    /// <returns></returns>
    private static MeshFilter CreateBasicMergedMeshFilter(ref Component[] companents)
    {
        List<MeshFilter> MeshFilters = new List<MeshFilter>();
        List<CombineInstance> combines = new List<CombineInstance>();

        foreach (Component component in companents)
        {
            if ((component is MeshFilter))
            {
                MeshFilter targetFilter = (MeshFilter)component;

                CombineInstance combine = new CombineInstance();
                combine.mesh = targetFilter.sharedMesh;
                combine.transform = targetFilter.transform.localToWorldMatrix;

                MeshFilters.Add((MeshFilter)component);
                combines.Add(combine);

                component.gameObject.SetActive(false);
            }
        }

        MeshFilter meshFilter = new MeshFilter();
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combines.ToArray());

        return meshFilter;
    }

    /// <summary>
    ///  To Be sure Gameobject attached that script doesnt has an any companent other than this script
    /// </summary>
    [ContextMenu(nameof(ResetObjectCompanents))]
    private void ResetObjectCompanents()
    {


        foreach (Component component in GetComponents<Component>())
        {
            if (!(component is Transform) && !(component is MeshBachingAndBuilderManagar))
            {
                DestroyImmediate(component);
            }
        }

        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshRenderer.material = new Material(Shader.Find("Unlit/Texture"));
        _renderer = gameObject.GetComponent<Renderer>();
    }

    [ContextMenu(nameof(GenarateBlockGrid))]
    private void GenarateBlockGrid()
    {
        //tip get material from matfolder and genarate blocks from that matarial and use them when create submeshes each of that
        // assets doestn share same instance so creating submeshes from it doesnt crate problem

        //// Create a simple material asset

        //Material materiala = new Material(Shader.Find("Specular"));
        //materiala.color = Color.blue;
        //AssetDatabase.CreateAsset(materiala, "Assets/MatFolder/MyMaterial.mat");

        //// Print the path of the created asset
        //Debug.Log(AssetDatabase.GetAssetPath(materiala));

        byte listCount = (byte)Prefabs.Count;

        for (int a = 0; a < _max; a++)
        {
            GameObject obj = Instantiate(Prefabs[Random.Range(0, listCount)], transform);
            obj.transform.localPosition += new Vector3(a, 0, 0);

            for (int j = 1; j < _max; j++)
            {
                GameObject obj2 = Instantiate(Prefabs[Random.Range(0, listCount)], transform);
                obj2.transform.localPosition += new Vector3(obj.transform.position.x, j, 0);
            }
        }

        //_list.Add(obj);
        //MaterialPropertyBlock _prop = new MaterialPropertyBlock();

        //MergeMeshBasic();
        //StaticBatchingUtility.Combine(_list.ToArray(), gameObject);
    }

    [ContextMenu(nameof(KillAllChilds))]
    private void KillAllChilds()
    {
        List<Transform> child = new List<Transform>();
        child.AddRange(transform.GetComponentsInChildren<Transform>());

        for (int i = 0; i < child.Count; ++i)
        {
            if (child[i] == transform) continue;

            DestroyImmediate(child[i].gameObject);
        }
    }

    [ContextMenu(nameof(ChangeAllNestedChildParen))]
    private void ChangeAllNestedChildParen()
    {
        foreach(Transform transform in GetComponentsInChildren<Transform>())
        {
            if(transform.childCount > 0)
            {
                foreach(Transform child in transform.GetComponentsInChildren<Transform>())
                {
                    child.parent = this.transform;
                }
            }
        }
        return;

        
    }

}