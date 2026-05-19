import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CancelBooking } from './cancel-booking';

describe('CancelBooking', () => {
  let component: CancelBooking;
  let fixture: ComponentFixture<CancelBooking>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CancelBooking]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CancelBooking);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
