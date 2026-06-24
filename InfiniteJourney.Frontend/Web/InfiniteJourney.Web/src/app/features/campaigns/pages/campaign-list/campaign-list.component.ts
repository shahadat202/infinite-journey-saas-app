import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CampaignApiService } from '../../../../core/services/campaign-api.service';
import { CampaignListItem } from '../../../../core/models/campaign.model';

@Component({
  selector: 'app-campaign-list',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './campaign-list.component.html',
  styleUrl: './campaign-list.component.scss'
})
export class CampaignListComponent implements OnInit {
  private readonly campaignApi = inject(CampaignApiService);

  protected readonly campaigns = signal<CampaignListItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.campaignApi.getCampaigns('Active').subscribe({
      next: (items) => {
        this.campaigns.set(items);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Unable to load campaigns. Is the API running on port 5274?');
        this.loading.set(false);
      }
    });
  }

  progress(item: CampaignListItem): number {
    return item.targetAmount > 0 ? Math.round((item.raisedAmount / item.targetAmount) * 100) : 0;
  }
}
