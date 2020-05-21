using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYEngine
{
	public class BookShadowMask : MonoBehaviour
	{
		public Texture mask;
		public Vector2 offset = new Vector2(0, 0);
		public Vector2 size = new Vector2(1, 1);
		UITexture shadow;

		// Start is called before the first frame update
		void Start()
		{
			shadow = GetComponent<UITexture>();
			shadow.onRender = OnRenderCallback;
		}

		void OnRenderCallback(Material mat)
		{
			if (mask == null)
				return;

			Vector4 cr = new Vector4(offset.x, offset.y, size.x / 2, size.y / 2);

			mat.SetVector("_TexClipRange0", new Vector4(-cr.x / cr.z, -cr.y / cr.w, 1f / cr.z, 1f / cr.w));
			mat.SetTexture("_MaskTex", mask);
		}
	}
}
