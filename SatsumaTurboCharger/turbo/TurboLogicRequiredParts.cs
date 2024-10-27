using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MscModApi.Parts;

namespace SatsumaTurboCharger.part
{
	public class TurboLogicRequiredParts
	{
		protected Dictionary<Part, Part> requiredParts = new Dictionary<Part, Part>();

		protected int totalRequiredPartsCount = 0;

		public bool requiredInstalledAndBolted
		{
			get
			{
				int partCount = 0;
				foreach (KeyValuePair<Part, Part> keyValue in requiredParts)
				{
					Part mainPart = keyValue.Key;
					Part alternativePart = keyValue.Value;

					if (mainPart.bolted && mainPart.installedOnCar)
					{
						partCount++;
					} else if (alternativePart != null && alternativePart.bolted && alternativePart.installedOnCar)
					{
						partCount++;
					}
				}

				return partCount == totalRequiredPartsCount;
			}
		}

		public void Add(Part part)
		{
			Add(part, null);
		}

		public void Add(Part mainPart, Part alternativePart)
		{
			if (mainPart == null)
			{
				return;
			}

			if (requiredParts.ContainsKey(mainPart))
			{
				return;
			}
			requiredParts.Add(mainPart, alternativePart);
			totalRequiredPartsCount++;
		}
	}
}
