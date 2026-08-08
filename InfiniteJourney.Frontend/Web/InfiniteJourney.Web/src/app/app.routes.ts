import { Routes } from '@angular/router';
import { staffGuard } from '@core/guards/auth.guard';
import { CampaignAdminComponent } from '@features/campaigns/pages/campaign-admin/campaign-admin.component';
import { CampaignFormComponent } from '@features/campaigns/pages/campaign-form/campaign-form.component';
import { CampaignListComponent } from '@features/campaigns/pages/campaign-list/campaign-list.component';
import { CampaignDetailComponent } from '@features/campaigns/pages/campaign-detail/campaign-detail.component';
import { ThemeAdminComponent } from '@features/theme/pages/theme-admin/theme-admin.component';

export const routes: Routes = [
  { path: '', redirectTo: 'campaigns', pathMatch: 'full' },
  { path: 'campaigns', component: CampaignListComponent },
  { path: 'campaigns/manage', component: CampaignAdminComponent, canActivate: [staffGuard] },
  { path: 'campaigns/manage/new', component: CampaignFormComponent, canActivate: [staffGuard] },
  { path: 'campaigns/manage/edit/:id', component: CampaignFormComponent, canActivate: [staffGuard] },
  { path: 'theme/manage', component: ThemeAdminComponent, canActivate: [staffGuard] },
  { path: 'campaigns/:id', component: CampaignDetailComponent },
];
