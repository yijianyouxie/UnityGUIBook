using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;
namespace CYEngine
{
	public enum FlipMode
	{
		RightToLeft,
		LeftToRight
	}
	public class BookPro : MonoBehaviour
	{
		Canvas canvas;
		//[Min(0)]
		public float Width;
		//[Min(0)]
		public float Height;
		public UIPanel ClippingPlane;
		public UITexture LeftPageShadow;
		public UITexture RightPageShadow;
		public UITexture ClippingShadowL;
		public UITexture ClippingShadowR;
		public Transform LeftPageTransform;
		public Transform RightPageTransform;
		public BookHotSpot LeftHotSpot;
		public BookHotSpot RightHotSpot;
		public bool interactable = true;
		public bool enableShadowEffect = true;
		public int baseDepth = 0;
		[Tooltip("Uncheck this if the book does not contain transparent pages to improve the overall performance")]
		public bool hasTransparentPages = true;
		[HideInInspector]
		public int currentPaper = 0;
		[HideInInspector]
		public Paper[] papers;
		/// <summary>
		/// OnFlip invocation list, called when any page flipped
		/// </summary>
		public UnityEvent OnFlip;

		public float m_tweenForwardTime = 0.3f;
		public float m_tweenBackTime = 0.3f;
		public float m_spotWidthPercent = 0.33333f;

		/// <summary>
		/// The Current Shown paper (the paper its front shown in right part)
		/// </summary>
		public int CurrentPaper
		{
			get { return currentPaper; }
			set
			{
				if (value != currentPaper)
				{
					if (value < StartFlippingPaper)
						currentPaper = StartFlippingPaper;
					else if (value > EndFlippingPaper)
						currentPaper = EndFlippingPaper;
					else
						currentPaper = value;
					UpdatePages();
				}
			}
		}
		[HideInInspector]
		public int StartFlippingPaper = 0;
		[HideInInspector]
		public int EndFlippingPaper = 1;

		public Vector3 EndBottomLeft
		{
			get { return ebl; }
		}
		public Vector3 EndBottomRight
		{
			get { return ebr; }
		}

		int flipBaseDepth = 0;

		//current flip mode
		FlipMode mode;

		/// <summary>
		/// this value should e true while the user darg the page
		/// </summary>
		bool pageDragging = false;

		/// <summary>
		/// should be true when the page tween forward or backward after release
		/// </summary>
		bool tweening = false;

		// Use this for initialization
		void Start()
		{
			//Canvas[] c = GetComponentsInParent<Canvas>();
			//if (c.Length > 0)
			//	canvas = c[c.Length - 1];
			//else
			//	Debug.LogError("Book Must be a child to canvas diectly or indirectly");

			UpdatePages();

			CalcCurlCriticalPoints();


			float pageWidth = Width / 2.0f;
			float pageHeight = Height;

			ClippingPlane.clipping = UIDrawCall.Clipping.SoftClip;
			ClippingPlane.baseClipRegion = new Vector4(0, 0, pageWidth * 4, pageWidth * 4);
			//ClippingPlane.rectTransform.sizeDelta = new Vector2(pageWidth * 2 + pageHeight, pageHeight + pageHeight * 2);

			//hypotenous (diagonal) page length
			float hyp = Mathf.Sqrt(pageWidth * pageWidth + pageHeight * pageHeight);
			float shadowPageHeight = Mathf.Sqrt(pageWidth * pageWidth + pageHeight * pageHeight) * 2;

			ClippingShadowL.width = (int)pageWidth;
			ClippingShadowL.height = (int)shadowPageHeight;

			ClippingShadowR.width = (int)pageWidth;
			ClippingShadowR.height = (int)shadowPageHeight;

			RightPageShadow.width = (int)pageWidth;
			RightPageShadow.height = (int)shadowPageHeight;
			RightPageShadow.pivot = UIWidget.Pivot.Center;

			LeftPageShadow.width = (int)pageWidth;
			LeftPageShadow.height = (int)shadowPageHeight;
			LeftPageShadow.pivot = UIWidget.Pivot.Center;
		}

