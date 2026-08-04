import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { CampaignApiService } from '@core/services/campaign-api.service';

@Injectable({ providedIn: 'root' })
export class FileUploadService {
  private readonly campaignApi = inject(CampaignApiService);

  async uploadImage(file: File): Promise<string> {
    const processed = await this.compressImage(file, 1200);
    const base64 = await this.readAsDataUrl(processed);
    const result = await firstValueFrom(
      this.campaignApi.uploadFile(processed.name, processed.type, base64, 'Images')
    );

    if (!result?.path) throw new Error('Upload failed.');
    return result.path;
  }

  async uploadPdf(file: File): Promise<string> {
    const base64 = await this.readAsDataUrl(file);
    const result = await firstValueFrom(
      this.campaignApi.uploadFile(file.name, file.type, base64, 'Pdfs')
    );

    if (!result?.path) throw new Error('Upload failed.');
    return result.path;
  }

  private readAsDataUrl(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = () => reject(reader.error);
      reader.readAsDataURL(file);
    });
  }

  private compressImage(file: File, maxWidth: number): Promise<File> {
    if (!file.type.startsWith('image/')) return Promise.resolve(file);

    return new Promise((resolve, reject) => {
      const img = new Image();
      const url = URL.createObjectURL(file);

      img.onload = () => {
        URL.revokeObjectURL(url);
        const scale = Math.min(1, maxWidth / img.width);
        const canvas = document.createElement('canvas');
        canvas.width = Math.round(img.width * scale);
        canvas.height = Math.round(img.height * scale);
        const ctx = canvas.getContext('2d');
        if (!ctx) {
          resolve(file);
          return;
        }
        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
        canvas.toBlob(
          (blob) => {
            if (!blob) {
              resolve(file);
              return;
            }
            resolve(new File([blob], file.name, { type: file.type }));
          },
          file.type,
          0.85
        );
      };

      img.onerror = () => {
        URL.revokeObjectURL(url);
        reject(new Error('Unable to process image.'));
      };

      img.src = url;
    });
  }
}
