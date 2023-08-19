using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SatsumaTurboCharger.turbo
{
	public class TurboConditionStorage
	{
		protected Dictionary<string, Condition> conditions = new Dictionary<string, Condition>();
		protected Action conditionsHaveUpdatedAction;

		public TurboConditionStorage()
		{

		}

		public void DefineConditionsHaveUpdatedAction(Action action)
		{
			if (conditionsHaveUpdatedAction != null)
			{
				return;
			}

			conditionsHaveUpdatedAction = action;
			conditionsHaveUpdatedAction.Invoke();


		}

		public void AddCondition(Condition condition)
		{
			conditions.Add(condition.id, condition);
		}

		public void AddConditions(IEnumerable<Condition> conditions)
		{
			foreach (Condition condition in conditions)
			{
				AddCondition(condition);
			}
		}

		public List<Condition> GetConditions()
		{
			return conditions.Values.ToList();
		}

		public Condition GetCondition(string conditionId)
		{
			return conditions.TryGetValue(conditionId, out Condition condition) ? condition : null;
		}

		public void UpdateCondition(string conditionId, bool applyCondition)
		{
			Condition condition = GetCondition(conditionId);
			if (condition == null)
			{
				return;
			}

			if (condition.applyCondition == applyCondition)
			{
				return;
			}

			condition.applyCondition = applyCondition;
			conditionsHaveUpdatedAction?.Invoke();
		}
	}
}