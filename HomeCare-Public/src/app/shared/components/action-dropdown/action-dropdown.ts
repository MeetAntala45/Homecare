import { Component, Input, Output, EventEmitter, HostListener, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { IActionItem } from '../../../core/models/shared/IActionDropDownModel';
import { DropdownStateService } from '../../../core/services/shared/dropdown-state.service';


@Component({
  selector: 'app-action-dropdown',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './action-dropdown.html',
  styleUrl: './action-dropdown.css'
})
export class ActionDropdown implements OnInit, OnDestroy {
  @Input() actions: IActionItem[] = [];
  @Output() actionClicked = new EventEmitter<string>();

  id = Math.random().toString(36).slice(2);
  isOpen = false;
  menuStyle: Record<string, string> = {};
  private sub!: Subscription;

  constructor(private dropdownState: DropdownStateService) { }

  ngOnInit() {
    this.sub = this.dropdownState.closeAll.subscribe(exceptId => {
      if (exceptId !== this.id) this.isOpen = false;
    });
  }

  ngOnDestroy() {
    this.sub.unsubscribe();
  }

  toggle(event: MouseEvent) {
    event.stopPropagation();
    const opening = !this.isOpen;

    if (opening) {
      this.dropdownState.requestClose(this.id);
      const btn = (event.target as HTMLElement).closest('button')!;
      const rect = btn.getBoundingClientRect();

      const menuHeight = 120;
      const spaceBelow = window.innerHeight - rect.bottom;
      const showAbove = spaceBelow < menuHeight;

      this.menuStyle = {
        position: 'fixed',
        top: showAbove ? `${rect.top - menuHeight + 50}px` : `${rect.bottom + 4}px`,
        left: `${rect.left - 120}px`,
        zIndex: '9999',
      };
    }

    this.isOpen = opening;
  }

  @HostListener('document:click', ['$event'])
  onOutsideClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (!target.closest('app-action-dropdown')) {
      this.isOpen = false;
    }
  }

  @HostListener('window:scroll', [])
  onWindowScroll() {
    if (this.isOpen) this.isOpen = false;
  }

  onActionClick(action: string) {
    this.isOpen = false;
    this.actionClicked.emit(action);
  }
}