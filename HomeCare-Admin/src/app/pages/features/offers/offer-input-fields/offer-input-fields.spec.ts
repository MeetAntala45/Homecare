import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OfferInputFields } from './offer-input-fields';

describe('OfferInputFields', () => {
  let component: OfferInputFields;
  let fixture: ComponentFixture<OfferInputFields>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OfferInputFields]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OfferInputFields);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
