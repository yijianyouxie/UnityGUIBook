using UnityEngine;
using System.Collections;
using System;

namespace CYEngine
{
	public class PageFlipper : MonoBehaviour
	{
		float duration;
		BookPro book;
		bool isFlipping = false;
		Action finish;
		float elapsedTime = 0;
		//center x-coordinate 
		float pageWidth, pageHalfHeight;
		float startPoint = 0, dragPoint = 0;
		bool oldInteractable;
		FlipMode flipMode;
		public static bool FlipPage(BookPro book, float duration, FlipMode mode, Action OnComplete, float startPoint = 0, float dragPoint = 0.5f)
		{
			if (!book.CouldFlip(mode))
				return false;

			PageFlipper flipper = book.GetComponent<PageFlipper>();
			if (!flipper)
				flipper = book.gameObject.AddComponent<PageFlipper>();
			flipper.enabled = true;
			flipper.book = book;
			flipper.isFlipping = true;
			flipper.duration = duration - Time.deltaTime;
			flipper.finish = OnComplete;
			flipper.pageWidth = book.Width / 2;
			flipper.pageHalfHeight = book.Height / 2;
			flipper.flipMode = mode;
			flipper.elapsedTime = 0;
			flipper.startPoint = -flipper.pageHalfHeight + startPoint * 2 * flipper.pageHalfHeight;
			flipper.dragPoint = -flipper.pageHalfHeight + dragPoint * 2 * flipper.pageHalfHeight;
			flipper.oldInteractable = book.interactable;
			book.interactable = false;
			book.BeginDrag();
			if (mode == FlipMode.RightToLeft)
			{
				float x = book.EndBottomRight.x;
				float y = flipper.startPoint;
				book.SetupRightDrag(flipper.startPoint);
				book.DragRightPageToPoint(new Vector3(x, y, 0));
			}
			else
			{
				float x = book.EndBottomLeft.x;
				float y = flipper.startPoint;
				book.SetupLeftDrag(flipper.startPoint);
				book.DragLeftPageToPoint(new Vector3(x, y, 0));
			}
			return true;
		}
		// Update is called once per frame
		void Update()
		{
			if (isFlipping)
			{
				elapsedTime += Time.deltaTime;
				if (elapsedTime < duration)
				{
					if (flipMode == FlipMode.RightToLeft)
					{
						float t = elapsedTime / duration;
						t *= Mathf.PI;

						float x = pageWidth * Mathf.Cos(t);
						float y = startPoint + (dragPoint - startPoint) * Mathf.Sin(t);

						book.UpdateBookToPoint(new Vector3(x, y, 0), false);
					}
					else
					{
						float t = 1 - elapsedTime / duration;
						t *= Mathf.PI;

						float x = pageWidth * Mathf.Cos(t);
						float y = startPoint + (dragPoint - startPoint) * Mathf.Sin(t);

						book.UpdateBookToPoint(new Vector3(x, y, 0), true);
					}

				}
				else
				{
					book.EndDrag();
					book.interactable = oldInteractable;
					book.Flip();
					isFlipping = false;
					this.enabled = false;
					if (finish != null)
						finish();
				}
			}

		}
	}
}