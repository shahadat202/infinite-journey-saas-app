import { DecimalPipe } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GridColumn, GridQuery, PagedResult } from '@core/models/grid.model';

@Component({
  selector: 'app-data-grid',
  imports: [FormsModule, DecimalPipe],
  templateUrl: './data-grid.component.html',
  styleUrl: './data-grid.component.scss',
})
export class DataGridComponent<T extends object> {
  readonly title = input<string>('');
  readonly columns = input.required<GridColumn<T>[]>();
  readonly result = input.required<PagedResult<T>>();
  readonly loading = input(false);
  readonly searchPlaceholder = input('Search…');

  readonly queryChange = output<GridQuery>();
  readonly rowAction = output<{ action: string; row: T }>();

  protected search = '';
  protected query: GridQuery = { pageIndex: 0, pageSize: 10, sortBy: undefined, sortDirection: 'desc' };

  cellValue(row: T, column: GridColumn<T>): string {
    if (column.format) return column.format(row);
    const value = row[column.key as keyof T];
    if (value == null) return '';
    return String(value);
  }

  onSearch(): void {
    this.query = { ...this.query, pageIndex: 0, search: this.search.trim() || undefined };
    this.queryChange.emit(this.query);
  }

  onSort(column: GridColumn<T>): void {
    if (!column.sortable) return;
    const key = String(column.key);
    const same = this.query.sortBy === key;
    const direction = same && this.query.sortDirection === 'asc' ? 'desc' : 'asc';
    this.query = { ...this.query, sortBy: key, sortDirection: direction };
    this.queryChange.emit(this.query);
  }

  goToPage(index: number): void {
    if (index < 0) return;
    const totalPages = Math.ceil(this.result().total / this.result().pageSize);
    if (index >= totalPages) return;
    this.query = { ...this.query, pageIndex: index };
    this.queryChange.emit(this.query);
  }

  sortIndicator(column: GridColumn<T>): string {
    if (!column.sortable || this.query.sortBy !== String(column.key)) return '';
    return this.query.sortDirection === 'asc' ? ' ↑' : ' ↓';
  }

  totalPages(): number {
    const r = this.result();
    return Math.max(1, Math.ceil(r.total / r.pageSize));
  }
}
