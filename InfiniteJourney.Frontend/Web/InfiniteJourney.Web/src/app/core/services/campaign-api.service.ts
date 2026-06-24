import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CampaignDetail, CampaignListItem, CreateCampaignRequest } from '../models/campaign.model';
import { TenantContextService } from './tenant-context.service';

@Injectable({ providedIn: 'root' })
export class CampaignApiService {
  private readonly http = inject(HttpClient);
  private readonly tenantContext = inject(TenantContextService);

  private url(path: string): string {
    return `${this.tenantContext.apiBaseUrl()}${path}`;
  }

  getCampaigns(status?: string): Observable<CampaignListItem[]> {
    const query = status ? `?status=${encodeURIComponent(status)}` : '';
    return this.http.get<CampaignListItem[]>(this.url(`/api/campaigns${query}`));
  }

  getCampaign(id: string): Observable<CampaignDetail> {
    return this.http.get<CampaignDetail>(this.url(`/api/campaigns/${id}`));
  }

  createCampaign(request: CreateCampaignRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.url('/api/campaigns'), request);
  }

  activateCampaign(id: string): Observable<CampaignDetail> {
    return this.http.post<CampaignDetail>(this.url(`/api/campaigns/${id}/activate`), {});
  }
}
