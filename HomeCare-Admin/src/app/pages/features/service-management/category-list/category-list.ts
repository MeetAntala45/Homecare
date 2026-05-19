import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { ICategory } from '../../../../core/models/service-management/service';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  templateUrl: './category-list.html',
  styleUrl: './category-list.css'
})
export class CategoryList {

  @Input() categories: ICategory[] = [];
  @Input() activeCategoryId!: number | null;
  @Input() loadingCategories: boolean = true;

  @Output() categorySelected = new EventEmitter<ICategory>();

  select(cat: ICategory) {
    this.categorySelected.emit(cat);
  }

}