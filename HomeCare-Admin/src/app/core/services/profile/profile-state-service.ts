import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ProfileStateService {
  private photoUrl = new BehaviorSubject<string>('');
  photoUrl$ = this.photoUrl.asObservable();

  updatePhoto(url: string): void {
    this.photoUrl.next(url);
  }
}
