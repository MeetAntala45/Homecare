import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-page-content',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './page-content.html',
  styleUrls: ['./page-content.css']
})
export class PageContentComponent {
  @Input() pageTitle = '';
  @Input() showFilter = false;
  @Input() showAdd = false;

  @Output() filterClick  = new EventEmitter<void>();
  @Output() addClick     = new EventEmitter<void>();
  @Output() logoutClick  = new EventEmitter<void>();
}