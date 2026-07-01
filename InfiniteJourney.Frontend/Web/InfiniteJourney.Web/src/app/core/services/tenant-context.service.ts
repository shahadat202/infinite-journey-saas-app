import { Injectable, computed, inject, signal } from '@angular/core';
import { AppConfigService } from '@core/config/app-config.service';

@Injectable({ providedIn: 'root' })
export class TenantContextService {
  private readonly appConfig = inject(AppConfigService);
  private readonly _subdomain = signal(this.resolveSubdomain());

  readonly subdomain = this._subdomain.asReadonly();
  readonly tenantLabel = computed(() =>
    this._subdomain()
      ? this._subdomain()!.charAt(0).toUpperCase() + this._subdomain()!.slice(1)
      : 'Platform'
  );

  apiBaseUrl(): string {
    const host = window.location.hostname;
    const port = this.appConfig.apiPort;
    const protocol = window.location.protocol;

    if (host.endsWith('.localhost') || host.includes('infinitejourney.com')) {
      return `${protocol}//${host}:${port}`;
    }

    return `${protocol}//localhost:${port}`;
  }

  private resolveSubdomain(): string | null {
    const host = window.location.hostname;

    if (host.endsWith('.localhost')) {
      return host.replace('.localhost', '');
    }

    if (host.endsWith('.infinitejourney.com')) {
      return host.replace('.infinitejourney.com', '');
    }

    return null;
  }
}
