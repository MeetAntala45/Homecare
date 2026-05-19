import { ComponentFixture, TestBed } from '@angular/core/testing';

import { JobsCalendar } from './jobs-calendar';

describe('JobsCalendar', () => {
  let component: JobsCalendar;
  let fixture: ComponentFixture<JobsCalendar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobsCalendar]
    })
    .compileComponents();

    fixture = TestBed.createComponent(JobsCalendar);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
