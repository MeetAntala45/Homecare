import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ProfileStateService {
  private photoUrl = new BehaviorSubject<string>('');
  private name = new BehaviorSubject<string>('Partner');
  photoUrl$ = this.photoUrl.asObservable();
  name$ = this.name.asObservable();

  updatePhoto(url: string): void {
    const img = new Image();

    img.onload = () => this.photoUrl.next(url);
    img.onerror = () => this.photoUrl.next("");

    img.src = url;
  }

  updateName(changedName: string): void{
    this.name.next(changedName);
  }
}
