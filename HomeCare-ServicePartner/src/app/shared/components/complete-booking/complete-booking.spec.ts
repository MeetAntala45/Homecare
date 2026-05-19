import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CompleteBooking } from './complete-booking';

describe('CompleteBooking', () => {
  let component: CompleteBooking;
  let fixture: ComponentFixture<CompleteBooking>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CompleteBooking]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CompleteBooking);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
