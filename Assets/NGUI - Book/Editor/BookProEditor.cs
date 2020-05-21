using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using CYEngine;

namespace CYEngineEditor
{
	[CustomEditor(typeof(BookPro))]
	public class BookProEditor : Editor
	{
		ReorderableList list;
		private void OnEnable()
		{
			list = new ReorderableList(serializedObject,
					serializedObject.FindProperty("papers"),
					true, true, true, true);
			list.elementHeight = 75;
			list.drawElementCallback = DrawElement;
			list.drawHeaderCallback = drawHeader;
			list.onAddCallback = addElement;

			list.onCanRemoveCallback = canremove;

			list.onRemoveCallback = (ReorderableList l) =>
			{
				if (EditorUtility.DisplayDialog("Warning!",
					"Are you sure you want to delete this Paper?\r\nThe paper pages (front and back) will be deleted from the scene", "Yes", "No"))
				{
					BookPro book = target as BookPro;
					if (book.EndFlippingPaper == book.papers.Length - 1)
						book.EndFlippingPaper--;
					OnInspectorGUI();
					Paper paper = book.papers[l.index];

					book.LeftPageShadow.gameObject.SetActive(false);
					book.LeftPageShadow.transform.SetParent(book.transform);

					book.RightPageShadow.gameObject.SetActive(false);
					book.RightPageShadow.transform.SetParent(book.transform);

					if (paper.Back)
						Undo.DestroyObjectImmediate(paper.Back);
					if (paper.Front)
						Undo.DestroyObjectImmediate(paper.Front);
					ReorderableList.defaultBehaviours.DoRemoveButton(l);
					EditorUtility.SetDirty(book);
				}
			};
		}

		private bool canremove(ReorderableList list)
		{
			if (list.count == 1)
				return false;
			return true;
		}

		private void addElement(ReorderableList list)
		{
			BookPro book = target as BookPro;

			if (book.EndFlippingPaper == book.papers.Length - 1)
			{
				book.EndFlippingPaper = book.papers.Length;
				OnInspectorGUI();
			}

			list.serializedProperty.arraySize++;
			var lastElement = list.serializedProperty.GetArrayElementAtIndex(list.count - 1);

			GameObject rightPage = Instantiate(book.RightPageTransform.gameObject) as GameObject;
			rightPage.transform.SetParent(book.transform, true);
			rightPage.name = "Page" + ((list.serializedProperty.arraySize - 1) * 2);
			var rPage = rightPage.AddComponent<BookPage>();
			var rPanel = rightPage.AddComponent<UIPanel>();
			rPanel.clipping = UIDrawCall.Clipping.SoftClip;
			rPanel.baseClipRegion = new Vector4(0, 0, book.Width / 2, book.Height);
			rPanel.clipSoftness = new Vector2(0, 0);
			rPanel.dontRoundInClip = true;
			rPage.pagePanel = rPanel;
			lastElement.FindPropertyRelative("Front").objectReferenceInstanceIDValue = rightPage.GetInstanceID();

			GameObject leftPage = Instantiate(book.LeftPageTransform.gameObject) as GameObject;
			leftPage.transform.SetParent(book.transform, true);
			leftPage.name = "Page" + ((list.serializedProperty.arraySize - 1) * 2 + 1);
			var lPage = leftPage.AddComponent<BookPage>();
			var lPanel = leftPage.AddComponent<UIPanel>();
			lPanel.clipping = UIDrawCall.Clipping.SoftClip;
			lPanel.baseClipRegion = new Vector4(0, 0, book.Width / 2, book.Height);
			lPanel.clipSoftness = new Vector2(0, 0);
			lPanel.dontRoundInClip = true;
			lPage.pagePanel = lPanel;
			lastElement.FindPropertyRelative("Back").objectReferenceInstanceIDValue = leftPage.GetInstanceID();
			list.index = list.count - 1;

			Undo.RegisterCreatedObjectUndo(leftPage, "");
			Undo.RegisterCreatedObjectUndo(rightPage, "");
		}

		private void drawHeader(Rect rect)
		{
			EditorGUI.LabelField(rect, "Papers");
		}

		private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			BookPro book = target as BookPro;

			var serialzedpaper = list.serializedProperty.GetArrayElementAtIndex(index);
			rect.y += 2;

