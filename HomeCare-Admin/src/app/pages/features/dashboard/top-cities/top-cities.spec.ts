import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TopCities } from './top-cities';

describe('TopCities', () => {
  let component: TopCities;
  let fixture: ComponentFixture<TopCities>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TopCities]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TopCities);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
