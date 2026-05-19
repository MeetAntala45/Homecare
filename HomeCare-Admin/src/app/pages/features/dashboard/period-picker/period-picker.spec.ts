import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PeriodPicker } from './period-picker';

describe('PeriodPicker', () => {
  let component: PeriodPicker;
  let fixture: ComponentFixture<PeriodPicker>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PeriodPicker]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PeriodPicker);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
