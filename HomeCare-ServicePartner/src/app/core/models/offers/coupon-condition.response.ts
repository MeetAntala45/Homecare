export interface IConditionResponse {
    id: number;
    conditionTypeId: number;
    conditionType: string;
    operator: string;
    value: string;
    failBehaviour: string;
  }