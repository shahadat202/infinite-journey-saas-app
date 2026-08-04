import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CampaignListItem } from '@core/models/campaign.model';
import { GridColumn, GridQuery, PagedResult } from '@core/models/grid.model';
import { CampaignApiService } from '@core/services/campaign-api.service';
import { FileUploadService } from '@core/services/file-upload.service';
import { ToastService } from '@core/services/toast.service';
import { DataGridComponent } from '@shared/components/data-grid/data-grid.component';

@Component({
  selector: 'app-campaign-admin',
  imports: [ReactiveFormsModule, RouterLink, DataGridComponent],
  templateUrl: './campaign-admin.component.html',
  styleUrl: './campaign-admin.component.scss',
})
export class CampaignAdminComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(CampaignApiService);
  private readonly upload = inject(FileUploadService);
  private readonly toast = inject(ToastService);

  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly showForm = signal(false);
  protected readonly editingId = signal<string | null>(null);
  protected readonly coverPreview = signal<string | null>(null);

  protected readonly result = signal<PagedResult<CampaignListItem>>({
    data: [],
    pageIndex: 0,
    pageSize: 10,
    total: 0,
  });

  protected readonly columns: GridColumn<CampaignListItem>[] = [
    { key: 'title', label: 'Title', sortable: true },
    { key: 'status', label: 'Status', sortable: true },
    {
      key: 'raisedAmount',
      label: 'Raised / Target',
      sortable: true,
      format: (row) => `${row.raisedAmount} / ${row.targetAmount}`,
    },
  ];

  protected readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.maxLength(4000)]],
    targetAmount: [0, [Validators.required, Validators.min(1)]],
    coverImageUrl: [''],
    startDate: [''],
    endDate: [''],
  });

  ngOnInit(): void {
    this.load({ pageIndex: 0, pageSize: 10, sortBy: 'createdat', sortDirection: 'desc' });
  }

  load(query: GridQuery): void {
    this.loading.set(true);
    this.api.getPaged(query).subscribe({
      next: (page) => {
        this.result.set(page);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  openCreate(): void {
    this.editingId.set(null);
    this.coverPreview.set(null);
    this.form.reset({
      title: '',
      description: '',
      targetAmount: 0,
      coverImageUrl: '',
      startDate: '',
      endDate: '',
    });
    this.showForm.set(true);
  }

  openEdit(row: CampaignListItem): void {
    this.editingId.set(row.id);
    this.coverPreview.set(this.api.resolveMediaUrl(row.coverImageUrl));
    this.form.patchValue({
      title: row.title,
      description: row.description,
      targetAmount: row.targetAmount,
      coverImageUrl: row.coverImageUrl ?? '',
      startDate: row.startDate ? row.startDate.slice(0, 10) : '',
      endDate: row.endDate ? row.endDate.slice(0, 10) : '',
    });
    this.showForm.set(true);
  }

  closeForm(): void {
    this.showForm.set(false);
    this.editingId.set(null);
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
      this.toast.success('Image uploaded.');
    } catch {
      this.toast.error('Image upload failed.');
    } finally {
      this.saving.set(false);
      input.value = '';
    }
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const body = {
      title: value.title,
      description: value.description,
      targetAmount: value.targetAmount,
      coverImageUrl: value.coverImageUrl || null,
      startDate: value.startDate ? new Date(value.startDate).toISOString() : null,
      endDate: value.endDate ? new Date(value.endDate).toISOString() : null,
    };

    this.saving.set(true);
    const id = this.editingId();

    const request$ = id ? this.api.update(id, body) : this.api.create(body);

    request$.subscribe({
      next: () => {
        this.toast.success(id ? 'Campaign updated.' : 'Campaign created.');
        this.saving.set(false);
        this.closeForm();
        this.load({ pageIndex: this.result().pageIndex, pageSize: this.result().pageSize });
      },
      error: () => this.saving.set(false),
    });
  }

  activate(row: CampaignListItem): void {
    this.api.activate(row.id).subscribe({
      next: () => {
        this.toast.success('Campaign activated.');
        this.load({ pageIndex: this.result().pageIndex, pageSize: this.result().pageSize });
      },
    });
  }

  onRowAction(event: { action: string; row: CampaignListItem }): void {
    if (event.action === 'edit') {
      this.openEdit(event.row);
      return;
    }

    if (event.action === 'activate') {
      this.activate(event.row);
      return;
    }

    if (event.action === 'delete') {
      if (!confirm(`Delete "${event.row.title}"?`)) return;
      this.api.delete(event.row.id).subscribe({
        next: () => {
          this.toast.success('Campaign deleted.');
          this.load({ pageIndex: this.result().pageIndex, pageSize: this.result().pageSize });
        },
      });
    }
  }
}
