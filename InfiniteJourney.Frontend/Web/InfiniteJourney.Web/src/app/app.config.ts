import { APP_INITIALIZER, ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { en_US, provideNzI18n } from 'ng-zorro-antd/i18n';

import { routes } from './app.routes';
import { authInterceptor } from '@core/interceptors/auth.interceptor';
import { errorInterceptor } from '@core/interceptors/error.interceptor';
import { AuthService } from '@core/services/auth.service';
import { AppConfigService } from '@core/config/app-config.service';
import { TenantContextService } from '@core/services/tenant-context.service';
import { ThemeService } from '@core/services/theme.service';
import { API_BASE_URL } from '@generated/infinite-journey-apis';

function initApplication(config: AppConfigService, auth: AuthService, theme: ThemeService): () => Promise<void> {
  return async () => {
    await config.load();
    await auth.init();
    await theme.loadTheme();
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    provideNzI18n(en_US),
    {
      provide: API_BASE_URL,
      useFactory: (tenantService: TenantContextService) => tenantService.apiBaseUrl(),
      deps: [TenantContextService]
    },
    {
      provide: APP_INITIALIZER,
      useFactory: initApplication,
      deps: [AppConfigService, AuthService, ThemeService],
      multi: true
    }
  ]
};
