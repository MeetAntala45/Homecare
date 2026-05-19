import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddSubcategoryDialog } from './add-subcategory-dialog';

describe('AddSubcategoryDialog', () => {
  let component: AddSubcategoryDialog;
  let fixture: ComponentFixture<AddSubcategoryDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddSubcategoryDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddSubcategoryDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
