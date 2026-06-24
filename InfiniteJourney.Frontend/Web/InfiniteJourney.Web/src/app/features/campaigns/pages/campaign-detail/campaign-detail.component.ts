import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CampaignApiService } from '../../../../core/services/campaign-api.service';
import { CampaignDetail } from '../../../../core/models/campaign.model';

@Component({
  selector: 'app-campaign-detail',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './campaign-detail.component.html',
  styleUrl: './campaign-detail.component.scss'
})
export class CampaignDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly campaignApi = inject(CampaignApiService);

  protected readonly campaign = signal<CampaignDetail | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error.set('Campaign not found.');
      this.loading.set(false);
      return;
    }

    this.campaignApi.getCampaign(id).subscribe({
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
