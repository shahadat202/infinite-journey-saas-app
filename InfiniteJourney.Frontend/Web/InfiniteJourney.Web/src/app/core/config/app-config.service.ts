import { Injectable } from '@angular/core';
import { AppConfig, defaultConfig } from './app-config.model';

@Injectable({ providedIn: 'root' })
export class AppConfigService {
  private config: AppConfig = defaultConfig;

  async load(): Promise<void> {
    try {
      const response = await fetch('/assets/app-config.json', { cache: 'no-store' });
      if (response.ok) {
        this.config = { ...defaultConfig, ...(await response.json()) };
      }
    } catch {
      this.config = defaultConfig;
    }
  }

  get apiPort(): number {
    return this.config.apiPort;
  }

  get keycloak(): AppConfig['keycloak'] {
    return this.config.keycloak;
  }
}
