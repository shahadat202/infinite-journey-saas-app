import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CampaignsClient, CampaignListItemDto, CampaignStatus, GetCampaignsQuery } from '@generated/infinite-journey-apis';

@Component({
  selector: 'app-campaign-list',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './campaign-list.component.html',
  styleUrl: './campaign-list.component.scss'
})
export class CampaignListComponent implements OnInit {
  private readonly campaignsClient = inject(CampaignsClient);

  protected readonly campaigns = signal<CampaignListItemDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.campaignsClient.campaigns_GetAll(new GetCampaignsQuery({ status: CampaignStatus.Active })).subscribe({
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

  progress(item: CampaignListItemDto): number {
    return (item.targetAmount && item.targetAmount > 0)
      ? Math.round(((item.raisedAmount || 0) / item.targetAmount) * 100)
      : 0;
  }
}
