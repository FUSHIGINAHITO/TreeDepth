using UnityEditor;
using UnityEngine;

public class MapEdit : MonoBehaviour
{
    public GameObject agentObj;
    [TextArea(30, 1000)]
    public string data;

    public void Create()
    {
        var obj = PrefabUtility.InstantiatePrefab(agentObj) as GameObject;
        obj.transform.parent = transform;
        var agent = obj.GetComponent<NodeAgent>();
        agent.Init(transform.childCount - 2);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MapEdit))]
    private class NodeAgentAdderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var mono = (MapEdit)target;

            if (GUILayout.Button("添加"))
            {
                mono.Create();
            }

            DrawDefaultInspector();
        }
    }
#endif
}