			GUIStyle style = new GUIStyle();

			EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, rect.height - 6), new Color(0.8f, 0.8f, 0.8f));
			EditorGUI.LabelField(new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight), "Paper#" + index);

			if (index == book.CurrentPaper)
				EditorGUI.DrawRect(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, 140, EditorGUIUtility.singleLineHeight), new Color(1, 0.3f, 0.3f));
			EditorGUI.LabelField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, 40, EditorGUIUtility.singleLineHeight), "Front:");
			EditorGUI.PropertyField(
				new Rect(rect.x + 40, rect.y + EditorGUIUtility.singleLineHeight, 100, EditorGUIUtility.singleLineHeight), serialzedpaper.FindPropertyRelative("Front"), GUIContent.none);

			if (index == book.CurrentPaper - 1)
				EditorGUI.DrawRect(new Rect(rect.x, rect.y + 3 * EditorGUIUtility.singleLineHeight, 140, EditorGUIUtility.singleLineHeight), new Color(1, 0.3f, 0.3f));
			EditorGUI.LabelField(new Rect(rect.x, rect.y + 3 * EditorGUIUtility.singleLineHeight, 35, EditorGUIUtility.singleLineHeight), "Back:");
			EditorGUI.PropertyField(
			new Rect(rect.x + 40, rect.y + 3 * EditorGUIUtility.singleLineHeight, 100, EditorGUIUtility.singleLineHeight), serialzedpaper.FindPropertyRelative("Back"), GUIContent.none);

			style.padding = new RectOffset(2, 2, 2, 2);
			if (GUI.Button(new Rect(rect.x + 70, rect.y + 2 * EditorGUIUtility.singleLineHeight, 20, EditorGUIUtility.singleLineHeight), "↕", GUIStyle.none))
			{
				Debug.Log("Clicked at index:" + index);
				Paper paper = book.papers[index];
				BookPage temp = paper.Back;
				paper.Back = paper.Front;
				paper.Front = temp;
			}
			if (GUI.Button(new Rect(rect.x + 150, rect.y + EditorGUIUtility.singleLineHeight, 80, (int)(1.5 * EditorGUIUtility.singleLineHeight)), "Show"))
			{
				for (int i = 0; i < book.papers.Length; ++i)
				{
					Paper paper = book.papers[i];
					if (i == index)
					{
						BookUtility.ShowPage(paper.Back);
						BookUtility.ShowPage(paper.Front);
					}
					else
					{
						BookUtility.HidePage(paper.Back);
						BookUtility.HidePage(paper.Front);
					}
				}

				book.LeftPageShadow.gameObject.SetActive(false);
				book.LeftPageShadow.transform.SetParent(book.transform);

				book.RightPageShadow.gameObject.SetActive(false);
				book.RightPageShadow.transform.SetParent(book.transform);
			}
			if (GUI.Button(new Rect(rect.x + 150, rect.y + (int)(2.5 * EditorGUIUtility.singleLineHeight), 80, (int)(1.5 * EditorGUIUtility.singleLineHeight)), "Set Current"))
			{
				book.CurrentPaper = index;
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			serializedObject.Update();
			list.DoLayoutList();
			serializedObject.ApplyModifiedProperties();

			Undo.RecordObject(target, "Inspector");

			EditorGUI.BeginChangeCheck();

			DrawNavigationPanel();
			DrawFlippingPapersRange();
			if (GUILayout.Button("Update Pages Order"))
			{
				(target as BookPro).UpdatePages();
			}
			if (GUILayout.Button("Update Pages Names"))
			{
				if (EditorUtility.DisplayDialog("Warning!",
					"All Pages will be renamed according to its order", "Ok", "Cancel"))
				{
					BookPro book = target as BookPro;
					for (int i = 0; i < book.papers.Length; i++)
					{
						book.papers[i].Front.name = "Page" + (i * 2);
						book.papers[i].Back.name = "Page" + (i * 2 + 1);
					}

				}
			}
			if (GUILayout.Button("Update Width/Height"))
			{
				UpdateSize(target as BookPro);
			}

			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(target);
			}
		}

		private void UpdateSize(BookPro book)
		{
			float bookWidth = book.Width;
			float bookHeight = book.Height;
			float pageWidth = bookWidth / 2;
			float pageHeight = bookHeight;
			float shadowPageHeight = Mathf.Sqrt(pageWidth * pageWidth + pageHeight * pageHeight) * 2;

			book.LeftPageTransform.localPosition = new Vector3(-book.Width / 4, 0, 0);
			book.RightPageTransform.localPosition = new Vector3(book.Width / 4, 0, 0);

			for (int i = 0; i < book.papers.Length; ++i)
			{
				var paper = book.papers[i];
				paper.Front.pagePanel.baseClipRegion = new Vector4(0, 0, pageWidth, pageHeight);
				paper.Back.pagePanel.baseClipRegion = new Vector4(0, 0, pageWidth, pageHeight);
			}

			book.ClippingPlane.clipping = UIDrawCall.Clipping.SoftClip;
			book.ClippingPlane.baseClipRegion = new Vector4(0, 0, pageWidth * 4, pageWidth * 4);

			UpdateShadowSize(book, book.LeftPageShadow, true, shadowPageHeight);
			UpdateShadowSize(book, book.RightPageShadow, false, shadowPageHeight);
			UpdateShadowSize(book, book.ClippingShadowL, true, shadowPageHeight);
			UpdateShadowSize(book, book.ClippingShadowR, false, shadowPageHeight);

			UpdateHotSpot(book, book.LeftHotSpot, true);
			UpdateHotSpot(book, book.RightHotSpot, false);

			book.UpdatePages();
		}

		private void UpdateShadowSize(BookPro book, UITexture shadow, bool isLeft, float shadowPageHeight)
		{
			if (shadow == null)
				return;

			float bookWidth = book.Width;
			float bookHeight = book.Height;
			float pageWidth = bookWidth / 2;
			float pageHeight = bookHeight;

			shadow.width = (int)pageWidth;
			shadow.height = (int)shadowPageHeight;

			var shadowMask = shadow.GetComponent<BookShadowMask>();
			if (shadowMask)
			{
				shadowMask.size = new Vector2(pageWidth, pageHeight);
				if (isLeft)
					shadowMask.size.x = -shadowMask.size.x;
			}
		}
		private void UpdateHotSpot(BookPro book, BookHotSpot hotSpot, bool isLeft)
		{
			if (hotSpot == null)
				return;

			float sign = isLeft ? -1 : 1;
			hotSpot.transform.localPosition = new Vector3(sign * book.Width / 2, 0, 0);

			var collider = hotSpot.GetComponent<BoxCollider>();
			if (collider == null)
				return;

			collider.size = new Vector3(book.Width / 2 * book.m_spotWidthPercent, book.Height, 0);
			collider.center = new Vector3(-sign * book.Width / 4 * book.m_spotWidthPercent, 0, 0);
		}

		private void DrawFlippingPapersRange()
		{
			BookPro book = target as BookPro;

			EditorGUILayout.BeginVertical(EditorStyles.helpBox);

			EditorGUILayout.LabelField("Flipping Range", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("First Flippable Paper: " + "Paper#" + book.StartFlippingPaper);
			EditorGUILayout.LabelField("Last Flippable Paper: " + "Paper#" + book.EndFlippingPaper);
			float start = book.StartFlippingPaper;
			float end = book.EndFlippingPaper;
			EditorGUILayout.MinMaxSlider(ref start, ref end, 0, book.papers.Length);
			book.StartFlippingPaper = Mathf.RoundToInt(start);
			book.EndFlippingPaper = Mathf.RoundToInt(end);
			EditorGUILayout.EndVertical();
		}

		private void DrawNavigationPanel()
		{

			BookPro book = target as BookPro;
			EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

			GUILayout.Label(new GUIContent("Current Paper", "represent current paper on the right side of the book. if you want to show the back page of the last paper you may set this value with the index of last paper + 1"));
			if (GUILayout.Button("|<"))
			{
				book.CurrentPaper = 0;
			}
			if (GUILayout.Button("<"))
			{
				book.CurrentPaper--;
			}
			book.CurrentPaper = EditorGUILayout.IntField(book.CurrentPaper, GUILayout.Width(30));

			if (GUILayout.Button(">"))
			{
				book.CurrentPaper++;
			}
			if (GUILayout.Button(">|"))
			{
				book.CurrentPaper = book.papers.Length;
			}

			EditorGUILayout.EndHorizontal();
		}
	}
}
