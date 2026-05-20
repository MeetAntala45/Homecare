export interface IWallet {
  balance: number;
  transactions: IWalletTransaction[];
}

export interface IWalletTransaction {
  amount: number;
  type: 'Credit' | 'Debit';
  description: string;
  createdAt: string;
}

export interface IReferralInfo {
  referralCode: string;
  totalReferrals: number;
  maxReferrals: number;
  referees: IRefereeStatus[];
}

export interface IRefereeStatus {
  name: string;
  status: 'Pending' | 'Rewarded' | 'Cancelled';
  joinedAt: string;
}
