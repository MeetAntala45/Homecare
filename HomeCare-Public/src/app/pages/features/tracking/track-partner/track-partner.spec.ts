import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TrackPartner } from './track-partner';

describe('TrackPartner', () => {
  let component: TrackPartner;
  let fixture: ComponentFixture<TrackPartner>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TrackPartner]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TrackPartner);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
