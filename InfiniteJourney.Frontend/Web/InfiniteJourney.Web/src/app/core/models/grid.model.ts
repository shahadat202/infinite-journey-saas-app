export interface PagedResult<T> {
  data: T[];
  pageIndex: number;
  pageSize: number;
  total: number;
}

export interface GridQuery {
  pageIndex: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface GridColumn<T> {
  key: keyof T | string;
  label: string;
  sortable?: boolean;
  format?: (row: T) => string;
}
