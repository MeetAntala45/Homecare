import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class DropdownStateService {
  private closeAll$ = new Subject<string>();
  closeAll = this.closeAll$.asObservable();

  requestClose(exceptId: string) {
    this.closeAll$.next(exceptId);
  }
}