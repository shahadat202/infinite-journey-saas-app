import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface ToastMessage {
  id: number;
  type: ToastType;
  text: string;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly _messages = signal<ToastMessage[]>([]);
  private nextId = 1;

  readonly messages = this._messages.asReadonly();

  success(text: string): void {
    this.show('success', text);
  }

  error(text: string): void {
    this.show('error', text);
  }

  warning(text: string): void {
    this.show('warning', text);
  }

  info(text: string): void {
    this.show('info', text);
  }

  dismiss(id: number): void {
    this._messages.update((items) => items.filter((m) => m.id !== id));
  }

  private show(type: ToastType, text: string): void {
    const id = this.nextId++;
    this._messages.update((items) => [...items, { id, type, text }]);
    setTimeout(() => this.dismiss(id), 5000);
  }
}
