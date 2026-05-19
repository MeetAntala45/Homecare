import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SelectAddressModal } from './select-address-modal';

describe('SelectAddressModal', () => {
  let component: SelectAddressModal;
  let fixture: ComponentFixture<SelectAddressModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SelectAddressModal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SelectAddressModal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
