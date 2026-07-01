import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CampaignsClient, CampaignDetailDto, GetCampaignByIdRoute } from '@generated/infinite-journey-apis';

@Component({
  selector: 'app-campaign-detail',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './campaign-detail.component.html',
  styleUrl: './campaign-detail.component.scss'
})
export class CampaignDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly campaignsClient = inject(CampaignsClient);

  protected readonly campaign = signal<CampaignDetailDto | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error.set('Campaign not found.');
      this.loading.set(false);
      return;
    }

    this.campaignsClient.campaigns_GetById(id, new GetCampaignByIdRoute({ id: id })).subscribe({
      next: (item) => {
        this.campaign.set(item);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Campaign not found.');
        this.loading.set(false);
      }
    });
  }
}
