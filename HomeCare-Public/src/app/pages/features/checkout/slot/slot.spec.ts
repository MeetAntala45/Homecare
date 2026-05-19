import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Slot } from './slot';

describe('Slot', () => {
  let component: Slot;
  let fixture: ComponentFixture<Slot>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Slot]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Slot);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
