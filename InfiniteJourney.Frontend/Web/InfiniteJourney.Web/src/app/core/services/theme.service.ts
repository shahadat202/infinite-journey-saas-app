import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { TenantContextService } from '@core/services/tenant-context.service';

export interface ThemeDto {
  id: string;
  primaryColor: string;
  secondaryColor: string;
  accentColor: string;
  fontFamily: string;
  isDarkMode: boolean;
}

export interface UpdateThemeRequest {
  primaryColor: string;
  secondaryColor: string;
  accentColor: string;
  fontFamily: string;
  isDarkMode: boolean;
}

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly http = inject(HttpClient);
  private readonly tenant = inject(TenantContextService);
  
  private readonly _currentTheme = signal<ThemeDto | null>(null);
  readonly currentTheme = this._currentTheme.asReadonly();
  
  private url(path: string): string {
    return `${this.tenant.apiBaseUrl()}${path}`;
  }

  loadTheme(): Promise<void> {
    return new Promise((resolve) => {
      this.http.get<ThemeDto>(this.url('/api/theme')).subscribe({
        next: (theme) => {
          this._currentTheme.set(theme);
          this.applyTheme(theme);
          resolve();
        },
        error: () => {
          const defaultTheme: ThemeDto = {
            id: '',
            primaryColor: '#1e3a8a',
            secondaryColor: '#10b981',
            accentColor: '#F59E0B',
            fontFamily: 'Inter, sans-serif',
            isDarkMode: false
          };
          this._currentTheme.set(defaultTheme);
          this.applyTheme(defaultTheme);
          resolve();
        }
      });
    });
  }

  applyTheme(theme: ThemeDto): void {
    const root = document.documentElement;
    root.style.setProperty('--primary', theme.primaryColor);
    root.style.setProperty('--secondary', theme.secondaryColor);
    root.style.setProperty('--accent', theme.accentColor);
    if (theme.fontFamily) {
      root.style.setProperty('font-family', theme.fontFamily);
    }
    if (theme.isDarkMode) {
      document.body.classList.add('dark-theme');
      root.style.setProperty('--background', '#0f172a');
      root.style.setProperty('--text', '#f8fafc');
      root.style.setProperty('--surface', '#1e293b');
      root.style.setProperty('--border', '#334155');
    } else {
      document.body.classList.remove('dark-theme');
      root.style.setProperty('--background', '#f8fafc');
      root.style.setProperty('--text', '#0f172a');
      root.style.setProperty('--surface', '#ffffff');
      root.style.setProperty('--border', '#e2e8f0');
    }
  }

  getTheme(): Observable<ThemeDto> {
    return this.http.get<ThemeDto>(this.url('/api/theme'));
  }

  updateTheme(request: UpdateThemeRequest): Observable<ThemeDto> {
    return this.http.put<ThemeDto>(this.url('/api/theme'), request).pipe(
      tap((updated) => {
        this._currentTheme.set(updated);
        this.applyTheme(updated);
      })
    );
  }
}
