import { Component, inject } from '@angular/core';
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
}
