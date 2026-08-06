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

/**
 * Defines a single row-level action button in the DataGrid.
 *
 * @example
 * { label: 'Edit',     action: 'edit' }
 * { label: 'Activate', action: 'activate', style: 'accent', visible: row => row.status !== 'Active' }
 * { label: 'Delete',   action: 'delete', style: 'danger' }
 */
export interface GridAction<T> {
  /** Identifier emitted in the rowAction output. */
  action: string;
  /** Button label. */
  label: string;
  /** Visual variant. Defaults to 'default'. */
  style?: 'default' | 'accent' | 'danger';
  /**
   * Optional per-row visibility predicate.
   * When omitted the button is always shown.
   */
  visible?: (row: T) => boolean;
}
