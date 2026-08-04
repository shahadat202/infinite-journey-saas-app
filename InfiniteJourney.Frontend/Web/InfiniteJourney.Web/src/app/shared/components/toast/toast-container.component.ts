import { Component, inject } from '@angular/core';
import { ToastService } from '@core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  templateUrl: './toast-container.component.html',
  styleUrl: './toast-container.component.scss',
})
export class ToastContainerComponent {
  protected readonly toast = inject(ToastService);
}
