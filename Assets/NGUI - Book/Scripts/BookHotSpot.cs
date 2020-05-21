using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYEngine
{
	[RequireComponent(typeof(BoxCollider))]
	public class BookHotSpot : BookDragDrop
	{
		public BookPro book;
		public bool isLeft;

		private Vector3 oriPos;

		protected override void OnDragDropStart()
		{
			oriPos = transform.localPosition;

			base.OnDragDropStart();
			if (isLeft)
				book.OnMouseDragLeftPage();
			else
				book.OnMouseDragRightPage();
		}

		protected override void OnDragDropRelease(GameObject surface)
		{
			book.OnMouseRelease();

			transform.localPosition = oriPos;
			mCollider.enabled = true;
		}

		//private void OnMouseDrag()
		//{
		//}

		//private void OnMouseUp()
		//{
		//}
	}

}
