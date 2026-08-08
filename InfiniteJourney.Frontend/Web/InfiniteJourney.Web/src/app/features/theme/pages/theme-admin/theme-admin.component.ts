import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ThemeService, ThemeDto } from '@core/services/theme.service';
import { ToastService } from '@core/services/toast.service';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzSwitchModule } from 'ng-zorro-antd/switch';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCardModule } from 'ng-zorro-antd/card';

@Component({
  selector: 'app-theme-admin',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    NzFormModule,
    NzInputModule,
    NzSelectModule,
    NzSwitchModule,
    NzButtonModule,
    NzCardModule
  ],
  templateUrl: './theme-admin.component.html',
  styleUrl: './theme-admin.component.scss'
})
export class ThemeAdminComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly themeService = inject(ThemeService);
  private readonly toast = inject(ToastService);

  protected readonly loading = signal(false);
  protected readonly saving = signal(false);

  protected readonly fontFamilies = [
    { value: 'Inter, sans-serif', label: 'Inter' },
    { value: 'Roboto, sans-serif', label: 'Roboto' },
    { value: 'Outfit, sans-serif', label: 'Outfit' },
    { value: 'Poppins, sans-serif', label: 'Poppins' }
  ];

  protected readonly form = this.fb.nonNullable.group({
    primaryColor: ['#1e3a8a', [Validators.required, Validators.pattern(/^#[0-9a-fA-F]{6}$/)]],
    secondaryColor: ['#10b981', [Validators.required, Validators.pattern(/^#[0-9a-fA-F]{6}$/)]],
    accentColor: ['#F59E0B', [Validators.required, Validators.pattern(/^#[0-9a-fA-F]{6}$/)]],
    fontFamily: ['Inter, sans-serif', [Validators.required]],
    isDarkMode: [false]
  });

  ngOnInit(): void {
    this.loadTheme();
  }

  loadTheme(): void {
    this.loading.set(true);
    this.themeService.getTheme().subscribe({
      next: (theme) => {
        if (theme) {
          this.form.patchValue({
            primaryColor: theme.primaryColor,
            secondaryColor: theme.secondaryColor,
            accentColor: theme.accentColor,
            fontFamily: theme.fontFamily,
            isDarkMode: theme.isDarkMode
          });
        }
        this.loading.set(false);
      },
      error: () => {
        this.toast.error('Failed to load theme settings.');
        this.loading.set(false);
      }
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const value = this.form.getRawValue();

    this.themeService.updateTheme(value).subscribe({
      next: () => {
        this.toast.success('Theme settings updated successfully.');
        this.saving.set(false);
      },
      error: () => {
        this.toast.error('Failed to update theme settings.');
        this.saving.set(false);
      }
    });
  }
}
