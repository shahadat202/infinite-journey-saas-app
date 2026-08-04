import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CampaignListItem } from '@core/models/campaign.model';
import { CampaignApiService } from '@core/services/campaign-api.service';

@Component({
  selector: 'app-campaign-list',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './campaign-list.component.html',
  styleUrl: './campaign-list.component.scss',
})
export class CampaignListComponent implements OnInit {
  private readonly api = inject(CampaignApiService);

  protected readonly campaigns = signal<CampaignListItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.api
      .getPaged({ pageIndex: 0, pageSize: 50, sortBy: 'createdat', sortDirection: 'desc', status: 'Active' })
      .subscribe({
        next: (page) => {
          this.campaigns.set(page.data);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('Unable to load campaigns. Is the API running on port 5274?');
          this.loading.set(false);
        },
      });
  }

  progress(item: CampaignListItem): number {
    return item.targetAmount && item.targetAmount > 0
      ? Math.round(((item.raisedAmount || 0) / item.targetAmount) * 100)
      : 0;
  }

  coverUrl(item: CampaignListItem): string {
    return this.api.resolveMediaUrl(item.coverImageUrl);
  }
}
