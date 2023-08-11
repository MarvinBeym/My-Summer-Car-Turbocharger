using MscModApi.Parts;
using UnityEngine;

namespace SatsumaTurboCharger.part
{
	public abstract class CustomPart : DerivablePart
	{
		public abstract int errorCode { get; }
		protected CustomPart(GameObject part, Part parentPart, PartBaseInfo partBaseInfo, bool uninstallWhenParentUninstalls = true, bool disableCollisionWhenInstalled = true) : base(part, parentPart, partBaseInfo, uninstallWhenParentUninstalls, disableCollisionWhenInstalled)
		{
		}

		protected CustomPart(GameObject parent, PartBaseInfo partBaseInfo, bool uninstallWhenParentUninstalls = true, bool disableCollisionWhenInstalled = true, string prefabName = null) : base(parent, partBaseInfo, uninstallWhenParentUninstalls, disableCollisionWhenInstalled, prefabName)
		{
		}

		protected CustomPart(PartBaseInfo partBaseInfo, bool uninstallWhenParentUninstalls = true, bool disableCollisionWhenInstalled = true, string prefabName = null) : base(partBaseInfo, uninstallWhenParentUninstalls, disableCollisionWhenInstalled, prefabName)
		{
		}

		protected CustomPart(Part parentPart, PartBaseInfo partBaseInfo, bool uninstallWhenParentUninstalls = true, bool disableCollisionWhenInstalled = true, string prefabName = null) : base(parentPart, partBaseInfo, uninstallWhenParentUninstalls, disableCollisionWhenInstalled, prefabName)
		{
		}


	}
}