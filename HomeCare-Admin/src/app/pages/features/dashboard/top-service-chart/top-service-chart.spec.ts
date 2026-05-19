import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TopServiceChart } from './top-service-chart';

describe('TopServiceChart', () => {
  let component: TopServiceChart;
  let fixture: ComponentFixture<TopServiceChart>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TopServiceChart]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TopServiceChart);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
