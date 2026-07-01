import { Routes } from '@angular/router';
import { CampaignListComponent } from '@features/campaigns/pages/campaign-list/campaign-list.component';
import { CampaignDetailComponent } from '@features/campaigns/pages/campaign-detail/campaign-detail.component';

export const routes: Routes = [
  { path: '', redirectTo: 'campaigns', pathMatch: 'full' },
  { path: 'campaigns', component: CampaignListComponent },
  { path: 'campaigns/:id', component: CampaignDetailComponent }
];
