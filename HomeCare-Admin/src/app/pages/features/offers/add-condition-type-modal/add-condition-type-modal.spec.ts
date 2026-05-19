import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddConditionTypeModal } from './add-condition-type-modal';

describe('AddConditionTypeModal', () => {
  let component: AddConditionTypeModal;
  let fixture: ComponentFixture<AddConditionTypeModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddConditionTypeModal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddConditionTypeModal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
