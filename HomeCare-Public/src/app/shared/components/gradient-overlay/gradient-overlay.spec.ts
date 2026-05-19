import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GradientOverlay } from './gradient-overlay';

describe('GradientOverlay', () => {
  let component: GradientOverlay;
  let fixture: ComponentFixture<GradientOverlay>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GradientOverlay]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GradientOverlay);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
