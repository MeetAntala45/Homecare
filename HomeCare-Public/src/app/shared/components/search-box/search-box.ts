import { Component, EventEmitter, Input, Output, HostListener, ElementRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIcon } from "@angular/material/icon";
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-search-box',
  standalone: true,
  imports: [CommonModule, MatIcon, FormsModule],
  templateUrl: './search-box.html',
  styleUrl: './search-box.css',
})
export class SearchBox<T> {

  constructor(private el: ElementRef) { }

  @Input() items: T[] = [];
  @Input() displayKey!: keyof T;
  @Input() variant: 'glass' | 'light' = 'light';
  @Input() placeholder: string = 'Search...';
  @Input() mode: 'dropdown' | 'live' = 'dropdown';

  @Output() itemSelected = new EventEmitter<T>();
  @Output() searchClicked = new EventEmitter<string>();
  @Output() searchCleared = new EventEmitter<void>();
  @Output() searchedText = new EventEmitter<string>();

  searchText = '';
  filteredItems: T[] = [];
  selectedItem?: T;
  showDropdown = false;

  onSearchChange() {
    const value = this.searchText.trim().toLowerCase();

    this.searchedText.emit(this.searchText);
    this.selectedItem = undefined;

    if (!value) {
      this.filteredItems = [];
      this.showDropdown = false;

      if (this.mode === 'live') {
        this.searchCleared.emit();
      }
      return;
    }
    if (this.mode === 'dropdown') {
      this.filteredItems = this.items.filter(item =>
        String(item[this.displayKey]).toLowerCase().includes(value)
      );
      this.showDropdown = true;
    } else {
      this.showDropdown = false;
    }
  }

  selectItem(item: T) {
    this.searchText = String((item)[this.displayKey]);
    this.selectedItem = item;
    this.showDropdown = false;
  }

  onEnterPress() {
    this.onSearchClick();
  }

  onSearchClick() {
    const value = this.searchText.trim();

    if (!value) return;

    if (this.mode === 'dropdown') {
      if (!this.selectedItem) return;
      this.itemSelected.emit(this.selectedItem);
      return;
    }

    if (this.mode === 'live') {
      this.searchClicked.emit(value);
    }
  }
  get isSearchDisabled(): boolean {
    if (this.mode == 'live') {
      return !this.searchText.trim();
    }

    return this.mode === 'dropdown' && !this.selectedItem;
  }

  clearSearch() {
    this.searchText = '';
    this.filteredItems = [];
    this.showDropdown = false;
    this.selectedItem = undefined;
  }

  @HostListener('document:click')
  closeDropdown() {
    this.showDropdown = false;
  }
}