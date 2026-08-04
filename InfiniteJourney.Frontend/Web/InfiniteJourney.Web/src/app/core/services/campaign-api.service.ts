import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CampaignDetail,
  CampaignListItem,
  CreateCampaignRequest,
  UpdateCampaignRequest,
  UploadFileResult,
} from '@core/models/campaign.model';
import { GridQuery, PagedResult } from '@core/models/grid.model';
import { TenantContextService } from '@core/services/tenant-context.service';

@Injectable({ providedIn: 'root' })
export class CampaignApiService {
  private readonly http = inject(HttpClient);
  private readonly tenant = inject(TenantContextService);

  private url(path: string): string {
    return `${this.tenant.apiBaseUrl()}${path}`;
  }

  getPaged(query: GridQuery & { status?: string }): Observable<PagedResult<CampaignListItem>> {
    let params = new HttpParams()
      .set('pageIndex', query.pageIndex)
      .set('pageSize', query.pageSize);

    if (query.search) params = params.set('search', query.search);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);
    if (query.status) params = params.set('status', query.status);

    return this.http.get<PagedResult<CampaignListItem>>(this.url('/api/campaigns'), { params });
  }

  getById(id: string): Observable<CampaignDetail> {
    return this.http.get<CampaignDetail>(this.url(`/api/campaigns/${id}`));
  }

  create(body: CreateCampaignRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.url('/api/campaigns'), body);
  }

  update(id: string, body: UpdateCampaignRequest): Observable<CampaignDetail> {
    return this.http.put<CampaignDetail>(this.url(`/api/campaigns/${id}`), body);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(this.url(`/api/campaigns/${id}`));
  }

  activate(id: string): Observable<CampaignDetail> {
    return this.http.post<CampaignDetail>(this.url(`/api/campaigns/${id}/activate`), {});
  }

  uploadFile(
    fileName: string,
    contentType: string,
    base64Data: string,
    category: 'Images' | 'Pdfs' | 'Documents' = 'Images'
  ): Observable<UploadFileResult> {
    return this.http.post<UploadFileResult>(this.url('/api/files/upload'), {
      fileName,
      contentType,
      base64Data,
      category,
    });
  }

  resolveMediaUrl(path?: string | null): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    return `${this.tenant.apiBaseUrl()}${path}`;
  }
}
