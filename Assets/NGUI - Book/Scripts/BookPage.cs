using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CYEngine
{
	public class BookPage : MonoBehaviour
	{
		public UIPanel pagePanel;

		[System.Serializable]
		public struct SubPanelInfo
		{
			public UIPanel panel;
			public int depthOffset;

			public SubPanelInfo(UIPanel panel, int depthOffset)
			{
				this.panel = panel;
				this.depthOffset = depthOffset;
			}
		}

		public List<SubPanelInfo> subPanelList = new List<SubPanelInfo>();

		// Start is called before the first frame update
		void Start()
		{
		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}
