import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditServiceDialog } from './edit-service-dialog';

describe('EditServiceDialog', () => {
  let component: EditServiceDialog;
  let fixture: ComponentFixture<EditServiceDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditServiceDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditServiceDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
