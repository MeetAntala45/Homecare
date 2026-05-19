import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GridLayout } from './grid-layout';

describe('GridLayout', () => {
  let component: GridLayout;
  let fixture: ComponentFixture<GridLayout>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GridLayout]
    })
    .compileComponents();

    fixture = TestBed.createComponent(GridLayout);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
