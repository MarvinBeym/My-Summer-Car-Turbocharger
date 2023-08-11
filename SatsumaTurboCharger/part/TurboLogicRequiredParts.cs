﻿using System;
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
		protected int requiredPartsInstalledCount = 0;

		public bool requiredInstalledAndBolted => requiredPartsInstalledCount == totalRequiredPartsCount;


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
			mainPart.AddEventListener(EventTime.Post, EventType.Bolted, () =>
			{
				requiredPartsInstalledCount++;
			});

			mainPart.AddEventListener(EventTime.Pre, EventType.Unbolted, () =>
			{
				if (mainPart.bolted)
				{
					requiredPartsInstalledCount--;
				}
			});

			if (alternativePart == null)
			{
				return;
			}

			alternativePart.AddEventListener(EventTime.Post, EventType.Bolted, () =>
			{
				requiredPartsInstalledCount++;
			});
			
			alternativePart.AddEventListener(EventTime.Pre, EventType.Unbolted, () =>
			{
				if (alternativePart.bolted)
				{
					requiredPartsInstalledCount--;
				}
			});


		}
	}
}
