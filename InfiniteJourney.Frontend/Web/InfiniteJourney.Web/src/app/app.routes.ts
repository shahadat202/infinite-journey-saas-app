import { Routes } from '@angular/router';
import { authGuard } from '@core/guards/auth.guard';
import { CampaignAdminComponent } from '@features/campaigns/pages/campaign-admin/campaign-admin.component';
import { CampaignListComponent } from '@features/campaigns/pages/campaign-list/campaign-list.component';
import { CampaignDetailComponent } from '@features/campaigns/pages/campaign-detail/campaign-detail.component';

export const routes: Routes = [
  { path: '', redirectTo: 'campaigns', pathMatch: 'full' },
  { path: 'campaigns', component: CampaignListComponent },
  { path: 'campaigns/manage', component: CampaignAdminComponent, canActivate: [authGuard] },
  { path: 'campaigns/:id', component: CampaignDetailComponent },
];
