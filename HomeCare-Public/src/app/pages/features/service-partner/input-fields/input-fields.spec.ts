import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InputFields } from './input-fields';

describe('InputFields', () => {
  let component: InputFields;
  let fixture: ComponentFixture<InputFields>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InputFields]
    })
    .compileComponents();

    fixture = TestBed.createComponent(InputFields);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
