import { Injectable, inject } from '@angular/core';
import Keycloak from 'keycloak-js';
import { AppConfigService } from '../config/app-config.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly appConfig = inject(AppConfigService);
  private keycloak!: Keycloak;

  private _authenticated = false;

  get authenticated(): boolean {
    return this._authenticated;
  }

  async init(): Promise<void> {
    const { url, realm, clientId } = this.appConfig.keycloak;

    this.keycloak = new Keycloak({ url, realm, clientId });

    this._authenticated = await this.keycloak.init({
      onLoad: 'check-sso',
      pkceMethod: 'S256',
      silentCheckSsoRedirectUri: window.location.origin + '/assets/silent-check-sso.html',
      checkLoginIframe: false
    });
  }

  login(): void {
    void this.keycloak.login({ redirectUri: window.location.origin + '/' });
  }

  logout(): void {
    void this.keycloak.logout({ redirectUri: window.location.origin + '/' });
  }

  get accessToken(): string {
    return this.keycloak.token ?? '';
  }

  get userName(): string {
    return this.keycloak.tokenParsed?.['preferred_username'] ?? this.keycloak.tokenParsed?.['email'] ?? '';
  }
}
