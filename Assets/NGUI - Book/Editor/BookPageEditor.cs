using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using CYEngine;

namespace CYEngineEditor
{
	//[CustomEditor(typeof(BookPage))]
	//public class BookPageEditor : Editor
	//{
	//	ReorderableList list;
	//	SerializedProperty subPanelList;
	//	SerializedProperty pagePanel;

	//	private void OnEnable()
	//	{
	//		pagePanel = serializedObject.FindProperty("pagePanel");

	//		subPanelList = serializedObject.FindProperty("subPanelList");

	//		list = new ReorderableList(serializedObject, subPanelList, false, true, false, false);
	//		list.drawElementCallback = DrawElement;
	//		list.drawHeaderCallback = DrawHeader;
	//	}

	//	private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
	//	{
	//		rect.y += 2;

	//		var serializedSubPanelInfo = list.serializedProperty.GetArrayElementAtIndex(index);

	//		var panel = serializedSubPanelInfo.FindPropertyRelative("panel");
	//		var depthOffset = serializedSubPanelInfo.FindPropertyRelative("depthOffset");

	//		EditorGUI.LabelField(new Rect(rect.x, rect.y, 40, EditorGUIUtility.singleLineHeight), "Panel:");
	//		EditorGUI.ObjectField(new Rect(rect.x + 40, rect.y, rect.width / 2 - 40, EditorGUIUtility.singleLineHeight), panel, typeof(UIPanel), GUIContent.none);
	//		EditorGUI.LabelField(new Rect(rect.x + rect.width / 2, rect.y, 80, EditorGUIUtility.singleLineHeight), "Depth Offset:");
	//		EditorGUI.PropertyField(new Rect(rect.x + rect.width / 2 + 80, rect.y, rect.width / 2 - 80, EditorGUIUtility.singleLineHeight), depthOffset, GUIContent.none);
	//	}

	//	private void DrawHeader(Rect rect)
	//	{
	//		EditorGUI.LabelField(rect, "Sub panels");
	//	}

	//	public override void OnInspectorGUI()
	//	{
	//		//using (var scope = new EditorGUI.ChangeCheckScope())
	//		//{
	//		//	serializedObject.Update();

	//		//	EditorGUILayout.PropertyField(pagePanel);

	//		//	list.DoLayoutList();

	//		//	if (GUILayout.Button("获取子Panel"))
	//		//	{
	//		//		UpdateSubPanels();
	//		//	}

	//		//	serializedObject.ApplyModifiedProperties();

	//		//	if (scope.changed)
	//		//	{
	//		//		UpdateSubPanelsChanged();
	//		//	}
	//		//}
	//	}

	//	int GetOldDepth(List<BookPage.SubPanelInfo> oldlist, UIPanel panel)
	//	{
	//		for (int i = 0; i < oldlist.Count; ++i)
	//		{
	//			if (oldlist[i].panel == panel)
	//				return oldlist[i].depthOffset;
	//		}

	//		return 0;
	//	}

	//	void UpdateSubPanels()
	//	{
	//		var page = target as BookPage;
	//		var subPanels = page.GetComponentsInChildren<UIPanel>();
	//		var oldList = page.subPanelList;
	//		page.subPanelList = new List<BookPage.SubPanelInfo>();
	//		for (int i = 0; i < subPanels.Length; ++i)
	//		{
	//			UIPanel panel = subPanels[i];
	//			if (panel.gameObject != page.gameObject)
	//			{
	//				int oldDepth = GetOldDepth(oldList, panel);
	//				page.subPanelList.Add(new BookPage.SubPanelInfo(panel, oldDepth));
	//			}
	//		}
	//	}

	//	void UpdateSubPanelsChanged()
	//	{
	//		var page = target as BookPage;
	//		if (page.enabled == false)
	//			return;

	//		BookPro book = null;
	//		Transform p = page.transform.parent;
	//		while (p)
	//		{
	//			book = p.GetComponent<BookPro>();
	//			if (book)
	//				break;

	//			p = p.parent;
	//		}

	//		//book?.UpdatePages();
 //           book.UpdatePages();
 //       }
	//}
}
