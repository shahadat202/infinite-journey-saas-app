import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { TenantContextService } from '@core/services/tenant-context.service';
import { ToastContainerComponent } from '@shared/components/toast/toast-container.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, ToastContainerComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly auth = inject(AuthService);
  protected readonly tenant = inject(TenantContextService);
  protected readonly sidebarCollapsed = signal(false);
  protected readonly sidebarIconOnly = signal(false);

  protected toggleSidebar(): void {
    this.sidebarCollapsed.update(collapsed => !collapsed);
  }

  protected toggleSidebarIconOnly(): void {
    this.sidebarIconOnly.update(iconOnly => !iconOnly);
  }

  protected closeSidebarOnMobile(): void {
    if (window.innerWidth < 1024) {
      this.sidebarCollapsed.set(true);
    }
  }
}