		/// <summary>
		/// transform point from global (world-space) to local space
		/// </summary>
		/// <param name="global">poit iin world space</param>
		/// <returns></returns>
		public Vector3 transformPoint(Vector3 global)
		{
			Vector2 localPos = transform.InverseTransformPoint(global);
			return localPos;
		}
		/// <summary>
		/// transform mouse position to local space
		/// </summary>
		/// <param name="mouseScreenPos"></param>
		/// <returns></returns>
		public Vector3 transformPointMousePosition(Vector3 mouseScreenPos)
		{
			//if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
			//{
			Vector3 mouseWorldPos = UICamera.currentCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0));
			Vector2 localPos = transform.InverseTransformPoint(mouseWorldPos);

			return localPos;
			//}
			//else if (canvas.renderMode == RenderMode.WorldSpace)
			//{
			//	Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			//	Vector3 globalEBR = transform.TransformPoint(ebr);
			//	Vector3 globalEBL = transform.TransformPoint(ebl);
			//	Vector3 globalSt = transform.TransformPoint(st);
			//	Plane p = new Plane(globalEBR, globalEBL, globalSt);
			//	float distance;
			//	p.Raycast(ray, out distance);
			//	Vector2 localPos = transform.InverseTransformPoint(ray.GetPoint(distance));
			//	return localPos;
			//}
			//else
			//{
			//	//Screen Space Overlay
			//	Vector2 localPos = transform.InverseTransformPoint(mouseScreenPos);
			//	return localPos;
			//}

		}

		/// <summary>
		/// Update page orders
		/// This function should be called whenever the current page changed, the dragging of the page started or the page has been flipped
		/// </summary>
		public void UpdatePages()
		{
			int previousPaper = pageDragging ? currentPaper - 2 : currentPaper - 1;

			//Hide all pages
			for (int i = 0; i < papers.Length; i++)
			{
				BookUtility.HidePage(papers[i].Front);
				papers[i].Front.transform.SetParent(transform);
				BookUtility.HidePage(papers[i].Back);
				papers[i].Back.transform.SetParent(transform);
			}

			int nextDepth = baseDepth + 1;
			if (hasTransparentPages)
			{
				int nDepth = baseDepth;
				//Show the back page of all previous papers
				for (int i = 0; i <= previousPaper; i++)
				{
					BookUtility.ShowPage(papers[i].Back);
					BookUtility.CopyTransform(LeftPageTransform.transform, papers[i].Back.transform);
					nDepth = UpdatePageDepth(papers[i].Back, nDepth);
				}
				nextDepth = Mathf.Max(nDepth, nextDepth);

				nDepth = baseDepth;
				//Show the front page of all next papers
				for (int i = papers.Length - 1; i >= currentPaper; i--)
				{
					BookUtility.ShowPage(papers[i].Front);
					BookUtility.CopyTransform(RightPageTransform.transform, papers[i].Front.transform);
					nDepth = UpdatePageDepth(papers[i].Front, nDepth);
				}
				nextDepth = Mathf.Max(nDepth, nextDepth);
			}
			else
			{
				//show back of previous page only
				if (previousPaper >= 0)
				{
					var currentLeft = papers[previousPaper].Back;
					BookUtility.ShowPage(currentLeft);
					BookUtility.CopyTransform(LeftPageTransform.transform, currentLeft.transform);
					int nDepth = UpdatePageDepth(currentLeft, baseDepth);
					nextDepth = Mathf.Max(nDepth, nextDepth);
				}
				//show front of current page only
				if (currentPaper <= papers.Length - 1)
				{
					var currentRight = papers[currentPaper].Front;
					BookUtility.ShowPage(currentRight);
					BookUtility.CopyTransform(RightPageTransform.transform, currentRight.transform);
					int nDepth = UpdatePageDepth(currentRight, baseDepth);
					nextDepth = Mathf.Max(nDepth, nextDepth);
				}
			}
			flipBaseDepth = nextDepth;
			#region Shadow Effect
			if (enableShadowEffect)
			{
				//the shadow effect enabled
				if (previousPaper >= 0 && previousPaper < papers.Length - 1)
				{
					//has at least one previous page, then left shadow should be active
					LeftPageShadow.gameObject.SetActive(true);
					LeftPageShadow.transform.SetParent(papers[previousPaper].Back.transform, true);
					LeftPageShadow.transform.localPosition = Vector3.zero;
					LeftPageShadow.transform.localRotation = Quaternion.identity;
				}
				else
				{
					//if no previous pages, the leftShaow should be disabled
					LeftPageShadow.gameObject.SetActive(false);
					LeftPageShadow.transform.SetParent(transform, true);
				}
				LeftPageShadow.ParentHasChanged();

				if (currentPaper > 0 && currentPaper < papers.Length)
				{
					//has at least one next page, the right shadow should be active
					RightPageShadow.gameObject.SetActive(true);
					RightPageShadow.transform.SetParent(papers[currentPaper].Front.transform, true);
					RightPageShadow.transform.localPosition = Vector3.zero;
					RightPageShadow.transform.localRotation = Quaternion.identity;
				}
				else
				{
					//no next page, the right shadow should be diabled
					RightPageShadow.gameObject.SetActive(false);
					RightPageShadow.transform.SetParent(transform, true);
				}
				RightPageShadow.ParentHasChanged();
			}
			else
			{
				//Enable Shadow Effect is Unchecked, all shadow effects should be disabled
				LeftPageShadow.gameObject.SetActive(false);
				LeftPageShadow.transform.SetParent(transform, true);

				RightPageShadow.gameObject.SetActive(false);
				RightPageShadow.transform.SetParent(transform, true);

			}
			#endregion
		}

		int UpdatePageDepth(BookPage page, int depthOffset)
		{
			page.pagePanel.depth = depthOffset;

			int maxSubDepthOffset = 0;
			for (int i = 0; i < page.subPanelList.Count; ++i)
			{
				int subDepthOffset = page.subPanelList[i].depthOffset;
				page.subPanelList[i].panel.depth = depthOffset + subDepthOffset;
				maxSubDepthOffset = Mathf.Max(subDepthOffset, maxSubDepthOffset);
			}

			return depthOffset + maxSubDepthOffset + 1;
		}

		public void BeginDrag()
		{
			pageDragging = true;
		}

		public void EndDrag()
		{
			pageDragging = false;
		}

		public bool CouldFlip(FlipMode mode)
		{
			if (mode == FlipMode.LeftToRight)
			{
				return CurrentPaper > 0 && CurrentPaper > StartFlippingPaper;
			}
			else
			{
				return CurrentPaper < papers.Length && CurrentPaper < EndFlippingPaper;
			}
		}

		//mouse interaction events call back
		public void OnMouseDragRightPage()
		{
			if (interactable && !tweening && CouldFlip(FlipMode.RightToLeft))
			{
				var p = transformPointMousePosition(Input.mousePosition);
				SetupRightDrag(p.y);

				BeginDrag();
				DragRightPageToPoint(corner);
			}
		}

		public void SetupRightDrag(float y)
		{
			if (y < -Height / 6)
			{
				part = 0;
				corner = ebr;
				backTarget = ebr;
				forwardTarget = ebl;

				t_radius1 = radius1;
				t_radius2 = radius2;
				t_defaultAngle = -Mathf.PI / 4;
				t_pageOffset = Height / 2;
			}
			else if (y < Height / 6)
			{
				part = 1;
				corner = ebr + new Vector3(0, Height / 2, 0);
				backTarget = ebr + new Vector3(0, Height / 2, 0);
				forwardTarget = ebl + new Vector3(0, Height / 2, 0);

				t_radius1 = radius3;
				t_radius2 = radius3;
				t_defaultAngle = 0;
				t_pageOffset = 0;
			}
			else
			{
				part = 2;
				corner = ebr + new Vector3(0, Height, 0);
				backTarget = ebr + new Vector3(0, Height, 0);
				forwardTarget = ebl + new Vector3(0, Height, 0);

				t_radius1 = radius2;
				t_radius2 = radius1;
				t_defaultAngle = Mathf.PI / 4;
				t_pageOffset = -Height / 2;
			}
		}

		public void DragRightPageToPoint(Vector3 point)
		{
			if (currentPaper >= EndFlippingPaper) return;
			mode = FlipMode.RightToLeft;
			f = point;

			currentPaper += 1;

			UpdatePages();

			var paper = papers[currentPaper - 1];
			t_back = paper.Front;
			t_defaultPageTransform = RightPageTransform;
			BookUtility.ShowPage(t_back);
			int nextDepth = UpdatePageDepth(t_back, flipBaseDepth);

			t_back.transform.SetParent(ClippingPlane.transform, true);
			t_back.transform.position = t_defaultPageTransform.position;
			t_back.transform.rotation = t_defaultPageTransform.rotation;
			t_back.pagePanel.ParentHasChanged();


			t_front = paper.Back;
			BookUtility.ShowPage(t_front);
			UpdatePageDepth(t_front, nextDepth);

			t_front.transform.SetParent(ClippingPlane.transform, true);
			t_front.transform.localPosition = Vector3.zero;
			t_front.transform.localRotation = Quaternion.identity;
			t_front.pagePanel.ParentHasChanged();

			ClippingPlane.gameObject.SetActive(true);

			if (enableShadowEffect)
			{
				if (currentPaper > 1)
				{
					ClippingShadowR.gameObject.SetActive(true);
					ClippingShadowR.transform.SetParent(t_back.transform);
					ClippingShadowR.ParentHasChanged();

					ClippingShadowR.transform.localPosition = new Vector3(0, 0, 0);
					ClippingShadowR.transform.localRotation = Quaternion.identity;
				}

				if (currentPaper < papers.Length)
				{
					ClippingShadowL.gameObject.SetActive(true);
					ClippingShadowL.transform.SetParent(t_front.transform);
					ClippingShadowL.ParentHasChanged();

					ClippingShadowL.transform.localPosition = new Vector3(0, 0, 0);
					ClippingShadowL.transform.localRotation = Quaternion.identity;
					ClippingShadowL.pivot = UIWidget.Pivot.Right;
				}

				RightPageShadow.pivot = UIWidget.Pivot.Left;
				t_pageShadow = RightPageShadow;
				t_clipShadow = ClippingShadowL;
			}


			UpdateBookToPoint(f, false);

			ClippingPlane.InvalideMatrix(true);
		}

		public void OnMouseDragLeftPage()
		{
			if (interactable && !tweening && CouldFlip(FlipMode.LeftToRight))
			{
				var p = transformPointMousePosition(Input.mousePosition);
				SetupLeftDrag(p.y);

				BeginDrag();
				DragLeftPageToPoint(corner);
			}
		}

		public void SetupLeftDrag(float y)
		{
			if (y < -Height / 6)
			{
				part = 0;
				corner = ebl;
				backTarget = ebl;
				forwardTarget = ebr;

				t_radius1 = radius1;
				t_radius2 = radius2;
				t_defaultAngle = Mathf.PI / 4;
				t_pageOffset = Height / 2;
			}
			else if (y < Height / 6)
			{
				part = 1;
				corner = ebl + new Vector3(0, Height / 2, 0);
				backTarget = ebl + new Vector3(0, Height / 2, 0);
				forwardTarget = ebr + new Vector3(0, Height / 2, 0);

				t_radius1 = radius3;
				t_radius2 = radius3;
				t_defaultAngle = 0;
				t_pageOffset = 0;
			}
			else
			{
				part = 2;
				corner = ebl + new Vector3(0, Height, 0);
				backTarget = ebl + new Vector3(0, Height, 0);
				forwardTarget = ebr + new Vector3(0, Height, 0);

				t_radius1 = radius2;
				t_radius2 = radius1;
				t_defaultAngle = -Mathf.PI / 4;
				t_pageOffset = -Height / 2;
			}
		}

		public void DragLeftPageToPoint(Vector3 point)
		{
			if (currentPaper <= StartFlippingPaper) return;
			mode = FlipMode.LeftToRight;
			f = point;

			UpdatePages();

			ClippingPlane.gameObject.SetActive(true);

			t_defaultPageTransform = LeftPageTransform;

			var paper = papers[currentPaper - 1];
			t_back = paper.Back;
			t_back = paper.Back;
			BookUtility.ShowPage(t_back);
			int nextDepth = UpdatePageDepth(t_back, flipBaseDepth);

			t_back.transform.SetParent(ClippingPlane.transform, true);
			t_back.pagePanel.ParentHasChanged();

			t_back.transform.position = t_defaultPageTransform.position;
			t_back.transform.rotation = t_defaultPageTransform.rotation;

			t_front = paper.Front;
			BookUtility.ShowPage(t_front);
			UpdatePageDepth(t_front, nextDepth);

			t_front.transform.SetParent(ClippingPlane.transform, true);
			t_front.pagePanel.ParentHasChanged();

			t_front.transform.localPosition = Vector3.zero;
			t_front.transform.localRotation = Quaternion.identity;


			if (enableShadowEffect)
			{
				if (currentPaper > 1)
				{
					ClippingShadowR.gameObject.SetActive(true);
					ClippingShadowR.transform.SetParent(t_front.transform);
					ClippingShadowR.ParentHasChanged();

					ClippingShadowR.transform.localPosition = new Vector3(0, 0, 0);
					ClippingShadowR.transform.localRotation = Quaternion.identity;
					ClippingShadowR.pivot = UIWidget.Pivot.Left;
				}

				if (currentPaper < papers.Length)
				{
					ClippingShadowL.gameObject.SetActive(true);
					ClippingShadowL.transform.SetParent(t_back.transform);
					ClippingShadowL.ParentHasChanged();

					ClippingShadowL.transform.localPosition = new Vector3(0, 0, 0);
					ClippingShadowL.transform.localRotation = Quaternion.identity;
				}

				LeftPageShadow.pivot = UIWidget.Pivot.Right;
				t_pageShadow = LeftPageShadow;
				t_clipShadow = ClippingShadowR;
			}

			UpdateBookToPoint(f, true);

			ClippingPlane.InvalideMatrix(true);
		}

		public void OnMouseRelease()
		{
			if (interactable)
				ReleasePage();
		}

		public void ReleasePage()
		{
			if (pageDragging)
			{
				EndDrag();
				tweening = true;
				float distanceToLeft = Vector2.Distance(c, ebl);
				float distanceToRight = Vector2.Distance(c, ebr);
				if (distanceToRight < distanceToLeft && mode == FlipMode.RightToLeft)
					TweenBack();
				else if (distanceToRight > distanceToLeft && mode == FlipMode.LeftToRight)
					TweenBack();
				else
					TweenForward();
			}
		}

		// Update is called once per frame
		void Update()
		{
			if (pageDragging && interactable)
			{
				UpdateBook();
			}
		}

		public void UpdateBook()
		{
			f = Vector3.Lerp(f, transformPointMousePosition(Input.mousePosition), Time.deltaTime * 10);
			UpdateBookToPoint(f, mode == FlipMode.LeftToRight);
		}

		private void FlipOverImpl()
		{
			t_back.transform.SetParent(transform, true);

			t_front.transform.SetParent(transform, true);

			ClippingShadowL.gameObject.SetActive(false);
			ClippingShadowR.gameObject.SetActive(false);
			ClippingPlane.gameObject.SetActive(false);

			LeftPageShadow.pivot = UIWidget.Pivot.Center;
			RightPageShadow.pivot = UIWidget.Pivot.Center;
			ClippingShadowL.pivot = UIWidget.Pivot.Center;
			ClippingShadowR.pivot = UIWidget.Pivot.Center;

			UpdatePages();

			tweening = false;
		}

		/// <summary>
		/// This function called when the page dragging point reached its distenation after releasing the mouse
		/// This function will call the OnFlip invocation list
		/// if you need to call any fnction after the page flipped just add it to the OnFlip invocation list
		/// </summary>
		public void Flip()
		{
			if (mode == FlipMode.LeftToRight)
				currentPaper -= 1;

			FlipOverImpl();

			if (OnFlip != null)
				OnFlip.Invoke();
		}

		public void TweenForward()
		{
			BookTween.ValueTo(gameObject, f, forwardTarget, m_tweenForwardTime, TweenUpdate, Flip);
		}

		void TweenUpdate(Vector3 follow)
		{
			UpdateBookToPoint(follow, mode == FlipMode.LeftToRight);
		}
		private void FlipBack()
		{
			if (mode == FlipMode.RightToLeft)
				currentPaper -= 1;

			FlipOverImpl();
		}
		public void TweenBack()
		{
			BookTween.ValueTo(gameObject, f, backTarget, m_tweenBackTime, TweenUpdate, FlipBack);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = Color.white;
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(Width, Height));
		}

		#region Page Curl Internal Calculations
		//for more info about this part please check this link : http://rbarraza.com/html5-canvas-pageflip/

		float radius1, radius2, radius3;
		//Spine Bottom
		Vector3 sb;
		//Spine Top
		Vector3 st;
		//corner of the page
		Vector3 c;
		//Edge Bottom Right
		Vector3 ebr;
		//Edge Bottom Left
		Vector3 ebl;
		//follow point 
		Vector3 f;
		Vector3 dragPoint;
		Vector3 corner;
		Vector3 backTarget, forwardTarget;
		float t_radius1, t_radius2;
		float t_defaultAngle, t_pageOffset;
		BookPage t_back, t_front;
		Transform t_defaultPageTransform;
		UITexture t_pageShadow, t_clipShadow;

		int part = 0;

		private void CalcCurlCriticalPoints()
		{
			sb = new Vector3(0, -Height / 2);
			ebr = new Vector3(Width / 2, -Height / 2);
			ebl = new Vector3(-Width / 2, -Height / 2);
			st = new Vector3(0, Height / 2);
			radius1 = Vector2.Distance(sb, ebr);
			float pageWidth = Width / 2.0f;
			float pageHeight = Height;
			float halfPageHeight = pageHeight / 2;
			radius2 = Mathf.Sqrt(pageWidth * pageWidth + pageHeight * pageHeight);
			radius3 = Mathf.Sqrt(pageWidth * pageWidth + halfPageHeight * halfPageHeight);
		}

		public void UpdateBookToPoint(Vector3 followLocation, bool isLeft)
		{
			f = followLocation;

			c = Calc_C_Position(followLocation, t_radius1, t_radius2);
			Vector3 t1;
			Vector3 t2;
			Vector3 t0;
			float T0_T1_Angle = Calc_T0_T1_Angle(c, corner, t_defaultAngle, 0.04f, t_pageOffset, isLeft, out t0, out t1, out t2);

			ClippingPlane.transform.localEulerAngles = new Vector3(0, 0, -90 + T0_T1_Angle * Mathf.Rad2Deg);
			ClippingPlane.transform.localPosition = t2;

			t_back.transform.position = t_defaultPageTransform.transform.position;
			t_back.transform.rotation = t_defaultPageTransform.transform.rotation;

			//page position and angle
			t_front.transform.position = transform.TransformPoint(t1);
			t_front.transform.rotation = transform.rotation * Quaternion.Euler(0, 0, T0_T1_Angle * 2 * Mathf.Rad2Deg);

			if (enableShadowEffect)
			{
				var shadowPos = transform.TransformPoint(t0);
				t_clipShadow.transform.position = shadowPos;
				t_clipShadow.transform.localEulerAngles = new Vector3(0, 0, -T0_T1_Angle * Mathf.Rad2Deg);

				t_pageShadow.transform.position = shadowPos;
				t_pageShadow.transform.localEulerAngles = new Vector3(0, 0, T0_T1_Angle * Mathf.Rad2Deg);
			}
		}


		Vector3 RotatePoint(Vector3 point, float angle)
		{
			float cos = Mathf.Cos(angle);
			float sin = Mathf.Sin(angle);
			return new Vector3(point.x * cos - point.y * sin, point.x * sin + point.y * cos);
		}
		private float Calc_T0_T1_Angle(Vector3 c, Vector3 bookCorner, float defaultAngle, float percent, float py, bool isLTR, out Vector3 t0, out Vector3 t1, out Vector3 t2)
		{
			t0 = (c + bookCorner) / 2;
			float T0_CORNER_dy = bookCorner.y - c.y;
			float T0_CORNER_dx = bookCorner.x - c.x;
			float T0_CORNER_Angle = Mathf.Atan(T0_CORNER_dy / T0_CORNER_dx);
			if (float.IsNaN(T0_CORNER_Angle))
				T0_CORNER_Angle = defaultAngle;

			float lj = Width * percent;
			float dcc = Vector3.Distance(c, bookCorner);
			if (T0_CORNER_Angle < lj)
				T0_CORNER_Angle = Mathf.Lerp(defaultAngle, T0_CORNER_Angle, dcc / lj);

			float l = Mathf.Sqrt(Width * Width / 4 + Height * Height) / 2;
			float la = Mathf.Atan((Width / 4) / (Height / 2));
			float a = T0_CORNER_Angle * 2;
			float sign = isLTR ? 1 : -1;
			var p = RotatePoint(new Vector3(-sign * Width / 4, py), a);

			t1 = c;
			t1 += p;

			var p2 = RotatePoint(new Vector3(sign * Width * 1, 0), T0_CORNER_Angle);

			t2 = t0;
			t2 += p2;

			return T0_CORNER_Angle;
		}

		private Vector3 LimitPosition(Vector3 followLocation, Vector3 center, float radius)
		{
			float F_SB_dy = followLocation.y - center.y;
			float F_SB_dx = followLocation.x - center.x;
			float F_SB_Angle = Mathf.Atan2(F_SB_dy, F_SB_dx);
			Vector3 r1 = new Vector3(radius * Mathf.Cos(F_SB_Angle), radius * Mathf.Sin(F_SB_Angle), 0) + center;

			float F_SB_distance = Vector2.Distance(followLocation, center);

			if (F_SB_distance <= radius)
				return followLocation;
			else
				return r1;
		}

		private Vector3 Calc_C_Position(Vector3 followLocation, float bradius, float tradius)
		{
			Vector3 c = followLocation;
			c = LimitPosition(c, sb, bradius);
			c = LimitPosition(c, st, tradius);
			return c;
		}

		#endregion

	}
	[Serializable]
	public class Paper
	{
		public BookPage Front;
		public BookPage Back;
	}


	public static class BookUtility
	{
		/// <summary>
		/// Call this function to Show a Hidden Page
		/// </summary>
		/// <param name="page">the page to be shown</param>
		public static void ShowPage(BookPage page)
		{
			page.gameObject.SetActive(true);
		}

		/// <summary>
		/// Call this function to hide any page
		/// </summary>
		/// <param name="page">the page to be hidden</param>
		public static void HidePage(BookPage page)
		{
			page.gameObject.SetActive(false);
		}

		public static void CopyTransform(Transform from, Transform to)
		{
			to.position = from.position;
			to.rotation = from.rotation;
			to.localScale = from.localScale;
		}
	}
}