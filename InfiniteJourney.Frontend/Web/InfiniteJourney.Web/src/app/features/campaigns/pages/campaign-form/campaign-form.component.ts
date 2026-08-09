import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { CampaignApiService } from '@core/services/campaign-api.service';
import { FileUploadService } from '@core/services/file-upload.service';
import { ToastService } from '@core/services/toast.service';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzInputNumberModule } from 'ng-zorro-antd/input-number';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCardModule } from 'ng-zorro-antd/card';
import { QuillModule } from 'ngx-quill';

@Component({
  selector: 'app-campaign-form',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    NzFormModule,
    NzInputModule,
    NzInputNumberModule,
    NzDatePickerModule,
    NzButtonModule,
    NzCardModule,
    QuillModule
  ],
  templateUrl: './campaign-form.component.html',
  styleUrl: './campaign-form.component.scss'
})
export class CampaignFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(CampaignApiService);
  private readonly upload = inject(FileUploadService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly editingId = signal<string | null>(null);
  protected readonly coverPreview = signal<string | null>(null);

  protected readonly editorModules = {
    toolbar: [
      ['bold', 'italic', 'underline', 'strike'],
      ['blockquote', 'code-block'],
      [{ 'header': 1 }, { 'header': 2 }],
      [{ 'list': 'ordered'}, { 'list': 'bullet' }],
      [{ 'script': 'sub'}, { 'script': 'super' }],
      [{ 'indent': '-1'}, { 'indent': '+1' }],
      ['link', 'clean']
    ]
  };

  protected readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.maxLength(4000)]],
    targetAmount: [1000, [Validators.required, Validators.min(1)]],
    coverImageUrl: [''],
    startDate: [null as Date | null],
    endDate: [null as Date | null]
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.editingId.set(id);
      this.loadCampaign(id);
    }
  }

  loadCampaign(id: string): void {
    this.loading.set(true);
    this.api.getById(id).subscribe({
      next: (campaign) => {
        this.coverPreview.set(this.api.resolveMediaUrl(campaign.coverImageUrl));
        this.form.patchValue({
          title: campaign.title,
          description: campaign.description,
          targetAmount: campaign.targetAmount,
          coverImageUrl: campaign.coverImageUrl ?? '',
          startDate: campaign.startDate ? new Date(campaign.startDate) : null,
          endDate: campaign.endDate ? new Date(campaign.endDate) : null
        });
        this.loading.set(false);
      },
      error: () => {
        this.toast.error('Failed to load campaign details.');
        this.router.navigate(['/campaigns/manage']);
      }
    });
  }

  async onFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    try {
      this.saving.set(true);
      const path = await this.upload.uploadImage(file);
      this.form.patchValue({ coverImageUrl: path });
      this.coverPreview.set(this.api.resolveMediaUrl(path));
      this.toast.success('Campaign cover image uploaded successfully.');
    } catch {
      this.toast.error('Image upload failed.');
    } finally {
      this.saving.set(false);
      input.value = '';
    }
  }

  removeCover(): void {
    this.form.patchValue({ coverImageUrl: '' });
    this.coverPreview.set(null);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const value = this.form.getRawValue();
    const body = {
      title: value.title,
      description: value.description,
      targetAmount: value.targetAmount,
      coverImageUrl: value.coverImageUrl || null,
      startDate: value.startDate ? value.startDate.toISOString() : null,
      endDate: value.endDate ? value.endDate.toISOString() : null
    };

    const id = this.editingId();
    const request$ = id ? this.api.update(id, body) : this.api.create(body);

    request$.subscribe({
      next: () => {
        this.toast.success(id ? 'Campaign updated successfully.' : 'Campaign created successfully.');
        this.saving.set(false);
        this.router.navigate(['/campaigns/manage']);
      },
      error: () => this.saving.set(false)
    });
  }
}
