import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RatingsReviews } from './ratings-reviews';

describe('RatingsReviews', () => {
  let component: RatingsReviews;
  let fixture: ComponentFixture<RatingsReviews>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RatingsReviews]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RatingsReviews);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
