using System.Collections;
using System.Collections.Generic;
using Geogram;
using UnityEngine;
using WyrmTale;

public class SceneObject : MonoBehaviour
{
    public Transform modelTransform;
    public SpriteRenderer selectCircle;
    public Collider circleCollider;
    public float radius;
    public Renderer[] specialShaderRenderer;
    public string specialShaderID;
    private Renderer[] meshRenderers;
    private Vector3 targetPosition;
    private float step = 1f;
    private float currentHeight;
    private float currentScale;
    private float currentRotation;
    private bool isSelected;
    private bool isOverlapping;
    private Vector3 onSelectPosition;
    private int arCode;

    public bool IsSelected { get { return isSelected; } }
    public int ARCode { set { arCode = value; }}
    public bool IsOverlapping
    {
        get { return isOverlapping; }
        set
        {
            if (isOverlapping == value)
                return;

            isOverlapping = value;
            if (value)
            {
                selectCircle.color = Utils.OVERLAPPING_CIRCLE_COLOR;
                SetShaderProperty(Utils.SHADER_OUTLINE_COLOR_ID, Utils.OVERLAPPING_CIRCLE_COLOR);
            }
            else
            {
                selectCircle.color = Utils.FREE_CIRCLE_COLOR;
                SetShaderProperty(Utils.SHADER_OUTLINE_COLOR_ID, Utils.FREE_CIRCLE_COLOR);
            }
        }
    }// Use this for initialization

    private void Awake()
    {
        meshRenderers = GetComponentsInChildren<Renderer>(); //OBJECT MUST NOT HAVE ANY OTHER TYPE OF RENDERER, BESIDES MESH AND SKINNED MESH 

        currentScale = 1;
        isSelected = false;
    }

    protected virtual void Start()
    {
        //Debug.Log("name: " + name);
        selectCircle.transform.localScale = Vector3.one * radius;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //transform.position = Vector3.MoveTowards(transform.position, targetPosition, step * Time.deltaTime);
        if (isSelected)
        {
            IsOverlapping = Physics.CheckSphere(selectCircle.transform.position, radius * currentScale, Utils.SCENE_OBJECT_LAYER_MASK);
        }
    }

    public void Move(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    public void Height(float deltaHeight, bool set = false)
    {
        if (set)
            currentHeight = deltaHeight;
        else
            currentHeight += deltaHeight;

        modelTransform.localPosition = Vector3.up * currentHeight;
    }

    public void Scale(float scaleDelta, bool set = false)
    {
        if (set)
            currentScale = scaleDelta;
        else
            currentScale += scaleDelta;

        transform.localScale = Vector3.one * currentScale;
    }

    public void Rotate(float rotateDelta, bool set = false)
    {
        if (set)
            currentRotation = rotateDelta;
        else
            currentRotation += rotateDelta;

        //tranform.localRotation = new Vector3(0,currentRotation,0);
        transform.Rotate(Vector3.up, rotateDelta);
    }

    public void SetLocalPosition(Vector3 position)
    {
        transform.localPosition = position;
        //targetPosition = transform.position;
    }

    public void Select()
    {
        onSelectPosition = transform.position;
        isSelected = true;
        selectCircle.color = Utils.SELECTED_CIRCLE_COLOR;
        SetShader(Utils.SHADER_OUTLINE);
        SetShaderProperty(Utils.SHADER_OUTLINE_COLOR_ID, Utils.FREE_CIRCLE_COLOR);
        //SetShaderProperty(Utils.SHADER_OUTLINE_THICKNESS_ID, Utils.SHADER_OUTLINE_SELECTED_THICKNESS);
        circleCollider.enabled = false;
    }

    public void Deselect()
    {
        isSelected = false;
        selectCircle.color = Utils.IDLE_CIRCLE_COLOR;
        SetShader("Standard", true);
        //SetShaderProperty(Utils.SHADER_OUTLINE_THICKNESS_ID, 0f);
        circleCollider.enabled = true;
        //if (isOverlapping)
          //  Move(onSelectPosition);
    }

    public void ShowCircle(bool status)
    {
        selectCircle.gameObject.SetActive(status);
    }

    public virtual void Interact()
    {
        return;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void SetShader(string shaderID, bool overrideSpecialShader = false)
    {
        if(meshRenderers == null)
            meshRenderers = GetComponentsInChildren<Renderer>();

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if(meshRenderers[i] is MeshRenderer || meshRenderers[i] is SkinnedMeshRenderer)
                meshRenderers[i].material.shader = Shader.Find(shaderID);
        }
        if (specialShaderRenderer != null && overrideSpecialShader)
        {
            for (int i = 0; i < specialShaderRenderer.Length; i++)
            {
                specialShaderRenderer[i].material.shader = Shader.Find(specialShaderID);
            }
        }
    }

    private void SetShader(Shader[] shaders)
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material.shader = shaders[i];
        }
    }

    private void SetShaderProperty(string id, Color color)
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material.SetColor(id, color);
        }
    }

    private void SetShaderProperty(string id, float value)
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].material.SetFloat(id, value / meshRenderers[i].transform.localScale.x);
        }
    }

    #region JSON
    public static SceneObject[] InstatiateFromJSON(Transform parent, SceneObject[] prefabs, JSON[] sceneRawJSON)
    {
        SceneObject[] sceneObjects = new SceneObject[sceneRawJSON.Length];
        for (int i = 0; i < sceneRawJSON.Length; i++)
        {
            JSON json = sceneRawJSON[i];
            sceneObjects[i] = Instantiate(prefabs[json.ToInt("ar")],parent);
            sceneObjects[i].SetLocalPosition(new Vector3(json.ToFloat("x"), 0f, json.ToFloat("z")));
            sceneObjects[i].Height(json.ToFloat("hgt"), true);
            sceneObjects[i].Scale(json.ToFloat("scl"), true);
            sceneObjects[i].Rotate(json.ToFloat("rot"), true);
            sceneObjects[i].ShowCircle(false);
            sceneObjects[i].Deselect();
        }
        return sceneObjects;
    }

    public static string GetRawSceneJSON(List<SceneObject> list)
    {
        if (!(list != null && list.Count > 0))
           return "";

        string result = ((JSON)list[0]).serialized;
        for (int i = 1; i < list.Count; i++)
        {
            result += "," + ((JSON)list[i]).serialized;
        }
        return result;
    }

    public static JSON[] GetSceneJSON(List<SceneObject> list)
    {
        JSON[] sceneJSON = new JSON[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            sceneJSON[i] = (JSON)list[i];
        }
        return sceneJSON;
    }

    public static implicit operator JSON(SceneObject @object)
    {
        JSON js = new JSON();
        js["ar"] = @object.arCode;
        js["x"] = @object.transform.localPosition.x;
        js["z"] = @object.transform.localPosition.z;
        js["hgt"] = @object.currentHeight;
        js["scl"] = @object.currentScale;
        js["rot"] = @object.currentRotation;
        return js;
    }
    #endregion
}
