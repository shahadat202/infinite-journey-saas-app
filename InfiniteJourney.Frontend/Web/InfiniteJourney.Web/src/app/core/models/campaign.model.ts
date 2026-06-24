export interface CampaignListItem {
  id: string;
  title: string;
  description: string;
  targetAmount: number;
  raisedAmount: number;
  status: string;
  coverImageUrl?: string | null;
  startDate?: string | null;
  endDate?: string | null;
}

export interface CampaignDetail extends CampaignListItem {
  createdAt: string;
  progressPercent: number;
}

export interface CreateCampaignRequest {
  title: string;
  description: string;
  targetAmount: number;
  coverImageUrl?: string | null;
}
