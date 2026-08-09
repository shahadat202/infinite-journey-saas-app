import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { CampaignListItem } from '@core/models/campaign.model';
import { GridQuery } from '@core/models/grid.model';
import { CampaignApiService } from '@core/services/campaign-api.service';
import { ToastService } from '@core/services/toast.service';
import { NzTableModule, NzTableQueryParams } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzCardModule } from 'ng-zorro-antd/card';

@Component({
  selector: 'app-campaign-admin',
  imports: [CommonModule, RouterLink, NzTableModule, NzButtonModule, NzInputModule, NzCardModule],
  templateUrl: './campaign-admin.component.html',
  styleUrl: './campaign-admin.component.scss',
})
export class CampaignAdminComponent implements OnInit {
  protected readonly api = inject(CampaignApiService);
  private readonly toast = inject(ToastService);

  protected readonly loading = signal(false);
  protected readonly data = signal<CampaignListItem[]>([]);
  protected readonly total = signal(0);
  protected readonly pageIndex = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly search = signal('');
  protected readonly sortBy = signal('createdat');
  protected readonly sortDir = signal<'asc' | 'desc'>('desc');

  private readonly searchSubject = new Subject<string>();

  ngOnInit(): void {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(searchTerm => {
      this.search.set(searchTerm);
      this.pageIndex.set(1);
      this.loadData();
    });

    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    const query: GridQuery = {
      pageIndex: this.pageIndex() - 1,
      pageSize: this.pageSize(),
      search: this.search(),
      sortBy: this.sortBy(),
      sortDirection: this.sortDir()
    };

    this.api.getPaged(query).subscribe({
      next: (page) => {
        this.data.set(page.data);
        this.total.set(page.total);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  onQueryParamsChange(params: NzTableQueryParams): void {
    const { pageSize, pageIndex, sort } = params;
    this.pageIndex.set(pageIndex);
    this.pageSize.set(pageSize);

    const currentSort = sort.find(item => item.value !== null);
    if (currentSort) {
      this.sortBy.set(currentSort.key);
      this.sortDir.set(currentSort.value === 'ascend' ? 'asc' : 'desc');
    } else {
      this.sortBy.set('createdat');
      this.sortDir.set('desc');
    }

    this.loadData();
  }

  onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchSubject.next(input.value);
  }

  toggleActivation(row: CampaignListItem): void {
    const isActive = row.status === 'Active';
    const request$ = isActive ? this.api.deactivate(row.id) : this.api.activate(row.id);

    request$.subscribe({
      next: () => {
        this.toast.success(isActive ? 'Campaign deactivated successfully.' : 'Campaign activated successfully.');
        this.loadData();
      },
      error: (err: any) => {
        this.toast.error(err?.error?.message || 'Failed to toggle campaign status.');
      }
    });
  }

  deleteCampaign(row: CampaignListItem): void {
    if (!confirm(`Are you sure you want to delete "${row.title}"?`)) return;

    this.api.delete(row.id).subscribe({
      next: () => {
        this.toast.success('Campaign deleted successfully.');
        this.loadData();
      },
      error: (err: any) => {
        this.toast.error(err?.error?.message || 'Failed to delete campaign.');
      }
    });
  }
}
