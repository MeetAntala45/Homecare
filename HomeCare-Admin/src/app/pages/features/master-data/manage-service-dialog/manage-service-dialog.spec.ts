import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageServiceDialog } from './manage-service-dialog';

describe('ManageServiceDialog', () => {
  let component: ManageServiceDialog;
  let fixture: ComponentFixture<ManageServiceDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageServiceDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManageServiceDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
