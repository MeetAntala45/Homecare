export interface ICreateOfferRequest {
  offerCode: string;
  description: string;
  discountPct: number;
  conditions?: IConditionRow[];
}

export interface IConditionRow {
  conditionTypeId: number;
  operator: string;
  value: string;
  failBehaviour: string;
}