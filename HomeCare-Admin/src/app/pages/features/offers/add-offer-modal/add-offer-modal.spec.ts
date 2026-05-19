import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddOfferModal } from './add-offer-modal';

describe('AddOfferModal', () => {
  let component: AddOfferModal;
  let fixture: ComponentFixture<AddOfferModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddOfferModal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddOfferModal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
